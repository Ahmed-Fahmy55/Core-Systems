using System.Text.RegularExpressions;

namespace Zone8.Utilities
{
    public static class TextValidator
    {
        public static bool IsTypeValid(string text, InputType inputType)
        {
            switch (inputType)
            {
                case InputType.AlphaOnly:
                    return Regex.IsMatch(text, @"^[a-zA-Z]+$");
                case InputType.NumericOnly:
                    return Regex.IsMatch(text, @"^\d+$");
                case InputType.AlphaNumeric:
                    return Regex.IsMatch(text, @"^[a-zA-Z0-9]+$");
                case InputType.Email:
                    return Regex.IsMatch(text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                default:
                    return true;
            }
        }

        public enum InputType
        {
            Any,
            AlphaOnly,
            NumericOnly,
            AlphaNumeric,
            Email
        }
    }
}
