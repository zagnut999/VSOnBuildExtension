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
        private readonly IVsOutputWindowPane _buildPane;
        private readonly WritableSettingsStore _writableSettingsStore;
        private readonly string _collectionPath;

        private string CollectionPath
        {
            get { return _collectionPath + "IISReset"; }
        }

        private const string EnabledPropertyName = "Enabled";

        private MenuCommand _menuItem;
        private Boolean _alreadyRan;

        private bool IsEnabled 
        {
            get
            {
                var enabled = false;

                if (_writableSettingsStore.CollectionExists(CollectionPath) && _writableSettingsStore.PropertyExists(CollectionPath, EnabledPropertyName))
                {
                    enabled = _writableSettingsStore.GetBoolean(CollectionPath, EnabledPropertyName);
                }

                return enabled;
            }
            set
            {
                if (!_writableSettingsStore.CollectionExists(CollectionPath))
                    _writableSettingsStore.CreateCollection(CollectionPath);
                _writableSettingsStore.SetBoolean(CollectionPath, EnabledPropertyName, value);
            }
        }

        internal OnBuildCommand(DTE2 dte, IVsOutputWindowPane buildPane, WritableSettingsStore writeableSettingsStore, string collectionPath)
        {
            _buildPane = buildPane;
            if (_buildPane != null) _buildPane.Activate(); // Adds the pane to the output if its not there and brings this pane into view

            _writableSettingsStore = writeableSettingsStore;
            _collectionPath = collectionPath;

            dte.Events.BuildEvents.OnBuildBegin += OnBuildBegin;
            dte.Events.BuildEvents.OnBuildProjConfigDone += (project, projectConfig, platform, solutionConfig, success) => _alreadyRan = false;
            
        }

        private void OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            if (!IsEnabled || _alreadyRan) return;

            _alreadyRan = true;

            WriteToPane("INFO: Starting IIS Reset....\n");
            
            var output = DoIisReset();

            if (output.Contains("administrator"))
                WriteToPane("ERROR: Requires admin rights.\n");
            else
                WriteToPane(output);
            WriteToPane("INFO: Finishing IIS Reset....\n");
            
        }

        private static string DoIisReset()
        {
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
            return output;
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

        private void WriteToPane(string message)
        {
            if (_buildPane != null)
                _buildPane.OutputStringThreadSafe(message);
        }
    }
}
