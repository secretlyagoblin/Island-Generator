using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen2D : MonoBehaviour {

    public GameObject CirclePrefab;
    public int PropCount = 12;
    public float TimeCount;
    List<GameObject> _chain = new List<GameObject>();
    LineRenderer _renderer;

    SpringJoint2D[] _joints;

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
            objs[i] = CreateNewObject(points[i], RNG.NextFloat(0.7f, 1.5f));
        }

        CreateChain(objs[0], objs[1], Random.Range(3, 7));
        CreateChain(objs[1], objs[2], Random.Range(3, 7));
        CreateChain(objs[3], objs[4], Random.Range(3, 7));
        CreateChain(objs[5], objs[6], Random.Range(3, 7));
        CreateChain(objs[0], objs[3], Random.Range(3, 7));
        CreateChain(objs[3], objs[5], Random.Range(3, 7));
        CreateChain(objs[5], objs[7], Random.Range(3, 7));
        CreateChain(objs[1], objs[4], Random.Range(3, 7));
        CreateChain(objs[4], objs[6], Random.Range(3, 7));
        CreateChain(objs[1], objs[3], Random.Range(3, 7));
        CreateChain(objs[2], objs[4], Random.Range(3, 7));
        CreateChain(objs[4], objs[5], Random.Range(3, 7));
        CreateChain(objs[6], objs[7], Random.Range(3, 7));

        /*

        var length = PropCount;
        var startPos = new Vector3[length];

        _chain = new List<GameObject>();
        _chain.Add(CreateNewObject(new Vector2(Mathf.Sin(0), Mathf.Cos(0)) * (length / 2), RNG.NextFloat(0.3f, 1f)));
        startPos[0] = _chain[0].transform.position;


        //for (int i = 1; i < length; i++)
        //{
        //    chain.Add(CreateNewObjectAndAddLink(Vector2.up * i*2 + Vector2.left* RNG.NextFloat(-0.25f,0.25f), RNG.NextFloat(), chain[i - 1]));
        //}

        for (int i = 1; i < length; i++)
        {
            var pointOnDomain = Mathf.InverseLerp(0, length, i);
            pointOnDomain = pointOnDomain * Mathf.PI * 2f;

            _chain.Add(CreateNewObjectAndAddLink(new Vector2(Mathf.Sin(pointOnDomain), Mathf.Cos(pointOnDomain)) * (length / 2), RNG.NextFloat(0.3f, 1f), _chain[i - 1]));
            startPos[i] = _chain[i].transform.position;
        }

        AddLink(_chain[0], _chain[_chain.Count - 1]);
        StartCoroutine(FreezePhysicsAfterTime(TimeCount));

        _renderer = GetComponent<LineRenderer>();
        _renderer.positionCount = length;

    */
        _joints = GetComponentsInChildren<SpringJoint2D>();
        StartCoroutine(FreezePhysicsAfterTime(TimeCount));
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _joints.Length; i++)
        {
            var j = _joints[i];
            Debug.DrawLine(j.transform.position, j.connectedBody.transform.position,Color.white,0.2f);
        }

    }

    IEnumerator FreezePhysicsAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        var rigids = GetComponentsInChildren<Rigidbody2D>();

        for (int i = 0; i < rigids.Length; i++)
        {
            rigids[i].simulated = false;
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

            chain.Add(CreateNewObjectAndAddLink(position, RNG.NextFloat(0.7f, 1.5f), chain[i - 1]));
        }

        AddLink(p2, chain[chain.Count - 1]);
    }
}
