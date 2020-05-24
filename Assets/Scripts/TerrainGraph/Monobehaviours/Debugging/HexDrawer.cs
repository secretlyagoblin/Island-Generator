using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Core.Random;
using WanderingRoad.Procgen.RecursiveHex;
using System.Linq;

public class HexDrawer : MonoBehaviour
{

    public GameObject Prefab;
    // Start is called before the first frame update
    void Start()
    {
        RNG.Init("Yeet");


        //var myList = new HexIndex().GenerateRosetteCircular(30).ToList();
        //myList.AddRange(new HexIndex().GenerateRosetteCircular(30));
        //myList.AddRange(new HexIndex().GenerateRosetteCircular(30));
        //
        //var list = myList.Distinct();
        //
        //foreach (var item in list)
        //{
        //    var g = Instantiate(Prefab);
        //    g.transform.position = item.Position3d;
        //}




        for (int i = 0; i < 5000; i++)
        {
            var a = RNG.NextVector2(-10, 10);
            var b = HexIndex.HexIndexFromPosition(a).Position2d;
        
            var a3 = new Vector3(a.x, 3, a.y);
            var b3 = new Vector3(b.x, 3, b.y);
        
            Debug.DrawLine(a3, b3, new Color(1,1,1,0.25f), 100f);
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
