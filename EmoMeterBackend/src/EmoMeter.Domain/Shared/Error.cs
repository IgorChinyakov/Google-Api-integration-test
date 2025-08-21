using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmoMeter.Domain.Shared
{
    public class Error
    {
        public string Message { get; set; }

        public string Code { get; set; }

        public string? PropertyName { get; set; }

        public ErrorType ErrorType { get; set; }

        public Error(
            string message,
            string code,
            string? propertyName,
            ErrorType errorType)
        {
            Message = message;
            Code = code;
            PropertyName = propertyName;
            ErrorType = errorType;
        }

        public static Error Validation(string code, string message, string? propertyName = null)
            => new Error(message, code, propertyName, ErrorType.Validation);

        public static Error NotFound(string code, string message, string? propertyName = null)
            => new Error(message, code, propertyName, ErrorType.NotFound);

        public static Error Conflict(string code, string message, string? propertyName = null)
            => new Error(code, message, propertyName, ErrorType.Conflict);

        public static Error Unauthorized(string code, string message)
            => new Error(code, message, null, ErrorType.Unauthorized);

        public static Error Failure(string code, string message)
            => new Error(code, message, null, ErrorType.Failure);

        public static Error Failure(string code, string message, ErrorType errorType)
            => new Error(code, message, null, ErrorType.Failure);

        public ErrorsList ToErrorsList() => new([this]);
    }

    public enum ErrorType
    {
        Validation,
        NotFound,
        Conflict,
        Failure,
        Unauthorized
    }
}
