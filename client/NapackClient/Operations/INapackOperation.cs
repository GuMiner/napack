using Napack.Client.Common;

namespace Napack.Client
{
    internal interface INapackOperation
    {
        string Operation { get; set; }

        bool IsValidOperation();

        void PerformOperation();
    }
}