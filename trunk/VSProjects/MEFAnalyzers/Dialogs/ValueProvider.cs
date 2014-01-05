using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFAnalyzers.Dialogs
{
    public static class ValueProvider
    {
        public static string GetSearchPattern(string oldPattern)
        {
            return GetValue("Specify searching pattern for DirectoryCatalog", "Library searching pattern", oldPattern);
        }

        public static string GetValue(string hintText, string dialogCaption, string oldValue = null)
        {
            var inputValue = Microsoft.VisualBasic.Interaction.InputBox(hintText, dialogCaption, oldValue);

            return inputValue;
        }

    }
}
