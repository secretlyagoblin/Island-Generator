using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using WanderingRoad.Procgen.Topology;

namespace WanderingRoad.Procgen.Meshes
{
    public partial class SmartMesh {

        public List<SmartNode> Nodes { get; private set; }
        public List<SmartCell> Cells { get; private set; }
        public List<SmartLine> Lines { get; private set; }

        private Vector3[] _vertices;
        private int[] _triangles;

        Bounds _bounds = new Bounds();
        List<SmartNode>[,,] _buckets;

        #region constructors

        //public SmartMesh(Mesh mesh) : this(mesh.vertices,mesh.triangles)
        //{    
        //}

        public SmartMesh(Vector3[] vertices, int[] triangles)
        {
            Nodes = new List<SmartNode>(vertices.Length);
            Cells = new List<SmartCell>(triangles.Length/3);
            Lines = new List<SmartLine>(triangles.Length);

            _vertices = vertices;

            for (int i = 0; i < _vertices.Length; i++)
            {
                Nodes.Add(new SmartNode(_vertices[i], i));
                _bounds.Encapsulate(_vertices[i]);
            }

            _triangles = triangles;

            var halfEdges = new Dictionary<Vector2Int, SmartLine>();

            for (var x = 0; x < _triangles.Length; x += 3)
            {
                var cell = new SmartCell(Nodes[_triangles[x ]], Nodes[_triangles[x + 1]], Nodes[_triangles[x + 2]]);
                cell.CreateNodeConnections();
                Cells.Add(cell);

                CreateLineConnections(_triangles, halfEdges, cell, x, x + 1);
                CreateLineConnections(_triangles, halfEdges, cell, x+1, x + 2);
                CreateLineConnections(_triangles, halfEdges, cell, x+2, x);

            }

            //Here we iterate through creating lines by creating half-lines and then searching for the inverse in the set
            //So we should be able to close off a decent chunk of this pretty early

            //here first chris





            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].CreateCellConnections();
            }

            //for (int i = 0; i < Cells.Count; i++)
            //{
            //    Cells[i].CreateLineConnections();
            //    Lines.AddRange(Cells[i].Lines);
            //}
            //
            //Lines = Lines.Distinct().ToList();
            //
            for (int i = 0; i < Lines.Count; i++)
            {
                Lines[i].Nodes[0].Nodes.Add(Lines[i].Nodes[1]);
                Lines[i].Nodes[1].Nodes.Add(Lines[i].Nodes[0]);
            }

            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i].Index = i;

            for (int i = 0; i < Cells.Count; i++)
                Cells[i].Index = i;

            for (int i = 0; i < Lines.Count; i++)
                Lines[i].Index = i;

            CreateBuckets(1, 1, 1);
        }

        #endregion        

        public void SetCustomBuckets(int sizeX, int sizeY, int sizeZ)
        {
            CreateBuckets(sizeX, sizeY, sizeZ);
        }

        public int[] TriangleMap()
        {
            var output = new List<int>();

            for (int i = 0; i < Cells.Count; i++)
            {
                output.AddRange(Cells[i].Nodes.Select(n => n.Index).ToList());
            }

            return output.ToArray();
        }

        public Mesh ToXZMesh()
        {
            var triangles = new int[_triangles.Length];
            for (int i = 0; i < _triangles.Length; i+=3)
            {
                triangles[i] = _triangles[i + 2];
                triangles[i + 1] = _triangles[i + 1];
                triangles[i+2] = _triangles[i];
            } 

            var m = new Mesh
            {
                vertices = _vertices.Select(x => new Vector3(x.x,0,x.y)).ToArray(),
                triangles = triangles
            };
            m.RecalculateBounds();
            m.RecalculateNormals();

            return m;
        }

        public Mesh ToXZMesh(float[] heights)
        {
            var triangles = new int[_triangles.Length];
            for (int i = 0; i < _triangles.Length; i += 3)
            {
                triangles[i] = _triangles[i + 2];
                triangles[i + 1] = _triangles[i + 1];
                triangles[i + 2] = _triangles[i];
            }

            var m = new Mesh
            {
                vertices = _vertices.Select((x,i) => new Vector3(x.x, heights[i], x.y)).ToArray(),
                triangles = triangles
            };
            m.RecalculateBounds();
            m.RecalculateNormals();

            return m;
        }

        public Mesh ToXYMesh()
        {
            var m = new Mesh
            {
                vertices = _vertices,
                triangles = _triangles
            };

            m.RecalculateBounds();
            m.RecalculateNormals();

            return m;
        }

        /*

        public SmartMesh()
        {
            Nodes = new List<SmartNode>();
            Nodes.Add(new SmartNode(Vector3.zero, 0));

            Lines = new List<SmartLine>();


            for (int i = 0; i < 2; i++)
            {
                var node = new SmartNode(new Vector3(Random.Range(1f, -1f), 0, Random.Range(1f, -1f)), i);
                Lines.Add(new SmartLine(node, Nodes[0]));
            }

            Nodes[0].UpdateLineSort();

            //var gradient = new Gradient();
            //gradient.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.blue,0f), new GradientColorKey(Color.red, 1f) },new GradientAlphaKey[] { new GradientAlphaKey(1f,0f), new GradientAlphaKey(1f, 1f) });

            //var time = 0f;

            var color = Color.black;

            for (int i = 0; i < Nodes[0].Lines.Count; i++)
            {
                color.r += 0.1f;

                var nodes = Nodes[0].Lines[i].Nodes;
                Debug.DrawLine(nodes[0].Vert, nodes[1].Vert, color, 100f);
                
            }

            var nextLine = Nodes[0].GetNextClockwiseLineSegment(Nodes[0].Lines[1]);
            Debug.DrawLine(Nodes[0].Lines[1].Nodes[0].Vert, Nodes[0].Lines[1].Nodes[1].Vert, Color.green, 100f);
            Debug.DrawLine(nextLine.Nodes[0].Vert, nextLine.Nodes[1].Vert, Color.white, 100f);

        }

    */ //Debug mesh

        public void DrawMesh(Matrix4x4 transform)
        {
            DrawMesh(Color.red, Color.green, transform);
        }

        public void DrawMesh(Color graphColor, Color inverseGraphColor, Matrix4x4 transform)
        {
            DrawMesh(graphColor, inverseGraphColor, 0f, transform);
        }

        public void DrawMesh(Color graphColor, Color inverseGraphColor, float time, Matrix4x4 transform)
        {
            foreach (var cell in Cells)
            {
                foreach (var neighbour in cell.Neighbours)
                {

                    var scaledVector = neighbour.Center + cell.Center;
                    float scale = 0.5f;
                    scaledVector = new Vector3(scaledVector.x * scale, scaledVector.y * scale, scaledVector.z * scale);

                    Debug.DrawLine(transform.MultiplyPoint(cell.Center), transform.MultiplyPoint(scaledVector), inverseGraphColor, time);
                }
            }

            foreach (var line in Lines)
            {
                Debug.DrawLine(transform.MultiplyPoint(line.Nodes[0].Vert), transform.MultiplyPoint(line.Nodes[1].Vert), graphColor, time);
            }

        }

        public static Vector3[] NodeArrayToVector3Array(List<SmartNode> smartNodesList)
        {
            List<Vector3> vectors = new List<Vector3>();
            foreach (var smartNode in smartNodesList)
            {
                vectors.Add(smartNode.Vert);
            }

            return vectors.ToArray();
        }

        //public MeshState

        public int[] ShortestWalkNode(int startIndex, int endIndex)
        {
            var nodesToEvaluate = new Dictionary<int, aStarHelper>();
            var evaluatedNodes = new Dictionary<int, aStarHelper>();
            var startVert = Nodes[startIndex].Vert;
            var endVert = Nodes[endIndex].Vert;
            var shortCircuit = -1;

            var output = new List<int>();

            nodesToEvaluate.Add(startIndex, new aStarHelper()
            {
                gCost = 0,
                hCost = 0
            });

            while(shortCircuit <= 99999)
            {
                shortCircuit++;

                float shortestDist = float.MaxValue;
                int shortestNode = 0;

                foreach (var node in nodesToEvaluate)
                {
                    if(node.Value.fCost < shortestDist)
                    {
                        shortestNode = node.Key;
                        shortestDist = node.Value.fCost;
                    }
                }

                var currentKey = shortestNode;
                var currentValue = nodesToEvaluate[currentKey];
                var currentNode = Nodes[currentKey];
                nodesToEvaluate.Remove(currentKey);
                evaluatedNodes.Add(currentKey, currentValue);

                if(currentKey == endIndex)
                {
                    var parent = currentKey;
                    while(parent != startIndex)
                    {
                        output.Add(parent);
                        parent= evaluatedNodes[parent].parent;
                    }

                    output.Add(startIndex);
                    return output.ToArray();
                }

                for (int i = 0; i < Nodes[currentKey].Nodes.Count; i++)
                {
                    var neigh = Nodes[currentKey].Nodes[i];
                    if (evaluatedNodes.ContainsKey(neigh.Index))
                        continue;

                    var gCost = currentValue.gCost + Vector3.Distance(currentNode.Vert, neigh.Vert);

                    if (nodesToEvaluate.ContainsKey(neigh.Index))
                    {
                        var star = nodesToEvaluate[neigh.Index];
                        if (gCost > star.gCost)
                            continue;

                        star.gCost = gCost;
                        star.parent = currentKey;
                    }
                    else
                    {
                        nodesToEvaluate.Add(neigh.Index, new aStarHelper()
                        {
                            //can be updated to include weights on lines by searching for actual line weights
                            gCost = gCost,
                            hCost = Vector3.Distance(neigh.Vert, endVert),
                            parent = currentKey
                        });
                    }


                }

            }

            Debug.Log("Short Circuit, Lad");

            return new int[0];



        }

        public void Resize(Bounds bounds)
        {
            var oldMin = _bounds.min;
            var oldMax = _bounds.max;
            var newMin = bounds.min;
            var newMax = bounds.max;

            for (int i = 0; i < Nodes.Count; i++)
            {
                var n = Nodes[i].Vert;

                var x = Mathf.InverseLerp(oldMin.x, oldMax.x, n.x);
                var y = Mathf.InverseLerp(oldMin.y, oldMax.y, n.y);
                var z = Mathf.InverseLerp(oldMin.z, oldMax.z, n.z);

                x = Mathf.Lerp(newMin.x, newMax.x, x);
                y = Mathf.Lerp(newMin.y, newMax.y, y);
                z = Mathf.Lerp(newMin.z, newMax.z, z);

                Nodes[i].Vert = new Vector3(x, y, z);
            }

            for (int i = 0; i < Cells.Count; i++)
            {
                Cells[i].Resize();
            }



            _bounds = bounds;
        }

        public int ClosestIndex(Vector3 testPoint)
        {
            if (!_bounds.Contains(testPoint))
            {
                testPoint = _bounds.ClosestPoint(testPoint);
            }

            var min = _bounds.min;
            var max = _bounds.max;

            var lengthX = _buckets.GetLength(0);
            var lengthY = _buckets.GetLength(1);
            var lengthZ = _buckets.GetLength(2);

            var pointX = Mathf.FloorToInt(Mathf.InverseLerp(min.x, max.x, testPoint.x) * (lengthX-1));
            var pointY = Mathf.FloorToInt(Mathf.InverseLerp(min.y, max.y, testPoint.y) * (lengthY-1));
            var pointZ = Mathf.FloorToInt(Mathf.InverseLerp(min.z, max.z, testPoint.z) * (lengthZ-1));

            var minDist = float.MaxValue;
            SmartNode node = null;

            for (int x = pointX; x < Mathf.Min(lengthX,pointX+2); x++)
            {
                for (int y = pointY; y < Mathf.Min(lengthY, pointY + 2); y++)
                {
                    for (int z = pointZ; z < Mathf.Min(lengthZ, pointZ + 2); z++)
                    {
                        for (int i = 0; i < _buckets[x,y,z].Count; i++)
                        {
                            var dist = Vector3.Distance(testPoint, _buckets[x, y, z][i].Vert);
                            if(dist < minDist)
                            {
                                minDist = dist;
                                node = _buckets[x, y, z][i];
                            }
                        }
                    }
                }
            }

            if (node == null)
            {
                Debug.Log("I'm in hell");
            }

            //Debug.DrawLine(testPoint, node.Vert, Color.green, 100f);

            return node.Index;
        }

        public void CreateLineConnections(int[] tris,Dictionary<Vector2Int, SmartLine> halfEdges, SmartCell cell, int a, int b)
        {
            var halfEdge = new Vector2Int(tris[a], tris[b]);
            var inverseHalfEdge = new Vector2Int(halfEdge.y, halfEdge.x);

            if (halfEdges.ContainsKey(inverseHalfEdge))
            {
                var line = halfEdges[inverseHalfEdge];
                line.AddNeighbour(cell);
                cell.AddLine(line);

                halfEdges.Remove(inverseHalfEdge);
            }
            else
            {
                var line = new SmartLine(Nodes[tris[a]], Nodes[tris[b]]);
                cell.AddLine(line);
                Lines.Add(line);
                line.AddNeighbour(cell);

                halfEdges.Add(halfEdge, line);
            }
        }

        void CreateBuckets(int divX, int divY, int divZ)
        {
            _buckets = new List<SmartNode>[divX, divY, divZ];

            for (int x = 0; x < divX; x++)
            {
                for (int y = 0; y < divY; y++)
                {
                    for (int z = 0; z < divZ; z++)
                    {
                        _buckets[x, y, z] = new List<SmartNode>();
                    }
                }
            }



            var min = _bounds.min;
            var max = _bounds.max;

            for (int i = 0; i < Nodes.Count; i++)
            {
                var n = Nodes[i].Vert;

                var x = Mathf.InverseLerp(min.x, max.x, n.x) * (divX-1);
                var y = Mathf.InverseLerp(min.y, max.y, n.y) * (divY-1);
                var z = Mathf.InverseLerp(min.z, max.z, n.z) * (divZ-1);

                var intX = Mathf.RoundToInt(x);
                var intY = Mathf.RoundToInt(y);
                var intZ = Mathf.RoundToInt(z);


                    _buckets[intX, intY, intZ].Add(Nodes[i]);


                
            }
        }

        internal class aStarHelper {
            public float gCost;
            public float hCost;
            public float fCost { get { return gCost + hCost; } }
            public int parent;
        }

        public MeshState<T> CreateMeshState<T>()
        {
            return new MeshState<T>()
            {
                Nodes = new T[this.Nodes.Count],
                Cells = new T[this.Cells.Count],
                Lines = new T[this.Lines.Count],
            };
        }


    }
}  
