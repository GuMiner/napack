using System.Collections.Generic;
using System.IO;
using System.Text;
using Napack.Common;

namespace Napack.Client
{
    /// <summary>
    /// Defines the Napack .targets file, stored as XML
    /// </summary>
    public class NapackTargets
    {
        private const string NapackTargetsFilename = "napack.targets";
        
        public static void SaveNapackTargetsFile(string napackDirectory, List<NapackVersionIdentifier> currentNapacks)
        {
            StringBuilder fileBuilder = new StringBuilder();
            fileBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");

            foreach (NapackVersionIdentifier napack in currentNapacks)
            {
                // TODO remove code duplication here.
                fileBuilder.AppendLine("<Import Project=\"" + napack.GetFullName() + "\\" + 
                    napack.NapackName + "_" + napack.Major + "_" + napack.Minor + "_" + napack.Patch + ".targets\">");
            }

            File.WriteAllText(Path.Combine(napackDirectory, NapackTargets.NapackTargetsFilename), fileBuilder.ToString());
        }
    }
}
