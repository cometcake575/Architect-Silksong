using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Architect.Placements;

[JsonConverter(typeof(LevelDataConverter))]
public class LevelData(List<ObjectPlacement> placements, List<(int, int)> tilemapChanges)
{
    public readonly List<ObjectPlacement> Placements = placements;

    public readonly List<(int, int)> TilemapChanges = tilemapChanges;

    public void ToggleTile((int, int) pos)
    {
        if (!TilemapChanges.Remove(pos)) TilemapChanges.Add(pos);
    }

    public class LevelDataConverter : JsonConverter<LevelData>
    {
        public override void WriteJson(JsonWriter writer, LevelData value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("placements");
            serializer.Serialize(writer, value.Placements);
            
            writer.WritePropertyName("tilemap"); 
            serializer.Serialize(writer, value.TilemapChanges);
            writer.WriteEndObject();
        }

        public override LevelData ReadJson(JsonReader reader, Type objectType, LevelData existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                return new LevelData(serializer.Deserialize<List<ObjectPlacement>>(reader), []);
            }

            List<ObjectPlacement> placements = [];
            List<(int, int)> tiles = [];
            
            while (reader.Read())
            {
                if (reader.Value is not string value) break;
                switch (value)
                {
                    case "placements":
                        reader.Read();
                        placements = serializer.Deserialize<List<ObjectPlacement>>(reader);
                        break;
                    case "tilemap":
                        reader.Read();
                        tiles = serializer.Deserialize<List<(int, int)>>(reader);
                        break;
                }
            }

            return new LevelData(placements, tiles);
        }
    }
}