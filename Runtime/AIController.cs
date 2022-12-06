using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.AI
{
    [RequireComponent(typeof(Pathfinding), typeof(MovementComponent2D))]
    public class AIController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private bool movementEnabled = true;
        private Vector2 targetPosition;
        private int movementIndex = 0;
        private List<Unity.Mathematics.float2> path = new List<Unity.Mathematics.float2>();

        [Tooltip("Should the AI movement be 2D or 3D?")]
        [SerializeField] private bool horizontalMovementOnly = false;
        private MovementComponent2D movement;
        private Vector2 movementDirection;

        [Header("Components")]
        Pathfinding pathfinding;

        ///<summary>
        /// True if the AI can move, false otherwise.
        ///</summary>
        public bool canMove
        {
            get
            {
                return movementEnabled;
            }
            set
            {
                movementEnabled = value;
                if (value == false)
                    movementDirection = Vector2.zero;
            }
        }

        void Awake()
        {
            movement = GetComponent<MovementComponent2D>();
            pathfinding = GetComponent<Pathfinding>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
           
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying)
                foreach (Unity.Mathematics.float2 point in path)
                    Gizmos.DrawWireSphere((Vector2)point, .1f);
        }

        void FixedUpdate()
        {
            if (horizontalMovementOnly)
                movement.AddHorizontalMovement(movementDirection.x);
            else
                movement.AddMovement(movementDirection);
        }

        ///<summary>
        /// Move towards target position.
        ///</summary>
        ///<param name="target"> The position you want the AI to reach</param>
        ///<returns> true if the AI has reached the position, false otherwise</returns>
        public bool MoveTo(Vector2 target, float acceptanceRadius)
        {
            Vector2 currentPosition = transform.position;
            // Check if AI is already at position, if true skip all the operations
            // and simply return.
            bool hasReachedTarget = Vector2.Distance(currentPosition, target) <= acceptanceRadius;
            if (!hasReachedTarget)
            {
                if (!currentPosition.Equals(target))
                {
                    // If the target position is changed calculate a new path and reset movement.
                    if (!target.Equals(targetPosition))
                    {
                        // Cast back float2 array to Vector2 array. This is not necessary but helps keeping
                        // the MonoBehavior design pattern instead of ECS.
                        path = pathfinding.FindPath(currentPosition, target).ToList();
                        targetPosition = target;
                        movementIndex = 0;
                    }

                    if (path.Count > 0)
                    {
                        Vector2 targetPoint = path[movementIndex];

                        // Check if the AI has reached the current point.
                        float distance = Vector2.Distance(currentPosition, targetPoint);
                        bool hasReachedPoint = distance <= acceptanceRadius;

                        // If AI has not reached the current point keep calculating the direction from this
                        // object to the point and set it as the movement direction
                        if (!hasReachedPoint)
                            movementDirection = Math.GetUnitDirectionVector(currentPosition, targetPoint);
                        else
                        {
                            // If the AI has reached it's end goal clear the path list and return true, 
                            // otherwise if it only reached one of the path points keep incrementing the 
                            // path index to move to the next point.
                            hasReachedTarget = movementIndex == path.Count - 1;
                            if (hasReachedTarget)
                            {
                                path.Clear();
                                movementIndex = 0;
                                movementDirection = Vector2.zero;
                            }
                            else
                                movementIndex++;
                        }
                    }

                }
            }
            return hasReachedTarget;
        }

        ///<summary>
        /// Move towards target object.
        ///</summary>
        ///<param name="target"> The target object you want the AI to reach</param>
        ///<returns> true if the AI has reached the position, false otherwise</returns>
        public bool MoveTo(GameObject target, float acceptanceRadius)
        {
            return MoveTo(target.transform.position, acceptanceRadius);
        }
    }
}

