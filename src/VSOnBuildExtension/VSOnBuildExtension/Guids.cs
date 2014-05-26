// Guids.cs
// MUST match guids.h
using System;

namespace CleverMonkeys.VSOnBuildExtension
{
    static class GuidList
    {
        public const string guidVSOnBuildExtensionPkgString = "66e736a8-e261-4e34-8ffa-a98e9526c172";
        public const string guidVSOnBuildExtensionCmdSetString = "8c78af30-24da-49a4-b025-7cbd0c8ee2cf";

        public static readonly Guid guidVSOnBuildExtensionCmdSet = new Guid(guidVSOnBuildExtensionCmdSetString);
    };
}