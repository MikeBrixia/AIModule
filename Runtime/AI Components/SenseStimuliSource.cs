using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.AI
{
    ///<summary>
    /// Component used to handle stimuli events and notify AI Sensing Components.
    /// When attached to a game object it will mark it as Sensable.
    /// If you want to implement your own Report events for your custom senses
    /// you can create a new class and inherit from this component.
    ///</summary>
    public class SenseStimuliSource : MonoBehaviour
    {
        ///<summary>
        /// Report a 2D Noise event.
        ///</summary>
        public void ReportHearingEvent2D(GameObject noiseInstigator, float radius, float strength = 1f)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(noiseInstigator.transform.position, radius * strength);
            foreach (Collider2D collider in colliders)
            {
                if (collider != null)
                {
                    AISensingComponent sensingComponent = collider.GetComponent<AISensingComponent>();
                    if (sensingComponent != null)
                    {
                        AISense hearingSense = sensingComponent.registeredSenses[typeof(AISenseHearing)];
                        sensingComponent.UpdateSense(hearingSense, noiseInstigator);
                    }
                }
            }
        }
    }
}

