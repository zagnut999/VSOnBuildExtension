using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

namespace CleverMonkeys.VSOnBuildExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(ToolsOptions), ToolsOptionName, ToolsOptionSubSection, 101, 106, true)]
    [Guid(GuidList.guidVSOnBuildExtensionPkgString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionBuilding)]  //UIContextGuids80.SolutionHasMultipleProjects
    public sealed class VsOnBuildExtensionPackage : Package
    {
        private const string ToolsOptionName = "OnBuild Extension";
        private const string ToolsOptionSubSection = "General";
        
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VsOnBuildExtensionPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        private OnBuildCommand _onBuildCommand;

        private DTE2 _dte;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this));
            base.Initialize();

            _dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;
            var buildPane = GetBuildPane();
            // Add our command handlers for menu (commands must exist in the .vsct file)
            _onBuildCommand = new OnBuildCommand(_dte, buildPane);

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                var menuCommandId = new CommandID(GuidList.guidVSOnBuildExtensionCmdSet, (int)PkgCmdIDList.cmdidIISReset);
                var menuItem = new MenuCommand(_onBuildCommand.MenuItemCallback, menuCommandId );
                _onBuildCommand.ManageMenuItem(menuItem);
                mcs.AddCommand( menuItem );
            }
        }
        #endregion

        private static IVsOutputWindowPane GetBuildPane()
        {
            var outWindow = GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            var buildPaneGuid = VSConstants.GUID_BuildOutputWindowPane;//.GUID_OutWindowGeneralPane; // P.S. There's also the GUID_OutWindowDebugPane available.
            IVsOutputWindowPane buildPane;
            outWindow.GetPane(ref buildPaneGuid, out buildPane);
            return buildPane;
        }

        private void Test()
        {
            var outWindow = GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            var buildPaneGuid = VSConstants.GUID_BuildOutputWindowPane;//.GUID_OutWindowGeneralPane; // P.S. There's also the GUID_OutWindowDebugPane available.
            IVsOutputWindowPane buildPane;
            outWindow.GetPane(ref buildPaneGuid, out buildPane);

            buildPane.OutputString("Hello World!\n");
            buildPane.Activate(); // Adds the pane to the output if its not there and brings this pane into view

            var shouldRunIisReset = false;
            var shouldLogToBuildOutput = false;
            var obj = GetAutomationObject(string.Format("{0}.{1}", ToolsOptionName, ToolsOptionSubSection));

            var options = obj as ToolsOptions;
            if (options != null)
            {
                shouldLogToBuildOutput = options.ShouldLogToBuildOutput;
                shouldRunIisReset = options.ShouldRunIisReset;
            }

            buildPane.OutputString(string.Format("LogToBuildOutput {0}\n", shouldLogToBuildOutput));
            buildPane.OutputString(string.Format("Run IISRESET {0}\n", shouldRunIisReset));

            //This requires admin
            var p = new Process
            {
                StartInfo = 
                { 
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = Environment.SystemDirectory + @"\iisreset.exe" 
                }
            };
            // Redirect the output stream of the child process.
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            p.WaitForExit();
            
            //var proc = System.Diagnostics.Process.Start(Environment.SystemDirectory + @"\iisreset.exe");
            //var output = proc.StandardOutput.ReadToEnd();

            buildPane.OutputString(output);
            buildPane.OutputString(error);

            //var proc = new Process();
            //proc.StartInfo.FileName = "iisreset.exe";
            //proc.Start();

            
        }

    }
}
