using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition.Hosting;

namespace RecommendedExtensions.Core.Dialogs
{
    /// <summary>
    /// Dialog provider of user input values.
    /// </summary>
    public static class ValueProvider
    {
        /// <summary>
        /// Gets the search pattern for <see cref="DirectoryCatalog"/>from user.
        /// </summary>
        /// <param name="oldPattern">The old pattern.</param>
        /// <returns>System.String.</returns>
        public static string GetSearchPattern(string oldPattern)
        {
            return GetValue("Specify searching pattern for DirectoryCatalog", "Library searching pattern", oldPattern);
        }

        /// <summary>
        /// Gets the value from user's input.
        /// </summary>
        /// <param name="hintText">The hint text.</param>
        /// <param name="dialogCaption">The dialog caption.</param>
        /// <param name="oldValue">The old value.</param>
        /// <returns>User's input.</returns>
        public static string GetValue(string hintText, string dialogCaption, string oldValue = null)
        {
            var inputValue = Microsoft.VisualBasic.Interaction.InputBox(hintText, dialogCaption, oldValue);

            return inputValue;
        }

    }
}
