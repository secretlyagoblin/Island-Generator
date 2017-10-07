using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen2D : MonoBehaviour {

    public bool DoTheLevel = false;
    public AnimationCurve Curve;

    public float PerlinScale = 0.5f;

    public GameObject CirclePrefab;
    public int PropCount = 12;
    public float TimeCount;
    LineRenderer _renderer;
    Rigidbody2D[] _rigids;

    SpringJoint2D[] _joints;

    public ProcTerrainSettings Settings;
    public Rect Size;

    // Use this for initialization
    void Start()
    {
        RNG.DateTimeInit();

        var points = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(10,0),
            new Vector2(20,0),
            new Vector2(5,10),
            new Vector2(15,10),
            new Vector2(10,20),
            new Vector2(20,20),
            new Vector2(15,30)
        };

        var objs = new GameObject[points.Length];

        for (int i = 0; i < objs.Length; i++)
        {
            objs[i] = CreateNewObject(points[i], RNG.NextFloat(0.7f, 4f));
        }

        CreateChain(objs[0], objs[1], RNG.Next(3, 7, Curve));
        CreateChain(objs[1], objs[2], RNG.Next(3, 7, Curve));
        CreateChain(objs[3], objs[4], RNG.Next(3, 7, Curve));
        CreateChain(objs[5], objs[6], RNG.Next(3, 7, Curve));
        CreateChain(objs[0], objs[3], RNG.Next(3, 7, Curve));
        CreateChain(objs[3], objs[5], RNG.Next(3, 7, Curve));
        CreateChain(objs[5], objs[7], RNG.Next(3, 7, Curve));
        CreateChain(objs[1], objs[4], RNG.Next(3, 7, Curve));
        CreateChain(objs[4], objs[6], RNG.Next(3, 7, Curve));
        CreateChain(objs[1], objs[3], RNG.Next(3, 7, Curve));
        CreateChain(objs[2], objs[4], RNG.Next(3, 7, Curve));
        CreateChain(objs[4], objs[5], RNG.Next(3, 7, Curve));
        CreateChain(objs[6], objs[7], RNG.Next(3, 7, Curve));

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

    IEnumerator FreezePhysicsAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        _rigids = GetComponentsInChildren<Rigidbody2D>();

        for (int i = 0; i < _rigids.Length; i++)
        {
            _rigids[i].simulated = false;
        }

        SetHeightsFromSimplex();
        PreviewBoundingRect();
    }

    void PreviewBoundingRect()
    {
        var combinedBounds = new Bounds(_rigids[0].transform.position,Vector2.zero);
        

        var totalColliders = new List<CircleCollider2D>();

        for (int i = 0; i < _rigids.Length; i++)
        {
            var results = new CircleCollider2D[_rigids[i].attachedColliderCount];
            var resultsCount = _rigids[i].GetAttachedColliders(results);
            totalColliders.AddRange(results);

            for (int u = 0; u < resultsCount; u++)
            {
                combinedBounds.Encapsulate(results[u].bounds);
                results[u].radius = results[u].radius * 0.9f;
            }            
        }

        combinedBounds.size = combinedBounds.size.x > combinedBounds.size.y ? new Vector2(combinedBounds.size.x, combinedBounds.size.x) : new Vector2(combinedBounds.size.y, combinedBounds.size.y);
        combinedBounds.size += (Vector3.one * combinedBounds.size.magnitude * 0.05f);

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
    }

    void SetHeightsFromSimplex()
    {
        for (int i = 0; i < _rigids.Length; i++)
        {
            var r = _rigids[i];
            var ender = r.gameObject.GetComponentInChildren<SpriteRenderer>();

            var height = Mathf.PerlinNoise(r.transform.position.x* PerlinScale, r.transform.position.y* PerlinScale);
            r.transform.position = new Vector3(r.transform.position.x, r.transform.position.y, height);
            ender.color = new Color(height, height, height);
            
        }
    }

    GameObject CreateNewObject(Vector2 position, float radius)
    {
        var obj = Instantiate(CirclePrefab);
        obj.transform.position = position;
        obj.transform.localScale = Vector2.one*radius;
        obj.transform.parent = transform;
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

    GameObject CreateNewObjectAndAddLink(Vector2 position, float radius,GameObject link)
    {
        var obj = CreateNewObject(position, radius);
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

            chain.Add(CreateNewObjectAndAddLink(position, RNG.NextFloat(0.7f, 2.5f), chain[i - 1]));
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
