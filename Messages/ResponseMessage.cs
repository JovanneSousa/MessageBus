
using FluentValidation.Results;

namespace Messages
{
    public class ResponseMessage
    {
        public ValidationResult ValidationResult {  get; set; }

        public ResponseMessage(ValidationResult validationResult)
        {
            ValidationResult = validationResult;
        }
    }
}
