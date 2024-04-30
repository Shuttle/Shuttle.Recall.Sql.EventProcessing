using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class SqlEventProcessingOptionsValidator : IValidateOptions<SqlEventProcessingOptions>
    {
        public ValidateOptionsResult Validate(string name, SqlEventProcessingOptions options)
        {
            Guard.AgainstNull(options, nameof(options));

            if (string.IsNullOrWhiteSpace(options.ConnectionStringName))
            {
                return ValidateOptionsResult.Fail(Resources.ConnectionStringException);
            }

            return ValidateOptionsResult.Success;
        }
    }
}