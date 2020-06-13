using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using WanderingRoad.Core.Random;
using WanderingRoad.Procgen.Topology;
using WanderingRoad.Core;
using WanderingRoad.IO;

namespace WanderingRoad.Procgen.RecursiveHex
{
    public class HexGroup:IStreamable
    {
        public Bounds Bounds { get { return _inside.Bounds; } }
        private HexDictionary _inside;
        private Dictionary<Vector3Int, Hex> _border;

        /// <summary>
        /// Creates a single hex cell at 0,0 with 6 border cells
        /// </summary>
        public HexGroup()
        {
            _inside = new HexDictionary(1);
            _border = new Dictionary<Vector3Int, Hex>(6);

            AddHex(new Hex(new HexIndex(),new HexPayload(),false));

            var neighbours = Neighbourhood.StaticHexNeighbours;
            //
            for (int i = 0; i < neighbours.Length; i++)
            {
                AddBorderHex(new Hex(neighbours[i],new HexPayload(),true), i);
            }
        }

        private HexGroup(HexDictionary inside, Dictionary<Vector3Int, Hex> border)
        {
            _inside = inside;
            _border = border;
        }

        public ISerialisable ToSerialisable()
        {
            var hexes = new Hex[_inside.Count + _border.Count];
            var count = 0;

            foreach (var item in _inside)
            {
                hexes[count] = item.Value;
                count++;
            }

            foreach (var item in _border)
            {
                hexes[count] = item.Value;
                count++;
            }

            return new SerialisableHexGroup(hexes);
        }

        public HexGroup(SerialisableHexGroup hexgroup)
        {
            _inside = new HexDictionary();
            _border = new Dictionary<Vector3Int, Hex>();

            var hexes = hexgroup.ToHexes();

            foreach (var hex in hexes)
            {
                if (hex.IsBorder)
                {
                    _border.Add(hex.Index.Index3d, hex);
                }
                else
                {
                    _inside.Add(hex.Index.Index3d, hex);
                }
            }
        }

        /// <summary>
        /// Creates a new hexgroup given a parent.
        /// </summary>
        /// <param name="parent"></param>
        private HexGroup(HexGroup parent, int rosetteSize, Func<HexPayload, int> connectionIndentifier)
        {
            var hoods = parent.GetNeighbourhoods();
            _inside = new HexDictionary(parent._inside.Count * 19); //magic numbers 
            _border = new Dictionary<Vector3Int, Hex>(parent._border.Count * 10); //magic numbers

            for (int i = 0; i < hoods.Length; i++)
            {         
                var hood = hoods[i];

                var cells = hood.Subdivide(rosetteSize, connectionIndentifier);

                for (int u = 0; u < cells.Length; u++)
                {
                    if (cells[u].IsBorder)
                        this.AddBorderHex(cells[u], u);
                    else
                        this.AddHex(cells[u]);
                }
            }
        }

        private void AddBorderHex(Hex hex, int indexTrack)
        {
            try
            {
                _border.Add(hex.Index.Index3d, hex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Duplicate Border Hex Discovered, Reference Index: {indexTrack}, 2D Pos: {hex.Index.Position2d}", ex);
                //Debug.LogError($"Duplicate Border Hex Discovered, Reference Index: {indexTrack}, 2D Pos: {hex.Index.Position2d}, 3D Index: {hex.Index.Index3d}");
            }           
        }

        private void AddHex(Hex hex)
        {
            try
            {
                _inside.Add(hex.Index.Index3d, hex);
            }
            catch (Exception ex)
            {
                throw new Exception("Duplicate Inner Hex Discovered", ex);
            }
            
        }

        public Dictionary<Vector3Int, Neighbourhood> GetNeighbourhoodDictionary()
        {
            return GetNeighbourhoods().ToDictionary(x => x.Center.Index.Index3d, x => x);
        }



        /// <summary>
        /// Get an array of neighbourhoods that allow all cells to be subdivided
        /// </summary>
        /// <returns></returns>
        public Neighbourhood[] GetNeighbourhoods(bool includeBorder = true)
        {
            var hood = new Neighbourhood[includeBorder?_inside.Count + _border.Count:_inside.Count];

            var count = 0;

            foreach (var hexDictEntry in _inside)
            {
                var hexes = new Hex[6];

                var neighbours = Neighbourhood.StaticHexNeighbours;

                for (int i = 0; i < 6; i++)
                {
                    var key = hexDictEntry.Key + neighbours[i].Index3d;
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
                        hexes[i] = Hex.InvalidHex(new HexIndex(key));
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

            if (!includeBorder) return hood;

            foreach (var hexDictEntry in _border)
            {
                var hexes = new Hex[6];
                var neighbours = Neighbourhood.StaticHexNeighbours;

                var borderOrNullCount = 0;

                for (int i = 0; i < 6; i++)
                {
                    var key = hexDictEntry.Key + neighbours[i].Index3d;
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
                        hexes[i] = Hex.InvalidHex(new HexIndex(key));
                    }
                }

                var finalCenter = borderOrNullCount < 6 ? hexDictEntry.Value : Hex.InvalidHex(hexDictEntry.Value.Index);

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
        public HexGroup Subdivide(int amount, Func<HexPayload, int> connectionIndentifier)
        {
            //Debug.Log("Starting subdivide");
            return new HexGroup(this, amount, connectionIndentifier);
        }

        /// <summary>
        /// Subdivide this hexgroup returning results for debugging purposes.
        /// </summary>
        /// <returns></returns>
        //public HexGroup DebugSubdivide()
        //{
        //    return new HexGroup(this, true);
        //}

        /// <summary>
        /// Hypothetical function that subdivides and returns a specific subset, based on a function, like Where() in linq.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public HexGroup GetSubGroup(Func<Hex, bool> func)
        {
                        
            var inside = _inside.Where(x => func(x.Value)).ToList();
            var hexDict = new HexDictionary(inside.Count);

            inside.ForEach(x => hexDict.Add(x.Key, x.Value));


            return new HexGroup(hexDict, GetBorderOfSubgroup(hexDict));
        }

        public List<HexGroup> GetSubGroups(Func<Hex, int> func)
        {
            return _inside.GroupBy(x => func(x.Value))
                .Select(x =>
                {
                    var inside = x.ToList();
                    var hexDict = new HexDictionary(inside.Count);
                    inside.ForEach(y => hexDict.Add(y.Key, y.Value));
                    return new HexGroup(hexDict, GetBorderOfSubgroup(hexDict));
                }).ToList();
        }

        private Dictionary<Vector3Int, Hex> GetBorderOfSubgroup(HexDictionary subgroup)
        {
            var border = new Dictionary<Vector3Int, Hex>();

            var subgroupInternalEdgeHexes = new List<Vector2Int>();

            foreach (var hexDictEntry in subgroup)
            {
                var neighbours = Neighbourhood.StaticHexNeighbours;

                for (int i = 0; i < 6; i++)
                {
                    var key = hexDictEntry.Key + neighbours[i].Index3d;

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
            foreach (var pair in _inside.ToList())
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

            foreach (var pair in _inside.ToList())
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

        private static Mesh ToMesh(Dictionary<Vector3Int, Hex> dict, Func<Hex,float> heightCalculator)
        {
            var verts = new Vector3[dict.Count * 3 * 6];
            var tris = new int[dict.Count * 6 * 3];
            var colors = new Color[dict.Count * 3 * 6];

            var count = 0;

            foreach (var item in dict)
            {
                //var index2d = item.Value.Index.Index2d;
                var center = item.Value.Index.Position3d;
                var isOdd = item.Key.y % 2 != 0;

                var yHeight = Vector3.up * heightCalculator(item.Value);

                for (int i = 0; i < 6; i++)
                {
                    var index = (count * 3 * 6) + (i * 3);
                    verts[index] = center + yHeight;
                    verts[index + 1] = GetPointyCornerXZ(center, i)+ yHeight;
                    verts[index + 2] = GetPointyCornerXZ(center, i+1) + yHeight;

                    //if (isOdd)
                    //{
                    //    verts[index].x += 0.5f;
                    //    verts[index + 1].x += 0.5f;
                    //    verts[index + 2].x += 0.5f;
                    //}

                    verts[index] = verts[index].AddNoiseOffset(0.2f);
                    verts[index + 1] = verts[index + 1].AddNoiseOffset(0.2f);
                    verts[index + 2] = verts[index + 2].AddNoiseOffset(0.2f);

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

            Vector3 GetPointyCornerXZ(Vector3 vec, int i)
            {
                var angle_deg = 60f * i - 30f;
                var angle_rad = Mathf.PI / 180f * angle_deg;
                return vec + new Vector3(HexIndex.HalfHex * Mathf.Cos(-angle_rad), 0,
                             HexIndex.HalfHex * Mathf.Sin(-angle_rad));
            }
        }

        private static Mesh ToMesh(Dictionary<Vector3Int, Hex> dict)
        {
            return ToMesh(dict, x => 0);
        }

        public Mesh ToMesh()
        {
            return HexGroup.ToMesh(_inside.GetDictionary());
        }

        public Mesh ToMesh(Func<Hex, float> heightCalculator)
        {
            return HexGroup.ToMesh(_inside.GetDictionary(),heightCalculator);
        }

        public Hex[] GetHexes()
        {
            return _inside.Select(x => x.Value).ToArray();
        }

        public (Vector3[] vertices, int[] triangles)  ToNetwork(Func<HexPayload,float> zOffset)
        {
            var count = 0;
            var indexes = _inside.Values.Select(x => x.Index);
            var verts = indexes.ToDictionary(x => x.Index3d, x => { var i = count; count++; return i; });
            HashSet<Vector3Int> triangles = new HashSet<Vector3Int>();

            var neighbourhood = Neighbourhood.StaticHexNeighbours;

            var triangleStore = new List<int>() {0,0,0};

            foreach (var index in indexes)
            {

                for (int i = 0; i < neighbourhood.Length; i++)
                {
                    var hexCenter = index;
                    var hexN1 = hexCenter + neighbourhood[i];
                    var hexN2 = hexCenter + (i < neighbourhood.Length - 1 ? neighbourhood[i + 1] : neighbourhood[0]);

                    //Ditch if edge isn't in network
                    if (!(verts.ContainsKey(hexN1.Index3d) && verts.ContainsKey(hexN2.Index3d)))
                        continue;

                    triangleStore[0] = (verts[hexCenter.Index3d]);
                    triangleStore[1] = (verts[hexN1.Index3d]);
                    triangleStore[2] = (verts[hexN2.Index3d]);

                    triangleStore.Sort();

                    var tri = new Vector3Int(triangleStore[0], triangleStore[1], triangleStore[2]);                    

                    if (triangles.Contains(tri))
                        continue;
                    triangles.Add(tri);
                }
            }

            var vertices = indexes.Select(x => x.Position3d).ToArray();
            var finalTriangles = 
                triangles.SelectMany(x => {
                    var cross = Vector3.Cross(vertices[x.y] - vertices[x.x], vertices[x.z] - vertices[x.x]);
                    if (cross.y > 0)
                        return new[] { x.x, x.y, x.z };
                    else
                        return new[] { x.z, x.y, x.x };
                    }).ToArray();

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

        #endregion

        #region graphs

        public T ToGraph<T>(Func<HexPayload, int> regionIndentifier, Func<HexPayload, int[]> regionConnector) where T : Graph<HexPayload>
        {
            (var vertices, var triangles) = this.ToNetwork(x => 0);

            //Have to do this as you can't generate a T with a specific constructor
            var type = Activator.CreateInstance(typeof(T),
                vertices,
                triangles,
                this._inside.Select(x => x.Value.Payload).ToArray(),
                regionIndentifier,
                regionConnector) as T;

            return type;
        }


        public HexGroup MassUpdateHexes(HexPayload[] data)
        {
            return this.ForEach((x, i) => data[i]);
        }

        #endregion
    }

}