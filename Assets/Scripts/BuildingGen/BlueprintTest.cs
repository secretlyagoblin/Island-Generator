using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BuildingGenerator;
using MeshMasher;
using Nurbz;

public class BlueprintTest : MonoBehaviour {

    public Gradient Radient;
    public Gradient Slaydient;

    // Use this for initialization
    void Start () {

        var mesh = GetComponent<MeshFilter>().mesh;

        mesh = AutoWelder.AutoWeld(mesh, 0.5f, 5f);

        var verts = mesh.vertices;
        var newVerts = new List<Vector2>();

        for (int i = 0; i < verts.Length; i++)
        {
            newVerts.Add(new Vector2(verts[i].x, verts[i].z));
        }


        var tris = mesh.triangles;

        var offset = 4;

        var triCount = 75; //104

        var subTris = new int[triCount * 3];
        var wallTypes = new int[triCount * 3];

        for (int i = offset*3; i < (triCount+offset)*3; i+=3)
        {
            subTris[i- (offset * 3)] = tris[i];
            subTris[i+1-(offset * 3)] = tris[i+1];
            subTris[i+2- (offset * 3)] = tris[i+2];

            wallTypes[i - (offset * 3)] = 0;
            wallTypes[i + 1 - (offset * 3)] =0;
            wallTypes[i + 2- (offset * 3)] = 0;
        }

        Debug.Log(verts);

        var debug = true;

        var boost = BlueprintRoomGenerator.GetOutlinePatterns(newVerts.ToArray(), subTris, wallTypes, debug);

        //foreach (var vert in boost)
        //{
        //    var s = "";
        //    foreach (var soop in vert)
        //    {
        //        s = s + soop + ", ";
        //    }
        //
        //        Debug.Log(s);
        //}

        foreach (var vert in boost)
        {
            var vertz = new List<Vector2>();
            for (int i = 0; i < vert.Length; i+=2)
            {
                vertz.Add(newVerts[vert[i]]);
            }

            var offsetDistance = -0.125f;

            var poly = new Polyline2(vertz.ToArray(), true);
            //poly.DebugDraw(Slaydient, 100f);
            //poly.ForceAntiClockwise();
            var olly = poly.OffsetInPlane(offsetDistance);
            olly.DebugDraw(Slaydient, 100f);

            var c = 0;

            for (int i = 0; i < vert.Length; i+=2)
            {

                verts[vert[i]] = new Vector3(olly.Vectors[c].x, 0, olly.Vectors[c].y);
                    c++;
            }

            //var goop = new List<Vector2>(vertz);
            //goop.Reverse();
            //
            //var fopoly = new Polyline2(goop.ToArray(), true);
            //fopoly.ForceClockwise();
            //var golly = fopoly.OffsetInPlane(offsetDistance);
            //golly.DebugDraw(Color.green, 100f);
        }

        //var newTris = new List<int>(subTris);
        //var relevantPoints = new List<Vector3>();
        //var newIndexes = new List<int>();
        //
        //newTris.Distinct();
        //
        //for (int i = 0; i < newTris.Count; i++)
        //{
        //    relevantPoints.Add(verts[newTris[i]]);
        //    newIndexes.Add(i);
        //}
        //
        //var finalIdexes = new List<int>();
        //for (int i = 0; i < subTris.Length; i++)
        //{
        //    finalIdexes.Add(newIndexes[subTris[i]]);
        //    newIndexes.Add(i);
        //}
        //
        //mesh.vertices = relevantPoints.ToArray();
        //mesh.triangles = newIndexes.ToArray();
        //mesh.RecalculateNormals();

        mesh.vertices = verts;




    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
