using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecursiveHex;

public class LocalHexGeneration : MonoBehaviour
{
    public GameObject Prefab;

    private static readonly Vector2Int[] _outerOffsets = new Vector2Int[]
{
        new Vector2Int(+1,+0),
                new Vector2Int(+0,+1),
                        new Vector2Int(-1,+1),
                                new Vector2Int(-1,+0),
                                        new Vector2Int(-1,-1),
        new Vector2Int(+0,-1)
};



    private static readonly Vector2Int[] _2x2ChildrenOffsets = new Vector2Int[]
{
        //Center
        new Vector2Int(0,0),

            new Vector2Int(+1,+0),
            new Vector2Int(+0,+1),
            new Vector2Int(-1,+1),
            new Vector2Int(-1,+0),
            new Vector2Int(-1,-1),
            new Vector2Int(+0,-1),
            new Vector2Int(-2,+1),
            new Vector2Int(-2,-1),
};

    private static readonly Vector3 _innerScale = Vector3.one * (1f /3f);

    // Start is called before the first frame update
    void Start()
    {
        RNG.Init();

        

        TriggerNewSet();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerNewSet();
        }

        Debug.DrawLine(_points[0], _points[1], Color.red);
        Debug.DrawLine(_points[1], _points[2], Color.red);
        Debug.DrawLine(_points[2], _points[3], Color.red);
        Debug.DrawLine(_points[3], _points[4], Color.red);
        Debug.DrawLine(_points[4], _points[5], Color.red);
        Debug.DrawLine(_points[5], _points[0], Color.red);

        Debug.DrawLine(_points[0], _points[6], Color.green);
        Debug.DrawLine(_points[1], _points[6], Color.green);
        Debug.DrawLine(_points[2], _points[6], Color.green);
        Debug.DrawLine(_points[3], _points[6], Color.green);
        Debug.DrawLine(_points[4], _points[6], Color.green);
        Debug.DrawLine(_points[5], _points[6], Color.green);
    }

    List<GameObject> _gobjects = new List<GameObject>();
    Vector3[] _points = new Vector3[7];

    void TriggerNewSet()
    {
        _gobjects.ForEach(x => Destroy(x));
        _gobjects.Clear();

        var hex = new Hex();

        RandomSeedProperties.SetRandomSeed(RNG.NextFloat(-100000, 100000), RNG.NextFloat(-100000, 100000));

        

        for (int i = 0; i < 6; i++)
        {
            var pont = Hex.GetStaticFlatCornerXY(i);
            var spont = Hex.GetStaticFlatCornerXY(i) + hex.GetNoiseOffset(_outerOffsets[i] + new Vector2Int(14, 12));

            _points[i] = new Vector3(spont.x, 0, spont.y);

            //var bob = GameObject.Instantiate(Prefab, new Vector3(pont.x, 0, pont.y), Quaternion.identity);
            //bob.name = $"Outer Hex {i}";
            var gob = GameObject.Instantiate(Prefab, new Vector3(spont.x, 0, spont.y), Quaternion.identity);
            gob.name = $"Moved Point Hex {i} [{_outerOffsets[i]}]";
            gob.transform.localScale = Vector3.one * 0.02f;

            //_gobjects.Add(bob);
            _gobjects.Add(gob);

        }

        var goffset = hex.GetNoiseOffset(new Vector2Int(14, 12));
        var sbop = GameObject.Instantiate(Prefab, new Vector3(goffset.x, 0, goffset.y), Quaternion.identity);
        sbop.name = $"Moved Point Hex center";
        _gobjects.Add(sbop);

        sbop.transform.localScale = Vector3.one * 0.02f;

        _points[6] = new Vector3(goffset.x, 0, goffset.y);

        for (int i = 0; i < _2x2ChildrenOffsets.Length; i++)
        {
            var pont = hex.GetNestedHexLocalCoordinateFromOffset(_2x2ChildrenOffsets[i]);

            var gob = GameObject.Instantiate(Prefab, new Vector3(pont.x, 0, pont.y), Quaternion.identity);
            gob.name = $"Inner Hex {i} [{pont.ToString()}]";
            gob.transform.localScale = _innerScale;
            _gobjects.Add(gob);
        }
    }
}
