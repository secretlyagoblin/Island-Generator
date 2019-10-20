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
        new Vector2Int(+0,+1),
        new Vector2Int(-1,+1),
        new Vector2Int(-1,+0),
        new Vector2Int(-1,-1),
        new Vector2Int(+0,-1),
        };

        /// <summary>
        /// The offsets called when manually setting up a grid.
        /// </summary>
        private static readonly Vector2Int[] _DebugOffsets = new Vector2Int[]
    {
        //Center
        new Vector2Int(0,0),

        new Vector2Int(+1,+0),
        new Vector2Int(+0,+1),
        new Vector2Int(-1,+1),
        new Vector2Int(-1,+0),
        new Vector2Int(-1,-1),
        new Vector2Int(+0,-1),
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

        private static readonly int[] _triangleIndexPairs = new int[]
{
                0,1,
                1,2,
                2,3,
                3,4,
                4,5,
                5,0
};

        private Hex[] Subdivide(Vector2Int[] offsets)
        {
            var center = this.Center.GetNoiseOffset();

            var largeHexPoints = new Vector2[]
            {
                
                Hex.StaticFlatHexPoints[0] +  this.N0.GetNoiseOffset(),
                Hex.StaticFlatHexPoints[1] +  this.N1.GetNoiseOffset(),
                Hex.StaticFlatHexPoints[2] +  this.N2.GetNoiseOffset(),
                Hex.StaticFlatHexPoints[3] +  this.N3.GetNoiseOffset(),
                Hex.StaticFlatHexPoints[4] +  this.N4.GetNoiseOffset(),
                Hex.StaticFlatHexPoints[5] +  this.N5.GetNoiseOffset()
            };

            var children = new Hex[offsets.Length];


            for (int i = 0; i < offsets.Length; i++)
            {
                var innerCoord = this.Center.GetNestedHexLocalCoordinateFromOffset(offsets[i]);

                Vector3 weight = Vector3.zero;
                int index = 0;
                for (int u = 0; u < _triangleIndexPairs.Length; u += 2)
                {
                    weight = CalculateBarycentricWeight(center, largeHexPoints[_triangleIndexPairs[u]], largeHexPoints[_triangleIndexPairs[u + 1]], innerCoord);

                    if (weight.x >= 0 && weight.x <= 1 && weight.y >= 0 && weight.y <= 1 && weight.z >= 0 && weight.z <= 1)
                    {
                        break;
                    }

                    index++;
                }

                children[i] = new Hex(
                    this.Center.GetNestedHexIndexFromOffset(offsets[i]),
                    this.InterpolateHexPayload(weight, index)
                    );
                
            }

            return children;
        }

        //private void BuildHexagon

            private HexPayload InterpolateHexPayload(Vector3 weights, int triangleIndex)
        {
            switch (triangleIndex)
            {
                default:
                case 0: return HexPayload.Blerp(Center, N0, N1, weights);
                case 1: return HexPayload.Blerp(Center, N1, N2, weights);
                case 2: return HexPayload.Blerp(Center, N2, N3, weights);
                case 3: return HexPayload.Blerp(Center, N3, N4, weights);
                case 4: return HexPayload.Blerp(Center, N4, N5, weights);
                case 5: return HexPayload.Blerp(Center, N5, N0, weights);
            }
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