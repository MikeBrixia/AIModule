using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT.Runtime;
using Core;

public class LineOfSight : BT_Decorator
{
    [Tooltip("The behavior tree owner")]
    public string ownerKey;

    [Tooltip("The target which you want to be seen")]
    public string targetKey;

    [Tooltip("The sight radius")]
    public float radius = 8f;

    [Tooltip("The peripheal vision angle.")]
    public float visionAngle = 45;

    [Tooltip("Layers which will not block the line of sight.")]
    public LayerMask targetsToIgnore;
    
    private GameObject target;
    private GameObject owner;

    // Called when the behavior tree wants to execute this decorator.
    // Modify the 'state' has you need, return SUCCESS when you want this node
    // to succed, RUNNING when you want to notify the tree that this node is still running
    // and has not finished yet and FAILED when you want this node to fail
    public override EBehaviorTreeState Execute()
    {
        // Compute direction between the behavior tree owner and the target
        Vector2 targetDirection = Math.GetUnitDirectionVector(owner.transform.position, target.transform.position);
        // Check if the target is in line of sight or not. Blocking objects will break the line of sight.
        RaycastHit2D result = Physics2D.Raycast(owner.transform.position, targetDirection, radius, ~targetsToIgnore);
        bool canSee = Vector2.Angle(owner.transform.right, targetDirection) <= visionAngle && result.collider != null;
        state = canSee? EBehaviorTreeState.Success : EBehaviorTreeState.Failed;
        return state;
    }

    // Called when the behavior tree starts executing this action
    protected override void OnStart()
    {
       target = blackboard.GetBlackboardValueByKey<GameObject>(targetKey);
       // Initialize behavior tree owner
       if(owner == null)
          owner = blackboard.GetBlackboardValueByKey<GameObject>(ownerKey);
    }

    // Called when the behavior tree stops executing this action
    protected override void OnStop()
    {

    }

    #if UNITY_EDITOR
    public LineOfSight() : base()
    {
        description = "Check if the target is in line of sight";
    }
    #endif
}