namespace MigrationHelper.PluginBase
{
    using System.Linq;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    public static class EnvironmentVariablesHelper
    {
        private static readonly string _fetchEnvVars =
            @"<fetch distinct='true' mapping='logical' output-format='xml-platform' version='1.0'>
                        <entity name='environmentvariabledefinition'>
                            <attribute name='environmentvariabledefinitionid'/>
                            <attribute name='schemaname'/>
                            <attribute name='valueschema'/>
                            <attribute name='type'/>
                            <attribute name='displayname'/>
                            <attribute name='description'/>
                            <attribute name='defaultvalue'/>
                        <link-entity name='environmentvariablevalue' alias='ab' link-type='inner' to='environmentvariabledefinitionid' from='environmentvariabledefinitionid'>
                          <attribute name='value'/>
                        <filter type='and'>
                                <condition attribute='value' operator='not-null'/>
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>";

        private static readonly string _attrSchemaname = "schemaname";
        private static readonly string _attrDefaultValue = "defaultvalue";
        private static readonly string _attrAbValue = "ab.value";

        public static string GetEnvVarValue(string schemaName, IOrganizationService userContext)
        {
            var envVars = userContext.RetrieveMultiple(new FetchExpression(_fetchEnvVars)).Entities.ToList();
            var envVar = envVars.FirstOrDefault(x => x.GetAttributeValue<string>(_attrSchemaname) == schemaName);

            if (envVar == null)
            {
                throw new InvalidPluginExecutionException($"Environment variable with Schemaname {schemaName} not found or no environment variable value set");
            }

            var currentVal = (string)envVar.GetAttributeValue<AliasedValue>(_attrAbValue)?.Value;
            if (!string.IsNullOrWhiteSpace(currentVal))
            {
                return currentVal;
            }

            var defaultVal = envVar.GetAttributeValue<string>(_attrDefaultValue);
            if (!string.IsNullOrWhiteSpace(defaultVal))
            {
                return defaultVal;
            }

            throw new InvalidPluginExecutionException($"Environment variable with Schemaname {schemaName} has no environment variable value set");
        }
    }
}
