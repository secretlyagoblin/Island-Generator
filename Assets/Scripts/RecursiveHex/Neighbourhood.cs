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

        #region Static Vertex Lists

        /// <summary>
        /// The neighbourhood around a hex when Y index is even - currently hardcoded, and shouldn't be changed
        /// </summary>
        private static readonly Vector2Int[] NeighboursEven = new Vector2Int[]
        {
            new Vector2Int(+1,+0),
            new Vector2Int(+0,+1),
            new Vector2Int(-1,+1),
            new Vector2Int(-1,+0),
            new Vector2Int(-1,-1),
            new Vector2Int(+0,-1),
        };

        /// <summary>
        /// The neighbourhood around a hex when Y index is odd - currently hardcoded, and shouldn't be changed
        /// </summary>
        private static readonly Vector2Int[] NeighboursOdd = new Vector2Int[]
        {
            new Vector2Int(+1,+0),
            new Vector2Int(+1,+1),
            new Vector2Int(-0,+1),
            new Vector2Int(-1,+0),
            new Vector2Int(-0,-1),
            new Vector2Int(+1,-1),
        };

        /// <summary>
        /// The children of a hexagon in a rough 3x3 grid - currently hardcoded, and shouldn't be changed
        /// </summary>
        private static readonly Vector2Int[] _3x3ChildrenOffsets = new Vector2Int[]
        {
            //Center
            new Vector2Int(0,0),
            NeighboursEven[0],
            NeighboursEven[1],
            NeighboursEven[2],
            NeighboursEven[3],
            NeighboursEven[4],
            NeighboursEven[5],
            new Vector2Int(-2,+1),
            new Vector2Int(-2,-1),
        };

        /// <summary>
        /// The offsets called when manually setting up a grid. Used for testing.
        /// </summary>
        private static readonly Vector2Int[] _DebugOffsets = new Vector2Int[]
        {
            new Vector2Int(0,0),
            NeighboursEven[0],
            NeighboursEven[1],
            NeighboursEven[2],
            NeighboursEven[3],
            NeighboursEven[4],
            NeighboursEven[5],
        };

        #endregion

        /// <summary>
        /// Get the correct neighbourhood for a given hexagon index
        /// </summary>
        /// <param name="index">XY index to test</param>
        /// <returns></returns>
        public static Vector2Int[] GetNeighbours(Vector2Int index)
        {
            return index.y % 2 == 0 ? NeighboursEven : NeighboursOdd;
        }

        /// <summary>
        /// Subdivides the grid by one level
        /// </summary>
        /// <returns></returns>
        public Hex[] Subdivide()
        {
            return Subdivide(_3x3ChildrenOffsets);
        }

        /// <summary>
        /// Subdivides the grid by one level using the debug grid array. Should only really be used at the top of a stack.
        /// </summary>
        /// <returns></returns>
        public Hex[] DebugSubdivide()
        {
            return Subdivide(_DebugOffsets);
        }

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
            if (this.IsBorder)
            {
                if (Hex.IsInvalid(this.Center))
                {                    
                    return new Hex[0];
                }
            }


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
                var weight = Vector3.zero;
                var index = 0;
                var foundChild = false;

                for (int u = 0; u < _triangleIndexPairs.Length; u += 2)
                {
                    weight = CalculateBarycentricWeight(center, largeHexPoints[_triangleIndexPairs[u]], largeHexPoints[_triangleIndexPairs[u + 1]], innerCoord);
                    var testX = weight.x;
                    var testY = weight.y;
                    var testZ = weight.z;

                    if (testX >= 0 && testX <= 1 && testY >= 0 && testY <= 1 && testZ >= 0 && testZ <= 1)
                    {
                        foundChild = true;
                        break;
                    }
                    index++;
                }

                if (!foundChild)
                {
                    Debug.LogError($"No containing barycenter detected at {this.Center.Index} - inner hex not contained by outer hex. Using weight {weight}.");
                }

                var isBorder = this.InterpolateIsBorder(weight, index);

                var payload = isBorder ? this.Center.Payload : this.InterpolateHexPayload(weight, index);

                children[i] = new Hex(
                    this.Center.GetNestedHexIndexFromOffset(offsets[i]),
                    payload,
                    isBorder,
                    ""//$"{Center.Index}\n{N0.Index}\n{N1.Index}\n{N2.Index}\n{N3.Index}\n{N4.Index}\n{N5.Index}"
                    );
            }

            var childSubset = children;//ResolveEdgeCases(children);

            return childSubset;
        }

        /// <summary>
        /// Goes through the neighbours in a hardcoded way and lerps the correct data
        /// </summary>
        /// <param name="weights"></param>
        /// <param name="triangleIndex"></param>
        /// <returns></returns>
        private bool InterpolateIsBorder(Vector3 weights, int triangleIndex)
        {
            switch (triangleIndex)
            {
                default:
                case 0: return Utils.Blerp<bool>(Center.IsBorder, N0.IsBorder, N1.IsBorder, weights);
                case 1: return Utils.Blerp<bool>(Center.IsBorder, N1.IsBorder, N2.IsBorder, weights);
                case 2: return Utils.Blerp<bool>(Center.IsBorder, N2.IsBorder, N3.IsBorder, weights);
                case 3: return Utils.Blerp<bool>(Center.IsBorder, N3.IsBorder, N4.IsBorder, weights);
                case 4: return Utils.Blerp<bool>(Center.IsBorder, N4.IsBorder, N5.IsBorder, weights);
                case 5: return Utils.Blerp<bool>(Center.IsBorder, N5.IsBorder, N0.IsBorder, weights);
            }
        }

        /// <summary>
        /// Goes through the neighbours in a hardcoded way and lerps the correct data
        /// </summary>
        /// <param name="weights"></param>
        /// <param name="triangleIndex"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculates the barycentric weight of the inner triangles.
        /// </summary>
        /// <param name="vertA"></param>
        /// <param name="vertB"></param>
        /// <param name="vertC"></param>
        /// <param name="test"></param>
        /// <returns></returns>
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

        //I didn't need to do this :(
        private Hex[] ResolveEdgeCases(Hex[] ch)
        {
            if (Hex.IsInvalid(this.Center))
            {
                return new Hex[0];
            }

            var a = !Hex.IsInvalid(this.N0);
            var b = !Hex.IsInvalid(this.N1);
            var c = !Hex.IsInvalid(this.N2);
            var d = !Hex.IsInvalid(this.N3);
            var e = !Hex.IsInvalid(this.N4);
            var f = !Hex.IsInvalid(this.N5);

            

            if(a && b && c && d && e && f)// 1-1-1-1-1-1
            {
                return ch;
            }
            else if (a && b && c && d && e && !f) // 1-1-1-1-1-0
            {   
                return new Hex[]
                {
                    ch[1],ch[2],ch[3],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && b && c && d && !e && !f) // 1-1-1-1-0-0
            {
                return new Hex[]
                {
                    ch[1],ch[2],ch[3],ch[4],ch[6],ch[7],ch[8]
                };
            }
            else if (a && b && c && !d && !e && !f) // 1-1-1-0-0-0
            {
                return new Hex[]
                {
                    ch[1],ch[2],ch[3],ch[6],ch[7]
                };
            }
            else if (a && b && !c && !d && !e && !f) // 1-1-0-0-0-0
            {
                return new Hex[]
                {
                    ch[1],ch[2],ch[3],ch[6]
                };
            }
            else if (a && !b && !c && !d && !e && !f) // 1-0-0-0-0-0
            {
                return new Hex[]
                {
                    ch[1],ch[2],ch[6]
                };
            }
            else if (!a && !b && !c && !d && !e && !f) // 0-0-0-0-0-0
            {
                return new Hex[0];
            }
            //Rest are sorted by order of occurence
            else if (a && !b && !c && d && e && f) // 1-0-0-1-1-1
            {
                return new Hex[]
                {
                    ch[1],ch[2],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && b && c && !d && e && f) // 1-1-1-0-1-1
            {
                return new Hex[]
                {
                    ch[1],ch[2],ch[3],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (!a && !b && c && d && e && !f) // 0-0-1-1-1-0
            {
                return new Hex[]
                {
                    ch[3],ch[4],ch[5],ch[7],ch[8]
                };
            }
            else if (!a && b && c && d && e && f) // 0-1-1-1-1-1
            {
                return new Hex[]
                {
                    ch[2],ch[3],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && b && c && d && !e && f) // 1-1-1-1-0-1
            {
                return new Hex[]
                {
                    ch[1],ch[2],ch[3],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && b && !c && d && e && f) // 1-1-0-1-1-1
            {
                return new Hex[]
                {
                    ch[1],ch[2],ch[3],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (!a && !b && !c && d && e && f) // 0-0-0-1-1-1
            {
                return new Hex[]
                {
                    ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && !b && c && d && e && f) // 1-0-1-1-1-1
            {
                return new Hex[]
                {
                    ch[1],ch[2],ch[3],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && !b && !c && !d && !e && f) // 1-0-0-0-0-1
            {
                return new Hex[]
                {
                    ch[1],ch[2],ch[5],ch[6]
                };
            }
            else if (!a && b && c && d && !e && !f) // 0-1-1-1-0-0
            {
                return new Hex[]
                {
                   ch[2],ch[3],ch[4],ch[7],ch[8]
                };
            }
            else if (!a && !b && c && d && e && f) // 0-0-1-1-1-1
            {
                return new Hex[]
                {
                   ch[3],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && !b && !c && !d && e && f) // 1-0-0-0-1-1
            {
                return new Hex[]
                {
                   ch[1],ch[2],ch[5],ch[6],ch[8]
                };
            }
            else if (!a && b && c && d && e && !f) // 0-1-1-1-1-0
            {
                return new Hex[]
                {
                   ch[2],ch[3],ch[4],ch[5],ch[7],ch[8]
                };
            }
            else if (a && !b && c && d && e && !f) // 1-0-1-1-1-0
            {
                return new Hex[]
                {
                   ch[1],ch[2],ch[3],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && b && !c && d && e && !f) // 1-1-0-1-1-0
            {
                return new Hex[]
                {
                   ch[1],ch[2],ch[3],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && !b && c && d && !e && f) // 1-0-1-1-0-1
            {
                return new Hex[]
                {
                   ch[1],ch[2],ch[3],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && b && !c && !d && !e && f) // 1-1-0-0-0-1
            {
                return new Hex[]
                {
                   ch[1],ch[2],ch[3],ch[5],ch[6]
                };
            }
            else if (a && !b && !c && d && !e && f) // 1-0-0-1-0-1
            {
                return new Hex[]
                {
                   ch[1],ch[2],ch[4],ch[5],ch[6],ch[7],ch[8]
                };
            }
            else if (a && b && !c && d && !e && !f) // 1-1-0-1-0-0
            {
                return new Hex[]
                {
                   ch[1],ch[2],ch[3],ch[4],ch[6],ch[7],ch[8]
                };
            }


            Debug.LogError($"Unhandled case {(a?1:0)}-{(b ? 1 : 0)}-{(c ? 1 : 0)}-{(d ? 1 : 0)}-{(e ? 1 : 0)}-{(f ? 1 : 0)}");

            return ch;

            
        }
    }


}