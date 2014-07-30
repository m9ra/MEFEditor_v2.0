using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Diagnostics;

namespace MEFEditor.Plugin
{
    /// <summary>
    /// Visual studio version detector.
    /// Code taken from answer at: http://stackoverflow.com/questions/11082436/detect-the-visual-studio-version-inside-a-vspackage
    /// </summary>
    internal static class VisualStudioVersionDetector
    {
        /// <summary>
        /// The m lock.
        /// </summary>
        static readonly object mLock = new object();

        /// <summary>
        /// The m vs version.
        /// </summary>
        static Version mVsVersion;

        /// <summary>
        /// The m os version.
        /// </summary>
        static Version mOsVersion;

        /// <summary>
        /// Gets the full version.
        /// </summary>
        /// <value>The full version.</value>
        public static Version FullVersion
        {
            get
            {
                lock (mLock)
                {
                    if (mVsVersion == null)
                    {
                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msenv.dll");

                        if (File.Exists(path))
                        {
                            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);

                            string verName = fvi.ProductVersion;

                            for (int i = 0; i < verName.Length; i++)
                            {
                                if (!char.IsDigit(verName, i) && verName[i] != '.')
                                {
                                    verName = verName.Substring(0, i);
                                    break;
                                }
                            }
                            mVsVersion = new Version(verName);
                        }
                        else
                            mVsVersion = new Version(0, 0); // Not running inside Visual Studio!
                    }
                }

                return mVsVersion;
            }
        }

        /// <summary>
        /// Gets the os version.
        /// </summary>
        /// <value>The os version.</value>
        public static Version OSVersion
        {
            get { return mOsVersion ?? (mOsVersion = Environment.OSVersion.Version); }
        }

        /// <summary>
        /// Gets a value indicating whether [v S2012 or later].
        /// </summary>
        /// <value><c>true</c> if [v S2012 or later]; otherwise, <c>false</c>.</value>
        public static bool VS2012OrLater
        {
            get { return FullVersion >= new Version(11, 0); }
        }

        /// <summary>
        /// Gets a value indicating whether [v S2010 or later].
        /// </summary>
        /// <value><c>true</c> if [v S2010 or later]; otherwise, <c>false</c>.</value>
        public static bool VS2010OrLater
        {
            get { return FullVersion >= new Version(10, 0); }
        }

        /// <summary>
        /// Gets a value indicating whether [v S2008 or older].
        /// </summary>
        /// <value><c>true</c> if [v S2008 or older]; otherwise, <c>false</c>.</value>
        public static bool VS2008OrOlder
        {
            get { return FullVersion < new Version(9, 0); }
        }

        /// <summary>
        /// Gets a value indicating whether [v S2005].
        /// </summary>
        /// <value><c>true</c> if [v S2005]; otherwise, <c>false</c>.</value>
        public static bool VS2005
        {
            get { return FullVersion.Major == 8; }
        }

        /// <summary>
        /// Gets a value indicating whether [v S2008].
        /// </summary>
        /// <value><c>true</c> if [v S2008]; otherwise, <c>false</c>.</value>
        public static bool VS2008
        {
            get { return FullVersion.Major == 9; }
        }

        /// <summary>
        /// Gets a value indicating whether [v S2010].
        /// </summary>
        /// <value><c>true</c> if [v S2010]; otherwise, <c>false</c>.</value>
        public static bool VS2010
        {
            get { return FullVersion.Major == 10; }
        }

        /// <summary>
        /// Gets a value indicating whether [v S2012].
        /// </summary>
        /// <value><c>true</c> if [v S2012]; otherwise, <c>false</c>.</value>
        public static bool VS2012
        {
            get { return FullVersion.Major == 11; }
        }
    }
}
