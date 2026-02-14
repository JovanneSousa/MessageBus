using System.ComponentModel.DataAnnotations;

namespace auth.Infra.Messages
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
