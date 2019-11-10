using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RecursiveHex
{
    public struct HexPayload
    {
        public float Height;
        public Color Color;
        public int Code;

        public static HexPayload Blerp(Hex a, Hex b, Hex c, Vector3 weights)
        {
            return new HexPayload()
            {
                Height = Utils.Blerp(a.Payload.Height, b.Payload.Height, c.Payload.Height, weights),
                Color = Utils.Blerp(a.Payload.Color, b.Payload.Color, c.Payload.Color, weights),
                Code = Utils.Blerp(a.Payload.Code, b.Payload.Code, c.Payload.Code, weights),
            };
        }

        public void PopulatePayloadObject(PayloadData data)
        {
            data.KeyValuePairs =
                new Dictionary<string, object>()
                {
                    {"Height",Height },
                    {"Color",Color },
                    {"Code",Code }
                };
        }
    }
}
