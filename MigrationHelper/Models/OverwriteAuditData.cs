using System;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;

namespace MigrationHelper.MigrationHelper.Models
{
    public class OverwriteAuditData
    {
        [JsonProperty("_overriddencreatedby")]
        public EntityReference OverriddenCreatedBy { get; set; }

        [JsonProperty("_overriddencreatedon")]
        public DateTime? OverriddenCreatedOn { get; set; }

        [JsonProperty("_overriddenmodifiedby")]
        public EntityReference OverriddenModifiedBy { get; set; }

        [JsonProperty("_overriddenmodifiedon")]
        public DateTime? OverriddenModifiedOn { get; set; }
    }
}
