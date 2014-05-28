using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace CleverMonkeys.VSOnBuildExtension
{
    public class ToolsOptions : DialogPage
    {
        public bool ShouldRunIisReset { get; set; }
        public bool ShouldLogToBuildOutput { get; set; }
    }
}
