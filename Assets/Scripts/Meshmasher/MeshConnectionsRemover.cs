using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshMasher {

    public partial class SmartMesh {

        public Mesh BuildMeshSurfaceWithCliffs()
        {
            return BuildMeshSurfaceWithCliffs(CreateMeshState<int>());
        }

        public Mesh BuildMeshSurfaceWithCliffs(MeshState<int> state)
        {

            //for (int i = 0; i < tris.Count; i++)
            //{
            //    var t = tris[i];
            //
            //    for (int l = 0; l < t.Lines.Count; l++)
            //    {
            //        /*
            //        var line = t.Lines[l];
            //        if (lineDict[line])
            //            Debug.DrawLine(line.Center, t.Center, Color.blue);
            //        else
            //            //Debug.DrawLine(line.Center, t.Center, Color.red);
            //            */
            //    }
            //}


            //for (int i = 0; i < nodes.Count; i++)
            //{
            //    var n = nodes[i];
            //    n.Vert.y = n.Vert.z;
            //}

            var outputVerts = new List<Vector3>();
            var outputTris = new List<int>();

            var index = 0;

            for (int i = 0; i < Cells.Count; i++)
            {
                var t = Cells[i];

                if (t.Lines.Count != 3)
                {
                    //Debug.Log("uhh");
                    continue;
                }

                //getting correct order

                var poly = new SmartPolyline(t.Lines[0]);
                poly.Intergrate(t.Lines[1]);
                poly.Intergrate(t.Lines[2]);

                poly.ForceClockwiseXZ();

                var sections = poly.LineSections;

                if (sections.Count != 3)
                {
                    Debug.Log("buhh");
                    continue;
                }

                //ResolveTri1

                var l0 = sections[0];
                var l1 = sections[1];
                var sharedNode = l0.GetSharedNode(l1).Vert;

                var v0c = l0.Center;
                var v2c = l1.Center;

                var v0 = state.Lines[l0.Index] == 1 ? l0.Center : new Vector3(v0c.x,sharedNode.y,v0c.z);
                var v1 = sharedNode;
                var v2 = state.Lines[l1.Index] == 1 ? l1.Center : new Vector3(v2c.x, sharedNode.y, v2c.z);

                outputVerts.Add(v0);
                outputVerts.Add(v1);
                outputVerts.Add(v2);

                outputTris.Add(index);
                outputTris.Add(index+1);
                outputTris.Add(index+2);

                index += 3;


                //Debug.DrawLine(v0, v1, Color.green, 100f);
                //Debug.DrawLine(v1, v2, Color.green, 100f);
                //Debug.DrawLine(v2, v0, Color.green, 100f);

                //ResolveTri2

                l0 = sections[1];
                l1 = sections[2];
                sharedNode = l0.GetSharedNode(l1).Vert;

                v0c = l0.Center;
                v2c = l1.Center;

                v0 = state.Lines[l0.Index] == 1 ? l0.Center : new Vector3(v0c.x, sharedNode.y, v0c.z);
                v1 = sharedNode;
                v2 = state.Lines[l1.Index] == 1 ? l1.Center : new Vector3(v2c.x, sharedNode.y, v2c.z);

                outputVerts.Add(v0);
                outputVerts.Add(v1);
                outputVerts.Add(v2);

                outputTris.Add(index);
                outputTris.Add(index + 1);
                outputTris.Add(index + 2);

                index += 3;

                //Debug.DrawLine(v0, v1, Color.green, 100f);
                //Debug.DrawLine(v1, v2, Color.green, 100f);
                //Debug.DrawLine(v2, v0, Color.green, 100f);
                //
                //ResolveTri3

                l0 = sections[2];
                l1 = sections[0];
                sharedNode = l0.GetSharedNode(l1).Vert;

                v0c = l0.Center;
                v2c = l1.Center;

                v0 = state.Lines[l0.Index] == 1 ? l0.Center : new Vector3(v0c.x, sharedNode.y, v0c.z);
                v1 = sharedNode;
                v2 = state.Lines[l1.Index] == 1 ? l1.Center : new Vector3(v2c.x, sharedNode.y, v2c.z);

                outputVerts.Add(v0);
                outputVerts.Add(v1);
                outputVerts.Add(v2);

                outputTris.Add(index);
                outputTris.Add(index + 1);
                outputTris.Add(index + 2);

                index += 3;

                //Debug.DrawLine(v0, v1, Color.green, 100f);
                //Debug.DrawLine(v1, v2, Color.green, 100f);
                //Debug.DrawLine(v2, v0, Color.green, 100f);


                //ResolveTriMid

                l0 = sections[0];
                l1 = sections[1];
                var l2 = sections[2];
                sharedNode = l0.GetSharedNode(l1).Vert;

                v0c = l0.Center;
                v2c = l1.Center;
                var v3c = l2.Center;

                v0 = state.Lines[l1.Index] == 1 ? l0.Center : new Vector3(v0c.x, sharedNode.y, v0c.z);
                v1 = state.Lines[l1.Index] == 1 ? l1.Center : new Vector3(v2c.x, sharedNode.y, v2c.z);
                v2 = state.Lines[l2.Index] == 1 ? l2.Center : new Vector3(v3c.x, sharedNode.y, v3c.z);

                outputVerts.Add(v0);
                outputVerts.Add(v1);
                outputVerts.Add(v2);

                outputTris.Add(index);
                outputTris.Add(index + 1);
                outputTris.Add(index + 2);

                index += 3;

                //Debug.DrawLine(v0, v1, Color.green, 100f);
                //Debug.DrawLine(v1, v2, Color.green, 100f);
                //Debug.DrawLine(v2, v0, Color.green, 100f);
            }

            var output = new Mesh();

            output.vertices = outputVerts.ToArray();
            output.triangles = outputTris.ToArray();

            output.RecalculateNormals();

            return output;
        }

    }
}