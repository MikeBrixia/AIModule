
using ComputationGeometry_DOTS;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Core;

namespace AIModule.AI_Pathfinding
{
    ///<summary>
    /// A* Pathfinding algorithm.
    ///</summary>
    [BurstCompile]
    public struct AStar : IJob
    {
        public NativeList<float2> path;

        private PathfindingNode start;
        private PathfindingNode end;
        private NativeList<PathfindingNode> toSearch;
        private NativeList<PathfindingNode> processed;
        private NativeArray<PathfindingNode> neighbours;
        private NativeParallelHashMap<int, PathfindingNode> pathList;
        [ReadOnly] private NativeArray<Triangle2D> navData;
        [ReadOnly] private GridData grid;

        public AStar(float2 start, float2 end, GridData grid, NativeArray<Triangle2D> navData)
        {
            this.grid = grid;
            // Initialize end node
            this.end = new PathfindingNode(end, grid);
            // Initialize start node
            this.start = new PathfindingNode(start, grid);
            this.start.f = int.MaxValue;
            // Initialize search list
            this.toSearch = new NativeList<PathfindingNode>(1, Allocator.TempJob) { this.start };
            // Initialize path list hash map
            this.pathList = new NativeParallelHashMap<int, PathfindingNode>(0, Allocator.TempJob);

            this.path = new NativeList<float2>(Allocator.TempJob);
            this.processed = new NativeList<PathfindingNode>(Allocator.TempJob);
            this.neighbours = new NativeArray<PathfindingNode>(8, Allocator.TempJob);
            this.navData = navData;
        }

        public void Execute()
        {
            ComputePath();
        }

        ///<summary>
        /// Find the path between start point and end point.
        ///</summary>
        public void ComputePath()
        {
            while (toSearch.Length > 0)
            {
                PathfindingNode current = toSearch[0];
                // Choose best node.
                foreach (PathfindingNode node in toSearch)
                    if (node.f < current.f || (node.f == current.f && node.h < current.h))
                        current = node;

                // If we've reached the end goal trace back and build path, 
                // otherwise keep adding processed path
                if (current.Equals(end))
                {
                    // Trace back the path
                    CalculatePath(current);
                    break;
                }

                for (int i = 0; i < toSearch.Length; i++)
                    if (toSearch[i].Equals(current))
                    {
                        toSearch.RemoveAtSwapBack(i);
                        break;
                    }
                processed.Add(current);
                pathList.TryAdd(current.index, current);
                neighbours = GetNeighbours(current);
                for (int i = 0; i < neighbours.Length; i++)
                {
                    PathfindingNode neighbour = neighbours[i];
                    if (neighbour.index != -1)
                    {
                        bool inSearch = Contains(toSearch, neighbour);
                        // Check if this neighbour node has already been processed, if yes discard it, 
                        // otherwise keep computing it.
                        if (!Contains(processed, neighbour) & IsNavigablePoint(navData, neighbour.position)
                           & !inSearch)
                        {
                            neighbour.connection = current.index;
                            neighbour.g = current.g + GetDistance(current, neighbour);
                            neighbour.h = GetDistance(current, end);
                            neighbour.f = neighbour.g + neighbour.h;
                            toSearch.Add(neighbour);
                        }
                    }
                }
            }
        }

        // Trace back calculated path and build pathfinding result.
        public void CalculatePath(PathfindingNode start)
        {
            PathfindingNode node = start;
            path.Add(node.position);
            while (node.connection != -1)
            {
                PathfindingNode cameFrom = pathList[node.connection];
                path.Add(cameFrom.position);
                node = cameFrom;
            }
        }
       
        private NativeArray<PathfindingNode> GetNeighbours(PathfindingNode node)
        {
            int x = node.coordinates.x;
            int y = node.coordinates.y;
            neighbours[0] = SetNode(x + 1, y);
            neighbours[1] = SetNode(x - 1, y);
            neighbours[2] = SetNode(x, y + 1);
            neighbours[3] = SetNode(x, y - 1);
            neighbours[4] = SetNode(x + 1, y + 1);
            neighbours[5] = SetNode(x - 1, y + 1);
            neighbours[6] = SetNode(x + 1, y - 1);
            neighbours[7] = SetNode(x - 1, y - 1);
            return neighbours;
        }

        private PathfindingNode SetNode(int x, int y)
        {
            PathfindingNode node;
            if ((x >= 0 && y >= 0) && (x <= grid.size.x && y <= grid.size.y))
                node = new PathfindingNode(x, y, grid);
            else
            {
                node = new PathfindingNode();
                node.index = -1;
            }
            return node;
        }

        private int GetDistance(PathfindingNode start, PathfindingNode target)
        {
            int xDistance = math.abs(start.coordinates.x - target.coordinates.x);
            int yDistance = math.abs(start.coordinates.y - target.coordinates.y);
            int remainingCost = math.abs(xDistance - yDistance);
            return PathfindingNode.MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + PathfindingNode.MOVE_STRAIGHT_COST * remainingCost;
        }
        
        private bool IsNavigablePoint(NativeArray<Triangle2D> navigationInfo, float2 point)
        {
            foreach(Triangle2D triangle in navigationInfo)
                if(GeometryLibrary.ContainsPoint(triangle, point))
                    return true;
            return false;
        }

        public void Dispose()
        {
            processed.Dispose();
            toSearch.Dispose();
            neighbours.Dispose();
            pathList.Dispose();
            path.Dispose();
            navData.Dispose();
        }

        // For some reason NativeList.Contains() is not implemented, that's a workaround which
        // is also compatible with burst compiler.
        private bool Contains(NativeList<PathfindingNode> array, PathfindingNode element)
        {
            foreach (PathfindingNode node in array)
                if (node.Equals(element))
                    return true;
            return false;
        }
    }
}

