using System.ComponentModel.DataAnnotations;

namespace DoAnWebThiTracNghiem.Models
{
    public class OptionsValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var options = value as List<string>;
            if (options != null && options.Count < 4)
            {
                return new ValidationResult("Câu hỏi phải có 4 tùy chọn đáp án.");
            }
            return ValidationResult.Success;
        }
    }
}
