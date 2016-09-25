using UnityEngine;
using System.Collections.Generic;

using Nurbz;

namespace BuildingGenerator {

    public class BlueprintBuilder {

        List<Node> _nodes = new List<Node>();
        List<Edge> _edges = new List<Edge>();
        List<Plate> _plates = new List<Plate>();

        List<Room> _rooms = new List<Room>();

        public BlueprintBuilder(Vector3[] verts, IndexCollection[] outlines, IndexCollection[] cellNeighbourhoods)
        {
            //create building nodes
            for (int i = 0; i < verts.Length; i++)
            {
                _nodes.Add(new Node(verts[i], i));
            }

            
            foreach (var outline in outlines)
            {
                var plate = new Plate();

                foreach (var index in outline.contents)
                {
                    plate.AddNode(_nodes[index]);
                }
                plate.Bake();
                _plates.Add(plate);
            }

            for (var i = 0; i < cellNeighbourhoods.Length; i++)
            {
                var mainPlate = _plates[i];

                foreach (var index in cellNeighbourhoods[i].contents)
                {
                    var otherPlate = _plates[index];
                    _edges.AddRange(mainPlate.FindSharedEdges(otherPlate, _nodes));
                }
            }
        }

        List<Vector3> _verts = new List<Vector3>();
        List<int> _tris = new List<int>();

        public BlueprintBuilder(Mesh meshFloorplate)
        {
            _verts.AddRange(meshFloorplate.vertices);
            _tris.AddRange(meshFloorplate.triangles);

            for (int i = 0; i < _tris.Count; i+=3)
            {

            }
        }

        public bool AddRoom(List<int> triangleIndexList)
        {
            return true;
        }

    }

    public class Node {
        public Vector3 Position
        {
            get; private set;
        }

        public int Index
        {
            get; private set;
        }


        public Node(Vector3 position, int index)
        {
            Position = position;
            Index = index;
            Edges = new List<Edge>();
        }

        public List<Edge> Edges
        {
            get; private set;
        }

        public void AddEdge(Edge edge)
        {
            Edges.Add(edge);
        }
    }

    public class Edge {

        public Node Start
        {
            get; private set;
        }

        public Node End
        {
            get; private set;
        }

        public Line3 Line
        {
            get; private set;
        }

        public List<Plate> Partners
        { get; private set; }

        public Edge(Node start, Node end, Plate plate1, Plate plate2)
        {
            Start = start;
            End = end;

            Partners = new List<Plate>();

            Partners.Add(plate1);
            Partners.Add(plate2);

            Line = new Line3(start.Position, end.Position);
        }
    }

    public class Plate {
        public List<Node> nodes
        { get; private set; }
        private bool _linesGenerated = false;
        private IndexPair[] _lines;
        public List<IndexPair> FreeLines;

        public Plate()
        {
            nodes = new List<Node>();
        }

        public void AddNode(Node node)
        {
            nodes.Add(node);
            _linesGenerated = false;

        }

        public Polyline3 getPolyline()
        {
            var points = new List<Vector3>();

            foreach (var node in nodes)
            {
                points.Add(node.Position);
            }

            return new Polyline3(points.ToArray(), true);
        }

        public void Bake()
        {

            if (!_linesGenerated)
            {

                _lines = getIndexPair();
                _linesGenerated = true;
                FreeLines = new List<IndexPair>(_lines);
            }
            else
            {
            }
        }

        IndexPair[] getIndexPair()

        {
            var lineList = new List<IndexPair>();
            for (var x = 0; x < nodes.Count - 1; x++)
            {
                lineList.Add(new IndexPair(nodes[x].Index, nodes[x + 1].Index));
            };

            lineList.Add(new IndexPair(nodes[nodes.Count - 1].Index, nodes[0].Index));

            return lineList.ToArray();
        }

        public List<Edge> FindSharedEdges(Plate plateToCompare, List<Node> nodes)
        {
            var outputList = new List<Edge>(); //Rework so that this does edgezzz

            var indexesToRemoveLocal = new List<int>();
            var indexesToRemoveOther = new List<int>();

            var indexCount = 0;

            for (var u = 0; u < FreeLines.Count; u++)
            {
                indexCount = 0;
                foreach (var index in indexesToRemoveOther)
                {
                    plateToCompare.FreeLines.RemoveAt(index - indexCount);
                    indexCount++;
                }

                for (var v = 0; v < plateToCompare.FreeLines.Count; v++)

                {
                    if (FreeLines[u].a == plateToCompare.FreeLines[v].a && FreeLines[u].b == plateToCompare.FreeLines[v].b)
                    {
                        indexesToRemoveLocal.Add(u);
                        indexesToRemoveOther.Add(v);
                        var edge = new Edge(nodes[FreeLines[u].a], nodes[FreeLines[u].b], this, plateToCompare);
                        nodes[FreeLines[u].a].AddEdge(edge);
                        nodes[FreeLines[u].b].AddEdge(edge);
                        outputList.Add(edge);
                        goto NextLoop;
                    }
                }

            NextLoop:
                continue;

            }

            indexCount = 0;

            foreach (var index in indexesToRemoveLocal)
            {
                FreeLines.RemoveAt(index - indexCount);
                indexCount++;
            }

            return outputList;
        }

    }

    public class Room {

    }

    public struct IndexCollection {
        public int[] contents
        { get; private set; }

        public IndexCollection(int[] guh)
        {
            contents = guh;
        }
    }

    public struct IndexPair {
        public int a;
        public int b;

        public IndexPair(int a, int b)
        {
            this.a = a;
            this.b = b;
        }
    }
}