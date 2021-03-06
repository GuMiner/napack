﻿using System;
using System.IO;
using Napack.Common;

namespace Napack.Client
{
    public class NapackClient
    {
        public const int ERROR = 1;
        public const int SUCCESS = 0;

        /// <summary>
        /// Support custom logging operations for tests and other scenarios where we want the output programmatically
        /// </summary>
        public static Action<string> Log { get; set; } = (line) => Console.WriteLine(line);

        public static string GetDefaultCredentialFilePath()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            path = Environment.OSVersion.Version.Major >= 6 ? Directory.GetParent(path).ToString() : path;
            return Path.Combine(path, "NapackDefaultCredentials.json");
        }

        public static int Main(string[] args)
        {
            Serializer.Setup();

            // Determine what operation to perform.
            INapackOperation operation = NapackOperationFinder.FindOperation(args);
            if (operation == null)
            {
                NapackOperationFinder.WriteGeneralUsageToConsole();
                return ERROR;
            }

            try
            {
                operation.PerformOperation();
            }
            catch (Exception ex)
            {
                NapackClient.Log(ex.Message);
                return ERROR;
            }

            return SUCCESS;
        }
    }
}
