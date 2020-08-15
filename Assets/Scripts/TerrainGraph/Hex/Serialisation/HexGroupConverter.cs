using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WanderingRoad.Procgen.RecursiveHex.Json
{
    public class HexGroupConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(HexGroup);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);

            var inHexes = obj["inHexes"] as JArray;
            var outHexes = obj["outHexes"] as JArray;

            var inHexDict = new HexDictionary();
            var outHexDict = new HexDictionary();

            foreach (var hex in inHexes)
            {
                var i = hex["i"];

                var vector = new Vector3Int(
                    i["x"].ToObject<int>(),
                    i["y"].ToObject<int>(),
                    i["z"].ToObject<int>());

                var payload = GetPayload(hex["payload"] as JArray);

                inHexDict.Add(vector, new Hex(new HexIndex(vector), payload, false));
            }

            foreach (var hex in outHexes)
            {
                var i = hex["i"];

                var vector = new Vector3Int(
                    i["x"].ToObject<int>(),
                    i["y"].ToObject<int>(),
                    i["z"].ToObject<int>());

                var payload = GetPayload(hex["payload"] as JArray);

                outHexDict.Add(vector, new Hex(new HexIndex(vector), payload, false));
            }

            return HexGroup.FromJson(inHexDict, outHexDict);

            //throw new NotImplementedException();


        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vec = (HexGroup)value;

            var group = new JObject
            {
                { "inHexes", GetHexes(vec.GetHexes()) },
                { "outHexes", GetHexes(vec.GetBorderHexes()) }
            };

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
                    p.Color.r,
                    p.Color.g,
                    p.Color.b,
                    p.Color.a,
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

        private HexPayload GetPayload(JArray payload)
        {
            var nums = Enumerable.Range(9, payload.Count()-9);
            var connections = nums.Select(
                x => payload[x].ToObject<int>()).ToArray();

            return new HexPayload()
            {
                Code = payload[0].ToObject<int>(),
                Color = new Color(
                    payload[1].ToObject<float>(), 
                    payload[2].ToObject<float>(), 
                    payload[3].ToObject<float>(), 
                    payload[4].ToObject<float>()),
                ConnectionStatus = payload[5].ToObject<Topology.Connection>(),
                EdgeDistance = payload[6].ToObject<float>(),
                Height = payload[7].ToObject<float>(),
                Region = payload[8].ToObject<int>(),
                Connections = new CodeConnections(connections)
            };
        }
    }

}
