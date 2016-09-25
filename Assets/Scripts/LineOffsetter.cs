using UnityEngine;
using System.Collections;

public class LineOffsetter : MonoBehaviour {

    // Use this for initialization
    void Start()
    {

        var p0 = new Vector3(31.124f, 0, 11.129f);
        var p1 = new Vector3(-31.124f, 0, -21.129f);
        var p2 = new Vector3(71.124f, 0, 5.129f);

        Debug.DrawLine(p0, p1, Color.white, 100f);
        Debug.DrawLine(p1, p2, Color.red, 100f);

        var p2reversed = Vector3.Cross((p2 - p1).normalized, Vector3.up);
        Debug.DrawRay(p1, p2reversed, Color.blue, 100f);

        Vector3 outcome = ((p0 - p1).normalized + (p2 - p1).normalized);
        outcome.Scale(new Vector3(0.5f, 1f, 0.5f));
        Debug.DrawRay(p1, outcome, Color.blue, 100f);

    }

    // Update is called once per frame
    void Update () {
	
	}
}
