using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Shared.Common.Exceptions
{

        public class NotFoundException : Exception
        {
            public NotFoundException(string entityName, object id)
                : base($"{entityName} with id '{id}' was not found.")
            {
            }

            public NotFoundException(string message) : base(message)
            {
            }
        }

        public class ValidationException : Exception
        {
            public List<string> Errors { get; }

            public ValidationException(List<string> errors)
                : base("One or more validation errors occurred.")
            {
                Errors = errors;
            }

            public ValidationException(string error)
                : base(error)
            {
                Errors = new List<string> { error };
            }
        }
        public class BusinessRuleException : Exception
        {
            public BusinessRuleException(string message) : base(message)
            {
            }
        }
        public class ForbiddenException : Exception
        {
            public ForbiddenException(string message = "You do not have permission to perform this action.")
                : base(message)
            {
            }
        }
        public class InsufficientStockException : Exception
        {
            public InsufficientStockException(string productName, int requested, int available)
                : base($"Insufficient stock for '{productName}'. Requested: {requested}, Available: {available}.")
            {
            }
        }
}
