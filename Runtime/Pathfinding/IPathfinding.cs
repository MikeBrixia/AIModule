using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Core
{
    public interface IPathfinding
    {
        public void Initialize(float2 start, float2 end, GridData gridData, NativeArray<float2> navData);
        public void FindPath();
        void ComputePath();
    }

}
