using Newtonsoft.Json.Linq;

namespace MigrationHelper.MigrationHelper.Utils
{
    /// <summary>
    /// Helper class for JSON-related utility functions.
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Determines if the provided JSON string is an empty object or array, or is null/whitespace.
        /// </summary>
        /// <param name="jsonString">The JSON string to check.</param>
        /// <returns>True if the JSON string is an empty object, empty array, or null/whitespace; otherwise, false.</returns>
        public static bool IsEmptyJsonObject(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return true;

            jsonString.Trim();
            var token = JToken.Parse(jsonString);

            switch (token.Type)
            {
                case JTokenType.Object:
                    return !token.HasValues;
                case JTokenType.Array:
                    return !token.HasValues;
                default:
                    return true;
            }
        }
    }
}
