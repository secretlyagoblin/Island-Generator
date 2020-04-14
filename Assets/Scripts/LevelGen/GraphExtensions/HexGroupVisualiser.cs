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

            var groups = _hexes
                .GroupBy(x => x.Payload.Color).SelectMany(x => x.Chunk(1023))
                .Select(x => x.ToArray())
                .ToArray();
            var matricesAndColours = groups
                .Select(x => (x.First().Payload.Color, x.Select(y => Matrix4x4.Translate(y.Index.Position3d-(Vector3.up*2))).ToArray())).ToArray();


            _renderData = matricesAndColours;
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
    private (Color Color, Matrix4x4[] Matrices)[] _renderData = new (Color Color, Matrix4x4[] Matrices)[0];
    private MaterialPropertyBlock _block = new MaterialPropertyBlock();

    public void DrawMeshes()
    {
        for (int i = 0; i < _renderData.Length; i++)
        {
            var d = _renderData[i];

            _block.SetColor("_Color", d.Color);

            Graphics.DrawMeshInstanced(this.PreviewMesh, 0, _material, d.Matrices,d.Matrices.Length,_block);
        }

        
    }



}
