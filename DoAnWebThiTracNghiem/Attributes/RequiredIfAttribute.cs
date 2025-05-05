using System.ComponentModel.DataAnnotations;
using System.Reflection;
namespace DoAnWebThiTracNghiem.Attributes
{
    

    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _conditionProperty;
        private readonly object _expectedValue;

        public RequiredIfAttribute(string conditionProperty, object expectedValue)
        {
            _conditionProperty = conditionProperty;
            _expectedValue = expectedValue;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Lấy giá trị của thuộc tính điều kiện
            var conditionProperty = validationContext.ObjectType.GetProperty(_conditionProperty);
            if (conditionProperty == null)
            {
                return new ValidationResult($"Property '{_conditionProperty}' not found.");
            }

            var conditionValue = conditionProperty.GetValue(validationContext.ObjectInstance);

            // Kiểm tra nếu giá trị của thuộc tính điều kiện khớp với giá trị mong đợi
            if (conditionValue?.Equals(_expectedValue) == true && value == null)
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
            }

            return ValidationResult.Success;
        }
    }

}
