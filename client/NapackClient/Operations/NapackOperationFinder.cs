using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ookii.CommandLine;

namespace Napack.Client
{
    internal class NapackOperationFinder
    {
        private static readonly IEnumerable<Type> KnownOperations = new List<Type>
        {
            typeof(UpdateOperation),
            typeof(UpdateMetadataOperation),
            typeof(UploadOperation),
            typeof(RegisterOperation),
            typeof(VerifyEmailOperation)
        };

        public static INapackOperation FindOperation(string[] args)
        {
            foreach (Type operationType in NapackOperationFinder.KnownOperations)
            {
                CommandLineParser parser = new CommandLineParser(operationType);
                try
                {
                    INapackOperation operation = parser.Parse(args) as INapackOperation;
                    if (operation.IsValidOperation())
                    {
                        // This check is performed because there's the potential for two operations to have identical arguments, but be different.
                        return operation;
                    }
                }
                catch { }
            }

            return null;
        }

        public static void WriteGeneralUsageToConsole()
        {
            const string separator = "******";
            NapackClient.Log("Operations: ");
            foreach (Type operationType in NapackOperationFinder.KnownOperations)
            {
                NapackClient.Log(separator + operationType.Name + separator);
                CommandLineParser parser = new CommandLineParser(operationType);

                StringBuilder builder = new StringBuilder();
                using (TextWriter writer = new StringWriter(builder))
                {
                    parser.WriteUsage(writer, 0);
                }

                string[] lines = builder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    NapackClient.Log(line);
                }

                NapackClient.Log(separator + new string('*', operationType.Name.Length) + separator);
            }
        }
    }
}