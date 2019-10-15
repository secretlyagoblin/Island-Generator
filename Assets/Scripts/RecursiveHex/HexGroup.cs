using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecursiveHex
{

    public class HexGroup
    {
        private Dictionary<Vector2Int, Hex> _inside;
        private Dictionary<Vector2Int, Hex> _border;

        private static readonly Vector2Int[] _offsets = new Vector2Int[]
            {
            new Vector2Int(+1,+0),
            new Vector2Int(+0,-1),
            new Vector2Int(-1,-1),
            new Vector2Int(-1,+0),
            new Vector2Int(-1,+1),
            new Vector2Int(+0,+1)
            };



        public HexGroup ForEach(Func<Hex, Hex> action)
        {
            foreach (var item in _inside)
            {
                _inside[item.Key] = action(item.Value);
            }
            return this;
        }

        public HexGroup ForEach(Action<Hex> action)
        {
            foreach (var item in _inside)
            {
                action(item.Value);
            }
            return this;
        }

        private Neighbourhood[] GetNeighbourhoods()
        {
            var hood = new Neighbourhood[_inside.Count];

            var count = 0;

            foreach (var item in _inside)
            {
                var hexes = new Hex[6];



                for (int i = 0; i < 6; i++)
                {
                    var key = item.Key + _offsets[i];
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
                        //Debug.LogError("Hey, Remove Null Neighbours Being Possible");
                        hexes[i] = new Hex();
                    }
                }

                hood[count] = new Neighbourhood
                {
                    Center = item.Value,
                    N0 = hexes[0],
                    N1 = hexes[1],
                    N2 = hexes[2],
                    N3 = hexes[3],
                    N4 = hexes[4],
                    N5 = hexes[5]
                };

                count++;
            }

            return hood;
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
        /// Creates a single hex cell at 0,0 with 6 border cells
        /// </summary>
        public HexGroup()
        {
            _inside = new Dictionary<Vector2Int, Hex>(1);
            _border = new Dictionary<Vector2Int, Hex>(6);

            AddHex(new Hex(0, 0));

            for (int i = 0; i < _offsets.Length; i++)
            {
                AddBorderHex(new Hex(_offsets[i].x, _offsets[i].y));
            }
        }

        /// <summary>
        /// Subdivide this hexgroup.
        /// </summary>
        /// <returns></returns>
        public HexGroup Subdivide()
        {
            return new HexGroup(this);
        }

        /// <summary>
        /// Subdivide this hexgroup returning results for debugging purposes.
        /// </summary>
        /// <returns></returns>
        public HexGroup DebugSubdivideRowOnly()
        {
            return new HexGroup(this, true);
        }

        /// <summary>
        /// Hypothetical function that subdivides and returns a specific subset, based on a function, like Where() in linq.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public HexGroup[] Subdivide(Func<Hex, object> func)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new hexgroup given a parent.
        /// </summary>
        /// <param name="parent"></param>
        private HexGroup(HexGroup parent)
        {
            var hoods = parent.GetNeighbourhoods();
            _inside = new Dictionary<Vector2Int, Hex>(parent._inside.Count * 19);
            _border = new Dictionary<Vector2Int, Hex>(parent._border.Count * 10);

            for (int i = 0; i < hoods.Length; i++)
            {
                var hood = hoods[i];
                var cells = hood.Subdivide();

                for (int u = 0; u < cells.Length; u++)
                {
                    if (hood.IsBorder)
                        this.AddBorderHex(cells[u]);
                    else
                        this.AddHex(cells[u]);
                }
            }
        }

        /// <summary>
        /// Bad implimentation of a debugger.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="debug"></param>
        private HexGroup(HexGroup parent, bool debug)
        {
            var hoods = parent.GetNeighbourhoods();
            _inside = new Dictionary<Vector2Int, Hex>(parent._inside.Count * 19);
            _border = new Dictionary<Vector2Int, Hex>(parent._border.Count * 10);

            for (int i = 0; i < hoods.Length; i++)
            {
                var hood = hoods[i];
                var cells = hood.DebugSubdivideRowOnly();

                for (int u = 0; u < cells.Length; u++)
                {
                    if (hood.IsBorder)
                        this.AddBorderHex(cells[u]);
                    else
                        this.AddHex(cells[u]);
                }
            }
        }

        private static Mesh ToMesh(Dictionary<Vector2Int, Hex> dict)
        {
            var verts = new Vector3[dict.Count * 3 * 6];
            var tris = new int[dict.Count * 6 * 3];

            var count = 0;

            foreach (var item in dict)
            {
                var center = new Vector3(item.Key.x, 0, item.Key.y * Hex.ScaleY);
                var isOdd = item.Key.y % 2 != 0;

                for (int i = 0; i < 6; i++)
                {
                    var index = (count * 3 * 6) + (i * 3);
                    verts[index] = center;
                    verts[index + 1] = item.Value.GetCornerXZ(i);
                    verts[index + 2] = item.Value.GetCornerXZ(i + 1);

                    if (isOdd)
                    {
                        verts[index].x += 0.5f;
                        verts[index + 1].x += 0.5f;
                        verts[index + 2].x += 0.5f;
                    }

                    tris[index] = index;
                    tris[index + 1] = index + 1;
                    tris[index + 2] = index + 2;
                }
                count++;
            }

            return new Mesh()
            {
                vertices = verts,
                triangles = tris
            };
        }

        private static void ToGameObjects(Dictionary<Vector2Int, Hex> dict, GameObject prefab)
        {
            var count = 0;

            foreach (var item in dict)
            {
                var center = new Vector3(item.Key.x, 0, item.Key.y * Hex.ScaleY);
                var isOdd = item.Key.y % 2 != 0;

                var obj = GameObject.Instantiate(prefab);

                obj.name = item.Key.ToString();
                obj.transform.position = center;

                for (int i = 0; i < 6; i++)
                {
                    var index = (count * 3 * 6) + (i * 3);

                    if (isOdd)
                    {
                        obj.transform.position = center + new Vector3(0.5f, 0, 0);
                    }
                }
                count++;
            }
        }

        public Mesh ToMesh()
        {
            return HexGroup.ToMesh(_inside);
        }

        public void ToGameObjects(GameObject prefab)
        {
            HexGroup.ToGameObjects(_inside, prefab);
        }

        public Mesh ToMeshBorder()
        {
            return HexGroup.ToMesh(_border);
        }
    }
}