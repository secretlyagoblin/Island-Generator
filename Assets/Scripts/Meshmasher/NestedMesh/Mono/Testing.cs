using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;
using MeshMasher.NodeDataTypes;
using System.Linq;
using System;

public class Testing : MonoBehaviour {

    public GameObject InstantiationBase;
    /*
    void Start()
    {
        RNG.DateTimeInit();

        var tiles = new List<Vector2Int>() { Vector2Int.zero };
        var nestedMesh = new NestedMesh(tiles.ToArray());
        var smartMesh = new SmartMesh(nestedMesh.CreateMesh());

        //smartMesh.DrawMesh(transform,Color.blue,Color.cyan);

        var cellIndexes = new List<int>();

        //var choices = new int[] { 33, 38, 39, 41, 43, 45, 46, 48, 50, 52, 53, 54, 55, 57, 59, 60, 62, 63, 64, 65, 67, 68, 69, 70, 71, 72, 73, 74, 79, 80, 81, 82, 83, 85, 86, 87, 88, 89, 91, 92, 93, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 106, 109, 112, 113, 114, 115, 117, 118, 119, 120, 122, 123, 124, 125, 127, 131, 132, 134, 135, 136, 137, 138, 139, 142, 143, 144, 145, 146, 147, 148, 150, 152, 153, 154, 155, 156, 158, 161, 162, 163, 164, 165, 167, 168, 169, 170, 171, 173, 175, 177, 178, 179, 180, 182, 183, 184, 185, 186, 188, 189, 190, 192, 193, 194, 195, 196, 198 };

        //var cell = smartMesh.Cells[RNG.GetRandomItem(choices)];

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
        var roomCodes = new RoomCode[] { new RoomCode(0), new RoomCode(1), new RoomCode(2), new RoomCode(3) };

        var roomColorDict = new Dictionary<int, int>();

        roomColorDict.Add(cell.Nodes[0].Index, 1);
        roomColorDict.Add(cell.Nodes[1].Index, 2);
        roomColorDict.Add(cell.Nodes[2].Index, 3);


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
                if (roomColorDict.ContainsKey(n.Index))
                {
                    //Debug.Log("...and was a dup!");
                    roomIndexes.Add(roomColorDict[n.Index]);
                }
            
                else
                    roomIndexes.Add(0);
            }
        }

        var indexes = cellIndexes.ToArray();

        var innerMesh = new NestedMesh(nestedMesh, indexes);
        var innerRoomColors = nestedMesh.LerpBarycentricValues(roomIndexes.ToArray(), roomColors, indexes);
        var innerRoomCodes = nestedMesh.LerpBarycentricValues(roomIndexes.ToArray(), roomCodes, indexes);

        var innerSmartMesh = new SmartMesh(innerMesh.CreateMesh());

        var threepoints = cell.Nodes.Select(x => innerSmartMesh.ClosestIndex(x.Vert)).ToArray();
        var finalInts = new List<int>();

        finalInts.AddRange(innerSmartMesh.ShortestWalkNode(threepoints[0], threepoints[1]));
        finalInts.AddRange(innerSmartMesh.ShortestWalkNode(threepoints[1], threepoints[2]));
        finalInts.AddRange(innerSmartMesh.ShortestWalkNode(threepoints[2], threepoints[0]));


        var ints = new int[innerSmartMesh.Nodes.Count];

        for (int i = 0; i < finalInts.Count; i++)
        {
            ints[finalInts[i]] = 1;
        }



        innerSmartMesh.DrawMesh(transform, Color.yellow,Color.magenta);

        for (int i = 0; i < innerSmartMesh.Lines.Count; i++)
        {
            var l = innerSmartMesh.Lines[i];

            if (l.Neighbours.Count == 1)
            {
                l.DebugDraw(Color.white, 100f);
            }
            else if (innerRoomColors[l.Nodes[0].Index].Value == innerRoomColors[l.Nodes[1].Index].Value)
            {
                //l.DebugDraw(innerRoomColors[l.Nodes[0].Index].Value, 100f);
            }

            if (ints[l.Nodes[0].Index] == ints[l.Nodes[1].Index])
            {
                if (ints[l.Nodes[0].Index] == 1)
                {

                    l.DebugDraw(Color.red, 100f);
                }

            }
        }


        for (int i = 0; i < innerSmartMesh.Nodes.Count; i++)
        {
            //var n = innerSmartMesh.Nodes[i].Index;

            if (innerRoomCodes[i].Value == 0)
                continue;

            var obj = Instantiate(InstantiationBase);
            obj.GetComponent<MeshRenderer>().material.color = innerRoomColors[i].Value;
            obj.transform.position = innerSmartMesh.Nodes[i].Vert;
        }

        return;

       

        for (int v = 1; v < roomCodes.Length; v++)
        {
            var innerInnerNodes = new List<int>();
            var roomCode = roomCodes[v].Value;
            for (int i = 0; i < innerSmartMesh.Cells.Count; i++)
            {
                var c = innerSmartMesh.Cells[i];

                var trueCount = 0;

                if (innerRoomCodes[c.Nodes[0].Index].Value == roomCode)
                    trueCount++;
                if (innerRoomCodes[c.Nodes[1].Index].Value == roomCode)
                    trueCount++;
                if (innerRoomCodes[c.Nodes[2].Index].Value == roomCode)
                    trueCount++;

                if(trueCount>1)
                {
                    innerInnerNodes.Add(c.Index);
                }
            }

            var m = new NestedMesh(innerMesh, innerInnerNodes.ToArray());
            var fMesh = new SmartMesh(m.CreateMesh());
            fMesh.DrawMesh(transform, roomColors[v].Value,Color.white);
            //fMesh.DrawRoomOutlines
        }







        //innerSmartMesh.BubbleMesh(1);

        //smartMesh.DrawMesh(transform);
    }
    */


}

