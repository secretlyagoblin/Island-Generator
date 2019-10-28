using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RecursiveHex
{

    public class HexGroup
    {
        private Dictionary<Vector2Int, Hex> _inside;
        private Dictionary<Vector2Int, Hex> _border;

        /// <summary>
        /// Creates a single hex cell at 0,0 with 6 border cells
        /// </summary>
        public HexGroup()
        {
            _inside = new Dictionary<Vector2Int, Hex>(1);
            _border = new Dictionary<Vector2Int, Hex>(6);

            AddHex(new Hex(Vector2Int.zero,new HexPayload(),false));

            var neighbours = Neighbourhood.GetNeighbours(Vector2Int.zero);
            //
            for (int i = 0; i < neighbours.Length; i++)
            {
                AddBorderHex(new Hex(neighbours[i],new HexPayload(),true));
            }
        }

        private HexGroup(Dictionary<Vector2Int, Hex> inside, Dictionary<Vector2Int, Hex> border)
        {
            _inside = inside;
            _border = border;
        }

        /// <summary>
        /// Creates a new hexgroup given a parent.
        /// </summary>
        /// <param name="parent"></param>
        private HexGroup(HexGroup parent, bool debug = false)
        {
            var hoods = parent.GetNeighbourhoods();
            _inside = new Dictionary<Vector2Int, Hex>(parent._inside.Count * 19); //magic numbers
            _border = new Dictionary<Vector2Int, Hex>(parent._border.Count * 10); //magic numbers

            for (int i = 0; i < hoods.Length; i++)
            {         
                var hood = hoods[i];

                var cells = debug ? hood.DebugSubdivide() : hood.Subdivide();

                for (int u = 0; u < cells.Length; u++)
                {
                    if (cells[u].IsBorder)
                        this.AddBorderHex(cells[u]);
                    else
                        this.AddHex(cells[u]);
                }
            }
        }

        private void AddBorderHex(Hex hex)
        {
            _border.Add(hex.Index, hex);
        }

        private void AddHex(Hex hex)
        {
            _inside.Add(hex.Index, hex);
        }

        /// <summary>
        /// Get an array of neighbourhoods that allow all cells to be subdivided
        /// </summary>
        /// <returns></returns>
        private Neighbourhood[] GetNeighbourhoods()
        {
            var hood = new Neighbourhood[_inside.Count + _border.Count];

            var count = 0;

            foreach (var hexDictEntry in _inside)
            {
                var hexes = new Hex[6];

                var neighbours = Neighbourhood.GetNeighbours(hexDictEntry.Key);

                for (int i = 0; i < 6; i++)
                {
                    var key = hexDictEntry.Key + neighbours[i];
                    if (_border.ContainsKey(key))
                    {
                        hexes[i] = _border[key];
                    }
                    else if (_inside.ContainsKey(key))
                    {
                        hexes[i] = _inside[key];
                    }
                    else
                    {
                        Debug.LogError("A null neighbour detected on an inner hex.");
                        hexes[i] = Hex.InvalidHex;
                    }
                }

                hood[count] = new Neighbourhood
                {
                    Center = hexDictEntry.Value,
                    N0 = hexes[0],
                    N1 = hexes[1],
                    N2 = hexes[2],
                    N3 = hexes[3],
                    N4 = hexes[4],
                    N5 = hexes[5],
                    IsBorder = false
                };
                count++;
            }

            foreach (var hexDictEntry in _border)
            {
                var hexes = new Hex[6];
                var neighbours = Neighbourhood.GetNeighbours(hexDictEntry.Key);

                var borderOrNullCount = 0;

                for (int i = 0; i < 6; i++)
                {
                    var key = hexDictEntry.Key + neighbours[i];
                    if (_border.ContainsKey(key))
                    {
                        borderOrNullCount++;
                        hexes[i] = _border[key];
                    }
                    else if (_inside.ContainsKey(key))
                    {
                        hexes[i] = _inside[key];
                    }
                    else
                    {
                        borderOrNullCount++;
                        //Debug.Log("This border has some nulls! That is fine");
                        hexes[i] = Hex.InvalidHex;
                    }
                }

                var finalCenter = borderOrNullCount < 6 ? hexDictEntry.Value : Hex.InvalidHex;

                hood[count] = new Neighbourhood
                {
                    Center = finalCenter,
                    N0 = hexes[0],
                    N1 = hexes[1],
                    N2 = hexes[2],
                    N3 = hexes[3],
                    N4 = hexes[4],
                    N5 = hexes[5],
                    IsBorder = true
                };
                count++;
            }

            return hood;
        }

        #region Subdivision

        /// <summary>
        /// Subdivide this hexgroup.
        /// </summary>
        /// <returns></returns>
        public HexGroup Subdivide()
        {
            //Debug.Log("Starting subdivide");
            return new HexGroup(this);
        }

        /// <summary>
        /// Subdivide this hexgroup returning results for debugging purposes.
        /// </summary>
        /// <returns></returns>
        public HexGroup DebugSubdivide()
        {
            return new HexGroup(this, true);
        }

        /// <summary>
        /// Hypothetical function that subdivides and returns a specific subset, based on a function, like Where() in linq.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public HexGroup GetSubGroup(Func<Hex, bool> func)
        {
            var inside = _inside.Where(x => func(x.Value)).ToDictionary(x => x.Key, x => x.Value);


            return new HexGroup(inside, GetBorderOfSubgroup(inside));
        }

        public List<HexGroup> GetSubGroups(Func<Hex, int> func)
        {
            return _inside.GroupBy(x => func(x.Value))
                .Select(x =>
                {
                    var inside = x.ToDictionary(y => y.Key, y => y.Value);
                    return new HexGroup(inside, GetBorderOfSubgroup(inside));
                }).ToList();
        }

        private Dictionary<Vector2Int, Hex> GetBorderOfSubgroup(Dictionary<Vector2Int,Hex> subgroup)
        {
            var border = new Dictionary<Vector2Int, Hex>();

            foreach (var hexDictEntry in subgroup)
            {
                var neighbours = Neighbourhood.GetNeighbours(hexDictEntry.Key);

                for (int i = 0; i < 6; i++)
                {
                    var key = hexDictEntry.Key + neighbours[i];

                    if (border.ContainsKey(key))
                    {
                        continue;
                    }
                    if (subgroup.ContainsKey(key))
                    {
                        continue;
                    }
                    if (_inside.ContainsKey(key))
                    {
                        border.Add(key, new Hex(_inside[key], true));
                    }
                    else if (_border.ContainsKey(key))
                    {
                        border.Add(key, new Hex(_border[key], true));
                    }
                    else
                    {
                        Debug.LogError("The subregion being generated has invalid neighbours.");
                    }
                }
            }

            return border;
        }

        #endregion

        #region ForEach

        public HexGroup ForEach(Func<Hex, HexPayload> func)
        {
            foreach (KeyValuePair<Vector2Int, Hex> pair in _inside.ToList())
            {
                var hex = pair.Value;
                hex.Payload = func(hex);
                _inside[pair.Key] = hex;
            }

            return this;
        }

        public HexGroup ForEach(Func<Hex, int, HexPayload> func)
        {
            var i = 0;

            foreach (KeyValuePair<Vector2Int, Hex> pair in _inside.ToList())
            {
                var hex = pair.Value;
                hex.Payload = func(hex, i);
                _inside[pair.Key] = hex;
                i++;
            }

            return this;
        }

        #endregion

        #region ToMesh + ToGameObjects

        private static Mesh ToMesh(Dictionary<Vector2Int, Hex> dict, Func<Hex,float> heightCalculator)
        {
            var verts = new Vector3[dict.Count * 3 * 6];
            var tris = new int[dict.Count * 6 * 3];
            var colors = new Color[dict.Count * 3 * 6];

            var count = 0;

            foreach (var item in dict)
            {
                var center = new Vector3(item.Key.x, 0, item.Key.y * Hex.ScaleY);
                var isOdd = item.Key.y % 2 != 0;

                var yHeight = Vector3.up * heightCalculator(item.Value);

                for (int i = 0; i < 6; i++)
                {
                    var index = (count * 3 * 6) + (i * 3);
                    verts[index] = center + yHeight;
                    verts[index + 1] = item.Value.GetPointyCornerXZ(i)+ yHeight;
                    verts[index + 2] = item.Value.GetPointyCornerXZ(i + 1)+ yHeight;

                    if (isOdd)
                    {
                        verts[index].x += 0.5f;
                        verts[index + 1].x += 0.5f;
                        verts[index + 2].x += 0.5f;
                    }

                    verts[index] = verts[index].AddNoiseOffset();
                    verts[index + 1] = verts[index + 1].AddNoiseOffset();
                    verts[index + 2] = verts[index + 2].AddNoiseOffset();

                    tris[index] = index;
                    tris[index + 1] = index + 1;
                    tris[index + 2] = index + 2;

                    colors[index]         = item.Value.Payload.Color;
                        colors[index+1]   = item.Value.Payload.Color;
                    colors[index + 2] = item.Value.Payload.Color;
                }
                count++;
            }

            return new Mesh()
            {
                vertices = verts,
                triangles = tris,
                colors = colors
            };
        }

        private static Mesh ToMesh(Dictionary<Vector2Int, Hex> dict)
        {
            return ToMesh(dict, x => 0);
        }

        private static void ToGameObjects(Dictionary<Vector2Int, Hex> dict, GameObject prefab)
        {
            var count = 0;

            foreach (var item in dict)
            {
                var center = new Vector3(
                    item.Key.x,
                    0,
                    //item.Value.Payload.Height, 
                    item.Key.y * Hex.ScaleY);
                var isOdd = item.Key.y % 2 != 0;

                var obj = GameObject.Instantiate(prefab);

                obj.name = item.Key.ToString();
                obj.transform.position = center;
                var payload = obj.AddComponent<PayloadData>();
                item.Value.Payload.PopulatePayloadObject(payload);
                payload.NeighbourhoodData= item.Value.DebugData;

                if (Hex.IsInvalid(item.Value))
                {
                    payload.name += $"_NULL";
                }       

                if (isOdd)
                {
                    obj.transform.position = center + new Vector3(0.5f, 0, 0);
                }

                count++;
            }
        }

        public Mesh ToMesh()
        {
            return HexGroup.ToMesh(_inside);
        }

        public Mesh ToMesh(Func<Hex, float> heightCalculator)
        {
            return HexGroup.ToMesh(_inside,heightCalculator);
        }

        public void ToGameObjects(GameObject prefab)
        {
            HexGroup.ToGameObjects(_inside, prefab);
        }

        public void ToGameObjectsBorder(GameObject prefab)
        {
            HexGroup.ToGameObjects(_border, prefab);
        }

        public Mesh ToMeshBorder()
        {
            return HexGroup.ToMesh(_border);
        }

        private (Vector3[] vertices, int[] triangles)  ToNetwork(Func<HexPayload,float> func)
        {
            var count = 0;
            var indexes = _inside.Keys;
            var verts = indexes.ToDictionary(x => x, x => { var i = count; count++; return i; });
            HashSet<Vector3Int> triangles = new HashSet<Vector3Int>();

            foreach (var index in indexes)
            {
                var neighbourhood = Neighbourhood.GetNeighbours(index);

                for (int i = 0; i < neighbourhood.Length; i++)
                {
                    var n1 = index + neighbourhood[i];
                    var n2 = index + (i < neighbourhood.Length - 1 ? neighbourhood[i + 1]: neighbourhood[0]);

                    if(!(verts.ContainsKey(n1) && verts.ContainsKey(n2)))
                        continue;

                    //Determine triangle shape

                    var threePoints = new Vector2Int[3];
                    var indexIsOdd = index.y % 2 != 0;
                    var n1IsOdd = n1.y % 2 != 0;
                    var n2IsOdd = n2.y % 2 != 0;

                    var testIndex = indexIsOdd? index.x+0.5f:index.x;
                    var testn1 = n1IsOdd ? n1.x + 0.5f : n1.x;
                    var testn2 = n2IsOdd ? n2.x + 0.5f : n2.x;

                    if (testIndex<testn1 && testIndex< testn2)
                    {
                        threePoints[0] = index;
                        threePoints[1] = n1;
                        threePoints[2] = n2;
                    }
                    else if (testn1< testIndex && testn1< testn2)
                    {
                        threePoints[0] = n1;
                        threePoints[1] = index;
                        threePoints[2] = n2;
                    }
                    else if (testn2< testIndex && testn2< testn1)
                    {
                        threePoints[0] = n2;
                        threePoints[1] = n1;
                        threePoints[2] = index;
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

            var vertices = indexes.Select(x => new Vector3((x.y % 2 == 0 ? x.x : x.x + 0.5f), func(_inside[x].Payload), x.y * Hex.ScaleY)).ToArray();
            var finalTriangles = triangles.SelectMany(x => new[] { x.x, x.y, x.z }).ToArray();

            return (vertices, finalTriangles);
            //,indexes.Select(x => new Vector3(x.x, 0, x.y)).ToArray()
            //triangles.SelectMany(x => new[]{ x.x, x.y, x.z }).ToArray(),
            // _inside.Values.Select(x => x.Payload).ToArray());
        }

        public Mesh ToConnectedMesh(Func<HexPayload, float> heightFunc, Func<HexPayload, Color> colorFunc)
        {
            (var vertices, var triangles) = this.ToNetwork(heightFunc);

            var mesh = new Mesh()
            {
                vertices = vertices,
                triangles = triangles,
                colors = _inside.Select(x => colorFunc(x.Value.Payload)).ToArray()
            };

            mesh.RecalculateNormals();

            return mesh;
        }

        public Graph<HexPayload> ToGraph()
        {
            (var vertices, var triangles) = this.ToNetwork(x => 0);

            return new Graph<HexPayload>(vertices, triangles, this._inside.Select(x => x.Value.Payload).ToArray());
        }

        #endregion
    }
}