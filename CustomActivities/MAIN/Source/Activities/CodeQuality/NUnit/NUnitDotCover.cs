//-----------------------------------------------------------------------
// <copyright file="NUnitDotCover.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Microsoft.TeamFoundation.Build.Client;

    /// <summary>
    /// Run NUnit using dotCover for code coverage
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [Description("Activity to run NUnit with dotCover as part of a TFS Build")]
    public class NUnitDotCover : NUnit
    {
        /// <summary>
        /// Create instance
        /// </summary>
        public NUnitDotCover()
        {
            this.CoverageToolPath = @"C:\Program Files (x86)\JetBrains\dotCover\v2.0\Bin\dotCover.exe";
        }

        /// <summary>
        /// Path to dotCover
        /// </summary>
        [Browsable(true)]
        [Description("Path to dotCover coverage tool")]
        public InArgument<string> CoverageToolPath { get; set; }

        /// <summary>
        /// True if TargetArguments paths are made absolute
        /// </summary>
        [Browsable(true)]
        [Description("True if dotCover should resolve paths in TargetArguments")]
        public InArgument<bool> AnalyzeTargetArguments { get; set; }

        /// <summary>
        /// dotCover output file
        /// </summary>
        [Browsable(true)]
        [Description("Path to dotCover output file")]
        public InArgument<string> CoverageOutputFile { get; set; }

        /// <summary>
        /// XML output file
        /// </summary>
        [Browsable(true)]
        [Description("Path to dotCover output XML file")]
        public InArgument<string> CoverageOutputXmlFile { get; set; }

        /// <summary>
        /// HTML output file
        /// </summary>
        [Browsable(true)]
        [Description("Path to dotCover output HTML file")]
        public InArgument<string> CoverageOutputHtmlFile { get; set; }

        /// <summary>
        /// dotCover config file
        /// </summary>
        [Browsable(true)]
        [Description("Path to dotCover configuration file")]
        public InArgument<string> ConfigFile { get; set; }

        /// <summary>
        /// dotCover working directory
        /// </summary>
        [Browsable(true)]
        [Description("Path to dotCover working directory")]
        public InArgument<string> TargetWorkingDirectory { get; set; }

        /// <summary>
        /// Create body
        /// </summary>
        /// <returns>Activity object</returns>
        protected override Activity CreateInternalBody()
        {
            var sequence = new Sequence();

            var item = new NUnitDotCoverInternal()
            {
                Assemblies = new InArgument<IEnumerable<string>>(x => this.Assemblies.Get(x)),
                Configuration = new InArgument<string>(x => this.Configuration.Get(x)),
                Domain = new InArgument<string>(x => this.Domain.Get(x).ToString()),
                ErrorOutputFile = new InArgument<string>(x => this.ErrorOutputFile.Get(x)),
                Errors = new OutArgument<int>(x => this.Errors.GetLocation(x).Value),
                ExitCode = new OutArgument<int>(x => this.ExitCode.GetLocation(x).Value),
                ExcludeCategory = new InArgument<string>(x => this.ExcludeCategory.Get(x)),
                Failures = new OutArgument<int>(x => this.Failures.GetLocation(x).Value),
                Framework = new InArgument<string>(x => this.Framework.Get(x)),
                Flavor = new InArgument<string>(x => this.Flavor.Get(x)),
                Ignored = new OutArgument<int>(x => this.Ignored.GetLocation(x).Value),
                IncludeCategory = new InArgument<string>(x => this.IncludeCategory.Get(x)),
                Inconclusive = new OutArgument<int>(x => this.Inconclusive.GetLocation(x).Value),
                Invalid = new OutArgument<int>(x => this.Invalid.GetLocation(x).Value),
                Labels = new InArgument<bool>(x => this.Labels.Get(x)),
                NoShadow = new InArgument<bool>(x => this.NoShadow.Get(x)),
                NoThread = new InArgument<bool>(x => this.NoThread.Get(x)),
                NotRun = new OutArgument<int>(x => this.NotRun.GetLocation(x).Value),
                OutputFile = new InArgument<string>(x => this.OutputFile.Get(x)),
                OutputXmlFile = new InArgument<string>(x => this.OutputXmlFile.Get(x)),
                Platform = new InArgument<string>(x => this.Platform.Get(x)),
                Process = new InArgument<string>(x => this.Process.Get(x).ToString()),
                PublishTestResults = new InArgument<bool>(x => this.PublishTestResults.Get(x)),
                Run = new InArgument<string>(x => this.Run.Get(x)),
                Skipped = new OutArgument<int>(x => this.Skipped.GetLocation(x).Value),
                ToolPath = new InArgument<string>(x => this.ToolPath.Get(x)),
                Total = new OutArgument<int>(x => this.Total.GetLocation(x).Value),
                Use32Bit = new InArgument<bool>(x => this.Use32Bit.Get(x)),
                Version = new InArgument<string>(x => this.Version.Get(x)),
                AnalyzeTargetArguments = new InArgument<bool>(x => this.AnalyzeTargetArguments.Get(x)),
                ConfigFile = new InArgument<string>(x => this.ConfigFile.Get(x)),
                CoverageOutputFile = new InArgument<string>(x => this.CoverageOutputFile.Get(x)),
                CoverageOutputHtmlFile = new InArgument<string>(x => this.CoverageOutputHtmlFile.Get(x)),
                CoverageOutputXmlFile = new InArgument<string>(x => this.CoverageOutputXmlFile.Get(x)),
                CoverageToolPath = new InArgument<string>(x => this.CoverageToolPath.Get(x)),
                TargetWorkingDirectory = new InArgument<string>(x => this.TargetWorkingDirectory.Get(x)),
            };

            sequence.Activities.Add(item);

            var htmlReport = new GenerateReport
            {
                TargetWorkingDirectory = new InArgument<string>(x => this.TargetWorkingDirectory.Get(x)),
                CoverageOutputFile = new InArgument<string>(x => this.CoverageOutputHtmlFile.Get(x)),
                Type = new InArgument<ReportType>(ReportType.Html),
                CoverageToolPath = new InArgument<string>(x => this.CoverageToolPath.Get(x)),
                DisplayName = "HTML Report"
            };

            sequence.Activities.Add(htmlReport);

            var xmlReport = new GenerateReport
            {
                TargetWorkingDirectory = new InArgument<string>(x => this.TargetWorkingDirectory.Get(x)),
                CoverageOutputFile = new InArgument<string>(x => this.CoverageOutputXmlFile.Get(x)),
                Type = new InArgument<ReportType>(ReportType.Xml),
                CoverageToolPath = new InArgument<string>(x => this.CoverageToolPath.Get(x)),
                DisplayName = "XML Report"
            };

            sequence.Activities.Add(xmlReport);
            return sequence;
        }

    }
}