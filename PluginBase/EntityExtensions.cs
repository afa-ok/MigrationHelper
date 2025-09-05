using System.Text.Json;
using Microsoft.Xrm.Sdk;

namespace MigrationHelper.PluginBase
{
    public static class EntityExtensions
    {
        public static string ToFormattedString(this Entity entity)
        {
            if (entity == null) return null;

            var result = $"entity: {entity.LogicalName} id: {entity.Id} --->\r\n";
            foreach (var attributesKey in entity.Attributes.Keys)
            {
                result += $"{attributesKey}: {entity.Attributes[attributesKey]}\r\n";
            }

            return result;
        }

        public static string ToFormattedString(this EntityReference entityReference)
        {
            if (entityReference == null) return null;

            var result = $"entityReference: {entityReference.LogicalName} id: {entityReference.Id} --->\r\n";
            if (entityReference.KeyAttributes != null)
            {
                foreach (var attributesKey in entityReference.KeyAttributes.Keys)
                {
                    result += $"{attributesKey}: {entityReference.KeyAttributes[attributesKey]}\r\n";
                }
            }

            return result;
        }

        public static string ToFormattedString(this EntityCollection entityCollection)
        {
            if (entityCollection == null) return null;

            var result = $"entityCollection: {entityCollection.EntityName} count: {entityCollection.Entities?.Count} more records: {entityCollection.MoreRecords}";

            return result;
        }

        public static string ToFormattedString(this IPluginExecutionContext pluginExecutionContext)
        {
            if (pluginExecutionContext == null) return null;

            var result = $"pluginExecutionContect: {JsonSerializer.Serialize(pluginExecutionContext)}";

            return result;
        }

        public static bool ValidateAttributeIsNotNull(this Entity entity, string attribute,
            ITracingService tracingService)
        {
            if (entity == null)
            {
                tracingService?.Trace($"Entity is null, cannot validate attribute '{attribute}'.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(attribute))
            {
                tracingService?.Trace("Attribute name is null or empty.");
                return false;
            }
            if (!entity.Attributes.ContainsKey(attribute))
            {
                tracingService?.Trace($"Attribute '{attribute}' does not exist in the entity.");
                return false;
            }

            if (entity.Attributes[attribute] == null)
            {
                tracingService?.Trace($"Attribute '{attribute}' is null in the entity.");
                return false;
            }

            return true;
        }
    }
}
