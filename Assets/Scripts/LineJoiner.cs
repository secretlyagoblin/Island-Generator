using UnityEngine;
using System.Collections;
using Nurbz;

public class LineJoiner : MonoBehaviour {

	// Use this for initialization
	void Start () {

        var p0 = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        var p1 = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

        var p2 = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        var p3 = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

        var lineA = new Line3(p0, p1);
        var lineB = new Line3(p2, p3);

        var intersection = lineA.IntersectionPoint(lineB);



        Debug.DrawLine(lineA.middle, intersection, Color.green, 100);
        Debug.DrawLine(lineB.middle, intersection, Color.green, 100);

        lineA.DrawDebugView(100, Color.red);
        lineB.DrawDebugView(100, Color.blue);

        Debug.DrawRay(intersection, Vector3.up, Color.yellow, 100);
        Debug.DrawRay(intersection, Vector3.down, Color.yellow, 100);
        Debug.DrawRay(intersection, Vector3.back, Color.yellow, 100);
        Debug.DrawRay(intersection, Vector3.left, Color.yellow, 100);
        Debug.DrawRay(intersection, Vector3.right, Color.yellow, 100);
        Debug.DrawRay(intersection, Vector3.forward, Color.yellow, 100);


    }

    // Update is called once per frame
    void Update () {



    }
}
