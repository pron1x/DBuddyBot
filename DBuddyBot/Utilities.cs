using System.Globalization;

namespace DBuddyBot
{
    internal static class Utilities
    {
        #region constants
        private static CultureInfo _cultureInfo = new("en-US", false);
        private static TextInfo _textInfo = _cultureInfo.TextInfo;
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
