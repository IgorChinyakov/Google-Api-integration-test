using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.Shared
{
    public static class Errors
    {
        public static Error Validation(string? propertyName = null)
        {
            var label = propertyName ?? "value";
            return Error.Validation("value.is.invalid", $"{label} is invalid", propertyName ?? label);
        }

        public static Error NotFound(string? propertyName = null)
        {
            var label = propertyName ?? "value";
            return Error.Validation("value.not.found", $"{label} not found", propertyName ?? label);
        }

        public static Error Conflict(string? propertyName = null)
        {
            var label = propertyName == null ? "value" : propertyName;
            return Error.Conflict("already.exists", $"{label} already exists");
        }

        public static Error Unauthorized()
        {
            return Error.Unauthorized("authorization.failure", $"Failed to authorize user");
        }
    }
}
