using Architect.Events.Blocks;
using Architect.Editor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Architect.Placements;

[JsonConverter(typeof(LevelDataConverter))]
public class LevelData(List<ObjectPlacement> placements, List<(int, int)> tilemapChanges, List<ScriptBlock> scriptBlocks, List<Comment> comments)
{
    public readonly List<ObjectPlacement> Placements = placements;

    public readonly List<(int, int)> TilemapChanges = tilemapChanges;

    public readonly List<ScriptBlock> ScriptBlocks = scriptBlocks;

    public readonly List<Comment> Comments = comments;

    public void ToggleTile((int, int) pos)
    {
        if (!TilemapChanges.Remove(pos)) TilemapChanges.Add(pos);
    }

    public void Merge(LevelData levelData)
    {
        Placements.AddRange(levelData.Placements);
        TilemapChanges.AddRange(levelData.TilemapChanges.Where(t => !TilemapChanges.Contains(t)));
        ScriptBlocks.AddRange(levelData.ScriptBlocks);
        Comments.AddRange(levelData.Comments);
    }

    public void Clear()
    {
        Placements.Clear();
        TilemapChanges.Clear();
        ScriptBlocks.Clear();
        Comments.Clear();
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
            
            writer.WritePropertyName("script"); 
            serializer.Serialize(writer, value.ScriptBlocks);
            
            if (value.Comments.Count > 0)
            {
                writer.WritePropertyName("comments");
                serializer.Serialize(writer, value.Comments);
            }
            
            writer.WriteEndObject();
        }

        public override LevelData ReadJson(JsonReader reader, Type objectType, LevelData existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                return new LevelData(
                    serializer.Deserialize<List<ObjectPlacement>>(reader), 
                    [], 
                    [],
                    []);
            }

            List<ObjectPlacement> placements = [];
            List<(int, int)> tiles = [];
            List<ScriptBlock> scriptBlocks = [];
            List<Comment> comments = [];
            
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
                    case "script":
                        reader.Read();
                        scriptBlocks = serializer.Deserialize<List<ScriptBlock>>(reader);
                        break;
                    case "comments":
                        reader.Read();
                        comments = serializer.Deserialize<List<Comment>>(reader) ?? [];
                        break;
                }
            }

            placements.RemoveAll(o => o == null);

            return new LevelData(placements, tiles, scriptBlocks, comments);
        }
    }
}