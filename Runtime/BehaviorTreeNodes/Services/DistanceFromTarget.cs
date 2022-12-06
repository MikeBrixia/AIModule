using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT.Runtime;

public class DistanceFromTarget : BT_Service
{
    [Tooltip("The distance key you want to update")]
    public string distanceKey;

    [Tooltip("The target which will be used to measure distance from the behavior tree owner")]
    public string targetKey;

    public string ownerKey;
    
    private Transform owner;
    private Transform target; 

    // Called each UpdateInterval tick
    protected override void OnUpdate()
    {
        float distance = Vector2.Distance(owner.position, target.position);
        blackboard.SetBlackbordValue<float>(distanceKey, distance);
    }
    
    // Called when this service becomes active and starts updating
    protected override void OnStart()
    {
        target = blackboard.GetBlackboardValueByKey<GameObject>(targetKey).transform;
        if(owner == null)
           owner = blackboard.GetBlackboardValueByKey<GameObject>(ownerKey).transform;
    }
    
    // Called when this service gets deactivated and stops updating
    protected override void OnStop()
    {
        
    }
}