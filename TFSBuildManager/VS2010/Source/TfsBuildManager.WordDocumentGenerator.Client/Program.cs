//-----------------------------------------------------------------------
// <copyright file="Program.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace WordDocumentGenerator.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    using DocumentFormat.OpenXml.Packaging;

    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.TestManagement.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Microsoft.TeamFoundation.WorkItemTracking.Client;

    using WordDocumentGenerator.Client.Entities;
    using WordDocumentGenerator.Library;

    public class Program
    {
        private IBuildDetail[] builds;

        public Program()
        {
        }

        public Program(IBuildDetail[] builds, IEnumerable<string> options, IEnumerable<string> exclusions, WorkItemStore workItemStore, Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer versionControlServer, TestManagementService tms, string selectedProject)
        {
            this.Builds = builds;
            this.NoteOptions = options;
            this.Exclusions = exclusions;
            this.WorkItemStore = workItemStore;
            this.VersionControlServer = versionControlServer;
            this.TestManagementService = tms;
            this.SelectedProject = selectedProject;
        }

        public string SelectedProject { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Don't have a fix for this just now")]
        public IBuildDetail[] Builds
        {
            get
            {
                return this.builds;
            }

            set
            {
                this.builds = value;
            }
        }

        public IEnumerable<string> NoteOptions { get; set; }

        public IEnumerable<string> Exclusions { get; set; }

        public WorkItemStore WorkItemStore { get; set; }

        public Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer VersionControlServer { get; set; }

        public TestManagementService TestManagementService { get; set; }

        /// <summary>
        /// Generates the document using doc with table generator
        /// </summary>
        public void GenerateBuildNotesUsingBuildTemplate()
        {
            var otherDocs = new List<byte[]>();

            foreach (var buildDetail in this.GetDataContext())
            {
                var generationInfo = GetDocumentGenerationInfo("Build Notes", "1.0", buildDetail, @"Templates\BuildTemplateContent.docx", false);
                var docGenerator = new BuildNotesDocumentGenerator(generationInfo, this.Exclusions.ToList());
                otherDocs.Add(docGenerator.GenerateDocument());
            }

            this.MergeBuildNotesAndCreateFinalOutput(otherDocs);
        }

        /// <summary>
        /// Writes the output to file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileContents">The file contents.</param>
        private static void WriteOutputToFile(string fileName, byte[] fileContents)
        {
            File.WriteAllBytes(fileName, fileContents);
        }

        private static string CalculateTotalBuildExecutionTime(DateTime start, DateTime end)
        {
            var totalSeconds = end.Subtract(start).Duration().TotalSeconds;

            var elapsedTime = string.Empty;
            if (totalSeconds > 0 && totalSeconds <= 59)
            {
                elapsedTime = string.Format("{0} second(s)", (int)totalSeconds);
            }

            if (totalSeconds >= 60 && totalSeconds <= 3599)
            {
                elapsedTime = string.Format("{0} minute(s)", (int)end.Subtract(start).Duration().TotalMinutes);
            }

            if (totalSeconds >= 3600 && totalSeconds <= 86399)
            {
                elapsedTime = string.Format("{0} hour(s)", (int)end.Subtract(start).Duration().TotalHours);
            }

            if (totalSeconds >= 86400)
            {
                elapsedTime = string.Format("{0} day(s)", (int)end.Subtract(start).Duration().TotalDays);
            }

            return elapsedTime;
        }

        /// <summary>
        /// Gets the document generation info.
        /// </summary>
        /// <param name="docType">Type of the doc.</param>
        /// <param name="docVersion">The doc version.</param>
        /// <param name="dataContext">The data context.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="useDataBoundControls">if set to <c>true</c> [use data bound controls].</param>
        /// <returns>Returns the Document generation Information</returns>
        private static DocumentGenerationInfo GetDocumentGenerationInfo(string docType, string docVersion, object dataContext, string fileName, bool useDataBoundControls)
        {
            DocumentGenerationInfo generationInfo = new DocumentGenerationInfo();
            generationInfo.Metadata = new DocumentMetadata() { DocumentType = docType, DocumentVersion = docVersion };
            generationInfo.DataContext = dataContext;

            DirectoryInfo di = new DirectoryInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            generationInfo.TemplateData = File.ReadAllBytes(Path.Combine(di.Parent.FullName, fileName));
            generationInfo.IsDataBoundControls = useDataBoundControls;

            return generationInfo;
        }

        /// <summary>
        /// Generates the final report by appending documents.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Don't have a fix for this just now")]
        private void MergeBuildNotesAndCreateFinalOutput(IList<byte[]> otherDocs)
        {
            // Test final report i.e. created by merging documents generation
            var di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);
            byte[] primaryDoc = File.ReadAllBytes(Path.Combine(di.Parent.FullName, @"Templates\BuildTemplateCover.docx"));

            // Final report generation
            var openXmlHelper = new OpenXmlHelper(DocumentGenerationInfo.NamespaceUri);
            var result = openXmlHelper.AppendDocumentsToPrimaryDocument(primaryDoc, otherDocs);

            // Final Protected report generation
            using (var msfinalDocument = new MemoryStream())
            {
                msfinalDocument.Write(result, 0, result.Length);

                using (var finalDocument = WordprocessingDocument.Open(msfinalDocument, true))
                {
                    OpenXmlHelper.ProtectDocument(finalDocument);
                }

                msfinalDocument.Position = 0;
                result = new byte[msfinalDocument.Length];
                msfinalDocument.Read(result, 0, result.Length);
                msfinalDocument.Close();
            }

            this.EvaluateIfBuildNotesNeedCheckInAndDisplayResults(result);
        }

        private void EvaluateIfBuildNotesNeedCheckInAndDisplayResults(byte[] result)
        {
            // Get the path for check in
            var localDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var fileName = string.Format("BuildNotes_{0}_{1}.docx", this.SelectedProject, DateTime.Now.ToString("yyyyMMddhhmmss"));
            var filePath = Path.Combine(localDir, fileName);
            var serverpath = string.Empty;
            var di = new DirectoryInfo(Assembly.GetExecutingAssembly().Location);

            var pathToConfigFile = Path.Combine(di.Parent.FullName, @"Templates\BuildNotesConfiguration.xml");

            var xml = new XmlDocument();
            xml.Load(pathToConfigFile);
            var selectSingleNode = xml.SelectSingleNode("/EnvironmentSettings/Parameters/ServerPath");
            if (selectSingleNode != null)
            {
                serverpath =
                    selectSingleNode.InnerText.Replace("_SelectedTeamProject_", this.SelectedProject).Replace("\"", " ").Trim();
            }

            if (this.NoteOptions.Contains("CheckInToTfs"))
            {
                var workspace = this.VersionControlServer.CreateWorkspace(Guid.NewGuid().ToString(), this.VersionControlServer.AuthorizedUser);
                if (!this.VersionControlServer.ServerItemExists(serverpath, ItemType.Folder))
                {
                    serverpath = string.Format(@"{0}\{1}", "$", this.SelectedProject);
                }

                try
                {
                    workspace.Map(serverpath, localDir);
                    workspace.Get();
                    WriteOutputToFile(filePath, result);
                    workspace.PendAdd(filePath);
                    var pendingChanges = workspace.GetPendingChanges();
                    workspace.CheckIn(pendingChanges, string.Format(" ***NO_CI*** Checking in Build Note "));
                    Process.Start(filePath);
                }
                finally
                {
                    var pendingChanges = workspace.GetPendingChanges();
                    if (pendingChanges.Length > 0)
                    {
                        workspace.Undo(pendingChanges);
                    }

                    workspace.Delete();
                }
            }
            else
            {
                WriteOutputToFile(filePath, result);
                Process.Start(filePath);
            }
        }

        /// <summary>
        /// Gets the data context.
        /// </summary>
        /// <returns>Returns the Data context i.e. the build details</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Don't have an alternative implementation just yet."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Don't have a fix for this just now")]
        private List<WordDocumentGenerator.Client.Entities.BuildDetail> GetDataContext()
        {
            var buildDetails = new List<BuildDetail>();

            foreach (var build in this.Builds)
            {
                var buildDetail = new BuildDetail()
                {
                    DocumentDetail = new DocumentDetail(),
                    Build = new Build(),
                    BuildConfigurations = new List<BuildConfiguration>(),
                    BuildConfigurationSolutions = new List<BuildConfigurationSolution>(),
                    Changesets = new List<WordDocumentGenerator.Client.Entities.Changeset>(),
                    ChangesetChangeDetails = new List<ChangesetChangeDetail>(),
                    WorkItems = new List<WordDocumentGenerator.Client.Entities.WorkItem>(),
                    TestResult = new TestResult(),
                    TestResultPassedItems = new List<TestResultPassedItem>(),
                    TestResultFailedItems = new List<TestResultFailedItem>(),
                    TestResultInconclusiveItems = new List<TestResultInconclusiveItem>()
                };

                // Load Document Details into BuildDetails
                buildDetail.DocumentDetail.Title = build.BuildNumber;
                buildDetail.DocumentDetail.CreatedOn = DateTime.Now.ToLongDateString();
                buildDetail.DocumentDetail.CreatedBy = Environment.UserName;

                // Load Build Details into BuildDetails
                buildDetail.Build.BuildDefinition = !string.IsNullOrEmpty(build.BuildDefinition.Name) ? build.BuildDefinition.Name : string.Empty; // "Tailspin Toys";
                // Null check - only if the build controller was removed & the builds not deleted will the controller be null
                buildDetail.Build.BuildController = build.BuildDefinition.BuildController != null ? build.BuildDefinition.BuildController.Name : "Unavilable"; // "WIN-GS9GMUJITS8 - Controller";
                buildDetail.Build.BuildNumber = !string.IsNullOrEmpty(build.BuildNumber) ? build.BuildNumber : "Unavilable"; // "Tailspin Toys - Iteration 2_20100318.2";
                buildDetail.Build.BuildQuality = !string.IsNullOrEmpty(build.Quality) ? build.Quality : "Not Set";
                buildDetail.Build.BuildState = build.Status.ToString(); // "Succeeded";
                buildDetail.Build.BuildTeamProject = build.TeamProject;  // "Tailspin Toys";
                buildDetail.Build.BuildTestStatus = build.TestStatus.ToString(); // "Passed";

                // TODO: Add a new function to create this from start and finish time
                buildDetail.Build.BuildTotalExecutionTimeInSeconds = CalculateTotalBuildExecutionTime(build.StartTime, build.FinishTime); // "86 seconds";

                buildDetail.Build.BuildUri = build.Uri;  // new Uri(@"vstfs:///VersionControl/Build/17");
                buildDetail.Build.BuildStartTime = build.StartTime.ToString("dd MM YYYY hh:mm:ss");   // "01-01-2011 12:09:07";
                buildDetail.Build.BuildSourceGetVersion = !string.IsNullOrEmpty(build.SourceGetVersion) ? build.SourceGetVersion : "Unavilable"; // "70";
                buildDetail.Build.BuildShelvesetName = !string.IsNullOrEmpty(build.ShelvesetName) ? build.ShelvesetName : "Not Shelved"; // @"Tailspin Toys\@2_20100318.2";
                buildDetail.Build.BuildRetainIndefinitely = build.KeepForever.ToString(); // "false";
                buildDetail.Build.BuildRequestedBy = !string.IsNullOrEmpty(build.RequestedBy) ? build.RequestedBy : "Unavilable"; // @"WIN-GS9GMUJITS8\abuobe";
                buildDetail.Build.BuildReason = build.Reason.ToString(); // "CI";
                buildDetail.Build.BuildLogLocation = !string.IsNullOrEmpty(build.LogLocation) ? build.LogLocation : "Unavailable"; // @"\\Log\\Folder\abc\TailSpin Toys\Tailspin Toys - Iteration 2_20100318.2";
                buildDetail.Build.BuildLastChangedOn = build.LastChangedOn.ToString(); // "05-01-2012 13:01:37";
                buildDetail.Build.BuildLastChangedBy = build.LastChangedBy; // @"UK\tarora";

                // Todo: create a new function to generate the string for the last modified duration
                buildDetail.Build.BuildLastModifiedNoOfDaysAgo = CalculateTotalBuildExecutionTime(build.LastChangedOn, DateTime.Now);

                buildDetail.Build.BuildLabelName = !string.IsNullOrEmpty(build.LabelName) ? build.LabelName : "Not Labeled"; // @"Tailspin Toys@\Label\2_20100318.2";
                buildDetail.Build.BuildIsDeleted = build.IsDeleted.ToString(); // "false";
                buildDetail.Build.BuildEndTime = build.FinishTime.ToString(); // "01-01-2011 12:10:35";

                // ToDo: create a new function to generate the string for the completed no of days ago
                buildDetail.Build.BuildCompletedNoOfDaysAgo = CalculateTotalBuildExecutionTime(build.FinishTime, DateTime.Now);

                buildDetail.Build.BuildDropLocation = !string.IsNullOrEmpty(build.DropLocation) ? build.DropLocation : "Unavailable"; // @"\\Drop\\Folder\abc\TailSpin Toys\Tailspin Toys - Iteration 2_20100318.2";

                if (!this.Exclusions.Contains("BuildConfiguration"))
                {
                    // Load Build Configuration into BuildDetails
                    foreach (IConfigurationSummary configSummary in InformationNodeConverters.GetConfigurationSummaries(build))
                    {
                        var buildConfigSummary = new BuildConfiguration();
                        buildConfigSummary.BuildConfigFlavor = configSummary.Flavor;  // "Mixed Platform";
                        buildConfigSummary.BuildConfigPlatform = configSummary.Platform; // "Debug";
                        buildConfigSummary.BuildConfigTotalErrors = configSummary.TotalCompilationErrors.ToString(); // "0";
                        buildConfigSummary.BuildConfigTotalWarnings = configSummary.TotalCompilationWarnings.ToString(); // "1";

                        buildDetail.BuildConfigurations.Add(buildConfigSummary);
                    }

                    // Load Build Configuration Solutions into BuildDetails
                    foreach (IBuildProjectNode buildConfig in InformationNodeConverters.GetTopLevelProjects(build.Information))
                    {
                        var buildConfigurationSolution = new BuildConfigurationSolution();
                        buildConfigurationSolution.BuildRootNodeSolutionServerPath = buildConfig.ServerPath;
                        buildConfigurationSolution.BuildRootNodeErrorCount = buildConfig.CompilationErrors.ToString();
                        buildConfigurationSolution.BuildRootNodeWarningCount = buildConfig.CompilationWarnings.ToString();
                        buildConfigurationSolution.BuildRootNodeLogFile = buildConfig.LogFile != null ? buildConfig.LogFile.Url.AbsolutePath : string.Empty;

                        buildDetail.BuildConfigurationSolutions.Add(buildConfigurationSolution);
                    }
                }

                if (!this.Exclusions.Contains("Changesets"))
                {
                    // check if the user selected Changesets, Load changesets into BuildDetails
                    foreach (IChangesetSummary changesetInfo in InformationNodeConverters.GetAssociatedChangesets(build.Information))
                    {
                        var changesetDetail = this.VersionControlServer.GetChangeset(changesetInfo.ChangesetId);
                        buildDetail.Changesets.Add(new WordDocumentGenerator.Client.Entities.Changeset
                        {
                            ChangesetId = changesetDetail.ChangesetId.ToString(),
                            ChangesetUri = changesetDetail.ArtifactUri, // new Uri(@"vstfs:///VersionControl/Changeset/63"),
                            ChangesetCommittedOn = changesetDetail.CreationDate.ToString(), // "01-01-2011 12:05:01",
                            ChangesetCommittedBy = changesetDetail.Owner, // @"UK\tarora",
                            ChangesetComment = changesetDetail.Comment // "Refactoring code improvements"
                        });

                        var changes = new List<string>();
                        foreach (var ch in changesetDetail.Changes)
                        {
                            changes.Add(ch.Item.ServerItem);
                        }

                        buildDetail.ChangesetChangeDetails.Add(new ChangesetChangeDetail
                        {
                            ChangesetChangeId = changesetDetail.ChangesetId.ToString(),
                            ChangesetChangeServerPaths = changes
                        });
                    }
                }

                if (!this.Exclusions.Contains("WorkItems"))
                {
                    // check if the user selected WorkItems
                    // Load workItems into BuildDetails
                    foreach (IWorkItemSummary workItemInfo in InformationNodeConverters.GetAssociatedWorkItems(build))
                    {
                        var workItemDetail = WorkItemStore.GetWorkItem(workItemInfo.WorkItemId);
                        buildDetail.WorkItems.Add(new WordDocumentGenerator.Client.Entities.WorkItem()
                        {
                            WorkItemId = workItemDetail.Id.ToString(),
                            WorkItemUri = workItemDetail.Uri,
                            WorkItemType = workItemDetail.Type.Name,
                            WorkItemTitle = workItemDetail.Title,
                            WorkItemState = workItemDetail.State,
                            WorkItemIterationPath = workItemDetail.IterationPath,
                            WorkItemAreaPath = workItemDetail.AreaPath,
                            WorkItemAssignedTo = workItemInfo.AssignedTo
                        });
                    }

                    string introduction = string.Empty;
                    foreach (var wiT in buildDetail.WorkItems.Select(d => d.WorkItemType).Distinct())
                    {
                        introduction = string.Format("{0} {1}, ", buildDetail.WorkItems.Where(wi => wi.WorkItemType == wiT).ToList().Count, string.Format("{0}(s)", wiT));
                    }

                    if (introduction.Length != 0)
                    {
                        introduction = introduction.Remove(introduction.Length - 2);
                    }

                    buildDetail.WorkItemIntroduction = introduction;
                }

                if (!this.Exclusions.Contains("TestResults"))
                {
                    // check if user selected Test Results
                    // Load test results into BuildDetails
                    foreach (var tr in TestManagementService.GetTeamProject(buildDetail.Build.BuildTeamProject).TestRuns.ByBuild(build.Uri))
                    {
                        buildDetail.TestResult.TestResultId = tr.Id.ToString();
                        buildDetail.TestResult.TestResultTitle = tr.Title;
                        buildDetail.TestResult.TestResultStatus = tr.State.ToString(); // "Complete";
                        buildDetail.TestResult.TestResultTotalTest = tr.Statistics.TotalTests.ToString(); // "24";
                        buildDetail.TestResult.TestResultTotalTestCompleted = tr.Statistics.CompletedTests.ToString(); // "24";
                        buildDetail.TestResult.TestResultTotalTestPassed = tr.Statistics.PassedTests.ToString(); // "22";
                        buildDetail.TestResult.TestResultTotalTestFailed = tr.Statistics.FailedTests.ToString(); // "1";
                        buildDetail.TestResult.TestResultTotalTestInconclusive = tr.Statistics.InconclusiveTests.ToString(); // "1";

                        double result = 0.0;
                        if (tr.Statistics.TotalTests != 0)
                        {
                            // var passPerc = decimal.MinValue;
                            result = tr.Statistics.PassedTests * 100 / tr.Statistics.TotalTests;
                        }

                        buildDetail.TestResult.TestResultTotalTestPassRate = result.ToString();
                        buildDetail.TestResult.TestResultIsAutomated = tr.IsAutomated.ToString(); // "false";

                        // Load testResultPassedItems into BuildDetails
                        // Check for passed tests
                        foreach (ITestCaseResult lp in tr.QueryResultsByOutcome(TestOutcome.Passed))
                        {
                            buildDetail.TestResultPassedItems.Add(new WordDocumentGenerator.Client.Entities.TestResultPassedItem()
                            {
                                TestResultPassedListStatus = "Passed",
                                TestResultPassedListTitle = lp.TestCaseTitle
                            });
                        }

                        // Load testResultsFailedItems into BuildDetails
                        foreach (ITestCaseResult lf in tr.QueryResultsByOutcome(TestOutcome.Failed))
                        {
                            buildDetail.TestResultFailedItems.Add(new WordDocumentGenerator.Client.Entities.TestResultFailedItem()
                            {
                                TestResultFailedListStatus = "Failed",
                                TestResultFailedListTitle = lf.TestCaseTitle,
                                TestResultFailedListReason = lf.ErrorMessage
                            });
                        }

                        // Load testResultsInconclusiveItems into BuildDetails
                        foreach (ITestCaseResult li in tr.QueryResultsByOutcome(TestOutcome.Inconclusive))
                        {
                            buildDetail.TestResultInconclusiveItems.Add(new WordDocumentGenerator.Client.Entities.TestResultInconclusiveItem()
                            {
                                TestResultInconclusiveListStatus = "Inconclusive",
                                TestResultInconclusiveListTitle = li.TestCaseTitle
                            });
                        }
                    }
                }

                buildDetails.Add(buildDetail);
            }

            return buildDetails;
        }
    }
}