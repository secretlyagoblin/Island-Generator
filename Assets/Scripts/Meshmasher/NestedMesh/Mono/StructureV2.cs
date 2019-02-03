using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using U3D.Threading.Tasks;

public class StructureV2 : MonoBehaviour {


    public GameObject InstantiationBase;
    public AnimationCurve FalloffCurve;
    public Gradient Gradient;
    public TextAsset meshTileData;

    public Material MeshColourMaterial;
    public GameObject BlankTemplate;

    public bool EnablePreview = true;
    public bool RunAsync = false;

    Material _mat;

    Queue<CleverMesh> _workQueue = new Queue<CleverMesh>();

    // Use this for initialization
    void Start()
    {
        RNG.DateTimeInit();

        _mat = new Material(Shader.Find("Standard"));

        //MainTest();
        SingleTest();
    }

    private void Update()
    {
        var count = 0;

        //Debug.Log("Work Queue = " + _workQueue.Count);

        while (_workQueue.Count > 0 && count < 3)
        {
            count++;
            CreateObject(_workQueue.Dequeue());
        }
    }

    IEnumerator CreateBit(CleverMesh mesh, int chunkSize)
    {
        var mate = new Material(Shader.Find("Standard"));

        var waitTime = new WaitForSeconds(0.1f);

        var current = 0;

        for (int i = 0; i < mesh.Mesh.Cells.Count; i++)
        {
            var slayer4 = new CleverMesh(mesh, mesh.Mesh.Cells[i].Index, MeshMasher.NestedMeshAccessType.Triangles);

            var gameObjecte = new GameObject();
            var fe = gameObjecte.AddComponent<MeshFilter>();
            var re = gameObjecte.AddComponent<MeshRenderer>();
            re.sharedMaterial = mate;
            //f.mesh = layer5.Mesh.ToXYMesh();
            fe.mesh = slayer4.Mesh.ToXYMesh();
            fe.name = "Cell " + i;

            current++;

            if (current >= chunkSize)
            {
                current = 0;
                yield return null;
            }

        }
    }

    public void MainTest()
    {
        var colors = new Color[] { Color.red, Color.green, Color.yellow };

        ///Assumptions:
        /// Currently not considering height differences
        /// Pathfinding could be better
        /// Biome just means colours at this point

        ///Below we:
        /// 1: Create a single triangle
        /// 2: Give each triangle a different biome (3 zones)

        #region layer one

        /// 1: Create a single triangle

        var layer1 = new CleverMesh(new List<Vector2Int>() { Vector2Int.zero }, new MeshTile(meshTileData.text));
        var cellIndex = 2;

        /// 2: Give each triangle a different biome (3 zones)
        for (int i = 0; i < layer1.Mesh.Cells[cellIndex].Nodes.Count; i++)
        {
            var n = layer1.Mesh.Cells[cellIndex].Nodes[i];
            layer1.NodeMetadata[n.Index] = new NodeMetadata(i + 1, colors[i], new int[] { }, RNG.NextFloat(5));
        }

        #endregion

        ///Below we:
        /// 1: Create a boundary area that is a no-go zone.
        /// 2: Calculate a connectivity graph between regions (TODO: fix distance to be based on distance from node center based on layer 1)
        /// 3: Give each walkable node a special room code
        /// 4: Give each walkable node a connectivity map
        /// 5: TODO: Create mini-valleys using voronoi falloff where connectivity should be broken.
        /// 6: TODO: Define higher level biomes based on parent colour

        #region layer two

        var layer2 = new CleverMesh(layer1, layer1.Mesh.Cells[cellIndex].GetNeighbourhood(), MeshMasher.NestedMeshAccessType.Vertex);

        // 1: Create a boundary area that is a no-go zone.

        var layer2Border = layer2.Mesh.GetBorderNodes();

        for (int i = 0; i < layer2Border.Nodes.Length; i++)
        {
            if (layer2Border.Nodes[i] == 1)
                layer2.NodeMetadata[i].Code = 0;
        }

        // 2: Calculate a connectivity graph between regions (TODO: fix distance to be based on distance from node center based on layer 1)

        var layer2State = layer2.Mesh.GenerateSemiConnectedMesh(5, layer2Border);

        // 3: Give each walkable node a special room code and color

        var roomNumber = 1;

        for (int i = 0; i < layer2.NodeMetadata.Length; i++)
        {
            if (layer2.NodeMetadata[i].Code != 0)
            {
                layer2.NodeMetadata[i].Code = roomNumber;
                roomNumber++;
                //layer2.CellMetadata[i].SmoothColor = Color.white;
            }
            else
            {
                //layer2.CellMetadata[i].Code = 0;
                layer2.NodeMetadata[i].SmoothColor = Color.black;
            }
        }

        // 4: TODO: Give each walkable node a connectivity map

        for (int i = 0; i < layer2.Mesh.Nodes.Count; i++)
        {
            var n = layer2.Mesh.Nodes[i];

            if (layer2Border.Nodes[n.Index] == 1 | layer2.NodeMetadata[i].Code == 0)
                continue;

            layer2.NodeMetadata[n.Index].Height += RNG.NextFloat(-0.5f, 0.5f);
            layer2.NodeMetadata[n.Index].Connections = n
                .Lines
                .Where(x => layer2State.Lines[x.Index] == 1)
                .Select(x => x.GetOtherNode(n).Index + 1)
                .Union(new List<int>() { i + 1 })
                .ToArray();
            layer2.NodeMetadata[n.Index].Code = i + 1;
        }

        //CreateObject(layer2);

        #endregion

        ///Below we:
        /// 6: TODO: Identify key paths in and out of layer 3 regions
        /// 7: TODO: Define walkable paths, areas of interest
        /// 8: TODO: Define walkable/non walkable area for layer 4
        /// 9: TODO: Define tighter biomes, based more on creating variety at microscale

        #region layer three

        var layer3 = new CleverMesh(layer2, layer2.Mesh.Cells.Select(x => x.Index).ToArray(), MeshMasher.NestedMeshAccessType.Triangles);

        for (int i = 0; i < layer3.Mesh.Nodes.Count; i++)
        {
            var n = layer3.Mesh.Nodes[i];

            if (layer3.NodeMetadata[n.Index].Code == 0)
            {
                layer3.NodeMetadata[n.Index].SmoothColor = Color.black;
                continue;
            }

            var colour = layer3.NodeMetadata[n.Index].MeshDual;
            colour = Mathf.Max(layer3.NodeMetadata[n.Index].Distance, colour);
            //layer3.CellMetadata[n.Index].Code = i + 1;
            layer3.NodeMetadata[n.Index].SmoothColor = layer3.NodeMetadata[n.Index].Distance < 0.5f ? Color.black : layer3.NodeMetadata[n.Index].SmoothColor;
            //layer3.CellMetadata[n.Index].SmoothColor = new Color(colour, colour, colour);
            layer3.NodeMetadata[n.Index].Height += RNG.NextFloat(-0.1f, 0.1f);
        }

        //CreateObject(layer3);

        #endregion

        ///Below we:
        /// 10: TODO: Actually start only generating this stuff based on distance
        /// 11: TODO: Build an actual heightmesh
        /// 11.5: TODO: Create Triangle-focused nestedmesh that creates seamless terrains
        /// 12: TODO: Instantiate large props

        #region layer four

        //layer3.Mesh.DrawMesh(transform);

        var cellDicts = new Dictionary<int, List<int>>();

        //for (int i = 0; i < layer3.Mesh.Cells.Count; i++)
        //{
        //    var code = layer3.CellMetadata[i].Code;
        //
        //    if (cellDicts.ContainsKey(code))
        //    {
        //        cellDicts[code].Add(i);
        //    }
        //    else
        //    {
        //        cellDicts.Add(code, new List<int>() { i });
        //    }
        //}
        //
        //if (RunAsync)
        //{
        //    CreateSetAsync(layer3, cellDicts, 1);
        //}
        //else
        //{
        //    StartCoroutine(CreateSet(layer3, cellDicts, 0, 1));
        //}

        return;




        //foreach (var roomCode in cellDicts)
        //{
        //    var layer4 = new CleverMesh(layer3, roomCode.Value.Distinct().ToArray(), MeshMasher.NestedMeshAccessType.Triangles);
        //
        //    var go = CreateObject(layer4);
        //    go.name = "Region " +roomCode.Key;
        //}



        //var layer4 = 
        //layer4.Mesh.DrawMesh(transform, Color.green, Color.red);

        #endregion

        ///Below we:
        /// 13: TODO: Instantiate smaller props

        #region create final mesh

        //var finalLayer = layer4;
        //var pts = finalLayer.Mesh.Nodes;
        //
        //for (int i = 0; i < pts.Count; i++)
        //{
        //    //if (layer4.CellMetadata[i].Code == 0 )//|| layer4.CellMetadata[i].Distance < 0.5)
        //    //    continue;
        //
        //    //var jitter = RNG.NextFloat(0.1f);
        //    //
        //    //var smoothVal = FalloffCurve.Evaluate(layer4.CellMetadata[i].SmoothColor.r+jitter);
        //    //
        //    //var color = Gradient.Evaluate(smoothVal);
        //    var n = finalLayer.Mesh.Nodes[i];
        //   // var colour = finalLayer.CellMetadata[n.Index].
        //
        //    var obj = Instantiate(InstantiationBase);
        //    //obj.GetComponent<MeshRenderer>().sharedMaterial = layer4.CellMetadata[i].Distance < 0.8 | layer4.CellMetadata[i].Code == 0 ? matA : matB;
        //    obj.GetComponent<MeshRenderer>().material.color = finalLayer.CellMetadata[i].SmoothColor;
        //    obj.transform.position = pts[i].Vert + Vector3.forward * finalLayer.CellMetadata[i].Height;
        //    //obj.transform.position = new Vector3(obj.transform.position.x, -obj.transform.position.z, obj.transform.position.y);
        //    obj.transform.localScale = Vector3.one * 0.06f;
        //    obj.name = "Room " + finalLayer.CellMetadata[i].Code;
        //}

        #endregion

        if (EnablePreview)
        {
            //layer3.Mesh.DrawMesh(transform, Color.grey, Color.clear);
            //layer2.Mesh.DrawMesh(transform, Color.blue, Color.clear);
            layer1.Mesh.DrawMesh(transform, Color.clear, Color.white);

            // Preview Border

            for (int i = 0; i < layer2Border.Lines.Length; i++)
            {
                if (layer2Border.Lines[i] == 1)
                    layer2.Mesh.Lines[i].DebugDraw(Color.green, 100f);
                else if (layer2State.Lines[i] == 1 &&
                    layer2.NodeMetadata[layer2.Mesh.Lines[i].Nodes[0].Index].Code != 0 &&
                    layer2.NodeMetadata[layer2.Mesh.Lines[i].Nodes[1].Index].Code != 0)
                {

                    //layer2.Mesh.Lines[i].DebugDraw(Color.red, 100f);
                }
                else
                {
                    //layer2.Mesh.Lines[i].DebugDraw(Color.white, 100f);
                }
            }
        }
    }

    public void OffsetTest()
    {
        var cellIndex = 2;
        var colors = new Color[] { Color.red, Color.green, Color.blue };

        Debug.Log("Layer 1: ");

        var layer1 = new CleverMesh(new List<Vector2Int>() { Vector2Int.zero }, new MeshTile(meshTileData.text));
        layer1.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);

        for (int i = 0; i < layer1.Mesh.Cells[cellIndex].Nodes.Count; i++)
        {
            var n = layer1.Mesh.Cells[cellIndex].Nodes[i];
            layer1.NodeMetadata[n.Index] = new NodeMetadata(i + 1, colors[i], new int[] { }, RNG.NextFloat(5));
        }

        Debug.Log("Layer 2: ");

        var hood = layer1.Mesh.Cells[cellIndex].GetNeighbourhood();

        //for (int i = 0; i < hood.Length; i++)
        //{
        //    var layer2 = new CleverMesh(layer1, hood[i], MeshMasher.NestedMeshAccessType.Vertex);
        //    layer2.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);
        //    CreateObject(layer2).name = "Layer2";
        //}
        //
        //for (int i = 0; i < hood.Length; i++)
        //{
        //    var layer2 = new CleverMesh(layer1, hood[i], MeshMasher.NestedMeshAccessType.Triangles);
        //    layer2.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);
        //    CreateObject(layer2).name = "Layer2 - " + i;
        //}


        //var layer2 = new CleverMesh(layer1, layer1.Mesh.Cells[cellIndex].GetNeighbourhood(), MeshMasher.NestedMeshAccessType.Vertex);
        //
        //CreateObject(layer2).name = "Layer2";

        var layer2 = new CleverMesh(layer1, layer1.Mesh.Cells[cellIndex].GetNeighbourhood(), MeshMasher.NestedMeshAccessType.Triangles);
        //layer2.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);
        CreateObject(layer2).name = "Layer2";




        Debug.Log("Layer 3: ");

        var layer3 = new CleverMesh(layer2, layer2.Mesh.Cells.Select(x => x.Index).ToArray(), MeshMasher.NestedMeshAccessType.Vertex);
        //layer3.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);

        var go = CreateObject(layer3);
        go.name = "Layer3";
        //go.transform.Translate(Vector3.back);

        Debug.Log("Layer 4: ");

        for (int i = 0; i < layer3.Mesh.Cells.Count; i++)
        {
            var layer4 = new CleverMesh(layer3, new int[] { layer3.Mesh.Cells[i].Index }, MeshMasher.NestedMeshAccessType.Triangles);
            go = CreateObject(layer4);
            go.name = "Cell " + i;
        }

        /*

        var layer4 = new CleverMesh(layer3, new int[] { layer3.Mesh.Cells[0].Index }, MeshMasher.NestedMeshAccessType.Vertex);
        layer4.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);

        CreateObject(layer4).name = "Layer4";

        Debug.Log("Layer 5: ");

        var layer5 = new CleverMesh(layer4, new int[] { layer4.Mesh.Cells[0].Index }, MeshMasher.NestedMeshAccessType.Vertex);
        layer5.Mesh.DrawMesh(transform, RNG.GetRandomColor(), Color.clear);

    */

        //CreateObject(layer4).name = "Cell " + i;

    }

    public void SingleTest()
    {
        //var cellIndex = 159; //<- known error with edge
        var cellIndex = 155; //<- known error with edge
        //var cellIndex = 133; //<- known error with colour bleeding
        //var cellIndex = 122; //<- known error with edge
        var colors = new Color[] { Color.red, Color.green, Color.blue };

        Debug.Log("Layer 1: ");

        var layer1 = new CleverMesh(new List<Vector2Int>() { Vector2Int.zero, Vector2Int.right, Vector2Int.one }, new MeshTile(meshTileData.text));

        //CreateObject(layer1);

        var neighbourhood = layer1.Mesh.Nodes[cellIndex].Nodes.ToList().ConvertAll(x => x.Index);
        var widerNeighbourhood = neighbourhood.SelectMany(x => layer1.Mesh.Nodes[x].Nodes).Distinct().ToList().ConvertAll(x => x.Index);
        neighbourhood.Add(cellIndex);

        for (int i = 0; i < neighbourhood.Count; i++)
        {
            var n = layer1.Mesh.Nodes[neighbourhood[i]];
            layer1.NodeMetadata[n.Index] = new NodeMetadata(i + 1, RNG.GetRandomColor(), new int[] { }, RNG.NextFloat(5));
        }

        Debug.Log("Layer 2: ");

        var layer2 = new CleverMesh(layer1,
            widerNeighbourhood.ToArray(),
            //cellIndex,
            MeshMasher.NestedMeshAccessType.Vertex);

        var layer2obj = CreateObject(layer2);
        var layer2ring = CreateRing(layer2);
        layer2ring.transform.parent = layer2obj.transform;
        layer2ring.name = "Layer2ring";
        layer2obj.transform.Translate(Vector3.back);

        Debug.Log("Layer 3: ");

        //StartCoroutine(CreateSimple(layer2, MeshMasher.NestedMeshAccessType.Vertex));

        CreateSimpleJobAsync(layer2, MeshMasher.NestedMeshAccessType.Vertex);



    }

    List<Color> _createObjectColors = new List<Color>(100);

    public GameObject CreateObject(CleverMesh mesh)
    {
        var gameObject = Instantiate(BlankTemplate, transform);
        var f = gameObject.AddComponent<MeshFilter>();
        var r = gameObject.AddComponent<MeshRenderer>();
        r.sharedMaterial = MeshColourMaterial;
        //f.mesh = layer5.Mesh.ToXYMesh();
        f.mesh = mesh.Mesh.ToXYMesh();

        _createObjectColors.Clear();

        try
        {
            for (int i = 0; i < mesh.NodeMetadata.Length; i++)
            {
                _createObjectColors.Add(mesh.NodeMetadata[i].SmoothColor);
            }


            if (mesh.NodeMetadata.Length == f.mesh.vertices.Length)
            {
                f.mesh.SetColors(_createObjectColors);
            }
        }
        catch
        {
            //Debug.LogError("No colours to add");
        }



        return gameObject;
    }

    public GameObject CreateRing(CleverMesh mesh)
    {
        var gameObject = Instantiate(BlankTemplate, transform);
        var f = gameObject.AddComponent<MeshFilter>();
        var r = gameObject.AddComponent<MeshRenderer>();
        r.sharedMaterial = MeshColourMaterial;
        //f.mesh = layer5.Mesh.ToXYMesh();
        f.mesh = mesh.RingMesh.ToXYMesh();

        _createObjectColors.Clear();

        try
        {
            for (int i = 0; i < mesh.NodeMetadata.Length; i++)
            {
                _createObjectColors.Add(mesh.NodeMetadata[i].SmoothColor);
            }

            if (mesh.NodeMetadata.Length == f.mesh.vertices.Length)
            {
                f.mesh.SetColors(_createObjectColors);
            }
        }
        catch
        {
            Debug.LogError("No colours to add");
        }

        return gameObject;
    }

    public GameObject CreateBaryObject(CleverMesh mesh)
    {
        var gameObject = Instantiate(BlankTemplate, transform);
        var f = gameObject.AddComponent<MeshFilter>();
        var r = gameObject.AddComponent<MeshRenderer>();
        r.sharedMaterial = MeshColourMaterial;
        //f.mesh = layer5.Mesh.ToXYMesh();
        f.mesh = mesh.GetBarycenterDebugMesh();
        //f.mesh.SetColors(mesh.CellMetadata.Select(x => x.SmoothColor).ToList());

        return gameObject;
    }

    IEnumerator CreateSet(CleverMesh parent, Dictionary<int, List<int>> sets, float timeDelay, int batchCount)
    {
        var waitForSeconds = new WaitForSeconds(timeDelay);

        var count = 0;

        foreach (var roomCode in sets)
        {
            if (roomCode.Key == 0)
            {
                Debug.Log("Nope!!");
                continue;
            }

            try
            {
                var layer4 = new CleverMesh(parent, roomCode.Value.Distinct().ToArray(), MeshMasher.NestedMeshAccessType.Triangles);

                var go = CreateObject(layer4);
                go.name = "Region " + roomCode.Key;

            }
            catch
            {
                Debug.Log("MeshCreationFailed");
            }

            count++;

            if (count == batchCount)
            {
                count = 0;

                if (timeDelay == 0)
                {
                    yield return null;
                }
                else
                {
                    yield return waitForSeconds;
                }
            }
        }
    }

    void CreateSetAsync(CleverMesh parent, Dictionary<int, List<int>> sets, int divisions)
    {
        var queues = new Queue<KeyValuePair<int, List<int>>>[divisions];

        for (int i = 0; i < divisions; i++)
        {
            queues[i] = new Queue<KeyValuePair<int, List<int>>>();
        }

        var iterator = 0;

        foreach (var roomCode in sets)
        {
            queues[iterator].Enqueue(roomCode);
            iterator++;
            if (iterator == divisions)
                iterator = 0;
        }

        for (int i = 0; i < divisions; i++)
        {
            var q = queues[i];

            Task.Run(() =>
            {
                while (q.Count > 0)
                {
                    var roomCode = q.Dequeue();

                    if (roomCode.Key == 0)
                        continue;

                    var cleverMesh = new CleverMesh(parent, roomCode.Value.Distinct().ToArray(), MeshMasher.NestedMeshAccessType.Triangles);

                    lock (_workQueue)
                    {
                        _workQueue.Enqueue(cleverMesh);
                    }
                }
            }).ContinueInMainThreadWith((x) => { Debug.Log("Hell yeah we completed that one"); });
        }
    }

    void CreateSimpleJobAsync(CleverMesh parent, MeshMasher.NestedMeshAccessType type)
    {
        var count = parent.Mesh.Nodes.Count;

        Task.Run(() =>
        {
            for (int i = 0; i < count; i++)
            {
                var cleverMesh = new CleverMesh(parent, new int[] { parent.Mesh.Nodes[i].Index }, type);
                try
                {


                    var layer2cleverMesh = new CleverMesh(cleverMesh, cleverMesh.Mesh.Nodes.ConvertAll(x => x.Index).ToArray(), MeshMasher.NestedMeshAccessType.Triangles);

                    lock (_workQueue)
                    {
                        _workQueue.Enqueue(layer2cleverMesh);
                    }
                }
                catch (System.Exception e)
                {
                    var data = parent.GetDataAboutVertex(parent.Mesh.Nodes[i].Index);
                    Debug.Log(i + " " + data[0] + " " + data[1] + " " + data[2] + " ");
                    Debug.LogError(e);
                    //lock (_workQueue)
                    //{
                    //    _workQueue.Enqueue(cleverMesh);
                    //}
                }
            }
        });//.ContinueInMainThreadWith((x) => { Debug.Log("Task completed: " + x.IsCompleted); });

    }

    IEnumerator CreateSimple(CleverMesh parent, MeshMasher.NestedMeshAccessType type, float timeDelay = 0f)
    {
        var count = parent.Mesh.Nodes.Count;

        var waitForSeconds = new WaitForSeconds(timeDelay);

        for (int i = 0; i < count; i++)
        {

            var cleverMesh = new CleverMesh(parent, new int[] { parent.Mesh.Nodes[i].Index }, type);

            /*
            var cleverMeshMesh = CreateObject(cleverMesh);
            cleverMeshMesh.name = "Cell " + i;
            ;

            if (type == MeshMasher.NestedMeshAccessType.Vertex)
            {
                var cleverMeshRing = CreateRing(cleverMesh);
                cleverMeshRing.transform.parent = cleverMeshMesh.transform;
            }
            cleverMeshMesh.transform.Translate(Vector3.back * 0.5f);

    */
            try
            { 
                var layer2cleverMesh = new CleverMesh(cleverMesh, cleverMesh.Mesh.Nodes.ConvertAll(x => x.Index).ToArray(), MeshMasher.NestedMeshAccessType.Triangles);
                 //CreateObject(cleverMesh).name = "Cell " + i;
                 CreateObject(layer2cleverMesh).name = "Cell " + i +" - 2";
            
            }
            catch(System.Exception e)
            {
                var data = parent.GetDataAboutVertex(parent.Mesh.Nodes[i].Index);
                Debug.Log(i + " " + data[0] + " " + data[1] + " "+ data[2] + " ");
                Debug.LogError(e);
            }

            yield return waitForSeconds;
        }
    }
}