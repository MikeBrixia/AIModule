using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Core.AI
{
    public class AISensingComponent : MonoBehaviour
    {
        [SerializeField] private List<AISense> senses;

        ///<summary>
        /// When set to true senses will be updated each frame
        ///</summary>
        [Tooltip("When set to true senses will be update each frame")]
        public bool canTick = false;

        [Tooltip("The rate at which this component it's going to update senses. N.B. if canTick is set to true this value will be ignored")]
        public float updateInterval = 0.1f;

        public List<GameObject> sensableObjects;
        public UnityEvent<SenseResult> OnSenseUpdateCallback;

        ///<summary>
        /// All the senses registered inside this component by type.
        /// SensingComponent can only have 1 sense per type registered.
        ///</summary>
        public Dictionary<Type, AISense> registeredSenses { get; private set; }

        /// <summary>
        /// All the registered senses which this component it's goingto update.
        /// </summary>
        public List<AISense> componentSenses { get; private set; }

        private bool allReportBased = false;

        ///<summary>
        /// The interval at which this component is updating.
        ///</summary>
        private float interval = 0f;

        void Awake()
        {
            registeredSenses = new Dictionary<Type, AISense>();
            componentSenses = new List<AISense>();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Initialize senses for this component
            senses.ForEach(sense => CreateSense(sense));
            
            foreach (AISense sense in registeredSenses.Values)
            {
                allReportBased = sense.reportBased;
                // If at least one registered sense it's not report based
                // notify the component that it will need to update
                if (!allReportBased)
                    break;
            }

            if (!canTick && !allReportBased)
                InvokeRepeating("SenseUpdate", 0.1f, updateInterval);

            interval = canTick? Time.deltaTime : updateInterval;
        }

        ///<summary>
        /// Create a brand new sense from an asset and register it.
        ///</summary>
        public AISense CreateSense(AISense sense)
        {
            AISense createdSense = sense.Clone();
            createdSense.InitializeSense(gameObject);
            registeredSenses.Add(createdSense.GetType(), createdSense);
            return createdSense;
        }

        // Update is called once per frame
        void Update()
        {
            if (canTick && !allReportBased)
            {
                foreach (AISense sense in registeredSenses.Values)
                {
                    // If sense is not report based update it
                    if (!sense.reportBased)
                        UpdateSense(sense);
                }
            }
        }

        void SenseUpdate()
        {
            if (!allReportBased)
            {
                foreach (AISense sense in registeredSenses.Values)
                {
                    // If sense is not report based update it
                    if (!sense.reportBased)
                        UpdateSense(sense);
                }
            }
        }

        ///<summary>
        /// Perform an update of the given sense. This is the
        /// standard version.
        ///</summary>
        ///<param name="sense"> The sense to update</param>
        public void UpdateSense(AISense sense)
        {
            Collider2D[] sensedObjects = sense.SenseTargets();
            foreach(Collider2D target in sensedObjects)
            {
                SenseResult result = sense.OnSenseUpdate(target.gameObject, interval);
                OnSenseUpdateCallback.Invoke(result);
            }
        }
        
        ///<summary>
        /// Perform an update of the given sense. This is the
        /// report based version
        ///</summary>
        ///<param name="sense"> The sense to update</param>
        ///<param name="instigator"> The instigator of this update</param>
        public void UpdateSense(AISense sense, GameObject instigator)
        {
           SenseResult result = sense.OnSenseUpdate(instigator, interval);
           OnSenseUpdateCallback.Invoke(result);
        }
    }
}

