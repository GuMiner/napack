using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NapackExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0")] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SearchFormToolWindow), Style = VsDockStyle.Float, PositionX = 0, PositionY = 0, Width = 200, Height = 100, Orientation = ToolWindowOrientation.none, Transient = true)]
    [ProvideOptionPage(typeof(NapackExtensionOptionPane), "Napack Extension", "Extension Settings", 0, 0, true)]
    [Guid(NapackCommandsPackage.PackageGuidString)]
    public sealed class NapackCommandsPackage : Package
    {
        public const string PackageGuidString = "17bf7a7f-1bc8-4bee-8cdd-f110fc1e5425";

        public static NapackCommandsPackage Instance { get; private set; }

        public static NapackExtensionOptionPane Options
            => Instance.GetDialogPage(typeof(NapackExtensionOptionPane)) as NapackExtensionOptionPane;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NapackCommands"/> class.
        /// </summary>
        public NapackCommandsPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            NapackCommandsPackage.Instance = this;
        }

        public void ShowSearchPane()
        {
            ToolWindowPane toolWindow = this.FindToolWindow(typeof(SearchFormToolWindow), 0, true);
            if (toolWindow == null || toolWindow.Frame == null)
            {
                throw new NotSupportedException("The Napack VSIX package is invalid and cannot perform the requested operation.");
            }
            
            ErrorHandler.ThrowOnFailure((toolWindow.Frame as IVsWindowFrame)?.Show() ?? -1);
        }

        public void AddNapackToProject(string napackName, string mostRecentVersion)
        {
            try
            {
                // Find active project.
                DTE dte = Package.GetGlobalService(typeof(DTE)) as DTE;
                TextDocument activeDoc = dte.ActiveDocument.Object() as TextDocument;
                Project project = activeDoc.Parent.ProjectItem.ContainingProject;

                // Find Napacks.json file.
                string projectShortName = Path.GetFileName(project.FullName);
                string napackFileName = Path.Combine(Path.GetDirectoryName(project.FullName), "Napacks.json");
                if (!File.Exists(napackFileName))
                {
                    throw new Exception($"Found the '{projectShortName}' project, but not the Napacks.json file.");
                }
                
                // Add the item to the Napacks.json file.
                string napackFile = File.ReadAllText(napackFileName);
                Dictionary<string, string> napacks = JsonConvert.DeserializeObject<Dictionary<string, string>>(napackFile);
                if (napacks.ContainsKey(napackName))
                {
                    VsShellUtilities.ShowMessageBox(this,
                        $"The {projectShortName} project already contains the ${napackName} napack.",
                        "Napack already added",
                        OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
                else
                {
                    napacks.Add(napackName, mostRecentVersion);
                    File.WriteAllText(napackFileName, JsonConvert.SerializeObject(napacks, Formatting.Indented));

                    VsShellUtilities.ShowMessageBox(this,
                        $"Added the ${napackName} napack to the {projectShortName} project.",
                        "Napack added",
                        OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(this,
                    "Could not find the project to add a Napack to. Is your active document in the correct project? " + 
                        Environment.NewLine + Environment.NewLine + ex.Message,
                    "Unable to add Napack",
                    OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            NapackCommands.Initialize(this);
            base.Initialize();
        }
    }
}
