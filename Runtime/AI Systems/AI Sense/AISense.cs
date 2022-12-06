using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.AI
{
    public abstract class AISense : ScriptableObject
    {
        public float radius = 10f;
        public float lostRadius = 20f;
        
        [Tooltip("Time needed for this sense to deactivate")]
        public float age = 5f;
        
        [Tooltip("Time needed for this sense to deactivate when the sensed target has been lost")]
        public float lostAge = 10f;
        
        [Tooltip("True if you want this sense to manually call senses update through script." +
                 "N.B. report based senses will be ignored by the sensing component default evaluation process")]
        public bool reportBased = false;
        
        public AISense()
        {
           ID = GetType().ToString();
        }

        ///<summary>
        /// Unique identifier for this sense.
        ///</summary>
        public static string ID { get; private set; }

        ///<summary>
        /// The game object which owns the sensing component and implements
        /// this sense.
        ///</summary>
        protected GameObject owner;
        
        public abstract Collider2D[] SenseTargets();

        ///<summary>
        /// Called each time this sense is updated with a report event.
        ///</summary>
        public abstract SenseResult OnSenseUpdate(GameObject sensedObject, float deltaTime);
        
        public abstract void DrawDebugSense();
        
        ///<summary>
        /// Make a copy of this Sense asset.
        ///</summary>
        ///<returns> A copy of this sense asset.</returns>
        public AISense Clone()
        {
            return Instantiate(this);
        }

        public virtual void InitializeSense(GameObject owner)
        {
            this.owner = owner;
        }

        ///<summary>
        /// Check if the sensed object is a stimuli source.
        /// Check this condition when you want your sense to only
        /// sense objects which are registered as stimuli source.
        ///</summary>
        ///<returns> True if object is a stimuli source, false otherwise</returns>
        protected bool IsStimuliSource(GameObject target)
        {
            return target.GetComponent<SenseStimuliSource>() != null;
        }
    }
    
    public struct SenseResult
    {
        public GameObject sensedObject;
        public bool successfullySensed;
        public string senseTag;
        public string senseID;
        public float senseAge;
        public float senseLostAge;
    }
}



