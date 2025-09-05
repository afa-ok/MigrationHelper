using System;
using Microsoft.Xrm.Sdk;
using MigrationHelper.MigrationHelper.Models;
using MigrationHelper.MigrationHelper.Utils;
using MigrationHelper.PluginBase;
using Newtonsoft.Json;

namespace MigrationHelper.MigrationHelper
{
    /// <summary>
    ///     MigrationHelperPlugin
    ///
    /// Purpose:
    ///     Overwrites the auditing data (created by/on & modified by/on) in the target CRM
    ///     with the original data from the source CRM. This ensures audit data integrity
    ///     during the CRM Migration process.
    ///
    /// Trigger:
    ///     - Messages: Create, Update
    ///     - Primary Entity: All(!)
    ///     - Filtering Attributes: None
    ///     - Stage: PreOperation
    ///     - Mode: Synchronous
    ///
    /// </summary>
    public class MigrationHelperPlugin : Plugin
    {
        public MigrationHelperPlugin() : base(typeof(MigrationHelperPlugin)) { }

        public MigrationHelperPlugin(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(MigrationHelperPlugin))
        { }

        /// <summary>
        /// Executes the main logic for the MigrationHelperPlugin. Performs audit data overwrite if required.
        /// </summary>
        /// <param name="localPluginContext">The local plugin context containing execution information.</param>
        /// <exception cref="ArgumentNullException">Thrown if localPluginContext is null.</exception>
        /// <exception cref="NullReferenceException">Thrown if the target entity is not found.</exception>
        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            if (localPluginContext == null)
            {
                throw new ArgumentNullException(nameof(localPluginContext));
            }

            if (!localPluginContext.PluginExecutionContext.SharedVariables.ContainsKey(BaseWellKnowns.SharedVar_Tag))
            {
                localPluginContext.Trace($"{GetType().Name}: Plugin called without Shared Variable 'Tag' - Stopping!");
                return;
            }

            // Reference: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/optional-parameters?tabs=sdk#add-a-shared-variable-to-the-plugin-execution-context 
            //   JSON String containing overwrite data for (Created By, Modified By, Modified On)
            string overwriteDataJson = localPluginContext.PluginExecutionContext.SharedVariables[BaseWellKnowns.SharedVar_Tag].ToString();

            if (JsonHelper.IsEmptyJsonObject(overwriteDataJson))
            {
                localPluginContext.Trace($"{GetType().Name}: Shared Variable 'Tag' is missing data: '{overwriteDataJson}'");
                return;
            }
            OverwriteAuditData overwriteData = JsonConvert.DeserializeObject<OverwriteAuditData>(
                overwriteDataJson,
                new JsonSerializerSettings
                {
                    Converters =
                    {
                        new EntityReferenceConverter()
                    }
                });

            // If everything is null, we don't need to do anything
            if (overwriteData.OverriddenCreatedOn is null
                && overwriteData.OverriddenCreatedBy is null
                && overwriteData.OverriddenModifiedOn is null
                && overwriteData.OverriddenModifiedBy is null)
            {
                localPluginContext.Trace($"{GetType().Name}: Shared Variable 'Tag' is only nulls: '{JsonConvert.SerializeObject(overwriteData)}'");
                return;
            }

            // Realistically the plugin really only starts here. Before are just sanity checks.
            localPluginContext.Trace($"{GetType().Name}: Execution started.");

            // Since this Plugin runs on all Entities, we can not perform any Early-Bound-Type-Casting
            Entity targetEntity = localPluginContext.GetTargetEntity<Entity>()
                ?? throw new NullReferenceException($"{GetType().Name} ERROR: no targetEntity");

            localPluginContext.Trace($"{targetEntity.Id}: Starting overwrite of Audit Data.");

            // CREATE or UPDATE
            if (localPluginContext.PluginExecutionContext.MessageName.Equals(BaseWellKnowns.PluginMessage_Create)
                || localPluginContext.PluginExecutionContext.MessageName.Equals(BaseWellKnowns.PluginMessage_Update))
            {
                CreatedFields(targetEntity, localPluginContext, overwriteData);
                ModifiedFields(targetEntity, localPluginContext, overwriteData);
            }
            // Fall-through
            else
            {
                localPluginContext.Trace($"{GetType().Name}: Called with unknown Message type '{localPluginContext.PluginExecutionContext.MessageName}'");
            }

            localPluginContext.TracingService.Trace($"{GetType().Name}: Execution completed.");
        }

        /// <summary>
        /// Overwrites the CreatedBy and CreatedOn fields of the target entity if override data is provided.
        /// </summary>
        /// <param name="targetEntity">The entity whose fields are to be overwritten.</param>
        /// <param name="localPluginContext">The local plugin context for tracing and context.</param>
        /// <param name="overwriteData">The data containing override values for audit fields.</param>
        private void CreatedFields(Entity targetEntity, ILocalPluginContext localPluginContext, OverwriteAuditData overwriteData)
        {
            // Overwrite "createdby" 
            if (targetEntity.Contains(BaseWellKnowns.Audit_CreatedBy) && overwriteData.OverriddenCreatedBy != null)
            {
                localPluginContext.Trace($"{GetType().Name}: CREATED BY: [Before: {targetEntity[BaseWellKnowns.Audit_CreatedBy]}] | [After: {overwriteData.OverriddenCreatedBy}]");

                targetEntity[BaseWellKnowns.Audit_CreatedBy] = overwriteData.OverriddenCreatedBy;
            }

            // Overwrite "createdon"
            if (targetEntity.Contains(BaseWellKnowns.Audit_CreatedOn) && overwriteData.OverriddenCreatedOn != null)
            {
                localPluginContext.Trace($"{GetType().Name}: CREATED ON: [Before: {targetEntity[BaseWellKnowns.Audit_CreatedOn]}] | [After: {overwriteData.OverriddenCreatedOn}]");

                targetEntity[BaseWellKnowns.Audit_CreatedOn] = overwriteData.OverriddenCreatedOn;
            }
        }

        /// <summary>
        /// Overwrites the ModifiedBy and ModifiedOn fields of the target entity if override data is provided.
        /// </summary>
        /// <param name="targetEntity">The entity whose fields are to be overwritten.</param>
        /// <param name="localPluginContext">The local plugin context for tracing and context.</param>
        /// <param name="overwriteData">The data containing override values for audit fields.</param>
        private void ModifiedFields(Entity targetEntity, ILocalPluginContext localPluginContext, OverwriteAuditData overwriteData)
        {
            // Overwrite "modifiedby"
            if (targetEntity.Contains(BaseWellKnowns.Audit_ModifiedBy) && overwriteData.OverriddenModifiedBy != null)
            {
                localPluginContext.Trace($"{GetType().Name}: MODIFIED BY: [Before: {targetEntity[BaseWellKnowns.Audit_ModifiedBy]}] | [After: {overwriteData.OverriddenModifiedBy}]");

                targetEntity[BaseWellKnowns.Audit_ModifiedBy] = overwriteData.OverriddenModifiedBy;
            }

            // Overwrite "modifiedon"
            if (targetEntity.Contains(BaseWellKnowns.Audit_ModifiedOn) && overwriteData.OverriddenModifiedOn != null)
            {
                localPluginContext.Trace($"{GetType().Name}: MODIFIED ON: [Before: {targetEntity[BaseWellKnowns.Audit_ModifiedOn]}] | [After: {overwriteData.OverriddenModifiedOn}]");

                targetEntity[BaseWellKnowns.Audit_ModifiedOn] = overwriteData.OverriddenModifiedOn;
            }
        }
    }
}
