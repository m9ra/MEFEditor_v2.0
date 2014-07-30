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
        public static bool CheckInstall()
        {
            return !Directory.Exists(ExtensionPath);
        }

        /// <summary>
        /// Create extensions folder and install recommended extensions.
        /// </summary>
        public static void Install()
        {
            //Create extensions folder
            var extFolder = ExtensionPath;
            createExtensionsDirectory(extFolder);
            writeRecommendedExtensions(extFolder);
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
        /// <param name="extFolder">The ext folder.</param>
        private static void writeRecommendedExtensions(string extFolder)
        {
            FileStream fileStream = null;
            try
            {
                /*    var extensionsBinary = Resources.Recommended_Extensions;
                    var extensionOutput = extFolder + "Recommended_Extensions.dll";

                    fileStream = new FileStream(extensionOutput, FileMode.Create);
                    using (var writer = new BinaryWriter(fileStream))
                    {
                        fileStream = null;
                        writer.Write(extensionsBinary);
                    }*/

                //TODO Install extensions
            }
            catch (Exception ex)
            {
                showExceptionError("Cannot write extensions into extensions folder, because of following exception", ex);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Dispose();
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
