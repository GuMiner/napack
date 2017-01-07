using System;

namespace Napack.Common
{
    public class ExcessiveQueriesException : Exception
    {
        public ExcessiveQueriesException()
            : base("This IP has hit the request throttling limit. Please wait and try your query again later.")
        {
        }
    }
}