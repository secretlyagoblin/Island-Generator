using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionTester : MonoBehaviour {

	// Use this for initialization
	void Start () {

        var verts = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(0,2),
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(1,2),
            new Vector2(2,0),
            new Vector2(2,1),
            new Vector2(2,2),
            new Vector2(0,-1),
            new Vector2(3,2)
        };

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] += new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
        }

        var lines = new Line[]
        {
            new Line(0,1),
            new Line(0,3),
            new Line(1,2),
            new Line(1,4),
            new Line(2,5),
            new Line(3,4),
            new Line(3,6),
            new Line(4,5),
            new Line(4,7),
            new Line(5,8),
            new Line(6,7),
            new Line(7,8)
        };

        var iterations = 3;

        for (int i = 0; i < iterations; i++)
        {
            var lineList = new List<Line>(lines);
            lineList.RemoveAt(Random.Range(0, lineList.Count));
            lines = lineList.ToArray();
        }

        var sublines = new List<Line>(lines);

        sublines.AddRange(new Line[]
        {
            new Line(9,0),
            new Line(8,10)
        });

        lines = sublines.ToArray();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            Debug.DrawLine(verts[line.p1], verts[line.p2], Color.red, 100f);
        }
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    struct Line {
        public int p1;
        public int p2;

        public Line(int p1, int p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
