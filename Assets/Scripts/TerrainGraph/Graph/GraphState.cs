using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WanderingRoad.Procgen.Topology
{

    /// <summary>
    /// Smartmesh functions operate on the current mesh using MeshStates to hold the current state of the mesh. 
    /// This allows the current mesh to be analyised for various conditions.
    /// </summary>
    public class MeshState<T>
    {
        public T[] Nodes;
        public T[] Cells;
        public T[] Lines;

        public MeshState<T> Clone() => new MeshState<T>()
        {
            Nodes = (this.Nodes == null) ? null : (T[])this.Nodes.Clone(),
            Cells = (this.Cells == null) ? null : (T[])this.Cells.Clone(),
            Lines = (this.Lines == null) ? null : (T[])this.Lines.Clone(),
        };
    }
}