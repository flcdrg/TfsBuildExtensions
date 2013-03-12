//-----------------------------------------------------------------------
// <copyright file="NUnit.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------

using System.IO;

using Microsoft.TeamFoundation.Build.Workflow.Activities;

namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Tracking;

    /// <summary>
    /// Executes Test Cases using NUnit (Tested using v2.5.7)
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <!-- Run NUnit on all binaries (*.dll) for the the current configuration/platform-->    
    /// <Sequence>
    /// <Sequence.Variables>
    /// <Variable x:TypeArguments="scg2:IEnumerable(x:String)" Name="NUnitTestAssemblies" />
    ///  <Variable x:TypeArguments="x:Int32" Name="Total" />
    /// <Variable x:TypeArguments="x:Int32" Name="Errors" />
    /// </Sequence.Variables>
    /// <mtbwa:FindMatchingFiles DisplayName="Find NUnitTest assemblies" MatchPattern="[String.Format(&quot;{0}\\**.dll&quot;, BinariesDirectory)]" Result="[NUnitTestAssemblies]" />
    /// <tan:NUnit PublishTestResults="True" Flavor="[platformConfiguration.Configuration]" Platform="[platformConfiguration.Platform]" Configuration="{x:Null}" Domain="{x:Null}" ErrorOutputFile="{x:Null}" ExcludeCategory="{x:Null}" FailBuildOnError="{x:Null}" Failures="{x:Null}" Framework="{x:Null}" Ignored="{x:Null}" IncludeCategory="{x:Null}" Inconclusive="{x:Null}" Invalid="{x:Null}" Labels="{x:Null}" NoShadow="{x:Null}" NoThread="{x:Null}" NotRun="{x:Null}" OutputFile="{x:Null}" Process="{x:Null}" Run="{x:Null}" Skipped="{x:Null}" TestTimeout="{x:Null}" TimeTaken="{x:Null}" TreatWarningsAsErrors="{x:Null}" Use32Bit="{x:Null}" Assemblies="[NUnitTestAssemblies]" Errors="[Errors]" LogExceptionStack="True" OutputXmlFile="NUnitResults.xml" ToolPath="C:\Program Files (x86)\NUnit 2.5.10\bin\net-2.0\" Total="[Total]" Version="2.5.7" />
    /// </Sequence>
    /// ]]></code>    
    /// </example>
    /// </summary>
    [BuildActivity(HostEnvironmentOption.All)]
    [Description("Activity to run NUnit tests as part of a TFS Build")]
    [ActivityTracking(ActivityTrackingOption.ActivityTree)]
    public class NUnit : BaseActivity
    {
        /// <summary>
        /// Initializes a new instance of the NUnit class
        /// </summary>
        public NUnit()
        {
            this.Version = "2.5.7";
            this.Process = ProcessModel.Single;
        }

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

/*
        /// <summary>
        /// Set to true to publish test results back to TFS
        /// </summary>
        [Browsable(true)]
        [Description("The name to use for the TFS report")]
        [DefaultValue("TestResults")]
        public InArgument<string> PublishName { get; set; }
*/

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
        /// Gets the Time taken to run the tests
        /// </summary>
        [Browsable(true)]
        [Description("Gets the Time taken to run the tests")]
        public InArgument<DateTime> TimeTaken { get; set; }

        /// <summary>
        /// Process model for tests. Supports Single, Separate, Multiple. Single is the Default
        /// </summary>
        [Browsable(true)]
        [Description("Process model for tests. Supports Single, Separate, Multiple. Single is the Default")]
        [DefaultValue(ProcessModel.Default)]
        public InArgument<ProcessModel> Process { get; set; }

        /// <summary>
        /// AppDomain Usage for tests. Supports None, Single, Multiple. The default is to use multiple domains if multiple assemblies are listed on the command line. Otherwise a single domain is used.
        /// </summary>
        [Browsable(true)]
        [Description("AppDomain Usage for tests. Supports None, Single, Multiple. The default is to use multiple domains if multiple assemblies are listed on the command line. Otherwise a single domain is used.")]
        [DefaultValue(DomainUsage.Default)]
        public InArgument<DomainUsage> Domain { get; set; }

        /// <summary>
        /// Framework version to be used for tests
        /// </summary>
        [Browsable(true)]
        [Description("Framework version to be used for tests")]
        public InArgument<string> Framework { get; set; }

        /// <summary>
        /// Set timeout for each test case in milliseconds
        /// </summary>
        [Browsable(true)]
        [Description("Set timeout for each test case in milliseconds")]
        public InArgument<int> TestTimeout { get; set; }

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

        /// <summary>
        /// Create activity
        /// </summary>
        /// <returns>Activity object</returns>
        protected override Activity CreateInternalBody()
        {
            var workingDirectory = new Variable<string>
            {
                Name = "WorkingDirectory"
            };

            var sequence = new Sequence() { Variables = { workingDirectory } };

            
            sequence.Activities.Add(new Assign<string>
            {
                To = workingDirectory, 
                Value = new InArgument<string>(x => Path.GetDirectoryName(this.Assemblies.Get(x).First())), 
                DisplayName = "Assign working directory"
            });

            this.AddActivity(sequence);

            var processXml = new ProcessXmlResultsFile()
            {
                DisplayName = "Process XML Results",
                Errors = new OutArgument<int>(x => this.Errors.GetLocation(x).Value),
                Failures = new OutArgument<int>(x => this.Failures.GetLocation(x).Value),
                Ignored = new OutArgument<int>(x => this.Ignored.GetLocation(x).Value),
                Inconclusive = new OutArgument<int>(x => this.Inconclusive.GetLocation(x).Value),
                Invalid = new OutArgument<int>(x => this.Invalid.GetLocation(x).Value),
                NotRun = new OutArgument<int>(x => this.NotRun.GetLocation(x).Value),
                Skipped = new OutArgument<int>(x => this.Skipped.GetLocation(x).Value),
                Total = new OutArgument<int>(x => this.Total.GetLocation(x).Value),
                OutputXmlFile = new InArgument<string>(x => this.OutputXmlFile.Get(x)),
                WorkingDirectory = new InArgument<string>(x => workingDirectory.Get(x))
            };

            sequence.Activities.Add(processXml);

            var publishResults = new PublishTestResultsToTfs()
            {
                DisplayName = "Publish to TFS",
                Flavor = new InArgument<string>(x => this.Flavor.Get(x)),
                OutputXmlFile = new InArgument<string>(x => this.OutputXmlFile.Get(x)),
                Platform = new InArgument<string>(x => this.Platform.Get(x)),
                WorkingDirectory = new InArgument<string>(x => workingDirectory.Get(x)),
                ReportName = new InArgument<string>(x => string.Join(", ", this.Assemblies.Get(x)))
            };

            var condition = new If
            {
                DisplayName = "If PublishTestResults",
                Condition = new InArgument<bool>(x => this.PublishTestResults.Get(x)), 
                Then = publishResults,
                Else = new WriteBuildMessage() { Message = new InArgument<string>("Results not published"), Importance = new InArgument<BuildMessageImportance>(BuildMessageImportance.High) }
            };

            sequence.Activities.Add(condition);
            return sequence;
        }

        protected virtual void AddActivity(Sequence sequence)
        {
            var item = new NUnitInternal()
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
                Version = new InArgument<string>(x => this.Version.Get(x))
            };

            sequence.Activities.Add(item);
        }
    }
}
