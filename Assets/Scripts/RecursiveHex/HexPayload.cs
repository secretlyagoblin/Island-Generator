using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RecursiveHex
{
    public struct HexPayload:IGraphable
    {
        public float Height;
        public Color Color;

        public int Region;

        public int Code;
        public CodeConnections Connections;

        public Connection ConnectionStatus { get; set; }

        public static HexPayload Blerp(Hex a, Hex b, Hex c, Vector3 weights)
        {
            return new HexPayload()
            {
                Height = Utils.Blerp(a.Payload.Height, b.Payload.Height, c.Payload.Height, weights),
                Color = Utils.Blerp(a.Payload.Color, b.Payload.Color, c.Payload.Color, weights),
                Code = Utils.Blerp(a.Payload.Code, b.Payload.Code, c.Payload.Code, weights),
                Connections = Utils.Blerp(a.Payload.Connections, b.Payload.Connections, c.Payload.Connections, weights),
                Region = Utils.Blerp(a.Payload.Region, b.Payload.Region, c.Payload.Region, weights),
                ConnectionStatus = Utils.Blerp(a.Payload.ConnectionStatus, b.Payload.ConnectionStatus, c.Payload.ConnectionStatus, weights)
            };
        }

        public void PopulatePayloadObject(PayloadData data)
        {
            data.KeyValuePairs =
                new Dictionary<string, object>()
                {
                    {"Height",Height },
                    {"Color",Color },
                    {"Code",Code },
                    {"Region",Region },
                    {"NodeConnectionStatus",ConnectionStatus }
                };
        }
    }

    /// <summary>
    /// The connections this node has to other codes - this is used to match codes
    /// </summary>
    public struct CodeConnections
    {

        private readonly int Count;
        private readonly int C0;
        private readonly int C1;
        private readonly int C2;
        private readonly int C3;
        private readonly int C4;
        private readonly int C5;

        public CodeConnections(int[] codes)
        {

            Count = codes.Length;
            C0 = Count > 0 ? codes[0] : -1;
            C1 = Count > 1 ? codes[1] : -1;
            C2 = Count > 2 ? codes[2] : -1;
            C3 = Count > 3 ? codes[3] : -1;
            C4 = Count > 4 ? codes[4] : -1;
            C5 = Count > 5 ? codes[5] : -1;

            if(Count> 6)
            {
                throw new Exception("Should never be more than 6, as these are hexagons - investigate");
            }

        }

        public bool IsFullyDefined
        {
            get { return Count == 6; }
        }

        public int[] ToArray()
        {
            var outInts = new int[Count];
            if (Count == 0) return outInts;

            outInts[0] = C0;
            if (Count == 1) return outInts;

            outInts[1] = C1;
            if (Count == 2) return outInts;

            outInts[2] = C2;
            if (Count == 3) return outInts;

            outInts[3] = C3;
            if (Count == 4) return outInts;

            outInts[4] = C4;
            if (Count == 5) return outInts;

            outInts[5] = C5;

            return outInts;
        }
    }
}
