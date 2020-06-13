using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTest : MonoBehaviour
{
    public float size = 5;

    // Start is called before the first frame update
    void Start()
    {
        var stamp = new TerrainChunk(new BoundsInt(3, 5, 0, 76, 44, 0), new List<WanderingRoad.Procgen.RecursiveHex.HexGroup>(),5);
        var hexes = stamp.AssociatePixels();

        foreach (var hex in hexes)
        {
            var parent = new GameObject();
            parent.transform.position = hex.Key.Position3d;
            parent.name = hex.Key.ToString();
            foreach (var pixel in hex.Value)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                c.transform.position = new Vector3(pixel.x, 0, pixel.y);
                c.transform.parent = parent.transform;
            }
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
