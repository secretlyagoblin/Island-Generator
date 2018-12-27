using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Linq;

/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance {
    #region Utility functions
    /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
    /// <param name="text">String to print.</param>
    private void Print(string text) { __out.Add(text); }
    /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
    /// <param name="format">String format.</param>
    /// <param name="args">Formatting parameters.</param>
    private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
    /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
    /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
    /// <param name="obj">Object instance to parse.</param>
    private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
    #endregion

    #region Members
    /// <summary>Gets the current Rhino document.</summary>
    private RhinoDoc RhinoDocument;
    /// <summary>Gets the Grasshopper document that owns this script.</summary>
    private GH_Document GrasshopperDocument;
    /// <summary>Gets the Grasshopper script component that owns this script.</summary>
    private IGH_Component Component;
    /// <summary>
    /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
    /// Any subsequent call within the same solution will increment the Iteration count.
    /// </summary>
    private int Iteration;
    #endregion

    /// <summary>
    /// This procedure contains the user code. Input parameters are provided as regular arguments, 
    /// Output parameters as ref arguments. You don't have to assign output parameters, 
    /// they will have a default value.
    /// </summary>
    private void RunScript(Mesh mesh, List<string> offsetList, ref object C, ref object A, ref object B, ref object D, ref object E, ref object F, ref object AA)
    {

        var verts = mesh.Vertices;
        var tris = mesh.Faces;
        var topology = mesh.TopologyVertices;
        var topoDataTree = new DataTree<System.Object>();
        var offsetDataTree = new DataTree<System.Object>();
        var triangleDataTree = new DataTree<System.Object>();
        var originalVertexIndexToWrappingIndex = new int[mesh.Vertices.Count];
        var originalOffsetToWrappingOffset = new Vector3d[mesh.Vertices.Count];

        //Get old index, replacement index, x offset and y offset from datastruct
        var wrappingVerticesData = offsetList.Select(str =>
        {
            str = str.Substring(1);
            str = str.Substring(0, str.Length - 1);
            var array = str.Split(',');
            return new int[] { int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]), int.Parse(array[3]) };
        }).ToArray();

        //Init offset array and topo array
        for (int i = 0; i < originalVertexIndexToWrappingIndex.Length; i++)
        {
            originalVertexIndexToWrappingIndex[i] = i;
            originalOffsetToWrappingOffset[i] = new Vector3d(0, 0, 0);
        }

        //Populate topoRemap and offsetRemap from datastruct.
        for (int i = 0; i < wrappingVerticesData.Length; i++)
        {
            var wrappingVertexData = wrappingVerticesData[i];
            originalVertexIndexToWrappingIndex[wrappingVertexData[0]] = wrappingVertexData[1];
            originalOffsetToWrappingOffset[wrappingVertexData[0]].X = wrappingVertexData[2];
            originalOffsetToWrappingOffset[wrappingVertexData[0]].Y = wrappingVertexData[3];
        }

        //Get the topos of the points about to be replaced
        var wrappingTopology = new List<List<int>>();
        var wrappingOffset = new List<List<Vector3d>>();
        var wrappingTriangles = new List<int[]>();
        var wrappingVerticesDataList = wrappingVerticesData.ToList();

        for (int i = 0; i < wrappingVerticesData.Length; i++)
        {
            var wrappingVertex = wrappingVerticesData[i];
            var currentVertexTopology = topology.ConnectedTopologyVertices(wrappingVertex[0]);
            var currentVertexConnectedFaces = topology.ConnectedFaces(wrappingVertex[0]);

            var leftOutTopos = new List<int>();
            var leftOutOffsets = new List<Vector3d>();

            for (int u = 0; u < currentVertexTopology.Length; u++)
            {
                var index = wrappingVerticesDataList.FindIndex(wrappingIndexOriginal => { return wrappingIndexOriginal[0] == currentVertexTopology[u]; });
                if (index == -1)
                {
                    leftOutTopos.Add(currentVertexTopology[u]);
                    leftOutOffsets.Add(new Vector3d(-wrappingVerticesData[i][2], -wrappingVerticesData[i][3], 0));
                }
            }
            wrappingTopology.Add(leftOutTopos);
            wrappingOffset.Add(leftOutOffsets);
            wrappingTriangles.Add(currentVertexConnectedFaces);
            //todo, manage triangle offsets
        }

        ///Create a map of all distinct values and an
        ///associated remapping from the original set to
        ///the new set.
        var distinctIndices = new List<int>();
        var remapOfOriginalVertexIndicesToDistinctIndices = new int[mesh.Vertices.Count];

        //Iterate through the existing data and populate the above two maps.

        var distinctVertexCount = 0;

        for (int i = 0; i < remapOfOriginalVertexIndicesToDistinctIndices.Length; i++)
        {
            var testIndex = originalVertexIndexToWrappingIndex[i];

            var index = distinctIndices.FindIndex(ind => { return ind == testIndex; });

            if (index == -1)
            {
                distinctIndices.Add(testIndex);
                remapOfOriginalVertexIndicesToDistinctIndices[i] = distinctVertexCount;
                distinctVertexCount++;
            }
            else
            {
                remapOfOriginalVertexIndicesToDistinctIndices[i] = index;
            }
        }

        //Iterate through the distinct indexes and find the connected verts using the original mesh topo
        for (int i = 0; i < distinctIndices.Count; i++)
        {
            var currentTopology = topology.ConnectedTopologyVertices(distinctIndices[i]);
            var path = new GH_Path(i);

            for (int u = 0; u < currentTopology.Length; u++)
            {
                //Perform a remap of the topology using the looping edge, and then to the distinct values
                topoDataTree.Add(remapOfOriginalVertexIndicesToDistinctIndices[originalVertexIndexToWrappingIndex[currentTopology[u]]], path);
                offsetDataTree.Add(originalOffsetToWrappingOffset[currentTopology[u]], path);
                
            }

            var additionalToposIndex = wrappingVerticesDataList.FindIndex(ind => { return ind[1] == distinctIndices[i]; });

            //Finally, manage the other side of the grid so the whole thing wraps around nicely
            if (additionalToposIndex != -1)
            {
                var storedTopo = wrappingTopology[additionalToposIndex];
                var storedOffset = wrappingOffset[additionalToposIndex];
                var storedTriangle = wrappingTriangles[additionalToposIndex];

                for (int u = 0; u < storedTopo.Count; u++)
                {
                    topoDataTree.Add(remapOfOriginalVertexIndicesToDistinctIndices[originalVertexIndexToWrappingIndex[storedTopo[u]]], path);
                    offsetDataTree.Add(storedOffset[u], path);
                }

                for (int u = 0; u < storedTriangle.Count; u++)
                {
                    triangleDataTree.Add(storedTriangle[u], path);
                }
            }
        }

        //Iterate through the triangles and remap to the offsets
        var triangleIndexList = new List<int>();
        var triangleRemapList = new List<Vector3d>();

        for (int i = 0; i < tris.Count; i++)
        {
            var tri = tris[i];
            triangleIndexList.Add(remapOfOriginalVertexIndicesToDistinctIndices[originalVertexIndexToWrappingIndex[tri.A]]);
            triangleRemapList.Add(originalOffsetToWrappingOffset[tri.A]);

            triangleIndexList.Add(remapOfOriginalVertexIndicesToDistinctIndices[originalVertexIndexToWrappingIndex[tri.B]]);
            triangleRemapList.Add(originalOffsetToWrappingOffset[tri.B]);

            triangleIndexList.Add(remapOfOriginalVertexIndicesToDistinctIndices[originalVertexIndexToWrappingIndex[tri.C]]);
            triangleRemapList.Add(originalOffsetToWrappingOffset[tri.C]);
        }

        //Iterate through replaced indexes...

        C = distinctIndices;
        A = topoDataTree;
        B = offsetDataTree;
        D = triangleIndexList;
        E = triangleRemapList;
        F = triangleDataTree;

    }

    // <Custom additional code> 

    // </Custom additional code> 

    private List<string> __err = new List<string>(); //Do not modify this list directly.
    private List<string> __out = new List<string>(); //Do not modify this list directly.
    private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
    private IGH_ActiveObject owner;                  //Legacy field.
    private int runCount;                            //Legacy field.

    public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
    {
        //Prepare for a new run...
        //1. Reset lists
        this.__out.Clear();
        this.__err.Clear();

        this.Component = owner;
        this.Iteration = iteration;
        this.GrasshopperDocument = owner.OnPingDocument();
        this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

        this.owner = this.Component;
        this.runCount = this.Iteration;
        this.doc = this.RhinoDocument;

        //2. Assign input parameters
        Mesh mesh = default(Mesh);
        if (inputs[0] != null)
        {
            mesh = (Mesh)(inputs[0]);
        }

        List<string> offsetList = null;
        if (inputs[1] != null)
        {
            offsetList = GH_DirtyCaster.CastToList<string>(inputs[1]);
        }


        //3. Declare output parameters
        object C = null;
        object A = null;
        object B = null;
        object D = null;
        object E = null;
        object F = null;
        object AA = null;


        //4. Invoke RunScript
        RunScript(mesh, offsetList, ref C, ref A, ref B, ref D, ref E, ref F, ref AA);

        try
        {
            //5. Assign output parameters to component...
            if (C != null)
            {
                if (GH_Format.TreatAsCollection(C))
                {
                    IEnumerable __enum_C = (IEnumerable)(C);
                    DA.SetDataList(1, __enum_C);
                }
                else
                {
                    if (C is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(C));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(1, C);
                    }
                }
            }
            else
            {
                DA.SetData(1, null);
            }
            if (A != null)
            {
                if (GH_Format.TreatAsCollection(A))
                {
                    IEnumerable __enum_A = (IEnumerable)(A);
                    DA.SetDataList(2, __enum_A);
                }
                else
                {
                    if (A is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(2, (Grasshopper.Kernel.Data.IGH_DataTree)(A));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(2, A);
                    }
                }
            }
            else
            {
                DA.SetData(2, null);
            }
            if (B != null)
            {
                if (GH_Format.TreatAsCollection(B))
                {
                    IEnumerable __enum_B = (IEnumerable)(B);
                    DA.SetDataList(3, __enum_B);
                }
                else
                {
                    if (B is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(3, (Grasshopper.Kernel.Data.IGH_DataTree)(B));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(3, B);
                    }
                }
            }
            else
            {
                DA.SetData(3, null);
            }
            if (D != null)
            {
                if (GH_Format.TreatAsCollection(D))
                {
                    IEnumerable __enum_D = (IEnumerable)(D);
                    DA.SetDataList(4, __enum_D);
                }
                else
                {
                    if (D is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(4, (Grasshopper.Kernel.Data.IGH_DataTree)(D));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(4, D);
                    }
                }
            }
            else
            {
                DA.SetData(4, null);
            }
            if (E != null)
            {
                if (GH_Format.TreatAsCollection(E))
                {
                    IEnumerable __enum_E = (IEnumerable)(E);
                    DA.SetDataList(5, __enum_E);
                }
                else
                {
                    if (E is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(5, (Grasshopper.Kernel.Data.IGH_DataTree)(E));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(5, E);
                    }
                }
            }
            else
            {
                DA.SetData(5, null);
            }
            if (F != null)
            {
                if (GH_Format.TreatAsCollection(F))
                {
                    IEnumerable __enum_F = (IEnumerable)(F);
                    DA.SetDataList(6, __enum_F);
                }
                else
                {
                    if (F is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(6, (Grasshopper.Kernel.Data.IGH_DataTree)(F));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(6, F);
                    }
                }
            }
            else
            {
                DA.SetData(6, null);
            }
            if (AA != null)
            {
                if (GH_Format.TreatAsCollection(AA))
                {
                    IEnumerable __enum_AA = (IEnumerable)(AA);
                    DA.SetDataList(7, __enum_AA);
                }
                else
                {
                    if (AA is Grasshopper.Kernel.Data.IGH_DataTree)
                    {
                        //merge tree
                        DA.SetDataTree(7, (Grasshopper.Kernel.Data.IGH_DataTree)(AA));
                    }
                    else
                    {
                        //assign direct
                        DA.SetData(7, AA);
                    }
                }
            }
            else
            {
                DA.SetData(7, null);
            }

        }
        catch (Exception ex)
        {
            this.__err.Add(string.Format("Script exception: {0}", ex.Message));
        }
        finally
        {
            //Add errors and messages... 
            if (owner.Params.Output.Count > 0)
            {
                if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
                {
                    List<string> __errors_plus_messages = new List<string>();
                    if (this.__err != null)
                    { __errors_plus_messages.AddRange(this.__err); }
                    if (this.__out != null)
                    { __errors_plus_messages.AddRange(this.__out); }
                    if (__errors_plus_messages.Count > 0)
                        DA.SetDataList(0, __errors_plus_messages);
                }
            }
        }
    }
}