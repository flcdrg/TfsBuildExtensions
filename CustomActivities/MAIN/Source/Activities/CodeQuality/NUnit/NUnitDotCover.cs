//-----------------------------------------------------------------------
// <copyright file="NUnitDotCover.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;

    using Extended;

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

        private enum ReportType
        {
            /// <summary>
            /// XML Report
            /// </summary>
            Xml,

            /// <summary>
            /// HTML Report
            /// </summary>
            Html
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
                Domain = new InArgument<string>(x => this.Domain.Get(x)),
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
                Type = ReportType.Html,
                CoverageToolPath = new InArgument<string>(x => this.CoverageToolPath.Get(x)),
                DisplayName = "HTML Report"
            };

            sequence.Activities.Add(htmlReport);

            var xmlReport = new GenerateReport
            {
                TargetWorkingDirectory = new InArgument<string>(x => this.TargetWorkingDirectory.Get(x)),
                CoverageOutputFile = new InArgument<string>(x => this.CoverageOutputXmlFile.Get(x)),
                Type = ReportType.Xml,
                CoverageToolPath = new InArgument<string>(x => this.CoverageToolPath.Get(x)),
                DisplayName = "XML Report"
            };

            sequence.Activities.Add(xmlReport);
            return sequence;
        }

        private sealed class GenerateReport : BaseCodeActivity
        {
            public InArgument<string> TargetWorkingDirectory { private get; set; }

            public InArgument<string> CoverageOutputFile { private get; set; }

            public InArgument<string> CoverageToolPath { private get; set; }

            public InArgument<ReportType> Type { private get; set; } 

            protected override void InternalExecute()
            {
                string fullPath = this.CoverageToolPath.Get(this.ActivityContext);

                string workingDirectory = this.TargetWorkingDirectory.Get(this.ActivityContext);

                this.RunProcess(fullPath, workingDirectory, this.GenerateReportCommandLineCommands(this.ActivityContext, this.Type.Get(this.ActivityContext), this.CoverageOutputFile.Get(this.ActivityContext)));
            }

            private string GenerateReportCommandLineCommands(ActivityContext context, ReportType reportType, string outputPath)
            {
                // C:\Program Files (x86)\JetBrains\dotCover\v2.0\Bin\dotCover.exe report /Source="E:\30\67\Binaries\DotCoverReport\Coverage.bin" /ReportType=XML /Output=E:\30\67\Binaries\DotCoverReport\Coverage.xml
                var builder = new SimpleCommandLineBuilder();

                builder.AppendSwitch("report");

                builder.AppendSwitchIfNotNull("/Source=", this.CoverageOutputFile.Get(context));

                builder.AppendSwitchIfNotNull("/ReportType=", reportType.ToString().ToUpperInvariant());

                builder.AppendSwitchIfNotNull("/Output=", outputPath);

                return builder.ToString();
            }

            private void RunProcess(string fullPath, string workingDirectory, string arguments)
            {
                using (var proc = new Process())
                {
                    proc.StartInfo.FileName = fullPath;

                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.Arguments = arguments;
                    this.LogBuildMessage("Running " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments, BuildMessageImportance.High);

                    if (!string.IsNullOrEmpty(workingDirectory))
                    {
                        proc.StartInfo.WorkingDirectory = workingDirectory;
                    }

                    proc.Start();

                    string outputStream = proc.StandardOutput.ReadToEnd();
                    if (outputStream.Length > 0)
                    {
                        this.LogBuildMessage(outputStream);
                    }

                    string errorStream = proc.StandardError.ReadToEnd();
                    if (errorStream.Length > 0)
                    {
                        this.LogBuildError(errorStream);
                    }

                    proc.WaitForExit();
                }
            }
        }
    }
}