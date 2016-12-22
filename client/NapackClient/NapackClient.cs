using System;
using Napack.Common;

namespace Napack.Client
{
    public class NapackClient
    {
        public const int ERROR = 1;
        public const int SUCCESS = 0;

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
                Console.Error.WriteLine(ex.Message);
                return ERROR;
            }

            return SUCCESS;
        }
    }
}
