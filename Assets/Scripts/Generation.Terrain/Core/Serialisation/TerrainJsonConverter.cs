using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TerrainChunkConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(TerrainChunk);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);

        var bounds = obj["bounds"];

        return TerrainChunk.FromJson(
            obj["maxValue"].ToObject<float>(),
            obj["minValue"].ToObject<float>(),
            obj["multiplier"].ToObject<int>(),
            new RectInt(
                bounds["xMin"].ToObject<int>(),
                bounds["yMin"].ToObject<int>(),
                bounds["sizeX"].ToObject<int>(),
                bounds["sizeY"].ToObject<int>()
            ),
            GetStampData(
                bounds["sizeX"].ToObject<int>(),
                bounds["sizeY"].ToObject<int>(),
                obj["heights"] as JArray
                )
            );        
    }

    private StampData[,] GetStampData(int sizeX,int sizeY, JArray values)
    {
        var count = 0;

        var stampData = new StampData[sizeX, sizeY];

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                stampData[x, y] = new StampData()
                {
                    Height = values[count].ToObject<float>()
                };

                count++;
            }
        }

        return stampData;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var chunk = (TerrainChunk)value;

        var group = new JObject
        {
             { "maxValue", chunk.MaxValue },
             { "minValue", chunk.MinValue },
             { "multiplier", chunk.Multiplier },
             { "bounds", new JObject{
                 {"xMin",chunk.Bounds.xMin },
                 {"yMin",chunk.Bounds.yMin },
                 {"sizeX",chunk.Bounds.size.x },
                 {"sizeY",chunk.Bounds.size.y }
             }},
             { "sizeX", chunk.Map.GetLength(0) },
             { "sizeY", chunk.Map.GetLength(1) },
             { "heights", new JArray(chunk.To1dDataArray().Select(x => x.Height)) }
            //{ "other hypothetical value...", new JArray(chunk.To1dDataArray().Select(x => x.Height)) }
        };

        group.WriteTo(writer);
    }
}
