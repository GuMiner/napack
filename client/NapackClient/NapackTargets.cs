﻿using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NapackClient
{
    /// <summary>
    /// Defines the Napack .targets file, stored as XML
    /// </summary>
    public class NapackTargets
    {
        private const string napackFilename = "napack.targets";
        
        public static void SaveNapackTargetsFile(string napackDirectory, List<DefinedNapackVersion> currentNapacks)
        {
            StringBuilder fileBuilder = new StringBuilder();
            fileBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");

            foreach (DefinedNapackVersion napack in currentNapacks)
            {
                // TODO remove code duplication here.
                fileBuilder.AppendLine("<Import Project=\"" + napack.GetDirectoryName() + "\\" + 
                    napack.NapackName + "_" + napack.Major + "_" + napack.Minor + "_" + napack.Patch + ".targets\">");
            }

            File.WriteAllText(Path.Combine(napackDirectory, napackFilename), fileBuilder.ToString());
        }
    }
}
