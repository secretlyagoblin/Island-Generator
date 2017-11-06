using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelGen2D : MonoBehaviour {

    public bool DoTheLevel = false;
    public AnimationCurve Curve;
    public AnimationCurve RadiusCurve;

    public float PerlinScale = 0.5f;

    public GameObject CirclePrefab;
    public int PropMin = 3;
    public int PropMax = 7;
    public float TimeCount;
    LineRenderer _renderer;
    Rigidbody2D[] _rigids;

    SpringJoint2D[] _joints;

    public ProcTerrainSettings Settings;
    public Rect Size;

    Bounds _bounds;
    MeshMasher.SmartMesh _mesh;

    // Use this for initialization
    void Start()
    {
        RNG.DateTimeInit();

        var points = new Vector3[]
        {
            new Vector3(0,0,RNG.Next(0,2)),
            new Vector3(10,0,RNG.Next(0,2)),
            new Vector3(20,0,RNG.Next(0,2)),
            new Vector3(5,10,RNG.Next(1,3)),
            new Vector3(15,10,RNG.Next(1,3)),
            new Vector3(10,20,RNG.Next(2,4)),
            new Vector3(20,20,RNG.Next(2,4)),
            new Vector3(15,30,RNG.Next(3,5)),
            new Vector3(15,15,RNG.Next(3,5))            
        };

        var objs = new GameObject[points.Length];

        for (int i = 0; i < objs.Length; i++)
        {
            objs[i] = CreateNewObject(points[i], RNG.NextFloat(0.7f, 4f,RadiusCurve), points[i].z);
        }

        CreateChain(objs[0], objs[1], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[1], objs[2], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[3], objs[4], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[5], objs[6], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[0], objs[3], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[3], objs[5], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[5], objs[7], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[1], objs[4], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[8], objs[6], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[1], objs[3], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[2], objs[4], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[8], objs[5], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[6], objs[7], RNG.Next(PropMin, PropMax, Curve));
        CreateChain(objs[4], objs[8], RNG.Next(1, 3, Curve));

        _joints = GetComponentsInChildren<SpringJoint2D>();
        StartCoroutine(FreezePhysicsAfterTime(TimeCount));
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _joints.Length; i++)
        {
            var j = _joints[i];
            Debug.DrawLine(j.transform.position, j.connectedBody.transform.position,Color.white,0.1f);
        }

    }

    // Step 1 - Run Physics
    IEnumerator FreezePhysicsAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        _rigids = GetComponentsInChildren<Rigidbody2D>();

        for (int i = 0; i < _rigids.Length; i++)
        {
            _rigids[i].simulated = false;
        }

        RunAdditionalScripts();
    }

    // Step 2 - Run Scripts in order
    void RunAdditionalScripts()
    {
        ReorientToXZPlaneAndUpdateBounds();

        _mesh = MeshMasher.DelaunayGen.FromBounds(_bounds, 0.015f);
        _mesh.SetCustomBuckets(10, 1, 10);

        var connectivity = MapGraphToShortestWalk();

        var connected = new bool[_mesh.Nodes.Count];

        for (int i = 0; i < connectivity.Length; i++)
        {
            connected[connectivity[i]] = true;
        }

        for (int i = 0; i < _mesh.Lines.Count; i++)
        {
            var l = _mesh.Lines[i];
            if (connected[l.Nodes[0].Index] == true && connected[l.Nodes[1].Index] == true)
            {
                l.DrawLine(Color.white, 100f);
            }

        }


        var rooms = ApplyWeightsToNodes();

    }

    
    //Step 2
    void ReorientToXZPlaneAndUpdateBounds()
    {
        var resultsBase = new CircleCollider2D[_rigids[0].attachedColliderCount];
        _rigids[0].GetAttachedColliders(resultsBase);
        _bounds = new Bounds(new Vector3(resultsBase[0].transform.position.x, 0, resultsBase[0].transform.position.y), Vector3.zero);


        var totalColliders = new List<CircleCollider2D>();

        for (int i = 0; i < _rigids.Length; i++)
        {
            var results = new CircleCollider2D[_rigids[i].attachedColliderCount];
            var resultsCount = _rigids[i].GetAttachedColliders(results);
            totalColliders.AddRange(results);

            for (int u = 0; u < resultsCount; u++)
            {
                results[u].transform.position = new Vector3(results[u].transform.position.x, 0, results[u].transform.position.y);

                _bounds.Encapsulate(results[u].transform.position);
                results[u].radius = results[u].radius * 0.9f;
            }            
        }

        _bounds.size = _bounds.size.x > _bounds.size.z ? new Vector3(_bounds.size.x,0, _bounds.size.x) : new Vector3(_bounds.size.z,0, _bounds.size.z);
        _bounds.size += (Vector3.one * _bounds.size.magnitude * 0.15f);

        Debug.Log("Bounds: " + _bounds.size + " " + _bounds.center);







        /*


        //Debug.DrawLine(combinedBounds.min, combinedBounds.max,Color.red,100f);

        var physicalMap = new Maps.PhysicalMap(new Maps.Map(256, 256, 1), new Rect(combinedBounds.min, combinedBounds.size));

        for (int i = 0; i < totalColliders.Count; i++)
        {
            physicalMap.DrawShape(totalColliders[i], totalColliders[i].transform.position.z);
        }

        for (int i = 0; i < _joints.Length; i++)
        {
            var j = _joints[i];

            physicalMap.DrawLine(j.transform.position, j.connectedBody.transform.position,RNG.Next(3,6), j.transform.position.z, j.connectedBody.transform.position.z);
        }

        var stack = Maps.Map.SetGlobalDisplayStack();

        var heightMap = physicalMap.ToMap();
        var walkableMap = heightMap.Clone().BooleanMapFromThreshold(0.99f);
        var distMap = walkableMap.Clone();


        walkableMap.Display()
            //.SmoothMap(5)
            .Display()
            .GetDistanceMap(5)
            .Clamp(0.5f, 1f)
            .Normalise()
            .Display()
            .Normalise()
            .Add(walkableMap.Clone()
            .PerlinFill(5, 0, 0, RNG.NextFloat(0, 1000f))
            .Remap(-0.29f, 0.29f))
            .BooleanMapFromThreshold(0.3f)
            .GetDistanceMap(4)
            .Display()
            .Add(walkableMap.Clone()
            .PerlinFill(5, 0, 0, RNG.NextFloat(0, 1000f))
            .Remap(-0.2f, 0.2f))
            .Clamp(0.25f,0.75f)
            .Normalise()
            .Invert()
            .Display();

        heightMap = heightMap.SmoothMap(3).Resize(256,256);

        

        //distMap = distMap.Resize(50, 50).GetDistanceMap(10).Resize(256, 256).Display();

        var diffMap = Maps.Map.Blend(heightMap, new Maps.Map(heightMap).FillWith(1f), walkableMap);
        diffMap.Invert().Display();



        stack.CreateDebugStack(transform);

        if(DoTheLevel)
            MakeTheLevel(diffMap);

        

        //map.DrawRect(Color.white);
        */
    }

    int[] MapGraphToShortestWalk()
    {

        var nodes = new List<int>();

       //for (int i = 0; i < _mesh.Lines.Count; i++)
       //{
       //    _mesh.Lines[i].DrawLine(Color.white, 100f);
       //}

        for (int i = 0; i < _joints.Length; i++)
        {
            var j = _joints[i];
            var a = _mesh.ClosestIndex(j.transform.position);
            var b = _mesh.ClosestIndex(j.connectedBody.transform.position);

            var realNodes = _mesh.ShortestWalkNode(a, b);

           for (int u = 0; u < realNodes.Length; u++)
           {
               Debug.DrawRay(_mesh.Nodes[realNodes[u]].Vert, Vector3.up, Color.blue, 100f);
           }

            nodes.AddRange(realNodes);
        }

        return nodes.Distinct().ToArray();

    }

    MeshMasher.MeshState ApplyWeightsToNodes()
    {
        var state = _mesh.GetMeshState();

        for (int i = 0; i < _rigids.Length; i++)
        {
            var results = new CircleCollider2D[_rigids[i].attachedColliderCount];
            var resultsCount = _rigids[i].GetAttachedColliders(results);

            for (int u = 0; u < resultsCount; u++)
            {
                var a = _mesh.ClosestIndex(results[u].transform.position);

                //state.Nodes[a] = (int)(results[u].radius*12);

                state.Nodes[a] = RNG.Next(4, 7, RadiusCurve);

            }
        }

        var returnState = _mesh.ApplyRoomsBasedOnWeights(state);

        for (int i = 0; i < _mesh.Lines.Count; i++)
        {
            var l = _mesh.Lines[i];

            if (returnState.Nodes[l.Nodes[0].Index] != -1 && returnState.Nodes[l.Nodes[1].Index] != -1)
            {

                //if (returnState.Nodes[l.Nodes[0].Index] == returnState.Nodes[l.Nodes[1].Index])
                //{/
                //if (returnState.Nodes[l.Nodes[0].Index] == -1)
                //continue;
                var nodeValue = returnState.Nodes[l.Nodes[0].Index] > returnState.Nodes[l.Nodes[1].Index] ? returnState.Nodes[l.Nodes[0].Index] : returnState.Nodes[l.Nodes[1].Index];

                var colourHue = Mathf.InverseLerp(0f, 50f, nodeValue);
                l.DrawLine(Color.HSVToRGB(colourHue, 1f, 1f),100f);
            }

        }



        return returnState;
    }

    GameObject CreateNewObject(Vector3 position, float radius, float height)
    {
        var obj = Instantiate(CirclePrefab);
        obj.transform.position = new Vector3(position.x,position.y,0f);
        obj.transform.localScale = Vector2.one*radius;
        obj.transform.parent = transform;
        var heightData = obj.AddComponent<CellData>();
        heightData.Height = height;
        return obj;
    }

    void AddLink(GameObject a, GameObject b)
    {
        var spring = a.AddComponent<SpringJoint2D>();
        spring.connectedBody = b.GetComponent<Rigidbody2D>();
        spring.enableCollision = true;
        spring.distance = 0;
        //spring.
    }

    GameObject CreateNewObjectAndAddLink(Vector3 position, float radius, float height,GameObject link)
    {
        var obj = CreateNewObject(position, radius, height);

        AddLink(obj, link);
        return obj;
    }

    void CreateChain(GameObject p1, GameObject p2, int divisions)
    {
        var chain = new List<GameObject>();
        chain.Add(p1);

        for (int i = 1; i < divisions; i++)
        {
            var pointOnDomain = Mathf.InverseLerp(0, divisions, i);
            var position = Vector3.Lerp(p1.transform.position, p2.transform.position, pointOnDomain);
            var height = Mathf.Lerp(p1.GetComponent<CellData>().Height, p2.GetComponent<CellData>().Height, pointOnDomain);

            chain.Add(CreateNewObjectAndAddLink(position, RNG.NextFloat(0.7f, 2.5f), height, chain[i - 1]));
        }

        AddLink(p2, chain[chain.Count - 1]);
    }

    TerrainChunk[,] _terrainChunks;

    // Use this for initialization
    void MakeTheLevel(Maps.Map levelMap)
    {
        var sizeX = levelMap.SizeX/32;
        var sizeY = levelMap.SizeY/32;

        var chunks = levelMap.GenerateNonUniformSubmapsOverlappingWherePossible(sizeX);

        _terrainChunks = new TerrainChunk[sizeX, sizeY];


        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                var chunk = chunks[x, y]
                    .Clone()
                    .Resize(TerrainStaticValues.HeightmapResolution, TerrainStaticValues.HeightmapResolution);

                _terrainChunks[x, y] = TerrainFactory.MakeTerrainChunk(chunk, new Coord(x, y), new Rect(new Vector2(Size.width * x, Size.height * y), Size.size), Settings);
            }
        }

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (_terrainChunks[x, y].Terrain == null)
                    continue;

                Terrain left = null;
                if (x - 1 >= 0)
                    left = _terrainChunks[x - 1, y].Terrain;

                Terrain right = null;
                if (x + 1 < sizeX)
                    right = _terrainChunks[x + 1, y].Terrain;

                Terrain bottom = null;
                if (y - 1 >= 0)
                    bottom = _terrainChunks[x, y - 1].Terrain;

                Terrain top = null;
                if (y + 1 < sizeY)
                    top = _terrainChunks[x, y + 1].Terrain;

                _terrainChunks[x, y].Terrain.SetNeighbors(left, top, right, bottom);
            }
        }
    }

}
