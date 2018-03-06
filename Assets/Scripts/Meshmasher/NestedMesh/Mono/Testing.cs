using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshMasher;
using System.Linq;

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
                cellIndexes.Add(node.Cells[u].Index);
            }
        }

        var innerMesh = new NestedMesh(nestedMesh, cellIndexes.Distinct().ToArray());

        var innerSmartMesh = new SmartMesh(innerMesh.CreateMesh());

        //innerSmartMesh.DrawMesh(transform, Color.yellow,Color.magenta);

        for (int i = 0; i < innerSmartMesh.Lines.Count; i++)
        {
            if(innerSmartMesh.Lines[i].Neighbours[1] == null)
            {
                innerSmartMesh.Lines[i].Neighbours[0].DebugDraw(Color.white, 100f);
            }
        }



        //innerSmartMesh.BubbleMesh(1);

        //smartMesh.DrawMesh(transform);
    }
}

