using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecursiveHex
{

    public struct Neighbourhood
    {
        public Hex Center;
        public Hex N0;
        public Hex N1;
        public Hex N2;
        public Hex N3;
        public Hex N4;
        public Hex N5;

        public bool IsBorder;

        /// <summary>
        /// The neighbourhood around a 3x3 grid - currently not in use
        /// </summary>
        private static readonly Vector2Int[] _3x3ChildrenOffsets = new Vector2Int[]
        {
        //Center
        new Vector2Int(0,0),

        //One
        new Vector2Int(+0,+1),
        new Vector2Int(+1,+1),
        new Vector2Int(+0,-1),
        new Vector2Int(+1,-1),
        new Vector2Int(-1,+0),
        new Vector2Int(+1,+0),
        //Two
        new Vector2Int(-1,-2),
        new Vector2Int(+0,-2),
        new Vector2Int(+1,-2),

        new Vector2Int(-1,+2),
        new Vector2Int(+0,+2),
        new Vector2Int(+1,+2),

        new Vector2Int(+2,+1),
        new Vector2Int(+2,+0),
        new Vector2Int(+2,-1),

        new Vector2Int(-1,+1),
        new Vector2Int(-2,+0),
        new Vector2Int(-1,-1),
            //new Vector2Int(+1,-2),
            //new Vector2Int(+2,0)

        };

        /// <summary>
        /// The neighbourhood around a 2x2 grid - currently hardcoded, and shouldn't be changed
        /// </summary>
        private static readonly Vector2Int[] _2x2ChildrenOffsets = new Vector2Int[]
        {
        //Center
        new Vector2Int(0,0),

        new Vector2Int(+1,+0),
        new Vector2Int(+0,-1),
        new Vector2Int(-1,-1),
        new Vector2Int(-1,+0),
        new Vector2Int(-1,+1),
        new Vector2Int(+0,+1)
        };

        /// <summary>
        /// The offsets called when manually setting up a grid.
        /// </summary>
        private static readonly Vector2Int[] _DebugOffsets = new Vector2Int[]
    {
        //Center
        new Vector2Int(0,0),

        new Vector2Int(+1,+0),
        new Vector2Int(+0,-1),
        new Vector2Int(-1,-1),
        new Vector2Int(-1,+0),
        new Vector2Int(-1,+1),
        new Vector2Int(+0,+1)
    };



        /// <summary>
        /// Subdivides the grid by one level
        /// </summary>
        /// <returns></returns>
        public Hex[] Subdivide()
        {
            return Subdivide(_2x2ChildrenOffsets);
        }

        /// <summary>
        /// Subdivides the grid by one level using the debug grid array. Should only really be used at the top of a stack.
        /// </summary>
        /// <returns></returns>
        public Hex[] DebugSubdivide()
        {
            return Subdivide(_DebugOffsets);
        }

        /// <summary>
        /// Secondary grid rotation when we go down one layer
        /// </summary>
        private static float _innerHexRotation = Mathf.Tan(1f / 2.5f);

        private Hex[] Subdivide(Vector2Int[] offsets)
        {
            var children = new Hex[offsets.Length];

            for (int i = 0; i < offsets.Length; i++)
            {
                children[i] = CalculateNewOffset(offsets[i]);
            }

            return children;
        }



        /// <summary>
        /// Mapping a XY grid of hexes by a level requires a rhombus - this is the X translation of the bottom of this rhombus
        /// </summary>
        private static Vector2 _rhombusX = new Vector2(2.5f, 1);

        /// <summary>
        /// Mapping a XY grid of hexes by a level requires a rhombus - this is the Y translation of the bottom of this rhombus
        /// </summary>
        private static Vector2 _rhombusY = new Vector2(0.5f, 3);

        /// <summary>
        /// Rhombus offsetting obvious results in a rhombus. 
        /// We have to skew in the other direction to get a nice result - this is the magic number I chose for that.
        /// </summary>
        private const float MAGIC_GRID_SKEW_RATIO = 0.59f;

        /// <summary>
        /// The major offsetting function. Set up for a 2x2 grid only, with some magic numbers for keeping the grid sqaure.
        /// </summary>
        /// <param name="localOffset"></param>
        /// <returns></returns>
        private Hex CalculateNewOffset(Vector2Int localOffset)
        {
            var inUseIndex = this.Center.Index; //we'll be modifying this, so we make a local copy

            var xOffset = inUseIndex.y * MAGIC_GRID_SKEW_RATIO;

            inUseIndex.x -= Mathf.FloorToInt(xOffset);

            var shiftedIndex = (inUseIndex.x * _rhombusX) +
                (inUseIndex.y * _rhombusY);

            var evenX = inUseIndex.x % 2 == 0;
            var evenY = inUseIndex.y % 2 == 0;

            var offsetX = Mathf.FloorToInt(shiftedIndex.x);
            var offsetY = Mathf.FloorToInt(shiftedIndex.y);

            //Different grids require different edge cases - here's them.
            if (evenX && evenY)
            {

            }
            else if (evenX && !evenY)
            {
                if (localOffset.y % 2 != 0)
                {
                    offsetX++;
                }
            }
            else if (!evenX && evenY)
            {
                if (localOffset.y % 2 != 0)
                {
                    offsetX++;
                }
            }
            else if (!evenX && !evenY)
            {

            }

            //final offset
            var x = offsetX + localOffset.x;
            var y = offsetY + localOffset.y;

            return new Hex(x, y);
        }

        private static Vector3 CalculateBarycentricWeight(Vector2 vertA, Vector2 vertB, Vector2 vertC, Vector2 test)
        {
             // calculate vectors from point f to vertices p1, p2 and p3:
             var f1 = vertA - test;
             var f2 = vertB - test;
             var f3 = vertC - test;
             // calculate the areas and factors (order of parameters doesn't matter):
             var a = Vector3.Cross(vertA - vertB, vertA - vertC).magnitude; // main triangle area a
             var a1 = Vector3.Cross(f2, f3).magnitude / a; // p1's triangle area / a
             var a2 = Vector3.Cross(f3, f1).magnitude / a; // p2's triangle area / a 
             var a3 = Vector3.Cross(f1, f2).magnitude / a; // p3's triangle area / a

             return new Vector3(a1, a2, a3);            
        }
    }


}