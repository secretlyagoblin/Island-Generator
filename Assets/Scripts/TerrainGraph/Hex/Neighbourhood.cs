﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WanderingRoad.Core;
using WanderingRoad.Core.Random;

namespace WanderingRoad.Procgen.RecursiveHex
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
        /// The neighbourhood around a hex - currently hardcoded, and shouldn't be changed
        /// </summary>
        public static readonly HexIndex[] StaticHexNeighbours = new HexIndex[]
        {
            new HexIndex(+1,-1,0),
            new HexIndex(+1,0,-1),
            new HexIndex(0,+1,-1),
            new HexIndex(-1,+1,0),
            new HexIndex(-1,0,+1),
            new HexIndex(0,-1,+1),
        };

        private static Dictionary<int, Vector3Int[]> _cachedRosettes = new Dictionary<int, Vector3Int[]>();

        private static Vector3Int[] GenerateRosette(int radius)
        {
            if (_cachedRosettes.ContainsKey(radius))
                return _cachedRosettes[radius];

            var cells = new List<Vector3Int>();

            for (int q = -radius; q <= radius; q++)
            {
                int r1 = Mathf.Max(-radius, -q - radius);
                int r2 = Mathf.Min(radius, -q + radius);
                for (int r = r1; r <= r2; r++)
                {
                    cells.Add(new Vector3Int(q, r, -q - r));
                }
            }

            var result = cells.ToArray();

            _cachedRosettes.Add(radius, result);

            return result;
        }


        #endregion

        private static readonly int[] _triangleIndexPairs = new int[]
        {
            0,1,
            1,2,
            2,3,
            3,4,
            4,5,
            5,0
        };

        /// <summary>
        /// Subdivides the grid by one level
        /// </summary>
        /// <returns></returns>
        public Hex[] Subdivide(int scale, Func<HexPayload, int> connectionIndentifier)
        {
            if (this.IsBorder && Hex.IsInvalid(this.Center))
                return new Hex[0];

            var nestedCenter = this.Center.Index.NestMultiply(scale);
            var floatingNestedCenter = nestedCenter.Position2d + this.Center.Index.Position2d.AddNoiseOffset(scale - 1);

            var largeHexPoints = new Vector2[]
            {
                 this.N0.Index.NestMultiply(scale).Position2d + this.N0.Index.Position2d.AddNoiseOffset(scale-1),
                 this.N1.Index.NestMultiply(scale).Position2d + this.N1.Index.Position2d.AddNoiseOffset(scale-1),
                 this.N2.Index.NestMultiply(scale).Position2d + this.N2.Index.Position2d.AddNoiseOffset(scale-1),
                 this.N3.Index.NestMultiply(scale).Position2d + this.N3.Index.Position2d.AddNoiseOffset(scale-1),
                 this.N4.Index.NestMultiply(scale).Position2d + this.N4.Index.Position2d.AddNoiseOffset(scale-1),
                 this.N5.Index.NestMultiply(scale).Position2d + this.N5.Index.Position2d.AddNoiseOffset(scale-1)
            };

            var neighbourCodes = new bool[]
            {
                 Center.Payload.Connections.ContainsCode(connectionIndentifier(this.N1.Payload)),
                 Center.Payload.Connections.ContainsCode(connectionIndentifier(this.N2.Payload)),
                 Center.Payload.Connections.ContainsCode(connectionIndentifier(this.N3.Payload)),
                 Center.Payload.Connections.ContainsCode(connectionIndentifier(this.N4.Payload)),
                 Center.Payload.Connections.ContainsCode(connectionIndentifier(this.N5.Payload)),
                 Center.Payload.Connections.ContainsCode(connectionIndentifier(this.N0.Payload))
            };

            var neighbourLinks = new bool[]
            {
                 N0.Payload.Connections.ContainsCode(connectionIndentifier(this.N1.Payload)),
                 N1.Payload.Connections.ContainsCode(connectionIndentifier(this.N2.Payload)),
                 N2.Payload.Connections.ContainsCode(connectionIndentifier(this.N3.Payload)),
                 N3.Payload.Connections.ContainsCode(connectionIndentifier(this.N4.Payload)),
                 N4.Payload.Connections.ContainsCode(connectionIndentifier(this.N5.Payload)),
                 N5.Payload.Connections.ContainsCode(connectionIndentifier(this.N0.Payload))
            };

            var debugHexPoints = new List<Vector2>(largeHexPoints);
            debugHexPoints.Add(debugHexPoints[0]);

            var halfSegments = new LineEdge[6];

            if (!this.IsBorder)
            {
                var c2d = floatingNestedCenter;

                var triangleAverages = new HexIndex[7];

                for (int i = 0; i < 6; i++)
                {
                    var a2d = debugHexPoints[i];
                    var b2d = debugHexPoints[i + 1];

                    var average = (a2d + floatingNestedCenter + b2d) / 3;
                    triangleAverages[i] = HexIndex.HexIndexFromPosition(average);
                }

                triangleAverages[6] = triangleAverages[0];

                for (int i = 0; i < 6; i++)
                {
                    var centerAIndex = HexIndex.HexIndexFromPosition(
                        Vector2.Lerp(debugHexPoints[i + 1], c2d, 0.5f));

                    halfSegments[i] = new LineEdge
                    {
                        EdgeA = new List<HexIndex>(HexIndex.DrawLine(centerAIndex, triangleAverages[i])),
                        EdgeB = new List<HexIndex>(HexIndex.DrawLine(centerAIndex, triangleAverages[i + 1])),

                    };
                }

                var colour = RNG.NextColorBright();
                //var colourOther = RNG.NextColorBright();

                for (int i = 0; i < 6; i++)
                {
                    if (neighbourCodes[i])
                    {
                        var last = i - 1 < 0 ? 5 : i - 1;
                        var next = i + 1 > 5 ? 0 : i +1;

                        halfSegments[i].DebugDraw(colour);
                        halfSegments[i].ApplyEdgeConditions(
                            neighbourCodes[last],
                            neighbourCodes[i],
                            neighbourCodes[next],
                            neighbourLinks[i],
                            neighbourLinks[next]
                            );
                    }
                }

            }



            bool FoundChild(HexIndex testCenter, out Vector3 testWeight, out int testIndex)
            {
                testIndex = 0;

                for (int u = 0; u < _triangleIndexPairs.Length; u += 2)
                {
                    testWeight = CalculateBarycentricWeight(
                        floatingNestedCenter,
                        largeHexPoints[_triangleIndexPairs[u]],
                        largeHexPoints[_triangleIndexPairs[u + 1]],
                        testCenter.Position2d);

                    var testX = testWeight.x;
                    var testY = testWeight.y;
                    var testZ = testWeight.z;

                    if (testX >= 0 && testX <= 1 && testY >= 0 && testY <= 1 && testZ >= 0 && testZ <= 1)
                    {
                        if (!(testX >= testY && testX >= testZ))
                            break;
                        return true;
                    }
                    testIndex++;
                }

                testWeight = Vector3.zero;

                return false;
            }

            FoundChild(nestedCenter, out var weight, out var index);

            var children = new List<Hex>
            {
                FinaliseHex(nestedCenter,weight,index,BorderContains(halfSegments,nestedCenter))
            };

            var ringHasValidHexIndices = true;
            var radius = 1;

            while (ringHasValidHexIndices)
            {
                var foundChild = false;
                var results = nestedCenter.GenerateRing(radius);

                for (int i = 0; i < results.Length; i++)
                {
                    var testCenter = results[i];

                    if (FoundChild(testCenter, out weight, out index))
                    {
                        foundChild = true;
                        children.Add(
                            FinaliseHex(testCenter, weight, index, BorderContains(halfSegments, testCenter))
                        );
                    }
                }

                if (!foundChild && children.Count > 1)
                {
                    ringHasValidHexIndices = false;
                }

                radius++;
            }

            return children.ToArray();
        }

        Hex FinaliseHex(HexIndex hex, Vector3 weight, int index, bool isEdge)
        {
            var isBorder = InterpolateIsBorder(weight, index);

            var payload = isBorder ? this.Center.Payload : this.InterpolateHexPayload(weight, index);

            //Start here - CodeConnections status = check neighbourhood and give results

            payload.ConnectionStatus = isEdge ? Topology.Connection.NotPresent : Topology.Connection.Present;

            //Debug.DrawLine(nestedCenter.Position3d, indexChildren[i].Position3d, Color.blue, 100f);


            return new Hex(
                hex,
                payload,
                isBorder,
                $"Owner = {Center.Index.Index3d}"//$"{Center.Index}\n{N0.Index}\n{N1.Index}\n{N2.Index}\n{N3.Index}\n{N4.Index}\n{N5.Index}"
                );
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
                case 0: return InterpolationHelpers.Blerp<bool>(Center.IsBorder, N0.IsBorder, N1.IsBorder, weights);
                case 1: return InterpolationHelpers.Blerp<bool>(Center.IsBorder, N1.IsBorder, N2.IsBorder, weights);
                case 2: return InterpolationHelpers.Blerp<bool>(Center.IsBorder, N2.IsBorder, N3.IsBorder, weights);
                case 3: return InterpolationHelpers.Blerp<bool>(Center.IsBorder, N3.IsBorder, N4.IsBorder, weights);
                case 4: return InterpolationHelpers.Blerp<bool>(Center.IsBorder, N4.IsBorder, N5.IsBorder, weights);
                case 5: return InterpolationHelpers.Blerp<bool>(Center.IsBorder, N5.IsBorder, N0.IsBorder, weights);
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

        private static bool BorderContains(LineEdge[] set, HexIndex testIndex)
        {
            for (int i = 0; i < set.Length; i++)
            {
                if (set[i] == null)
                    continue;

                for (int u = 0; u < set[i].EdgeA.Count; u++)
                {
                    if (set[i].EdgeA[u] == testIndex)
                        return true;
                }

                for (int u = 0; u < set[i].EdgeB.Count; u++)
                {
                    if (set[i].EdgeB[u] == testIndex)
                        return true;
                }
            }

            return false;
        }

        private class LineEdge
        {
            private static float _jitter = 0f;

            public List<HexIndex> EdgeA { get; set; }
            public List<HexIndex> EdgeB { get; set; }

            public void DebugDraw(Color color)
            {
                Debug.DrawLine(EdgeA.Last().Position3d + (Vector3.up * 3) + RNG.NextVector3(-_jitter, _jitter), EdgeB.Last().Position3d + (Vector3.up * 3) + RNG.NextVector3(-_jitter, _jitter), color, 100f);

            }

            public void RemoveCenterNode()
            {
                EdgeA.RemoveAt(0);
                EdgeB.RemoveAt(0);
            }

            public void ApplyEdgeConditions(bool left, bool thisEdge, bool right, bool leftLink, bool rightLink)
            {
                if (thisEdge)
                {
                    RemoveCenterNode();
                }

                if(left && leftLink && thisEdge)
                {
                    EdgeA.Clear();
                }

                if(right && rightLink && thisEdge)
                {
                    EdgeB.Clear();
                }
            }
        }
    }
}