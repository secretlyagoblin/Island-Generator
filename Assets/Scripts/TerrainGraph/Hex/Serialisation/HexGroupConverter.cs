using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WanderingRoad.Procgen.RecursiveHex.Json
{
    public class HexGroupConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(HexGroup);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vec = (HexGroup)value;

            var group = new JObject();

            group.Add("inHexes", GetHexes(vec.GetHexes()));
            group.Add("outHexes", GetHexes(vec.GetBorderHexes()));

            group.WriteTo(writer);
        }

        private JArray GetHexes(List<Hex> hexes)
        {
            var array = new JArray();

            foreach (var hex in hexes)
            {
                var jHex = new JObject();

                jHex.Add("i", JObject.FromObject(new { hex.Index.Index3d.x, hex.Index.Index3d.y, hex.Index.Index3d.z }));

                var p = hex.Payload;
                var jPayload = new JArray
                {
                    p.Code,
                    p.Color.a,
                    p.Color.r,
                    p.Color.g,
                    p.Color.b,
                    (int)p.ConnectionStatus,
                    p.EdgeDistance,
                    p.Height,
                    p.Region
                };

                var connections = p.Connections.ToArray();

                for (int i = 0; i < connections.Length; i++)
                {
                    jPayload.Add(connections[i]);
                }

                jHex.Add("payload", jPayload);

                array.Add(jHex);
            }

            return array;
        }
    }

}
