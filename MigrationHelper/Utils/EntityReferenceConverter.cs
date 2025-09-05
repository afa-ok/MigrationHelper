using System;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MigrationHelper.MigrationHelper.Utils
{
    /// <summary>
    /// Custom JSON converter for serializing and deserializing EntityReference objects.
    /// </summary>
    public class EntityReferenceConverter : JsonConverter<EntityReference>
    {
        /// <summary>
        /// Reads JSON and converts it into an EntityReference object.
        /// </summary>
        /// <param name="reader">The JsonReader to read from.</param>
        /// <param name="objectType">The type of the object to convert.</param>
        /// <param name="existingValue">The existing value of the object being read.</param>
        /// <param name="hasExistingValue">Whether there is an existing value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>An EntityReference object or null if the JSON is null.</returns>
        public override EntityReference ReadJson(
            JsonReader reader,
            Type objectType,
            EntityReference existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            // Handle JSON null → C# null
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jsonObject = JObject.Load(reader);

            var id = jsonObject["Id"]?.ToObject<Guid>() ?? Guid.Empty;
            var logicalName = jsonObject["LogicalName"]?.ToString();
            var name = jsonObject["Name"]?.ToString();

            return new EntityReference(logicalName, id) { Name = name };
        }

        /// <summary>
        /// Writes an EntityReference object as JSON, since the XRM SDK does not support direct serialization.
        /// </summary>
        /// <param name="writer">The JsonWriter to write to.</param>
        /// <param name="value">The EntityReference value to write.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, EntityReference value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Id");
            writer.WriteValue(value.Id);
            writer.WritePropertyName("LogicalName");
            writer.WriteValue(value.LogicalName);
            writer.WritePropertyName("Name");
            writer.WriteValue(value.Name);
            writer.WriteEndObject();
        }
    }
}
