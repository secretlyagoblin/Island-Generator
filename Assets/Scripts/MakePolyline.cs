using UnityEngine;
using System.Collections.Generic;
using Nurbz;

public class MakePolyline : MonoBehaviour {

    public Material Material;

    List<Vector3> _points = new List<Vector3>();
    List<int> _indices = new List<int>();

    // Use this for initialization
    void Start()
    {

        var finalFrames = new List<Polyline3>();

        Vector3[] polylineProfile = {
            new Vector3(-0.478748f, 0.0f, 3.012691f),
            new Vector3(-2.548946f, 0.0f, 1.977592f),
            new Vector3(-2.880178f, 0.0f, 2.971287f),
            new Vector3(-1.720867f, 0.0f, 3.343922f),
            new Vector3(-1.22402f, 0.0f, 4.296213f),
            new Vector3(0.225119f, 0.0f, 6.076584f),
            new Vector3(1.508642f, 0.0f, 5.869564f),
            new Vector3(1.053198f, 0.0f, 4.379021f),
            new Vector3(2.667952f, 0.0f, 2.888479f),
            new Vector3(3.827263f, 0.0f, 0.983897f),
            new Vector3(4.199899f, 0.0f, -1.003493f),
            new Vector3(2.54374f, 0.0f, -1.210513f),
            new Vector3(2.378125f, 0.0f, -0.216818f),
            new Vector3(0.349331f, 0.0f, 0.238626f),
            new Vector3(-0.230325f, 0.0f, -1.003493f),
            new Vector3(0.76337f, 0.0f, -1.500341f),
            new Vector3(-0.685768f, 0.0f, -2.1214f),
            new Vector3(-2.424734f, 0.0f, -0.258222f),
            new Vector3(-2.217715f, 0.0f, 1.190917f),
            new Vector3(-0.147517f, 0.0f, 2.474439f),
            new Vector3(-0.478748f, 0.0f, 3.012691f)

        };

        Vector3[] polylinePath = {
            new Vector3(-6.398041f, 19.743649f, 0.0f),
            new Vector3(-3.913714f, 17.444722f, 0.0f),
            new Vector3(-1.87605f, 14.748007f, 0.0f),
            new Vector3(-0.761381f, 11.573313f, 0.0f),
            new Vector3(-1.19979f, 8.250091f, 0.0f),
            new Vector3(-3.023129f, 5.416588f, 0.0f),
            new Vector3(-5.298058f, 2.907579f, 0.0f),
            new Vector3(-6.945689f, -0.025684f, 0.0f),
            new Vector3(-7.305743f, -3.377654f, 0.0f),
            new Vector3(-6.854926f, -6.731156f, 0.0f),
            new Vector3(-6.005523f, -10.009206f, 0.0f)
        };

        var path = new Polyline3(polylinePath, false);
        var profile = new Polyline3(polylineProfile, true);
        var frames = path.GetLoftFrames();

        var length = polylineProfile.Length;
        foreach (var frame in frames)
        {
            Vector3[] points = new Vector3[length];

            for (var i = 0; i < length; i++)
            {
                points[i] = frame.MultiplyPoint(polylineProfile[i]);
            }

            var finalLine = new Polyline3(points);
            finalLine.Debugdraw();

            finalFrames.Add(finalLine);


        }

        for (var i = 0; i < finalFrames.Count - 1; i++)
        {
            for (var c = 0; c < polylineProfile.Length-1; c++)
            {
                var lineA = new Line3(finalFrames[i].Vectors[c], finalFrames[i].Vectors[c + 1]);
                var lineB = new Line3(finalFrames[i+1].Vectors[c], finalFrames[i+1].Vectors[c + 1]);

                MeshMaker(lineA, lineB);
            }
        }

        var tron = new Triangulatron(path);
        var verts = tron.Triangulate();

        var count = _points.Count;
		for (var x = 0; x < verts.Length; x++)
        {
			verts[x] += count;
        }

		_points.AddRange(finalFrames[finalFrames.Count-1].Vectors);
        _indices.AddRange(verts);

        // Create the mesh
        Mesh msh = new Mesh();
        msh.vertices = _points.ToArray();
        msh.triangles = _indices.ToArray();
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        //AutoWelder.AutoWeld(msh, 0.1f, 5f);

        // Set up game object with mesh;
        var renderer = gameObject.AddComponent<MeshRenderer>();
        var filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = msh;
        renderer.sharedMaterial = Material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    }

            public void MeshMaker(Line3 lineA, Line3 lineB)
    {

        var count = _points.Count;

        Vector3[] points = { lineA.start, lineB.start, lineB.end, lineA.end };
        int[] indices = { count, count + 1, count + 2, count, count + 2, count + 3 };

        _points.AddRange(points);
        _indices.AddRange(indices);

    }













	
	// Update is called once per frame
	void Update () {
	
	}
}
