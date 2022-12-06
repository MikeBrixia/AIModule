using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.AI
{
    [CreateAssetMenu(fileName ="AI Sense Hearing", menuName = "AI/Sensing/AI Sense Hearing")]
    public class AISenseHearing : AISense
    {
        ///<summary>
        /// The targets which can be sensed.
        ///</summary>
        public LayerMask targets;
        
        private SenseResult senseResult = new SenseResult();

        public override void DrawDebugSense()
        {
            throw new System.NotImplementedException();
        }

        public override SenseResult OnSenseUpdate(GameObject sensedObject, float deltaTime) 
        {
            // Calculate the distance from the object which is sensing and the sensed object
            float NoiseDistance = Vector2.Distance(owner.transform.position, sensedObject.transform.position);
            if(NoiseDistance <= radius)
            {
                senseResult.sensedObject = sensedObject;
                senseResult.successfullySensed = true;
                senseResult.senseAge = age;
                senseResult.senseID = ID;
            }
            return senseResult;
        }

        public override Collider2D[] SenseTargets()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(owner.transform.position, radius, targets);
            return colliders;
        }
    }
}