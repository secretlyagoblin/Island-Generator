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
        /// 

        public static readonly Vector2Int[] Neighbours = new Vector2Int[]
        {
            new Vector2Int(+1,+0),
            new Vector2Int(+0,+1),
            new Vector2Int(-1,+1),
            new Vector2Int(-1,+0),
            new Vector2Int(-1,-1),
            new Vector2Int(+0,-1),
        };

        private static readonly Vector2Int[] _2x2ChildrenOffsets = new Vector2Int[]
        {
            //Center
            new Vector2Int(0,0),
            Neighbours[0],
            Neighbours[1],
            Neighbours[2],
            Neighbours[3],
            Neighbours[4],
            Neighbours[5],
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

            //for (int u = 0; u < _triangleIndexPairs.Length; u += 2)
            //{
            //    var dist = Vector2.Lerp(largeHexPoints[_triangleIndexPairs[u]], largeHexPoints[_triangleIndexPairs[u + 1]],0.5f);
            //
            //   Debug.DrawLine(center, largeHexPoints[_triangleIndexPairs[u]], Color.red, 100f);
            //
            //    Debug.DrawLine(largeHexPoints[_triangleIndexPairs[u]], dist, Color.green,100f);
            //
            //}
            //
            //var col = RNG.NextColor();
            //var zOffset = Vector3.forward * RNG.NextFloat(0, 0.5f);
            //
            //for (int i = 0; i < offsets.Length - 1; i++)
            //{
            //    Vector3 sinnerCoord = this.Center.GetNestedHexLocalCoordinateFromOffset(offsets[i]);
            //    Vector3 sinnerCoord2 = this.Center.GetNestedHexLocalCoordinateFromOffset(offsets[i + 1]);
            //
            //    Debug.DrawLine(sinnerCoord+ zOffset, sinnerCoord2+ zOffset, col, 100f);
            //
            //
            //    //
            //}

            var children = new Hex[offsets.Length];

            for (int i = 0; i < offsets.Length; i++)
            {
                //Debug.Log($"Generating Offset {offsets[i]}");
                var innerCoord = this.Center.GetNestedHexLocalCoordinateFromOffset(offsets[i]);

                var weight = Vector3.zero;
                var index = 0;
                for (int u = 0; u < _triangleIndexPairs.Length; u += 2)
                {
                    


                    weight = CalculateBarycentricWeight(center, largeHexPoints[_triangleIndexPairs[u]], largeHexPoints[_triangleIndexPairs[u + 1]], innerCoord);

                    //Debug.Log($"Testing Baycenter {i} produced weight at [{weight.x.ToString("0.0000")}, {weight.y.ToString("0.0000")}, {weight.z.ToString("0.0000")}] in Triangle {index}");

                    if (weight.x >= 0 && weight.x <= 1 && weight.y >= 0 && weight.y <= 1 && weight.z >= 0 && weight.z <= 1)
                    {
                        break;
                    }

                    index++;
                }

                //Debug.Log($"Baycenter {i} located at [{weight.x.ToString("0.0000")}, {weight.y.ToString("0.0000")}, {weight.z.ToString("0.0000")}] in Triangle {index}");

                children[i] = new Hex(
                    this.Center.GetNestedHexIndexFromOffset(offsets[i]),
                    this.InterpolateHexPayload(weight, index),
                    $"N0:{N0.Index}, N1:{N1.Index} \n N2:{N2.Index},C:{Center.Index}, N3:{N3.Index},\n N4:{N4.Index}, N5:{N5.Index}"
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
            Vector2 v0 = vertB - vertA, v1 = vertC - vertA, v2 = test - vertA;
            float d00 = Vector2.Dot(v0, v0);
            float d01 = Vector2.Dot(v0, v1);
            float d11 = Vector2.Dot(v1, v1);
            float d20 = Vector2.Dot(v2, v0);
            float d21 = Vector2.Dot(v2, v1);
            float denom = d00 * d11 - d01 * d01;
            var v = (d11 * d20 - d01 * d21) / denom;
            var w = (d00 * d21 - d01 * d20) / denom;
            var u = 1.0f - v - w;

            return new Vector3(u, v, w);
        }
    }


}