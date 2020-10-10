using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;

public class RecursiveSubdivisionTest : MonoBehaviour
{
    public int amount;
    public int count = 1;
    //public int offset = 0;
    public bool doMatrix = false;

    // Start is called before the first frame update
    void Start()
    {





        
    }

    // Update is called once per frame
    void Update()
    {

        var hexes = HexIndex.HexIndexFromPosition(5, 3).GenerateRing(1);

        var matrix = Matrix4x4.identity;

        //var results = new HexIndex[hexes.Length];

        for (int i = 0; i < count; i++)
        {           

            for (int x = 0; x < hexes.Length; x++)
            {
                var hex = hexes[x];


                var result = hex.NestMultiply(amount);
                //var matrix = HexIndex.GetInverseMultiplicationMatrix(amount + offset);

                var p2 = result.Position3d;

                if (doMatrix)
                {

                    p2 = Matrix4x4.Translate(hex.Position3d- p2).MultiplyPoint(p2);
                }


                Debug.DrawLine(hex.Position3d, p2, Color.red);

                Debug.DrawRay(p2, Vector3.up * 5f);

                var secondLayer = 

                hexes[x] = result;
            }
        }
    }
}
