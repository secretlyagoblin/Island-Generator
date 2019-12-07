using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var map = new RecursiveHex.HexMap(x => x.Payload.Code);

        var layer2 = map.Subdivide();

        var graph = layer2.GetHexesOfKey(0).ToGraph<Levels.SingleConnectionGraph>(x => x.Code, x => x.Connections.ToArray());

        graph.DebugDraw(transform);

            //.Subdivide()
            //.Subdivide()
            //.Subdivide()
            //.ToGameObjects()
            //;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
