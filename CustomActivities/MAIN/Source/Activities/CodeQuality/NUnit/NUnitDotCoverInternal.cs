//-----------------------------------------------------------------------
// <copyright file="NUnitDotCoverInternal.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Extended;

    internal class NUnitDotCoverInternal : NUnitInternal
    {
        [Browsable(true)]
        [Description("Path to dotCover coverage tool")]
        public InArgument<string> CoverageToolPath { get; set; }

        [Browsable(true)]
        [Description("True if dotCover should resolve paths in TargetArguments")]
        public InArgument<bool> AnalyzeTargetArguments { get; set; }

        [Browsable(true)]
        [Description("Path to dotCover output file")]
        public InArgument<string> CoverageOutputFile { get; set; }

        [Browsable(true)]
        [Description("Path to dotCover output XML file")]
        public InArgument<string> CoverageOutputXmlFile { get; set; }

        [Browsable(true)]
        [Description("Path to dotCover output HTML file")]
        public InArgument<string> CoverageOutputHtmlFile { get; set; }

        [Browsable(true)]
        [Description("Path to dotCover configuration file")]
        public InArgument<string> ConfigFile { get; set; }

        [Browsable(true)]
        [Description("Path to dotCover working directory")]
        public InArgument<string> TargetWorkingDirectory { get; set; }

        protected override string GenerateFullPathToTool(ActivityContext context)
        {
            return this.CoverageToolPath.Get(context);
        }

        protected override string GetWorkingDirectory()
        {
            return this.TargetWorkingDirectory.Get(this.ActivityContext);
        }

        protected override string GenerateCommandLineCommands(ActivityContext context, string outputFolder)
        {
/* C:\Program Files (x86)\JetBrains\dotCover\v2.0\Bin\dotCover.exe 
  cover 
  "C:\Builds\3\67\Sources\dotCover.config" 
 /AnalyzeTargetArguments=False 
 /TargetExecutable="C:\Builds\3\67\Sources\packages\NUnit.2.5.10.11092\tools\nunit-console.exe"
 /TargetWorkingDir="C:\Builds\3\67\Binaries"
 /Output=C:\Builds\3\67\Binaries\DotCoverReport\Coverage.bin 
 /TargetArguments="/NoLogo /include=\"-Slow\" /framework=\"net-4.0\" \"C:\Builds\3\67\Binaries\Tests.dll\""
 */
            var builder = new SimpleCommandLineBuilder();

            builder.AppendSwitch("cover");

            builder.AppendFileNamesIfNotNull(new[] { this.ConfigFile.Get(context) }, string.Empty);

            builder.AppendSwitch(this.AnalyzeTargetArguments.Get(context) ? "/AnalyzeTargetArguments=True" : "/AnalyzeTargetArguments=False");

            builder.AppendSwitchIfNotNull("/TargetWorkingDir=", this.TargetWorkingDirectory.Get(context));
            builder.AppendSwitchIfNotNull("/Output=", this.CoverageOutputFile.Get(context));

            // path to NUnit
            builder.AppendSwitchIfNotNull("/TargetExecutable=", base.GenerateFullPathToTool(context));

            // NUnit arguments
            builder.AppendSwitchIfNotNull("/TargetArguments=", base.GenerateCommandLineCommands(context, outputFolder));

            return builder.ToString();
        }
    }
}