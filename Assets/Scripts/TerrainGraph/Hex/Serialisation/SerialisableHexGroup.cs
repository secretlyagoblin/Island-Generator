using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WanderingRoad.IO;

namespace WanderingRoad.Procgen.RecursiveHex
{

    [Serializable]
    public class SerialisableHexGroup : ISerialisable
    {
        public Vector3Int[] Indices;
        public float[] Height;
        public float[] EdgeDistance;
        public Color[] Color;

        public int[] Region;

        public int[] Code;
        public int[] Connections;

        public int[] BorderCode;

        public SerialisableHexGroup(Hex[] hexes)
        {
            Indices = new Vector3Int[hexes.Length];
            Height = new float[hexes.Length];
            EdgeDistance = new float[hexes.Length];
            Color = new Color[hexes.Length];
            Region = new int[hexes.Length];
            Code = new int[hexes.Length];
            Connections = new int[hexes.Length*6];
            BorderCode = new int[hexes.Length];

            for (int i = 0; i < hexes.Length; i++)
            {
                AddHex(hexes[i], i);
            }
        }

        private void AddHex(Hex hex, int index)
        {
            Indices[index] = hex.Index.Index3d;
            BorderCode[index] = hex.IsBorder?1:0;
            Height[index] = hex.Payload.Height;
            EdgeDistance[index] = hex.Payload.EdgeDistance;
            Color[index] = hex.Payload.Color;
            Region[index] = hex.Payload.Region;
            Code[index] = hex.Payload.Code;

           
            AddConnections(hex.Payload.Connections,index);
        }

        private Hex GetHex(int index)
        {
            return new Hex(
                new HexIndex(Indices[index]),
                new HexPayload() {
                    Height = Height[index],
                    EdgeDistance = EdgeDistance[index],
                    Color = Color[index],
                    Code = Code[index],
                    Region = Region[index],
                    Connections = GetConnections(index),
                },
                BorderCode[index] == 0?false:true);
        }

        private void AddConnections(CodeConnections connections, int index)
        {
            var codeIndex = index * 6;
            var asArray = connections.ToArray();

            for (int i = 0; i < 6; i++)
            {
                Connections[codeIndex + i] = i < asArray.Length ? asArray[i] : -1;
            }
        }

        private CodeConnections GetConnections(int index)
        {
            var codeIndex = index * 6;
            var count = 0;

            for (int i = 0; i < 6; i++)
            {
                if (Connections[codeIndex + i] != -1) count++;
            }

            var connections = new int[count];

            for (int i = 0; i < count; i++)
            {
                connections[i] = Connections[codeIndex + i];
            }

            return new CodeConnections(connections);
        }

        public Hex[] ToHexes()
        {
            var hexes = new Hex[Height.Length];

            for (int i = 0; i < hexes.Length; i++)
            {
                hexes[i] = GetHex(i);
            }

            return hexes;
        }

        public IStreamable RestoreAsset()
        {
            return new HexGroup(this);
        }
    }
}
