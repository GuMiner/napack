using System;
using System.IO;
using System.Reflection;

namespace Napack.Common
{
    public class LicenseManagement
    {
        private const string LicenseFormatStringFormatString = "{0}_FS.txt";

        public enum LicenseType
        {
            MIT = 0,
            SimplifiedBSD = 1,
            zLibLibPng = 2,
            ISC = 3,
            WTFPLv2 = 4,
            FreePL = 5,
            PublicDomain = 6,
            CopyLeft = 7, 
            Commercial = 8,
            Other = 9,
        }

        public static string GetLicenseName(LicenseType type)
        {
            switch (type)
            {
                case LicenseType.MIT:
                    return "MIT";
                case LicenseType.SimplifiedBSD:
                    return "Simplified BSD";
                case LicenseType.zLibLibPng:
                    return "zlib/libpng";
                case LicenseType.ISC:
                    return "ISC";
                case LicenseType.WTFPLv2:
                    return "WTFPL Version 2";
                case LicenseType.FreePL:
                    return "Free Public License 1.0.0";
                case LicenseType.PublicDomain:
                    return "Public Domain";
                case LicenseType.CopyLeft:
                case LicenseType.Commercial:
                case LicenseType.Other:
                    return "Other";
                default:
                    throw new NotSupportedException();
            }
        }

        public static bool IsSupportedLicense(LicenseType type)
        {
            switch (type)
            {
                case LicenseType.MIT:
                case LicenseType.SimplifiedBSD:
                case LicenseType.zLibLibPng:
                case LicenseType.ISC:
                case LicenseType.WTFPLv2:
                case LicenseType.FreePL:
                case LicenseType.PublicDomain:
                    return true;
                case LicenseType.CopyLeft:
                case LicenseType.Commercial:
                case LicenseType.Other:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns the license format string, with {0} representing the authors, and {1} the copyright date.
        /// </summary>
        public static string GetLicenseFormatString(LicenseType type)
        {
            switch (type)
            {
                case LicenseType.MIT:
                    return LicenseManagement.ReadManifestResource("MIT_FS.txt");
                case LicenseType.SimplifiedBSD:
                    return LicenseManagement.ReadManifestResource("SimplifiedBSD_FS.txt");
                case LicenseType.zLibLibPng:
                    return LicenseManagement.ReadManifestResource("Zlib-Libpng_FS.txt");
                case LicenseType.ISC:
                    return LicenseManagement.ReadManifestResource("ISC_FS.txt");
                case LicenseType.WTFPLv2:
                    return LicenseManagement.ReadManifestResource("WTFPLv2_FS.txt");
                case LicenseType.FreePL:
                    return LicenseManagement.ReadManifestResource("FPLv1_FS.txt");
                case LicenseType.PublicDomain:
                    return LicenseManagement.ReadManifestResource("PublicDomain_FS.txt");
                case LicenseType.CopyLeft:
                case LicenseType.Commercial:
                case LicenseType.Other:
                default:
                    throw new NotSupportedException();
            }
        }

        private static string ReadManifestResource(string manifestResourceName)
        {
            using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Napack.Common.LicenseFormatStrings." + manifestResourceName)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}