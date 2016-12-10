using Ookii.CommandLine;
using System.ComponentModel;

namespace Napack.Client
{
    [Description("Manages Napacks consumed within the current project.")]
    public class UpdateOperationArguments
    {
        [CommandLineArgument(Position = 0, IsRequired = true)]
        [Description("The JSON file listing the Napacks used by the current project.")]
        public string NapackJson { get; set; }

        [CommandLineArgument(Position = 1, IsRequired = true)]
        [Description("The JSON settings file used to configure how this application runs.")]
        public string NapackSettings { get; set; }

        [CommandLineArgument(Position = 2, IsRequired = true)]
        [Description("The directory storing downloaded Napacks")]
        public string NapackDirectory { get; set; }
    }
}