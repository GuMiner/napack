using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Napack.Common;

namespace Napack.Client
{
    /// <summary>
    /// Defines the Napack attributions file that stores all the attributions
    /// </summary>
    internal class NapackAttributions
    {
        private const string NapackAttributionsFileName = "napack.attributions";

        /// <summary>
        /// Saves the napack attributions file by parsing all the licenses of the specified napacks.
        /// </summary>
        internal static void SaveNapackAttributionsFile(string napackDirectory, Dictionary<string, NapackVersion> allNapackVersions)
        {
            const int lineLength = 70;

            StringBuilder fileBuilder = new StringBuilder();
            fileBuilder.AppendLine("======================== Napack Attributions ========================");

            foreach (KeyValuePair<string, NapackVersion> napack in allNapackVersions)
            {
                string napackVersionNameStart = napack.Key + "." + napack.Value.Major + " License Start ";
                fileBuilder.Append(napackVersionNameStart);
                fileBuilder.Append('=', Math.Max(10, lineLength - napackVersionNameStart.Length));
                fileBuilder.AppendLine();
                fileBuilder.AppendLine();

                NapackAttributions.AppendNapackLicense(napack.Value.Authors, napack.Value.License, fileBuilder);

                fileBuilder.AppendLine();
                fileBuilder.AppendLine();
                string napackVersionNameEnd = napack.Key + "." + napack.Value.Major + " License End ";
                fileBuilder.Append(napackVersionNameEnd);
                fileBuilder.Append('=', Math.Max(10, lineLength - napackVersionNameEnd.Length));
                fileBuilder.AppendLine();
            }

            fileBuilder.AppendLine("=====================================================================");
            File.WriteAllText(Path.Combine(napackDirectory, NapackAttributions.NapackAttributionsFileName), fileBuilder.ToString());
        }

        private static void AppendNapackLicense(List<string> authors, License license, StringBuilder fileBuilder)
        {
            string authorsString = string.Join("; ", authors);
            if (license.LicenseType != LicenseManagement.LicenseType.Other)
            {
                string licenseFormatString = LicenseManagement.GetLicenseFormatString(license.LicenseType);
                fileBuilder.Append(string.Format(licenseFormatString, authorsString, DateTime.UtcNow.ToString("YYYY")));
            }
            else
            {
                if (authors.Any())
                {
                    fileBuilder.AppendLine($"Authors: {authorsString}");
                    fileBuilder.AppendLine();
                }

                fileBuilder.AppendLine("Custom license: ");
                fileBuilder.AppendLine();
                fileBuilder.Append(license.LicenseText);
            }
        }
    }
}