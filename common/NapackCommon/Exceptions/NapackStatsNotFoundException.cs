using System;

namespace Napack.Common
{
    public class NapackStatsNotFoundException : Exception
    {
        public NapackStatsNotFoundException(string packageId)
            : base($"The Napack statistics for package '{packageId}' do not exist.")
        {
        }
    }
}