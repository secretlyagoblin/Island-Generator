using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManifestSerialiser : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Dictionary<Rect, Guid>);
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var dict = JArray.Load(reader);

        var outDict = new Dictionary<Rect, Guid>();

        foreach (var item in dict)
        {
            var rect = new Rect(
                item["rect"]["pos"]["x"].ToObject<float>(),
                item["rect"]["pos"]["y"].ToObject<float>(),
                item["rect"]["size"]["x"].ToObject<float>(),
                item["rect"]["size"]["x"].ToObject<float>()
                );
            var guid = item["guid"].ToObject<Guid>();
            outDict.Add(rect, guid);
        }

        return outDict;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var dict = (Dictionary<Rect, Guid>)value;

        var jArray = new JArray();

        foreach (var item in dict)
        {
            var jObject = JObject.FromObject(new { rect = new { pos = new { item.Key.position.x, item.Key.position.y }, size = new { item.Key.size.x, item.Key.size.y } }, guid = item.Value });
            jArray.Add(jObject);
        }

        jArray.WriteTo(writer);
    }
}
