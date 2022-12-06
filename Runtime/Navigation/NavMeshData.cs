using System.Collections;
using System.Collections.Generic;
using ComputationalGeometry;
using ComputationGeometry_DOTS;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Core
{
    public class NavMeshData : ScriptableObject
    {
        [HideInInspector] public int2 size;
        [HideInInspector] public float2 origin;
        [HideInInspector] public float2 cellSize;

        ///<summary>
        /// The navigable surface on which agents can move.
        /// it is represented as a list of triangles(Collision geometry).
        ///</summary>
        [HideInInspector] public Triangle2D[] navigableSurface;

        ///<summary>
        /// The type of grid the pathfinding algorithm will use
        /// to compute the best path fro
        ///</summary>
        [HideInInspector] public EGridType gridType;

        public GridData GetGrid()
        {
            return new GridData(size, origin, cellSize, gridType);
        }
        
        ///<summary>
        /// The navigable surface on which agents can move.
        /// it is represented as a list of triangles(Collision geometry).
        ///</summary>
        public Triangle[] GetNavigableSurface()
        {
            int length = this.navigableSurface.Length;
            Triangle[] navigableSurface = new Triangle[length];
            for(int i = 0; i < length; i++)
            {
                Triangle2D triangle = this.navigableSurface[i];
                navigableSurface[i] = new Triangle((Vector2)triangle.A, 
                                                     (Vector2)triangle.B, 
                                                     (Vector2)triangle.C);
            }
            return navigableSurface;
        }
    }
}

