using UnityEngine;
using System.Collections.Generic;
using UnityNURBS.Primitives;
using UnityNURBS.Types;

public class Nurbo : MonoBehaviour {

	// Use this for initialization
	void Start () {

        

        Vector3[] polylinePath = {
            new Vector3(-6.398041f, 19.743649f, 11.0f),
            new Vector3(-3.913714f, 17.444722f, 1.0f),
            new Vector3(-1.87605f, 14.748007f, 12.0f),
            new Vector3(-0.761381f, 11.573313f, -7.0f),
            new Vector3(-1.19979f, 8.250091f, 0.0f),
            new Vector3(-3.023129f, 5.416588f, 12.0f),
            new Vector3(-5.298058f, 2.907579f, -7.0f),
            new Vector3(-6.945689f, -0.025684f,  1.0f),
            new Vector3(-7.305743f, -3.377654f,-7.0f),
            new Vector3(-6.854926f, -6.731156f,  1.0f),
            new Vector3(-6.005523f, -10.009206f,-7.0f)
        };

        var mmVector = new List<mmVector3>();

        for(var x = 0; x<polylinePath.Length; x++)
        {
            mmVector.Add(new mmVector3(polylinePath[x]));
        }

        for (var x = 0; x < polylinePath.Length-1; x++)
        {
            Debug.DrawLine(polylinePath[x], polylinePath[x+1], Color.red);
        }

        //double[] doob = { 0,0,0, 1, 2, 3, 4, 5, 6, 7, 8 , 9, 10, 11,11,11};
        double[] doob = { 0,0,0, 1, 2, 3, 4, 5, 6, 7, 8 , 9, 9, 9};
        double[] doobo = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        var booster = new NurbsCurve(doob, mmVector, new List<double>(doobo),3);

        for (var x = 0.00; x < 9; x += 0.05)
        {

            var point1 = booster.GetPoint(x);
            var point2 = booster.GetPoint(x + 0.05);

            

            Debug.DrawLine(point1, point2);

            //Debug.Log(vectorFromMMVector(point));
        }



    }
	
	// Update is called once per frame
	void Update () {
	
	}

    Vector3 vectorFromMMVector(UnityNURBS.Types.mmVector3 vector)
    {
        return new Vector3((float)vector.x, (float)vector.y, (float)vector.z);

    }
}
