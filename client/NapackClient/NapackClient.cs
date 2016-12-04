using Ookii.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Napack.Common;

namespace Napack.Client
{
    class NapackClient
    {
        private const int ERROR = 1;
        private const int SUCCESS = 0;

        public static int Main(string[] args)
        {
            Serializer.Setup();

            NapackArguments arguments = NapackClient.ParseArguments(args);
            if (arguments == null)
            {
                return ERROR;
            }

            List<NapackVersionIdentifier> napacks = NapackClient.ParseNapackJsonFile(arguments.NapackJson);
            if (napacks == null)
            {
                return ERROR;
            }

            NapackClientSettings clientSettings = NapackClient.ParseNapackSettingsFile(arguments.NapackSettings);
            if (clientSettings == null)
            {
                return ERROR;
            }
            
            try
            {
                using (NapackServerClient client = new NapackServerClient(clientSettings.NapackFrameworkServer))
                {
                    NapackOperation napackOperation = new NapackOperation(client, napacks, clientSettings);
                    napackOperation.AcquireNapacks(arguments.NapackDirectory);

                    // TODO doesn't require client.
                    napackOperation.UpdateTargets(arguments.NapackDirectory);

                    // TODO implement. Given that this definitely needs to use the package metadata, I'll do this when I refactor
                    //  this section of the code to cache the metadata for future runs.
                    // napackOperation.CreateAttributionFile(arguments.NapackDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return ERROR;
            }

            return SUCCESS;
        }

        private static NapackClientSettings ParseNapackSettingsFile(string napackSettingsJsonFile)
        {
            return NapackClient.PerformTryCatchOperation(() =>
            {
                string napackSettingsJson = File.ReadAllText(napackSettingsJsonFile);
                return Serializer.Deserialize<NapackClientSettings>(napackSettingsJson);
            }, () => Console.Error.WriteLine("Error parsing the Napack Settings JSON file!"));
        }

        private static List<NapackVersionIdentifier> ParseNapackJsonFile(string napackJsonFile)
        {
            return NapackClient.PerformTryCatchOperation(() =>
            {
                string napackJson = File.ReadAllText(napackJsonFile);
                Dictionary<string, string> rawNapacks = Serializer.Deserialize<Dictionary<string, string>>(napackJson);
                List<NapackVersionIdentifier> napacks = rawNapacks.Select(item => new NapackVersionIdentifier(item.Key, item.Value)).ToList();
                return napacks;
            }, () => Console.Error.WriteLine("Error parsing the Napack JSON file!"));
        }

        private static NapackArguments ParseArguments(string[] args)
        {
            CommandLineParser parser = new CommandLineParser(typeof(NapackArguments));
            return NapackClient.PerformTryCatchOperation(() =>
            {
                NapackArguments arguments = parser.Parse(args) as NapackArguments;
                return arguments;
            }, () => parser.WriteUsageToConsole());
        }

        /// <summary>
        /// Performs an operation that may fail, returning the operation results or the default for the type.
        /// </summary>
        /// <param name="operation">The operation to perform.</param>
        /// <param name="errorCallback">The function to call when an error is encountered.</param>
        private static T PerformTryCatchOperation<T>(Func<T> operation, Action errorCallback)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                errorCallback();
                Console.Error.WriteLine(ex.Message);
                return default(T);
            }
        }
    }
}
