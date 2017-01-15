using System;
using System.Collections.Generic;
using Ookii.CommandLine;

namespace Napack.Client
{
    internal class NapackOperationFinder
    {
        private static readonly IEnumerable<Type> KnownOperations = new List<Type>
        {
            typeof(UpdateOperation),
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
            Console.WriteLine("Operations: ");
            foreach (Type operationType in NapackOperationFinder.KnownOperations)
            {
                Console.WriteLine(separator + operationType.Name + separator);
                CommandLineParser parser = new CommandLineParser(operationType);
                parser.WriteUsageToConsole();
                Console.WriteLine(separator + new string('*', operationType.Name.Length) + separator);
            }
        }
    }
}