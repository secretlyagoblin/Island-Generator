using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.Procgen.RecursiveHex;
using WanderingRoad.Core;
using System.Linq;

public class HexGroupVisualiser
{
    public HexGroup HexGroup
    {
        set
        {
            _hexes = value.GetHexes();
            _matrices = _hexes.Select(x => Matrix4x4.Translate(x.Index.Position3d)).Chunk(1023).Select(x => x.ToArray()).ToArray();
        }
    }

    private Hex[] _hexes;
    public Mesh PreviewMesh;

    public HexGroupVisualiser(Mesh mesh)
    {
        PreviewMesh = mesh;

        _material = new Material(Shader.Find("Standard"));
        _material.enableInstancing = true;
    }


    private Vector3 _scale = new Vector3(1, 0.001f, 1);

    public void DrawGizmos()
    {
        if (this._hexes == null)
            return;

        for (int i = 0; i < _hexes.Length; i++)
        {
            var h = _hexes[i];

            Gizmos.color = h.Payload.Color;

            //Gizmos.color *= 2;

            Gizmos.DrawMesh(
                PreviewMesh,
                h.Index.Position3d,
                Quaternion.identity
                ,_scale
                );
        }
    }

    private Material _material;
    private Matrix4x4[][] _matrices;

    public void DrawMeshes()
    {
        for (int i = 0; i < _matrices.Length; i++)
        {
            Graphics.DrawMeshInstanced(this.PreviewMesh, 0, _material, _matrices[i]);
        }

        
    }



}
