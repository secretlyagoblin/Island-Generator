using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RecursiveHex
{
    public class HexMap
    {
        Dictionary<Vector2Int, Hex[,]> _chunks = new Dictionary<Vector2Int, Hex[,]>();
        Dictionary<int, List<Vector2Int>> _chunksThatContainKey = new Dictionary<int, List<Vector2Int>>();

        Func<Hex, int> _lookup;

        Vector2Int _size = new Vector2Int(20,20);

        public HexMap(Func<Hex, int> lookup)
        {
            _lookup = lookup;

            AddHex(new Hex(Vector2Int.zero, new HexPayload(),false));
        }

        private HexMap(HexMap hexMap)
        {
            _lookup = hexMap._lookup;         

            hexMap.ForEachHex(hex =>
            {
                var neighbourhood = hexMap.GetNeighbourhood(hex.Index);
                var children = neighbourhood.Subdivide();

                for (int c = 0; c < children.Length; c++)
                {
                    AddHex(children[c]);
                }
            });
        }

        private Vector2Int ChunkFromIndex(Vector2Int hexIndex)
        {
            var offsetX = hexIndex.x + (_size.x * 0.5f);
            var offsetY = hexIndex.y + (_size.y * 0.5f);

            var fractionX = offsetX / _size.x;
            var fractionY = offsetY / _size.y;

            var cellX = Mathf.FloorToInt(fractionX);
            var cellY = Mathf.FloorToInt(fractionY);

            return new Vector2Int(cellX, cellY);
        }

        private Vector2Int GlobalToLocalIndex(Vector2Int hexIndex)
        {
            var chunk = ChunkFromIndex(hexIndex);

            var offsetX = hexIndex.x + (_size.x * 0.5f);
            var offsetY = hexIndex.y + (_size.y * 0.5f);

            var newX = offsetX - (_size.x* chunk.x);
            var newY = offsetY - (_size.y * chunk.y);

            return new Vector2Int(Mathf.RoundToInt(newX), Mathf.RoundToInt(newY));
        }

        public void AddHex(Hex hex)
        {
            var chunk = ChunkFromIndex(hex.Index);
            var localIndex = GlobalToLocalIndex(hex.Index);

            if (_chunks.ContainsKey(chunk))
            {
                _chunks[chunk][localIndex.x, localIndex.y] = hex;
            }
            else
            {
                var hexArray = new Hex[_size.x, _size.y];
                hexArray[localIndex.x, localIndex.y] = hex;
                _chunks.Add(chunk, hexArray);
            }

            var key = _lookup(hex);

            if (_chunksThatContainKey.ContainsKey(key))
            {
                _chunksThatContainKey[key].Add(chunk);
            }
            else
            {
                _chunksThatContainKey.Add(key, new List<Vector2Int>() { chunk });
            }
        }

        public Hex GetHex(Vector2Int index)
        {
            var chunk = ChunkFromIndex(index);
            var localIndex = GlobalToLocalIndex(index);

            try
            {
                return _chunks[chunk][localIndex.x, localIndex.y];
            }
            catch(Exception ex)
            {
                throw new Exception("Failure", ex);
            }            
        }

        public HexList GetHexesOfKey(int key)
        {
            var chunkIds = _chunksThatContainKey[key];
            var outputHexes = new List<Hex>();

            for (int i = 0; i < chunkIds.Count; i++)
            {
                var chunkId = chunkIds[i];

                var chunk = _chunks[chunkId];

                for (int x = 0; x < _size.x; x++)
                {
                    for (int y = 0; y < _size.y; y++)
                    {
                        var hex = chunk[x, y];

                        if (_lookup(hex) == key)
                            outputHexes.Add(hex);
                    }
                }
            }

            return new HexList(outputHexes);
        }

        public Neighbourhood GetNeighbourhood(Vector2Int index)
        {
            var neighbourhoodIndexes = Neighbourhood.GetNeighbours(index);

            //var chunkNeighbourhood = new Vector2Int[6];
            //
            //for (int i = 0; i < neighbourhoodIndexes.Length; i++)
            //{
            //    chunkNeighbourhood[i] = ChunkFromIndex(neighbourhoodIndexes[i]);
            //    neighbourhoodIndexes[i] = GlobalToLocalIndex(neighbourhoodIndexes[i]);
            //}

            var hexes = new Hex[6];

            //can optimise later...

            for (int i = 0; i < 6; i++)
            {
                hexes[i] = GetHex(neighbourhoodIndexes[i]);
            }

            return new Neighbourhood()
            {
                Center = GetHex(index),
                N0 = hexes[0],
                N1 = hexes[1],
                N2 = hexes[2],
                N3 = hexes[3],
                N4 = hexes[4],
                N5 = hexes[5]
            };
        }

        public HexMap Subdivide()
        {
            return new HexMap(this);
        }

        public HexMap ToGameObjects()
        {
            ForEachHex(x =>
            {


                var center = new Vector3(
                    x.Index.x,
                    0,
                    //item.Value.Payload.Height, 
                    x.Index.y * Hex.ScaleY);
                var isOdd = x.Index.y % 2 != 0;

                var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                obj.name = x.Index.ToString();
                obj.transform.position = center;
                var payload = obj.AddComponent<PayloadData>();
                x.Payload.PopulatePayloadObject(payload);
                payload.IsBorder = x.IsBorder;
                payload.NeighbourhoodData = x.DebugData;

                if (isOdd)
                {
                    obj.transform.position = center + new Vector3(0.5f, 0, 0);
                }

                if (Hex.IsInvalid(x))
                {
                    obj.transform.localScale = Vector3.one * 0.2f;
                    //return;
                    
                }
            });

            return this;
        }

        private void ForEachHex(Action<Hex> action)
        {
            foreach (var chunk in _chunks)
            {
                for (int x = 0; x < _size.x; x++)
                {
                    for (int y = 0; y < _size.y; y++)
                    {
                        var hex = chunk.Value[x, y];
                        action(hex);
                    }
                }
            }
        }     
    }

    public class HexList
    {
        internal HexList(List<Hex> hexes)
        {
            _hexes = hexes;
        }

        private List<Hex> _hexes;

        public T ToGraph<T>(Func<HexPayload, int> regionIndentifier, Func<HexPayload, int[]> regionConnector) where T : Graph<HexPayload>
        {
            (var vertices, var triangles) = this.ToMesh(x => 0);

            //Have to do this as you can't generate a T with a specific constructor
            var type = Activator.CreateInstance(typeof(T),
                vertices,
                triangles,
                this._hexes.Select(x => x.Payload).ToArray(),
                regionIndentifier,
                regionConnector) as T;

            return type;
        }

        private (Vector3[] vertices, int[] triangles) ToMesh(Func<HexPayload, float> zOffset)
        {
            var count = 0;
            var hexes = _hexes;
            var verts = hexes.ToDictionary(x => x.Index, x => { var i = count; count++; return i; });
            HashSet<Vector3Int> triangles = new HashSet<Vector3Int>();

            foreach (var hex in hexes)
            {
                var neighbourhood = Neighbourhood.GetNeighbours(hex.Index);

                for (int i = 0; i < neighbourhood.Length; i++)
                {
                    var n1 = hex.Index + neighbourhood[i];
                    var n2 = hex.Index + (i < neighbourhood.Length - 1 ? neighbourhood[i + 1] : neighbourhood[0]);

                    if (!(verts.ContainsKey(n1) && verts.ContainsKey(n2)))
                        continue;

                    //Determine triangle shape

                    var threePoints = new Vector2Int[3];
                    var indexIsOdd = hex.Index.y % 2 != 0;
                    var n1IsOdd = n1.y % 2 != 0;
                    var n2IsOdd = n2.y % 2 != 0;

                    var testIndex = indexIsOdd ? hex.Index.x + 0.5f : hex.Index.x;
                    var testn1 = n1IsOdd ? n1.x + 0.5f : n1.x;
                    var testn2 = n2IsOdd ? n2.x + 0.5f : n2.x;

                    if (testIndex < testn1 && testIndex < testn2)
                    {
                        threePoints[0] = hex.Index;
                        threePoints[1] = n1;
                        threePoints[2] = n2;
                    }
                    else if (testn1 < testIndex && testn1 < testn2)
                    {
                        threePoints[0] = n1;
                        threePoints[1] = hex.Index;
                        threePoints[2] = n2;
                    }
                    else if (testn2 < testIndex && testn2 < testn1)
                    {
                        threePoints[0] = n2;
                        threePoints[1] = n1;
                        threePoints[2] = hex.Index;
                    }
                    else
                    {
                        Debug.Log("Whelp");
                    }

                    if (threePoints[1].y < threePoints[2].y)
                    {
                        var temp = threePoints[1];
                        threePoints[1] = threePoints[2];
                        threePoints[2] = temp;
                    }

                    var tri = new Vector3Int(verts[threePoints[0]], verts[threePoints[1]], verts[threePoints[2]]);

                    if (triangles.Contains(tri))
                        continue;
                    triangles.Add(tri);
                }
            }

            var vertices = hexes.Select(x => new Vector3((x.Index.y % 2 == 0 ? x.Index.x : x.Index.x + 0.5f), zOffset(x.Payload), x.Index.y * Hex.ScaleY)).ToArray();
            var finalTriangles = triangles.SelectMany(x => new[] { x.x, x.y, x.z }).ToArray();

            return (vertices, finalTriangles);
        }
    }


}
