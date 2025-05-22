using System.ComponentModel.DataAnnotations;

namespace DoAnWebThiTracNghiem.Models
{
    public class StartEndTimeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var exam = (Exam)validationContext.ObjectInstance;
            if (exam.EndTime <= exam.StartTime)
            {
                return new ValidationResult("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");
            }
            return ValidationResult.Success;
        }
    }
}