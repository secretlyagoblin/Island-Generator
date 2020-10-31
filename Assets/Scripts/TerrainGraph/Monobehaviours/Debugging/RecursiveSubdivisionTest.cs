using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;

public class RecursiveSubdivisionTest : MonoBehaviour
{
    public int amount;
    public int count = 1;
    public int addOrSub;
    //public int offset = 0;
    public bool doMatrix = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private double _time = 0;

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;



        var hexes = HexIndex.HexIndexFromPosition(5, 3).GenerateRing(1);

        //var results = new HexIndex[hexes.Length];

        for (int i = 0; i < count; i++)
        {           

            for (int x = 0; x < hexes.Length; x++)
            {
                var hex = hexes[x];


                var result = hex.NestMultiply(amount);
                var ring = result.GenerateRing(1);

                var results = ring.ToList();
                results.Add(result);

                var results3d = results.ConvertAll(r => r.Position3d);
                var matrix = HexIndex.GetInverseMultiplicationMatrix(amount);



                if (doMatrix)
                {

                    //if (_time > 3)
                    //{
                    //
                    //    Debug.Log($"angle: {bangle}, scale:{scale}");
                    //    _time = 0;
                    //}
          

                    results3d = results3d.ConvertAll(r => matrix.MultiplyPoint(r));


                    //p2 = Matrix4x4.Translate(hex.Position3d- p2).MultiplyPoint(p2);
                }

                results3d.ForEach(r =>
                {
                    Debug.DrawLine(hex.Position3d, r, Color.red);

                    Debug.DrawRay(r, Vector3.up * 5f);
                });



                var secondLayer = 

                hexes[x] = result;
            }
        }
    }
}
