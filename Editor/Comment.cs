using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Architect.Editor {
    [JsonConverter(typeof(CommentConverter))]
    public class Comment {
        public string Title { get; set; }
        public Color Color { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public bool IsLocal { get; set; }

        public Comment() { }

        public Comment(string title, Color color, Vector2 position, Vector2 size, bool isLocal) {
            Title = title;
            Color = color;
            Position = position;
            Size = size;
            IsLocal = isLocal;
        }

        public class CommentConverter : JsonConverter<Comment> {
            public override void WriteJson(JsonWriter writer, Comment value, JsonSerializer serializer) {
                writer.WriteStartObject();
                writer.WritePropertyName("title");
                writer.WriteValue(value.Title);
                writer.WritePropertyName("color");
                serializer.Serialize(writer, new ColorData(value.Color));
                writer.WritePropertyName("position");
                serializer.Serialize(writer, value.Position);
                writer.WritePropertyName("size");
                serializer.Serialize(writer, value.Size);
                writer.WritePropertyName("isLocal");
                writer.WriteValue(value.IsLocal);
                writer.WriteEndObject();
            }

            public override Comment ReadJson(JsonReader reader, Type objectType, Comment existingValue, bool hasExistingValue,
                JsonSerializer serializer) {
                if (reader.TokenType != JsonToken.StartObject) return null;

                string title = "Comment";
                Color color = new Color(0f, 0f, 0f, 0.25f);
                Vector2 position = Vector2.zero;
                Vector2 size = Vector2.zero;
                bool isLocal = true;

                reader.Read();
                while (reader.TokenType == JsonToken.PropertyName) {
                    var propName = reader.Value as string;
                    reader.Read();

                    switch (propName) {
                        case "title":
                            title = reader.Value as string ?? "Comment";
                            break;
                        case "color":
                            var colorData = serializer.Deserialize<ColorData>(reader);
                            if (colorData != null) {
                                color = new Color(colorData.r, colorData.g, colorData.b, colorData.a);
                            }
                            break;
                        case "position":
                            position = serializer.Deserialize<Vector2>(reader);
                            break;
                        case "size":
                            size = serializer.Deserialize<Vector2>(reader);
                            break;
                        case "isLocal":
                            isLocal = reader.Value is bool b && b;
                            break;
                    }

                    reader.Read();
                }

                return new Comment(title, color, position, size, isLocal);
            }

            private class ColorData {
                public float r, g, b, a;

                public ColorData() { }

                public ColorData(Color color) {
                    r = color.r;
                    g = color.g;
                    b = color.b;
                    a = color.a;
                }
            }
        }
    }

    internal class CommentData : MonoBehaviour {
        public Comment Comment;
    }
}
