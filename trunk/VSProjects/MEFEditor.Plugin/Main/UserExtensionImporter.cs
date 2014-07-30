using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MEFEditor.TypeSystem;
using MEFEditor.Interoperability;

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

        /// <summary>
        /// Exported services.
        /// </summary>
        [Export]
        public readonly VisualStudioServices VisualStudioServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserExtensionImporter"/> class.
        /// </summary>
        /// <param name="vs">Exported services.</param>
        internal UserExtensionImporter(VisualStudioServices vs)
        {
            VisualStudioServices = vs;
        }
    }
}
