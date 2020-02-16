using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Core.Random;
using WanderingRoad.Procgen.RecursiveHex;

public class HexDrawer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RNG.Init();

        for (int i = 0; i < 2000; i++)
        {
            var a = RNG.NextVector2(-5, 5);
            var b = HexIndex.HexIndexFromPosition(a).Position2d;

            var a3 = new Vector3(a.x, 3, a.y);
            var b3 = new Vector3(b.x, 3, b.y);

            Debug.DrawLine(a3, b3, Color.white, 100f);
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
