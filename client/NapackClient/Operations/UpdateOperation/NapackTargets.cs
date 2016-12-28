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
        
        public static void SaveNapackTargetsFile(string projectDirectory, string napackDirectory, List<NapackVersionIdentifier> currentNapacks)
        {
            StringBuilder fileBuilder = new StringBuilder();
            fileBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            fileBuilder.AppendLine("<Project ToolsVersion=\"14.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");

            string directoryRelation = PathUtilities.GetRelativePath(projectDirectory, napackDirectory);
            foreach (NapackVersionIdentifier napack in currentNapacks)
            {
                fileBuilder.AppendLine($"  <Import Project=\"{directoryRelation}\\{napack.GetFullName()}\\{napack.GenerateTargetName()}.targets\" />");
            }

            fileBuilder.AppendLine("</Project>");
            File.WriteAllText(Path.Combine(projectDirectory, NapackTargets.NapackTargetsFilename), fileBuilder.ToString());
        }
    }
}
