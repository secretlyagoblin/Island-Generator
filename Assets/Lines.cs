using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maps;

public class Lines : MonoBehaviour {

    public AnimationCurve MyCurve;

    // Use this for initialization
    void Start()
    {

        RNG.DateTimeInit();

        var size = 200;

        var walkableAreaMap = new Map(size, size);
        var stack = Map.SetGlobalDisplayStack();

        walkableAreaMap.FillWithBoolNoise(0.5f, 0, 0)
            .ApplyMask(Map.BlankMap(walkableAreaMap)
                    .CreateCircularFalloff(size * 0.45f)
                    .SetRow((size / 2) - 1, 0)
                    .SetRow((size / 2) + 1, 0)
                    .SetRow((size / 2) - 2, 0)
                    .SetRow((size / 2) + 2, 0)
                    )
            .SetRow(size / 2, 0)
            //.Display()
            .BoolSmoothOperation(4)
            .RemoveSmallRegions(600)
            .Invert()
            .RemoveSmallRegions(300)
            .Invert();
            //.Display();

        var smalSize = 30;

        var topLeft = new Map(smalSize + 1, smalSize + 1);

        var seed = RNG.Next(10000);

        topLeft = topLeft
            .SetRow(0, 1)
            .SetColumn(0, 1)
            .SetIndex(smalSize, smalSize, 1)
            //.Display()
            .GetDistanceMap(smalSize / 2)
            //.Display()
            .Resize(size / 2, size / 2)
            //.Display()
            .Normalise();
            //.Add(Map.BlankMap(walkableAreaMap).FillWithBoolNoise().Normalise())
            //.Display();

        var topRight = new Map(smalSize + 1, smalSize + 1);
        topRight = topRight
            //.SetColumn(0, 1)
            .SetRow(0, 1)
            .SetIndex(smalSize, smalSize, 1)
            .SetIndex(smalSize, 0, 1)
            //.Display()
            .GetDistanceMap(smalSize / 2)
            //.Display()
            .Resize(size / 2, size / 2)
            //.Display()
            .Normalise();

        var bottomLeft = new Map(smalSize + 1, smalSize + 1);
        bottomLeft = bottomLeft
            //.SetColumn(0, 1)
            .SetRow(smalSize, 1)
            .SetIndex(0, 0, 1)
            .SetIndex(0, smalSize, 1)
            //.Display()
            .GetDistanceMap(smalSize / 2)
            //.Display()
            .Resize(size / 2, size / 2)
            //.Display()
            .Normalise();

        var bottomRight = new Map(smalSize + 1, smalSize + 1);
        bottomRight = bottomRight
            .SetColumn(smalSize, 1)
            //.SetRow(smalSize, 1)
            .SetIndex(0, 0, 1)
            .SetIndex(smalSize, 0, 1)
            //.Display()
            .GetDistanceMap(smalSize / 2)
            //.Display()
            .Resize(size / 2, size / 2)
            //.Display()
            .Normalise();

        var finalMap = Map.BlankMap(size, size)
            .FillWith(0)
            .ApplyMap(topLeft, new Coord(0, 0))
            .ApplyMap(topRight, new Coord(0, (int)(size * 0.5f)))
            .ApplyMap(bottomLeft, new Coord((int)(size * 0.5f), 0))
            .ApplyMap(bottomRight, new Coord((int)(size * 0.5f), (int)(size * 0.5f)))
            //.Display()
            .Add(Map.BlankMap(walkableAreaMap).PerlinFill(size * 0.15f, 1, 0, seed).Remap(-0.25f, 0.25f))
            .Clamp(-0.25f, 1)
            .Normalise()
            .Remap(MyCurve);
            //.Display()
            //BooleanMapFromThreshold(0.5f)
            //.Display();


        var mesh = MeshMasher.DelaunayGen.GetMeshFromMap(finalMap, 0.06f);
        //mesh = MeshMasher.AutoWelder.AutoWeld(mesh, 0.001f, 0.01f);
        var smartmesh = new MeshMasher.SmartMesh(mesh);

        smartmesh.DrawMesh(transform);

        var meshState = smartmesh.MinimumSpanningTree();
        //var other = smartmesh.InvertLineSelection(lines);

        for (int i = 0; i < smartmesh.Lines.Count; i++)
        {
            smartmesh.Lines[i].DrawLine(Color.blue, 100f);
        }

        var walkState = smartmesh.WalkThroughRooms(meshState.Clone());
        var roomState = smartmesh.CalculateRooms(meshState.Clone());
        meshState = smartmesh.RemoveLargeRooms(meshState, roomState, walkState, 8);

        for (int i = 0; i < smartmesh.Cells.Count; i++)
        {
            //smartmesh.Lines[i].DrawLine(Color.green, 100f);

            for (int u = 0; u < smartmesh.Cells[i].Neighbours.Count; u++)
            {
                var border = smartmesh.Cells[i].GetSharedBorder(smartmesh.Cells[i].Neighbours[u]);
                if (border == null)
                    continue;
                if (meshState.Lines[border.Index] == 1)
                    continue;

                var colour = (walkState.Cells[smartmesh.Cells[i].Index] + walkState.Cells[smartmesh.Cells[i].Neighbours[u].Index]) *0.5f;

                var num = Mathf.Sin(colour*0.2345f);
                num = (num + 1)*0.5f;

                Debug.DrawLine(smartmesh.Cells[i].Center, smartmesh.Cells[i].Neighbours[u].Center,new Color(num, 1f-num, 0,1f),100f);
            }   

             


        }

        //lines = smartmesh.CullToSingleLine(lines);



        for (int i = 0; i < meshState.Lines.Length; i++)
        {
            if(meshState.Lines[i] > 0)
            smartmesh.Lines[i].DrawLine(Color.white, 100f);
        }

        mesh = smartmesh.BuildMeshSurfaceWithCliffs(meshState);

        var heightMap = Map.Clone(finalMap).GetHeightmapFromSquareXZMesh(mesh).SmoothMap(3).Display();

        var walkableMap = heightMap.Clone().Normalise().GetAbsoluteBumpMap().Display().Normalise().BooleanMapFromThreshold(0.15f).Display();


        /*

        smal= smal.SetColumn(0, 1)
            .SetRow(0, 1)
            .SetIndex(smalSize, smalSize, 1)
            .Display()
            .GetDistanceMap(smalSize / 2)
            .Display()
            .Resize(size, size)
            .Display()
            .Normalise()
            //.Add(Map.BlankMap(walkableAreaMap).FillWithBoolNoise().Normalise())
            .Add(Map.BlankMap(walkableAreaMap).PerlinFill(size*0.15f,0,0,RNG.Next(10000)).Remap(-0.25f,0.25f))
            .Normalise()
            .Display()
            .BooleanMapFromThreshold(0.5f)
            .Display();

    */

        /*

        var guff = Map.BlankMap(walkableAreaMap)
            .FillWithBoolNoise(0.49f, 0, 0)
            .ApplyMask(smal)
            .Display()
            .BoolSmoothOperation(4)
            .Display()
            .RemoveSmallRegions(600)
            .Invert()
            //.RemoveSmallRegions(50)
            .Invert()
            .AddRoomLogic()
            .Display();

    */



        stack.CreateDebugStack(transform);




    }

    // Update is called once per frame
    void Update () {
		
	}
}
