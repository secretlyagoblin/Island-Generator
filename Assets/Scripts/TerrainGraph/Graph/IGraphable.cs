using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WanderingRoad.Procgen.Topology
{
    public interface IGraphable
    {
        Connection ConnectionStatus { get; set; }

        //int[] Neighbours { get; set; }
    }
}
