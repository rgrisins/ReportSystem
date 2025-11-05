using System.ComponentModel.DataAnnotations;

namespace ReportSystem.Validation
{
    public class MaxDateTodayAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime dateValue)
            {
                return dateValue.Date <= DateTime.Today;
            }
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} cannot be in the future.";
        }
    }
}
