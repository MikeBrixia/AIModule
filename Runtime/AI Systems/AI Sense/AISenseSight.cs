using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.AI
{
    [CreateAssetMenu(fileName = "AI Sense Sight", menuName = "AI/Sensing/AI Sense Sight")]
    public sealed class AISenseSight : AISense
    {
        ///<summary>
        /// The peripheal vision angle.
        ///</summary>
        public float visionAngle = 45;

        ///<summary>
        /// The targets which can be sensed.
        ///</summary>
        public LayerMask targets;

        ///<summary>
        /// The targets which we want to ignore.
        ///</summary>
        public LayerMask targetsToIgnore;

        private SenseResult senseResult = new SenseResult();
        private float currentTime = 0f;

        public override Collider2D[] SenseTargets()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(owner.transform.position, radius, targets);
            return colliders;
        }

        public override SenseResult OnSenseUpdate(GameObject sensedObject, float deltaTime)
        {
            if (IsStimuliSource(sensedObject))
            {
                Vector2 targetDirection = Math.GetUnitDirectionVector(owner.transform.position, sensedObject.transform.position);
                RaycastHit2D result = Physics2D.Raycast(owner.transform.position, targetDirection, radius, ~targetsToIgnore);
                if (result.collider != null)
                {
                    if (targets == (targets.value | (1 << result.collider.gameObject.layer))
                        && Vector2.Angle(owner.transform.right, targetDirection) <= visionAngle)
                    {
                        senseResult.successfullySensed = true;
                        senseResult.sensedObject = result.collider.gameObject;
                        senseResult.senseID = ID;
                        currentTime = 0f;
                    }
                    else
                        SenseFailed(deltaTime);
                }
                else
                    SenseFailed(deltaTime);
            }
            return senseResult;
        }

        public override void DrawDebugSense()
        {

        }

        private void SenseFailed(float deltaTime)
        {
            senseResult.senseID = ID;
            if (senseResult.successfullySensed && currentTime > age)
            {
                senseResult.successfullySensed = false;
                senseResult.sensedObject = null;
                currentTime = 0f;
            }
            else
            {
                currentTime += deltaTime;
            }
        }
    }
}

