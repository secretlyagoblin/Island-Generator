using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Random;
using WanderingRoad.Procgen.RecursiveHex;

public class LineDrawer : MonoBehaviour
{
    public Transform HandleA;
    public Transform HandleB;
    public GameObject Prefab;

    public List<GameObject> _pool;

    private HexIndex _lastA = new HexIndex();
    private HexIndex _lastB = new HexIndex();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 30; i++)
        {
            var obj = Instantiate(Prefab);
            obj.SetActive(false);
            _pool.Add(obj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var vecA = new Vector2(HandleA.position.x, HandleA.position.z);
        var vecB = new Vector2(HandleB.position.x, HandleB.position.z);

        Debug.DrawLine(HandleA.position, HandleB.position, Color.red);

        var trueA = HexIndex.HexIndexFromPosition(vecA);
        var trueB = HexIndex.HexIndexFromPosition(vecB);

        //if(trueA == _lastA && trueB == _lastB)
        //{
        //    return;
        //}


        foreach (var item in _pool)
        {
            item.SetActive(false);
        }

        var line = HexIndex.DrawLine(
            trueA,
            trueB);

        while(_pool.Count < line.Length)
        {
            var obj = Instantiate(Prefab);
            obj.SetActive(false);
            _pool.Add(obj);
        }

        for (int i = 0; i < line.Length; i++)
        {
            _pool[i].SetActive(true);
            _pool[i].transform.position = line[i].Position3d;
        }

        _lastA = trueA;
        _lastB = trueB;


    }

}
