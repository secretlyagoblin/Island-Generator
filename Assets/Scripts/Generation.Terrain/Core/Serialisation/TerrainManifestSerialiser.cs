using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManifestSerialiser : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(TerrainManifest);
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var ojb = JObject.Load(reader);


        var dict = ojb["terrains"] as JArray;

        var outDict = new Dictionary<Rect, Guid>();

        foreach (var item in dict)
        {
            var rect = new Rect(
                item["rect"]["pos"]["x"].ToObject<float>(),
                item["rect"]["pos"]["y"].ToObject<float>(),
                item["rect"]["size"]["x"].ToObject<float>(),
                item["rect"]["size"]["y"].ToObject<float>()
                );
            var guid = item["guid"].ToObject<Guid>();
            outDict.Add(rect, guid);
        }



        return new TerrainManifest()
        {
            MaxHeight = ojb["max"].ToObject<float>(),
            MinHeight = ojb["min"].ToObject<float>(),
            Terrains = outDict,
        };
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var manifest = (TerrainManifest)value;

        var jArray = new JArray();

        foreach (var item in manifest.Terrains)
        {
            var jObject = JObject.FromObject(new { rect = new { pos = new { item.Key.position.x, item.Key.position.y }, size = new { item.Key.size.x, item.Key.size.y } }, guid = item.Value });
            jArray.Add(jObject);
        }

        var obj = new JObject
        {
            {"max",manifest.MaxHeight },
             {"min",manifest.MinHeight },
            {"terrains",jArray }
        };

        obj.WriteTo(writer);
    }
}
