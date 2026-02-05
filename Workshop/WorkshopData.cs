using System;
using System.Collections.Generic;
using System.Linq;
using Architect.Workshop.Items;
using Architect.Workshop.Types;
using Newtonsoft.Json;

namespace Architect.Workshop;

[JsonConverter(typeof(WorkshopDataConverter))]
public class WorkshopData
{
    public readonly List<WorkshopItem> Items = [];
    
    public class WorkshopDataConverter : JsonConverter<WorkshopData>
    {
        public override void WriteJson(JsonWriter writer, WorkshopData value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("items");
            writer.WriteStartArray();

            foreach (var item in value.Items)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("type");
                writer.WriteValue(item.Type);

                writer.WritePropertyName("name");
                writer.WriteValue(item.Id);

                writer.WritePropertyName("config");
                serializer.Serialize(writer, item.CurrentConfig.Values
                    .ToDictionary(c => c.GetTypeId(), c =>
                        c.SerializeValue()));

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            
            writer.WriteEndObject();
        }

        public override WorkshopData ReadJson(JsonReader reader, Type objectType, WorkshopData existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var data = new WorkshopData();

            reader.Read();
            reader.Read();
            reader.Read();
            while (reader.TokenType == JsonToken.StartObject)
            {
                var type = "";
                var name = "";
                Dictionary<string, ConfigValue> config = [];

                reader.Read();
                while (reader.TokenType == JsonToken.PropertyName)
                {
                    var key = reader.Value as string;
                    
                    switch (key)
                    {
                        case "type":
                            type = reader.ReadAsString();
                            break;
                        case "name":
                            name = reader.ReadAsString();
                            break;
                        case "config":
                            reader.Read();
                            config = DeserializeConfig(serializer.Deserialize<Dictionary<string, string>>(reader));
                            break;
                    }
                    reader.Read();
                }

                var i = WorkshopManager.WorkshopItems[type!].Item2(name);
                i.CurrentConfig = config;
                data.Items.Add(i);
                
                reader.Read();
            }

            reader.Read();

            return data;
        }

        private static Dictionary<string, ConfigValue> DeserializeConfig(Dictionary<string, string> data)
        {
            var config = new Dictionary<string, ConfigValue>();

            foreach (var pair in data)
            {
                config[pair.Key] = ConfigurationManager.DeserializeConfigValue(pair.Key, pair.Value);
            }
            
            return config;
        }
    }
}