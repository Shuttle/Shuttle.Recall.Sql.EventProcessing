using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingValidator : IValidateOptions<EventProcessingOptions>
    {
        public ValidateOptionsResult Validate(string name, EventProcessingOptions options)
        {
            Guard.AgainstNull(options, nameof(options));

            if (string.IsNullOrWhiteSpace(options.EventStoreConnectionStringName))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.ConnectionStringEmptyException,
                    "EventStoreConnectionStringName"));
            }

            if (string.IsNullOrWhiteSpace(options.EventProjectionConnectionStringName))
            {
                return ValidateOptionsResult.Fail(string.Format(Resources.ConnectionStringEmptyException,
                    "EventProjectionConnectionStringName"));
            }

            return ValidateOptionsResult.Success;
        }
    }
}