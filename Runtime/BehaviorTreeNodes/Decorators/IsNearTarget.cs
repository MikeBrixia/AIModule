using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT.Runtime;

public class IsNearTarget : BT_Decorator
{
    public string ownerKey = "Owner";
    public string targetKey;
    public float distanceThreshold;
    public bool invertResult = false;

    private GameObject owner;
    private Vector3 targetPosition;

    // Called when the behavior tree wants to execute this decorator.
    // Modify the 'state' has you need, return SUCCESS when you want this node
    // to succed, RUNNING when you want to notify the tree that this node is still running
    // and has not finished yet and FAILED when you want this node to fail
    public override EBehaviorTreeState Execute()
    {
        float distance = Vector3.Distance(owner.transform.position, targetPosition);
        if(!invertResult)
            state = distance <= distanceThreshold? EBehaviorTreeState.Success : EBehaviorTreeState.Failed;
        else
            state = distance <= distanceThreshold? EBehaviorTreeState.Failed : EBehaviorTreeState.Success;
        return state;
    }
    
    // Called when the behavior tree starts executing this action
    protected override void OnStart()
    {
        if(owner == null)
            owner = blackboard.GetBlackboardValueByKey<GameObject>(ownerKey);
        targetPosition = blackboard.GetBlackboardValueByKey<GameObject>(targetKey).transform.position;
    }
    
    // Called when the behavior tree stops executing this action
    protected override void OnStop()
    {
        
    }
}