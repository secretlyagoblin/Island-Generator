using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BaryMorpher : MonoBehaviour
{
    public GameObject Corner1;
    public GameObject Corner2;
    public GameObject Corner3;
    public GameObject Test;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    Vector3 _weight = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        Vector2 a = Corner1.transform.position;
        Vector2 b = Corner2.transform.position;
        Vector2 c = Corner3.transform.position;
        Vector2 t = Test.transform.position;

        Debug.DrawLine(a, b, Color.green);
        Debug.DrawLine(b, c, Color.green);
        Debug.DrawLine(c, a, Color.green);

        _weight = CalculateBarycentricWeight(a, b, c, t);

    }

    void OnDrawGizmos()
    {
        if (_weight.x >= 0 && _weight.x <= 1 && _weight.y >= 0 && _weight.y <= 1 && _weight.z >= 0 && _weight.z <= 1)
        {
            Handles.Label(Test.transform.position, $"[{_weight.x.ToString("0.0000")}, {_weight.y.ToString("0.0000")}, {_weight.z.ToString("0.0000")}]");
            
        }
        else
        {
            Handles.Label(Test.transform.position, "NASTY");
        }       
    }

    private static Vector3 CalculateBarycentricWeight(Vector2 vertA, Vector2 vertB, Vector2 vertC, Vector2 test)
    {
        //// calculate vectors from point f to vertices p1, p2 and p3:
        //var f1 = vertA - test;
        //var f2 = vertB - test;
        //var f3 = vertC - test;
        //// calculate the areas and factors (order of parameters doesn't matter):
        //var a = Vector3.Cross(vertA - vertB, vertA - vertC).magnitude; // main triangle area a
        //var a1 = Vector3.Cross(f2, f3).magnitude / a; // p1's triangle area / a
        //var a2 = Vector3.Cross(f3, f1).magnitude / a; // p2's triangle area / a 
        //var a3 = Vector3.Cross(f1, f2).magnitude / a; // p3's triangle area / a
        //
        //return new Vector3(a1, a2, a3);


        Vector2 v0 = vertB - vertA, v1 = vertC - vertA, v2 = test - vertA;
        float d00 = Vector2.Dot(v0, v0);
        float d01 = Vector2.Dot(v0, v1);
        float d11 = Vector2.Dot(v1, v1);
        float d20 = Vector2.Dot(v2, v0);
        float d21 = Vector2.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        var v = (d11 * d20 - d01 * d21) / denom;
        var w = (d00 * d21 - d01 * d20) / denom;
        var u = 1.0f - v - w;

        return new Vector3(u, v, w);
    }
}
