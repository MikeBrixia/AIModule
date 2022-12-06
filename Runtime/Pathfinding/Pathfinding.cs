using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ComputationGeometry_DOTS;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Core
{

    public enum EPathfinding { AStar, Dijkstra }

    public struct PathfindingHandler
    {
        public IJob job;
        public JobHandle handle;
    }

    public class Pathfinding : MonoBehaviour
    {
        ///<summary>
        /// The pathfinding algorithm to use
        ///</summary>
        [Tooltip("The pathfinding algorithm to use")]
        public EPathfinding pathfindingMode = EPathfinding.AStar;
        public NavMeshData navigationData;

        // Start is called before the first frame update
        void Start()
        {
            //foreach(Triangle2D triangle in navigationData.navigableSurface)
            //    GeometryLibrary.DrawTriangle(triangle, Color.red, 10f);
        }

        // Update is called once per frame
        void Update()
        {

        }

        ///<summary>
        /// Find the shortest path between start and end using supplied grid and nav data.
        ///</summary>
        ///<param name="start"> The position from which we will start to search </param>
        ///<param name="end"> The target position you want to reach </param>
        ///<param name="gridData"> Informations about grid widht, height and type(hex, ecc..)</param>
        ///<param name="navData"> Navigation data used to determine what's walkable and what's not</param>
        ///<returns> The path from start to end</returns>
        public float2[] FindPath(Vector2 start, Vector2 end, NavMeshData navData)
        {
            GridData gridData = navData.GetGrid();
            float2[] path = null;
            NativeArray<Triangle2D> data = new NativeArray<Triangle2D>(navData.navigableSurface, Allocator.TempJob);
            if (pathfindingMode == EPathfinding.AStar)
            {
                AStar aStar = new AStar(start, end, gridData, data);
                aStar.ComputePath();
                path = aStar.path.ToArray();
                aStar.Dispose();
            }
            path.Reverse();
            return path;
        }

        ///<summary>
        /// Find the shortest path between start and end using supplied grid and nav data.
        ///</summary>
        ///<param name="start"> The position from which we will start to search </param>
        ///<param name="end"> The target position you want to reach </param>
        ///<param name="gridData"> Informations about grid widht, height and type(hex, ecc..)</param>
        ///<param name="navData"> Navigation data used to determine what's walkable and what's not</param>
        ///<returns> The path from start to end</returns>
        public float2[] FindPath(Vector2 start, Vector2 end)
        {
            GridData gridData = navigationData.GetGrid();
            float2[] path = null;
            NativeArray<Triangle2D> data = new NativeArray<Triangle2D>(navigationData.navigableSurface, Allocator.TempJob);
            if (pathfindingMode == EPathfinding.AStar)
            {
                AStar aStar = new AStar(start, end, gridData, data);
                aStar.ComputePath();
                path = aStar.path.ToArray();
                aStar.Dispose();
            }
            path = path.Reverse().ToArray();
            return path;
        }

        ///<summary>
        /// Find the shortest path between start and end using supplied grid and nav data.
        /// This is the Async version, meaning that it will be scheduled to run as a job
        /// on a worker thread.
        ///</summary>
        ///<param name="start"> The position from which we will start to search </param>
        ///<param name="end"> The target position you want to reach </param>
        ///<param name="gridData"> Informations about grid widht, height and type(hex, ecc..)</param>
        ///<param name="navData"> Navigation data used to determine what's walkable and what's not</param>
        ///<returns>The scheduled pathfinding job and handler, use the handler to block main thread when you need the result 
        /// and the job to retrieve the result(N.B. After retriving result job MUST be disposed.</returns>
        public PathfindingHandler FindPathAsync(Vector2 start, Vector2 end, NavMeshData navData)
        {
            GridData gridData = navData.GetGrid();
            PathfindingHandler pathfindingResult = new PathfindingHandler();
            NativeArray<Triangle2D> data = new NativeArray<Triangle2D>(navData.navigableSurface, Allocator.TempJob);
            if (pathfindingMode == EPathfinding.AStar)
            {
                AStar aStar = new AStar(start, end, gridData, data);
                pathfindingResult.handle = aStar.Schedule();
                pathfindingResult.job = aStar;
            }
            return pathfindingResult;
        }

        ///<summary>
        /// Find the shortest path between start and end using supplied grid and nav data.
        /// This is the Async version, meaning that it will be scheduled to run as a job
        /// on a worker thread.
        ///</summary>
        ///<param name="start"> The position from which we will start to search </param>
        ///<param name="end"> The target position you want to reach </param>
        ///<param name="gridData"> Informations about grid widht, height and type(hex, ecc..)</param>
        ///<param name="navData"> Navigation data used to determine what's walkable and what's not</param>
        ///<returns>The scheduled pathfinding job and handler, use the handler to block main thread when you need the result 
        /// and the job to retrieve the result(N.B. After retriving result job MUST be disposed.</returns>
        public PathfindingHandler FindPathAsync(Vector2 start, Vector2 end)
        {
            GridData gridData = navigationData.GetGrid();
            PathfindingHandler pathfindingResult = new PathfindingHandler();
            NativeArray<Triangle2D> data = new NativeArray<Triangle2D>(navigationData.navigableSurface, Allocator.TempJob);
            if (pathfindingMode == EPathfinding.AStar)
            {
                AStar aStar = new AStar(start, end, gridData, data);
                pathfindingResult.handle = aStar.Schedule();
                pathfindingResult.job = aStar;
            }
            return pathfindingResult;
        }
    }
}

