using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace NapackExtension
{
    /// <summary>
    /// Handles all Napack commands.
    /// </summary>
    internal sealed class NapackCommands
    {
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e441a6ff-58c4-4d48-9b86-a8ec0fb35105");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="NapackCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private NapackCommands(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            commandService?.AddCommand(new MenuCommand(this.FindCallback, new CommandID(CommandSet, 0x0100)));
            commandService?.AddCommand(new MenuCommand(this.CreateCallback, new CommandID(CommandSet, 0x0101)));
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static NapackCommands Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider => this.package;

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new NapackCommands(package);
        }

        /// <summary>
        /// When the menu item or hotkey selects the find selection, show the search pane.
        /// </summary>
        private void FindCallback(object sender, EventArgs e)
        {
            NapackCommandsPackage.Instance.ShowSearchPane();
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void CreateCallback(object sender, EventArgs e)
        {
            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                "This functionality will be pulled from the Napack Client in the *next* version of the extension."
                    + Environment.NewLine + Environment.NewLine
                    + "For now, please use the Napack Client.",
                "Create a Napack",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
