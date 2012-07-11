
namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Extended;

    using Microsoft.TeamFoundation.Build.Client;

    internal sealed class GenerateReport : BaseCodeActivity
    {
        public InArgument<string> TargetWorkingDirectory { get; set; }

        public InArgument<string> CoverageOutputFile { get; set; }

        public InArgument<string> CoverageToolPath { get; set; }

        public InArgument<ReportType> Type { get; set; }

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