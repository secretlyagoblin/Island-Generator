using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshNoise : MonoBehaviour
{

    public GameObject Prefab;

    public float OffsetX = 35;
    public float OffsetY = 124;

    public float Scale = 0.125f;

    public Gradient gradient;

    // Start is called before the first frame update
    void Start()
    {
        for (int x = 0; x < 30; x++)
        {
            for (int y = 0; y < 30; y++)
            {
                var fab = GameObject.Instantiate(Prefab);

                var perlin = Perlin(x, y);

                var perlinLength = Perlin(x+1000, y+1000);

                var radian = perlin * Mathf.PI*4;

                Debug.Log($"Radian: {radian}");

                var sinOffset = Mathf.Sin(radian)*1.5f* perlinLength;
                var cosOffset = Mathf.Cos(radian)*1.5f* perlinLength;

                var point = new Vector3(
                    x * 4 + sinOffset,
                    0,
                    y * 4 + cosOffset);

                //Debug.DrawLine(
                //    new Vector3(x*4, 0, y*4),
                //    point,
                //    gradient.Evaluate(perlin),
                //    100f
                //    );

                fab.transform.position = point;
            }
        }        
    }

    float Perlin(int x, int y)
    {
        return Mathf.PerlinNoise((x+OffsetX)* Scale, (y+ OffsetY)*Scale);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
