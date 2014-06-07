using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

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
    [Guid(GuidList.guidVSOnBuildExtensionPkgString)]
    //[ProvideAutoLoad(UIContextGuids80.ToolboxInitialized)]  //UIContextGuids80.SolutionHasMultipleProjects
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
    public sealed class VsOnBuildExtensionPackage : Package
    {
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

        private WritableSettingsStore _userSettingsStore;
        private const string VsOnBuildextensionCollectionPath = @"Clevermonkeys\VsOnBuildExtension\";

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this));
            base.Initialize();

            var settingsManager = new ShellSettingsManager(this);
            _userSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            
            _dte = GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;
            var buildPane = GetBuildPane();
            // Add our command handlers for menu (commands must exist in the .vsct file)
            _onBuildCommand = new OnBuildCommand(_dte, buildPane, _userSettingsStore, VsOnBuildextensionCollectionPath);

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
    }
}
