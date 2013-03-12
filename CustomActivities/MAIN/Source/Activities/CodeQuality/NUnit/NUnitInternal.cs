//-----------------------------------------------------------------------
// <copyright file="NUnitInternal.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

using System.Activities.Tracking;
using System.Text;

using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Extended;

    using Microsoft.TeamFoundation.Build.Client;

    internal class NUnitInternal : BaseCodeActivity
    {
        /// <summary>
        /// The version of NUnit to run. Default is 2.5.7
        /// </summary>
        [Browsable(true)]
        [Description("The version of NUnit to run. Default is 2.5.7")]
        public InArgument<string> Version { get; set; }

        /// <summary>
        /// Gets or sets the ToolPath (defaults to %ProgramFiles%\Nunit {version}\bin\nunit-console[-86].exe
        /// </summary>
        [Browsable(true)]
        [Description(@"Gets or sets the ToolPath (defaults to %ProgramFiles%\Nunit {version}\bin\nunit-console[-86].exe")]
        public InArgument<string> ToolPath { get; set; }

        /// <summary>
        /// The assemblies to process.
        /// </summary>
        [Browsable(true)]
        [Description("The assemblies to process")]
        [RequiredArgument]
        public InArgument<IEnumerable<string>> Assemblies { get; set; }

        /// <summary>
        /// Set to true to publish test results back to TFS
        /// </summary>
        [Browsable(true)]
        [Description("Set to true to publish test results back to TFS")]
        public InArgument<bool> PublishTestResults { get; set; }

        /// <summary>
        /// Which platform to publish test results for (ex. Any CPU)
        /// </summary>
        [Browsable(true)]
        [Description("Which platform to publish test results for (ex. Any CPU)")]
        public InArgument<string> Platform { get; set; }

        /// <summary>
        /// Which flavor to publish test results for (ex. Debug)
        /// </summary>
        [Browsable(true)]
        [Description("Which flavor to publish test results for (ex. Debug)")]
        public InArgument<string> Flavor { get; set; }

        /// <summary>
        /// Set to true to run nunit-console-x86.exe
        /// </summary>
        [Browsable(true)]
        [Description("Set to true to run nunit-console-x86.exe")]
        public InArgument<bool> Use32Bit { get; set; }

        /// <summary>
        /// Comma separated list of categories to include.
        /// </summary>
        [Browsable(true)]
        [Description("Comma separated list of categories to include")]
        public InArgument<string> IncludeCategory { get; set; }

        /// <summary>
        /// Comma separated list of categories to exclude.
        /// </summary>
        [Browsable(true)]
        [Description("Comma separated list of categories to exclude")]
        public InArgument<string> ExcludeCategory { get; set; }

        /// <summary>
        /// Sets the OutputXmlFile name
        /// </summary>
        [Browsable(true)]
        [Description("Sets the OutputXmlFile name")]
        public InArgument<string> OutputXmlFile { get; set; }

        /// <summary>
        /// Sets the File to receive test error output
        /// </summary>
        [Browsable(true)]
        [Description("Sets the File to receive test error output")]
        public InArgument<string> ErrorOutputFile { get; set; }

        /// <summary>
        /// File to receive test output
        /// </summary>
        [Browsable(true)]
        [Description("File to receive test output")]
        public InArgument<string> OutputFile { get; set; }

        /// <summary>
        /// Disable use of a separate thread for tests. Default is false.
        /// </summary>
        [Browsable(true)]
        [Description("Disable use of a separate thread for tests. Default is false")]
        public InArgument<bool> NoThread { get; set; }

        /// <summary>
        /// Gets the Failures count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Failures count")]
        public OutArgument<int> Failures { get; set; }

        /// <summary>
        /// Gets the NotRun count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the NotRun count")]
        public OutArgument<int> NotRun { get; set; }

        /// <summary>
        /// Gets the Total count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Total count")]
        public OutArgument<int> Total { get; set; }

        /// <summary>
        /// Gets the Errors count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Errors count")]
        public OutArgument<int> Errors { get; set; }

        /// <summary>
        /// Gets the Inconclusive count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Inconclusive count")]
        public OutArgument<int> Inconclusive { get; set; }

        /// <summary>
        /// Gets the Ignored count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Ignored count")]
        public OutArgument<int> Ignored { get; set; }

        /// <summary>
        /// Gets the Skipped count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Skipped count")]
        public OutArgument<int> Skipped { get; set; }

        /// <summary>
        /// Gets the Invalid count
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Invalid count")]
        public OutArgument<int> Invalid { get; set; }

        /// <summary>
        /// Disable shadow copy when running in separate domain. Default is false.
        /// </summary>
        [Browsable(true)]
        [Description("Disable shadow copy when running in separate domain. Default is false")]
        public InArgument<bool> NoShadow { get; set; }

        /// <summary>
        /// Sets the Project configuration (e.g.: Debug) to load
        /// </summary>
        [Browsable(true)]
        [Description("Sets the Project configuration (e.g.: Debug) to load")]
        public InArgument<string> Configuration { get; set; }

        /// <summary>
        /// Process model for tests. Supports Single, Separate, Multiple. Single is the Default
        /// </summary>
        [Browsable(true)]
        [Description("Process model for tests. Supports Single, Separate, Multiple. Single is the Default")]
        public InArgument<string> Process { get; set; }

        /// <summary>
        /// AppDomain Usage for tests. Supports None, Single, Multiple. The default is to use multiple domains if multiple assemblies are listed on the command line. Otherwise a single domain is used.
        /// </summary>
        [Browsable(true)]
        [Description("AppDomain Usage for tests. Supports None, Single, Multiple. The default is to use multiple domains if multiple assemblies are listed on the command line. Otherwise a single domain is used.")]
        public InArgument<string> Domain { get; set; }

        /// <summary>
        /// Framework version to be used for tests
        /// </summary>
        [Browsable(true)]
        [Description("Framework version to be used for tests")]
        public InArgument<string> Framework { get; set; }

        /// <summary>
        /// Label each test in stdOut. Default is false.
        /// </summary>
        [Browsable(true)]
        [Description("Label each test in stdOut. Default is false")]
        public InArgument<bool> Labels { get; set; }

        /// <summary>
        /// Name of the test case(s), fixture(s) or namespace(s) to run
        /// </summary>
        [Browsable(true)]
        [Description("Name of the test case(s), fixture(s) or namespace(s) to run")]
        public InArgument<string> Run { get; set; }

        /// <summary>
        /// Name of the test case(s), fixture(s) or namespace(s) to run
        /// </summary>
        [Browsable(true)]
        [Description("ExitCode for the NUnit process")]
        public OutArgument<int> ExitCode { get; set; }

        protected virtual string GenerateFullPathToTool(ActivityContext context)
        {
            string toolName = this.Use32Bit.Get(context) ? "nunit-console-x86.exe" : "nunit-console.exe";
            if (string.IsNullOrEmpty(this.ToolPath.Get(context)))
            {
                this.ToolPath.Set(context, string.Format(CultureInfo.InvariantCulture, Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Nunit {0}\bin"), this.Version.Get(context)));
            }

            return string.IsNullOrEmpty(this.ToolPath.Get(context)) ? toolName : Path.Combine(this.ToolPath.Get(context), toolName);
        }

        protected virtual string GenerateCommandLineCommands(ActivityContext context, string outputFolder)
        {
            var builder = new SimpleCommandLineBuilder();
            builder.AppendSwitch("/nologo");
            if (this.NoShadow.Get(context))
            {
                builder.AppendSwitch("/noshadow");
            }

            if (this.NoThread.Get(context))
            {
                builder.AppendSwitch("/nothread");
            }

            if (this.Labels.Get(context))
            {
                builder.AppendSwitch("/labels");
            }

            builder.AppendFileNamesIfNotNull(this.Assemblies.Get(context).ToArray(), " ");
            builder.AppendSwitchIfNotNull("/run=", this.Run.Get(context));
            builder.AppendSwitchIfNotNull("/config=", this.Configuration.Get(context));
            builder.AppendSwitchIfNotNull("/include=", this.IncludeCategory.Get(context));
            builder.AppendSwitchIfNotNull("/exclude=", this.ExcludeCategory.Get(context));
            builder.AppendSwitchIfNotNull("/process=", this.Process.Get(context));
            builder.AppendSwitchIfNotNull("/domain=", this.Domain.Get(context));
            builder.AppendSwitchIfNotNull("/framework=", this.Framework.Get(context));
            builder.AppendSwitchIfNotNull("/xml=", Path.Combine(outputFolder, this.OutputXmlFile.Get(context)));
            builder.AppendSwitchIfNotNull("/err=", this.ErrorOutputFile.Get(context));
            builder.AppendSwitchIfNotNull("/out=", this.OutputFile.Get(context));
            return builder.ToString();
        }

        protected override void InternalExecute()
        {
            string fullPath = this.GenerateFullPathToTool(this.ActivityContext);
 
            if (!File.Exists(fullPath))
            {
                this.LogBuildError(string.Format(fullPath + " was not found. Use ToolPath to specify it."));
                return;
            }

            if (!this.Assemblies.Get(this.ActivityContext).Any())
            {
                this.LogBuildMessage("No unit test assemblies passed to NUnit actitity. No tests will be executed");
                return;
            }

            string workingDirectory = this.GetWorkingDirectory();
            this.RunTests(fullPath, workingDirectory);
        }

        protected virtual string GetWorkingDirectory()
        {
            return Path.GetDirectoryName(this.Assemblies.Get(this.ActivityContext).First());
        }

        private void RunTests(string fullPath, string workingDirectory)
        {
            int exitCode = this.RunProcess(fullPath, workingDirectory, this.GenerateCommandLineCommands(this.ActivityContext, workingDirectory));
            this.ExitCode.Set(this.ActivityContext, exitCode);
        }

        private IList<string> _output;
        private IList<string> _error;

        private int RunProcess(string fullPath, string workingDirectory, string arguments)
        {
            _output = new List<string>();
            _error = new List<string>();

            using (var proc = new Process())
            {
                proc.StartInfo.FileName = fullPath;

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;

                proc.OutputDataReceived += ProcOnOutputDataReceived;
                proc.ErrorDataReceived += ProcOnErrorDataReceived;
                proc.StartInfo.Arguments = arguments;
                this.LogBuildMessage("Running " + proc.StartInfo.FileName + " " + proc.StartInfo.Arguments, BuildMessageImportance.High);

                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    proc.StartInfo.WorkingDirectory = workingDirectory;
                }

                proc.Start();

                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                proc.WaitForExit();

                proc.OutputDataReceived -= ProcOnOutputDataReceived;
                proc.ErrorDataReceived -= ProcOnErrorDataReceived;

                if (proc.ExitCode != 0)
                {
                    // write normal output as an error
                    foreach (var s in _output)
                        if (!string.IsNullOrWhiteSpace(s))
                            this.LogBuildError(s);
                }

                return proc.ExitCode;
            }
        }

        private void ProcOnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            string message = dataReceivedEventArgs.Data;

            if (!string.IsNullOrWhiteSpace(message))
            {
                _error.Add(message);
                this.LogBuildError(message);
            }
        }

        private void ProcOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            string message = dataReceivedEventArgs.Data;
            _output.Add(message);

            this.LogBuildMessage(message);
        }
    }
}