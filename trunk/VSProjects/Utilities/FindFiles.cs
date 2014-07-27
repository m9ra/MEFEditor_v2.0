using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;


namespace Utilities
{
    /// <summary>
    /// This class is based on sources available here: http://stackoverflow.com/questions/652037/how-do-i-check-if-a-filename-matches-a-wildcard-pattern
    /// </summary>
    public static class FindFiles
    {
        /// <summary>
        /// Regex for testing question marks presence.
        /// </summary>
        private static Regex HasQuestionMarkRegex = new Regex(@"\?", RegexOptions.Compiled);

        /// <summary>
        /// Regex for testing illegal characters presence.
        /// </summary>
        private static Regex IlegalCharactersRegex = new Regex("[" + @"\/:<>|" + "\"]", RegexOptions.Compiled);

        /// <summary>
        /// The catch extention regex
        /// </summary>
        private static Regex CatchExtentionRegex = new Regex(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Pattern for testing presence of non dots
        /// </summary>
        private static string NonDotCharacters = @"[^.]*";


        /// <summary>
        /// Filters the specified files according to given pattern.
        /// </summary>
        /// <param name="files">The files to be filtered.</param>
        /// <param name="pattern">The pattern for filtering.</param>
        /// <returns>Filtered files.</returns>
        public static IEnumerable<string> Filter(IEnumerable<string> files, string pattern)
        {
            if (pattern == null || files == null)
                yield break;

            var regex = PatternToRegex(pattern);

            foreach (var file in files)
            {
                if (regex.IsMatch(file))
                    yield return file;
            }
        }

        /// <summary>
        /// Emulates behaviour of Directory.GetFiles filtering.
        /// </summary>
        /// <param name="pattern">Search pattern.</param>
        /// <returns>Regular expression that can be used for filtering.</returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">
        /// Pattern is empty.
        /// or
        /// Patterns contains ilegal characters.
        /// </exception>
        public static Regex PatternToRegex(string pattern)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException();
            }
            pattern = pattern.Trim();
            if (pattern.Length == 0)
            {
                throw new ArgumentException("Pattern is empty.");
            }
            if (IlegalCharactersRegex.IsMatch(pattern))
            {
                throw new ArgumentException("Patterns contains ilegal characters.");
            }
            bool hasExtension = CatchExtentionRegex.IsMatch(pattern);
            bool matchExact = false;
            if (HasQuestionMarkRegex.IsMatch(pattern))
            {
                matchExact = true;
            }
            else if (hasExtension)
            {
                matchExact = CatchExtentionRegex.Match(pattern).Groups[1].Length != 3;
            }
            string regexString = Regex.Escape(pattern);
            regexString = "^" + Regex.Replace(regexString, @"\\\*", ".*");
            regexString = Regex.Replace(regexString, @"\\\?", ".");
            if (!matchExact && hasExtension)
            {
                regexString += NonDotCharacters;
            }
            regexString += "$";
            Regex regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return regex;
        }
    }
}
