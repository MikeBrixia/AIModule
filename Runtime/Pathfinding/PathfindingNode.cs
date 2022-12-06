using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Core;

namespace AIModule.AI_Pathfinding
{

    ///<summary>
    /// Pathfinding node used by the A* algorithm to calculate the path.
    ///</summary>
    public struct PathfindingNode : IEquatable<PathfindingNode>
    {
        ///<summary>
        /// The G Cost of this node. This determines how much it cost
        /// to go from the current node to the start node
        ///</summary>
        public int g;

        ///<summary>
        /// The H Cost of this node. This determines how much it cost
        /// to go from the current node to the target node
        ///</summary>
        public int h;

        public int f;
        public int2 coordinates;
        public float2 position;
        public int index;

        public int connection;

        public const int MOVE_STRAIGHT_COST = 10;
        public const int MOVE_DIAGONAL_COST = 14;

        public PathfindingNode(float2 worldPosition, GridData grid)
        {
            float2 coord = worldPosition / grid.cellSize - grid.origin;
            this.coordinates.x = (int)math.floor(coord.x);
            this.coordinates.y = (int)math.floor(coord.y);
            this.position = (this.coordinates * grid.cellSize + grid.origin) + grid.cellSize * 0.5f;
            this.g = 0;
            this.h = 0;
            this.f = 0;
            this.connection = -1;
            this.index = coordinates.x + coordinates.y * grid.size.x;
        }

        public PathfindingNode(int2 coord, GridData grid)
        {
            this.coordinates = coord;
            this.position = (coord * grid.cellSize + grid.origin) + grid.cellSize * 0.5f;
            this.g = 0;
            this.h = 0;
            this.f = 0;
            this.connection = -1;
            this.index = coordinates.x + coordinates.y * grid.size.x;
        }

        public PathfindingNode(int x, int y, GridData grid)
        {
            this.coordinates = new int2(x, y);
            this.position = (this.coordinates * grid.cellSize + grid.origin) + grid.cellSize * 0.5f;
            this.g = 0;
            this.h = 0;
            this.f = 0;
            this.connection = -1;
            this.index = coordinates.x + coordinates.y * grid.size.x;
        }

        public override bool Equals(object obj)
        {
            PathfindingNode other = (PathfindingNode)obj;
            return other.position.Equals(position);
        }

        public bool Equals(PathfindingNode obj)
        {
            return obj.position.Equals(position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(g, h, f, coordinates, position, index, connection);
        }
    }
}



