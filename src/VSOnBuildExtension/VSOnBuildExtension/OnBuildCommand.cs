using System;
using EnvDTE80;
using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using Process = System.Diagnostics.Process;

namespace CleverMonkeys.VSOnBuildExtension
{
    public class OnBuildCommand
    {
        private DTE2 _dte;
        private MenuCommand _menuItem;
        private Boolean _alreadyRan;
        private readonly IVsOutputWindowPane _buildPane;

        private bool Enabled 
        {
            get { return _menuItem != null && _menuItem.Checked; }
        }

        public OnBuildCommand(DTE2 dte, IVsOutputWindowPane buildPane)
        {
            _buildPane = buildPane;
            _dte = dte;
            _dte.Events.BuildEvents.OnBuildBegin += OnBuildBegin;
            _dte.Events.BuildEvents.OnBuildProjConfigDone += (project, projectConfig, platform, solutionConfig, success) => _alreadyRan = false;
        }

        private void OnBuildBegin(vsBuildScope scope, vsBuildAction action)
        {
            if (!Enabled || _alreadyRan) return;

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
            var error = p.StandardError.ReadToEnd();
            p.WaitForExit();

            _buildPane.OutputString(output);
            _buildPane.OutputString(error);
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        public void MenuItemCallback(object sender, EventArgs e)
        {
            _menuItem.Checked = !_menuItem.Checked;
        }

        internal void ManageMenuItem(MenuCommand menuItem)
        {
            _menuItem = menuItem;
            _menuItem.Checked = true;
        }
    }
}
