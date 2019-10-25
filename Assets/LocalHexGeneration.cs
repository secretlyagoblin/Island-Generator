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
};

    private static readonly Vector3 _innerScale = Vector3.one * (1 / Mathf.Sqrt(7));

    // Start is called before the first frame update
    void Start()
    {
        var hex = new Hex();

        for (int i = 0; i < 6; i++)
        {
            var pont = Hex.GetStaticFlatCornerXY(i);
            var spont= Hex.GetStaticFlatCornerXY(i) + hex.GetNoiseOffset(_outerOffsets[i] + new Vector2Int(14,12));

            GameObject.Instantiate(Prefab, new Vector3(pont.x, 0, pont.y),Quaternion.identity).name = $"Outer Hex {i}";
            var gob = GameObject.Instantiate(Prefab, new Vector3(spont.x, 0, spont.y), Quaternion.identity);
            gob.name = $"Moved Point Hex {i} [{_outerOffsets[i]}]";
            gob.transform.localScale = Vector3.one*0.02f;

        }

        var goffset = hex.GetNoiseOffset(new Vector2Int(14, 12));
        var sbop = GameObject.Instantiate(Prefab, new Vector3(goffset.x, 0, goffset.y), Quaternion.identity);
        sbop.name = $"Moved Point Hex center";
        sbop.transform.localScale = Vector3.one * 0.02f;

        for (int i = 0; i < _2x2ChildrenOffsets.Length; i++)
        {
            var pont = hex.GetNestedHexLocalCoordinateFromOffset(_2x2ChildrenOffsets[i]);

            var gob = GameObject.Instantiate(Prefab, new Vector3(pont.x, 0, pont.y), Quaternion.identity);
            gob.name = $"Inner Hex {i} [{pont.ToString()}]";
            gob.transform.localScale = _innerScale;
        }



    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
