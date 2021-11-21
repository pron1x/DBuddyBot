using System.Globalization;

namespace DBuddyBot
{
    internal static class Utilities
    {
        #region constants
        private static readonly CultureInfo _cultureInfo = new("en-US", false);
        private static readonly TextInfo _textInfo = _cultureInfo.TextInfo;
        #endregion constants


        #region internalmethods
        /// <summary>
        /// Extension method to easily turn strings into title case
        /// </summary>
        /// <param name="input"></param>
        /// <returns>String in title case</returns>
        internal static string ToTitleCase(this string input)
        {
            return _textInfo.ToTitleCase(input);
        }

        #endregion internalmethods
    }
}
