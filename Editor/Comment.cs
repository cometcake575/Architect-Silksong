using System;
using Architect.Events.Blocks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;

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
                                color = new Color(colorData.R, colorData.G, colorData.B, colorData.A);
                            }
                            break;
                        case "position":
                            position = serializer.Deserialize<Vector2>(reader);
                            break;
                        case "size":
                            size = serializer.Deserialize<Vector2>(reader);
                            break;
                        case "isLocal":
                            isLocal = reader.Value is true;
                            break;
                    }

                    reader.Read();
                }

                return new Comment(title, color, position, size, isLocal);
            }

            // ReSharper disable FieldCanBeMadeReadOnly.Local
            private class ColorData(Color color)
            {
                public float R = color.r;
                public float G = color.g;
                public float B = color.b;
                public float A = color.a;
            }
        }
    }

    internal class CommentData : MonoBehaviour, IPointerDownHandler {
        
        public Comment Comment;
        private float _time;

        public void Select()
        {
            var rt = (RectTransform)transform;
            
            var bCorners = new Vector3[4];
            rt.GetWorldCorners(bCorners);
            var bMin = new Vector2(float.MaxValue, float.MaxValue);
            var bMax = new Vector2(float.MinValue, float.MinValue);
            for (var i = 0; i < 4; i++)
            {
                var bsp = RectTransformUtility.WorldToScreenPoint(null, bCorners[i]);
                bMin = Vector2.Min(bMin, bsp);
                bMax = Vector2.Max(bMax, bsp);
            }
            var selectionRect = new Rect(bMin, bMax - bMin);
            
            ScriptEditorUI.BackgroundDrag.DoSelection(selectionRect);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) ScriptManager.ClearSelection();
            
            if (Time.realtimeSinceStartup - _time < 0.5f)
            {
                Select();
            }
            _time = Time.realtimeSinceStartup;
        }
    }
}
