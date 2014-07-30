using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Windows.Forms;

namespace MEFEditor.Plugin
{
    /// <summary>
    /// Class which provides installing routines.
    /// </summary>
    internal static class Installer
    {
        /// <summary>
        /// Gets the extension path root.
        /// </summary>
        /// <value>The extension path root.</value>
        public static string ExtensionPath_Root
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }

        /// <summary>
        /// Name of folder with extensions.
        /// </summary>
        public static string ExtensionPath_FolderName
        {
            get
            {
                return "Enhanced MEF Component Architecture Editor";
            }
        }

        /// <summary>
        /// Path where are extensions loaded from in Visual Studio 2010 version.
        /// </summary>
        /// <value>Extension path.</value>
        public static string ExtensionPath_VS2010
        {
            get
            {
                return Path.Combine(ExtensionPath_Root, "Visual Studio 2010", ExtensionPath_FolderName);
            }
        }

        /// <summary>
        /// Path where are extensions loaded from in Visual Studio 2012 version.
        /// </summary>
        /// <value>Extension path.</value>
        public static string ExtensionPath_VS2012
        {
            get
            {
                return Path.Combine(ExtensionPath_Root, "Visual Studio 2012", ExtensionPath_FolderName);
            }
        }

        /// <summary>
        /// Path where are extensions loaded from (according to actual visual studio version).
        /// </summary>
        /// <value>Extension path.</value>
        public static string ExtensionPath
        {
            get
            {
                if (VisualStudioVersionDetector.VS2010)
                {
                    return ExtensionPath_VS2012;
                }

                if (VisualStudioVersionDetector.VS2012)
                {
                    return ExtensionPath_VS2010;
                }

                return null;
            }
        }

        /// <summary>
        /// Determine if extensions should be installed.
        /// </summary>
        /// <returns>Return true if install should be called.</returns>
        public static bool NeedInstall()
        {
            var needInstall = !Directory.Exists(ExtensionPath);
            return needInstall;
        }

        /// <summary>
        /// Create extensions folder and install recommended extensions.
        /// </summary>
        public static void Install()
        {
            //Create extensions folder
            createExtensionsDirectory(ExtensionPath);
            writeRecommendedExtensions();
        }

        /// <summary>
        /// Creates the extensions directory.
        /// </summary>
        /// <param name="extFolder">The ext folder.</param>
        private static void createExtensionsDirectory(string extFolder)
        {
            try
            {
                Directory.CreateDirectory(extFolder);
            }
            catch (Exception ex)
            {
                showExceptionError("Cannot create extension folder, because of following exception.", ex);
            }
        }


        /// <summary>
        /// Writes the recommended extensions.
        /// </summary>
        private static void writeRecommendedExtensions()
        {
            try
            {
                //install recommended extensions
                var extensionPrefix = "RecommendedExtensions.";
                installFile(extensionPrefix + "Core", Resources.RecommendedExtensions_Core);
                installFile(extensionPrefix + "DrawingDefinitions", Resources.RecommendedExtensions_DrawingDefinitions);
                installFile(extensionPrefix + "TypeDefinitions", Resources.RecommendedExtensions_TypeDefinitions);
                installFile(extensionPrefix + "AssemblyProviders", Resources.RecommendedExtensions_AssemblyProviders);

                //install support libraries
                installFile("Utilities", Resources.Utilities);
                installFile("Mono.Cecil", Resources.Mono_Cecil);
            }
            catch (Exception ex)
            {
                showExceptionError("Cannot write extensions into extensions folder, because of following exception", ex);
            }
        }

        /// <summary>
        /// Install given file at given location.
        /// </summary>
        /// <param name="fileName">Name of installed extension.</param>
        /// <param name="fileData">Data of installed extension.</param>
        private static void installFile(string fileName, byte[] fileData)
        {
            var extensionOutput = Path.Combine(ExtensionPath, fileName + ".dll");
            using (var fileStream = new FileStream(extensionOutput, FileMode.Create))
            {
                using (var writer = new BinaryWriter(fileStream))
                {
                    writer.Write(fileData);
                    writer.Close();
                }
            }
        }

        /// <summary>
        /// Shows the exception error.
        /// </summary>
        /// <param name="errorDescription">The error description.</param>
        /// <param name="errorException">The error exception.</param>
        private static void showExceptionError(string errorDescription, Exception errorException)
        {
            MessageBox.Show(errorException.ToString(), errorDescription, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
