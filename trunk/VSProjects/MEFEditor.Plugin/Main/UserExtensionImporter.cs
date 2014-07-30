using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MEFEditor.TypeSystem;

using System.ComponentModel.Composition;

namespace MEFEditor.Plugin.Main
{
    /// <summary>
    /// Importer used for collecting user extensions.
    /// </summary>
    internal class UserExtensionImporter
    {
        /// <summary>
        /// Imported exports.
        /// </summary>
        [ImportMany]
        public IEnumerable<ExtensionExport> Exports = null;
    }
}
