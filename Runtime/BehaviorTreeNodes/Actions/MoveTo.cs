using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT.Runtime;

namespace Core.AI
{
    public class MoveTo : BT_ActionNode
    {
        public string ownerKey;
        public Vector2 targetLocation;
        public float acceptanceRadius;
        private AIController controller;

        // Called when the behavior tree wants to execute this action.
        // Modify the 'state' has you need, return SUCCESS when you want this node
        // to succed, RUNNING when you want to notify the tree that this node is still running
        // and has not finished yet and FAILED when you want this node to fail
        public override EBehaviorTreeState Execute()
        {
            bool movementResult = controller.MoveTo(targetLocation, acceptanceRadius);
            state = movementResult? EBehaviorTreeState.Success : EBehaviorTreeState.Running;
            return state;
        }

        // Called when the behavior tree starts executing this action
        protected override void OnStart()
        {
           if(controller == null)
               controller = blackboard.GetBlackboardValueByKey<GameObject>(ownerKey)
                                      .GetComponent<AIController>();
        }

        // Called when the behavior tree stops executing this action
        protected override void OnStop()
        {

        }
    }
}
