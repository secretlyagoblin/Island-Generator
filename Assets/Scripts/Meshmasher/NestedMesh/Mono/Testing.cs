using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;
using System.Linq;
using System;

public class Testing : MonoBehaviour {

    public GameObject InstantiationBase;

    void Start()
    {
        RNG.DateTimeInit();

        var tiles = new List<Vector2Int>() { Vector2Int.zero };
        var nestedMesh = new NestedMesh(tiles.ToArray());
        var smartMesh = new SmartMesh(nestedMesh.CreateMesh());

        smartMesh.DrawMesh(transform,Color.blue,Color.cyan);

        var cellIndexes = new List<int>();
        var cell = smartMesh.Cells[44];


        //cell.DebugDraw(Color.blue, 100f);

        for (int i = 0; i < cell.Nodes.Count; i++)
        {
            var node = cell.Nodes[i];

            for (int u = 0; u < node.Cells.Count; u++)
            {
                if(cellIndexes.Contains(node.Cells[u].Index))
                {
                }
                else
                {
                    cellIndexes.Add(node.Cells[u].Index);
                }                
            }
        }

        var roomIndexes = new List<int>();
        var roomColors = new RoomColor[] { new RoomColor(Color.black), new RoomColor(Color.red), new RoomColor(Color.green), new RoomColor(Color.blue) };

        var roomDict = new Dictionary<int, int>();

        roomDict.Add(cell.Nodes[0].Index, 1);
        //roomDict.Add(cell.Nodes[1].Index, 2);
        //roomDict.Add(cell.Nodes[2].Index, 3);


        //roomIndexes.Add(1);
        //roomIndexes.Add(2);
        //roomIndexes.Add(3);

        for (int i = 0; i < cellIndexes.Count; i++)
        {
            //Debug.Log("starting cell " + i);
            Debug.Log("Cell "+ smartMesh.Cells[cellIndexes[i]].Index + " has " + smartMesh.Cells[cellIndexes[i]].Nodes.Count + " children");
            for (int u = 0; u < smartMesh.Cells[cellIndexes[i]].Nodes.Count; u++)
            {
                var n = smartMesh.Cells[cellIndexes[i]].Nodes[u];
                //Debug.Log("index " + n.Index + " occured");
                if (roomDict.ContainsKey(n.Index))
                {
                    //Debug.Log("...and was a dup!");
                    roomIndexes.Add(roomDict[n.Index]);
                }
            
                else
                    roomIndexes.Add(0);
            }
        }

        var indexes = cellIndexes.ToArray();

        var innerMesh = new NestedMesh(nestedMesh, indexes);
        var innerRoomColors = nestedMesh.LerpBarycentricValues(roomIndexes.ToArray(), roomColors, indexes);

        var innerSmartMesh = new SmartMesh(innerMesh.CreateMesh());

        //innerSmartMesh.DrawMesh(transform, Color.yellow,Color.magenta);

        for (int i = 0; i < innerSmartMesh.Lines.Count; i++)
        {
            var l = innerSmartMesh.Lines[i];

            if ( l.Neighbours.Count == 1)
            {
                l.DebugDraw(Color.white, 100f);
            }
            else if (innerRoomColors[l.Nodes[0].Index].Value == innerRoomColors[l.Nodes[1].Index].Value)
            {
                //l.DebugDraw(innerRoomColors[l.Nodes[0].Index].Value, 100f);
            }
        }


        for (int i = 0; i < innerSmartMesh.Nodes.Count; i++)
        {
            //var n = innerSmartMesh.Nodes[i].Index;

            var obj = Instantiate(InstantiationBase);
            obj.GetComponent<MeshRenderer>().material.color = innerRoomColors[i].Value;
            obj.transform.position = innerSmartMesh.Nodes[i].Vert;
        }



        //innerSmartMesh.BubbleMesh(1);

        //smartMesh.DrawMesh(transform);
    }

    struct RoomCode : IBarycentricLerpable<RoomCode> {

        public int Value;

        public RoomCode(int value)
        {
            Value = value;
        }

        public RoomCode Lerp(RoomCode a, RoomCode b, RoomCode c, Vector3 weight)
        {
            if(weight.x >= weight.y && weight.x >= weight.z){
                return a;
            }
            else if (weight.y >= weight.z && weight.y >= weight.x)
            {
                return b;
            }
            else
            {
                return c;
            }
        }
    }

        struct RoomColor : IBarycentricLerpable<RoomColor> {

        public Color Value;

        public float r
        {
            get
            {
                return Value.r;
            }
            set
            {
                Value.r = value;
            }
        }

        public float g
        {
            get
            {
                return Value.g;
            }
            set
            {
                Value.g = value;
            }
        }

        public float b
        {
            get
            {
                return Value.b;
            }
            set
            {
                Value.b = value;
            }
        }

        public RoomColor(Color value)
        {
            Value = value;
        }

        public RoomColor Lerp(RoomColor a, RoomColor b, RoomColor c, Vector3 weight)
        {
            //var r = a.r * weight.x + b.r * weight.y + c.r * weight.z;
            //var g = a.g * weight.x + b.g * weight.y + c.g * weight.z;
            //var cb = a.b * weight.x + b.b * weight.y + c.b * weight.z;
            //
            //return new RoomColor(new Color(r, g, cb));

            if (weight.x >= weight.y && weight.x >= weight.z)
            {
                return a;
            }
            else if (weight.y >= weight.z && weight.y >= weight.x)
            {
                return b;
            }
            else
            {
                return c;
            }
        }
    }
}

