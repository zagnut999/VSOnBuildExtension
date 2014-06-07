using System;
using EnvDTE80;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;
using Process = System.Diagnostics.Process;

namespace CleverMonkeys.VSOnBuildExtension
{
    internal class OnBuildCommand
    {
        private DTE2 _dte;
        private readonly IVsOutputWindowPane _buildPane;
        private WritableSettingsStore _userSettingsStore;
        private string _collectionPath;

        private string CollectionPath
        {
            get { return _collectionPath + "IISReset"; }
        }

        private string _enabledPropertyName = "Enabled";

        private MenuCommand _menuItem;
        private Boolean _alreadyRan;

        private bool IsEnabled 
        {
            get
            {
                var enabled = false;

                if (_userSettingsStore.CollectionExists(CollectionPath) && _userSettingsStore.PropertyExists(CollectionPath, _enabledPropertyName))
                {
                    enabled = _userSettingsStore.GetBoolean(CollectionPath, _enabledPropertyName);
                }

                return enabled;
            }
            set
            {
                if (!_userSettingsStore.CollectionExists(CollectionPath))
                    _userSettingsStore.CreateCollection(CollectionPath);
                _userSettingsStore.SetBoolean(CollectionPath, _enabledPropertyName, value);
            }
        }

        internal OnBuildCommand(DTE2 dte, IVsOutputWindowPane buildPane, WritableSettingsStore userSettingsStore, string collectionPath)
        {
            _buildPane = buildPane;
            _dte = dte;
            _dte.Events.BuildEvents.OnBuildBegin += OnBuildBegin;
            _dte.Events.BuildEvents.OnBuildProjConfigDone += (project, projectConfig, platform, solutionConfig, success) => _alreadyRan = false;
            _userSettingsStore = userSettingsStore;
            _collectionPath = collectionPath;
        }

        private void OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            if (!IsEnabled || _alreadyRan) return;

            _alreadyRan = true;

            _buildPane.OutputString("INFO: Starting IIS Reset....\n");
            _buildPane.Activate(); // Adds the pane to the output if its not there and brings this pane into view
            
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
            p.Start();
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            if (output.Contains("administrator"))
                _buildPane.OutputString("ERROR: Requires admin rights.\n");
            else
                _buildPane.OutputString(output);
            _buildPane.OutputString("INFO: Finishing IIS Reset....\n");
            
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        internal void MenuItemCallback(object sender, EventArgs e)
        {
            IsEnabled = !IsEnabled;
            _menuItem.Checked = IsEnabled;
        }

        internal void ManageMenuItem(MenuCommand menuItem)
        {
            _menuItem = menuItem;
            _menuItem.Checked = IsEnabled;
        }
    }
}
