using System.Activities;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Xsl;

using Microsoft.TeamFoundation.Build.Client;

namespace TfsBuildExtensions.Activities.CodeQuality
{
    internal sealed class PublishTestResultsToTfs : BaseTestResultsCodeActivity
    {
        public InArgument<string> Platform { get; set; }

        /// <summary>
        /// Which flavor to publish test results for (ex. Debug)
        /// </summary>
        public InArgument<string> Flavor { get; set; }

        protected override void InternalExecute()
        {
            if (string.IsNullOrEmpty(this.Platform.Get(this.ActivityContext)) || string.IsNullOrEmpty(this.Flavor.Get(this.ActivityContext)))
            {
                this.LogBuildError("When publishing test results, both Platform and Flavor must be specified");
                return;
            }

            string folder = this.WorkingDirectory.Get(this.ActivityContext);

            string filename = Path.Combine(folder, this.GetResultFileName(this.ActivityContext));

            string resultTrxFile = Path.Combine(folder, Path.GetFileNameWithoutExtension(this.GetResultFileName(this.ActivityContext)) + ".trx");
            if (!File.Exists(filename))
            {
                return;
            }

            this.TransformNUnitToMsTest(filename, resultTrxFile);

            var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
            string collectionUrl = buildDetail.BuildServer.TeamProjectCollection.Uri.ToString();
            string buildNumber = buildDetail.BuildNumber;
            string teamProject = buildDetail.TeamProject;
            string platform = this.Platform.Get(this.ActivityContext);
            string flavor = this.Flavor.Get(this.ActivityContext);
            this.PublishMsTestResults(resultTrxFile, collectionUrl, buildNumber, teamProject, platform, flavor);
        }

        private void TransformNUnitToMsTest(string nunitResultFile, string mstestResultFile)
        {
            Stream s = this.GetType().Assembly.GetManifestResourceStream("TfsBuildExtensions.Activities.CodeQuality.NUnit.NUnitToMSTest.xslt");
            if (s == null)
            {
                this.LogBuildError("Could not load NUnitToMSTest.xslt from embedded resources");
                return;
            }

            using (var reader = new XmlTextReader(s))
            {
                var transform = new XslCompiledTransform();
                transform.Load(reader);
                transform.Transform(nunitResultFile, mstestResultFile);
            }
        }

        private void PublishMsTestResults(string resultTrxFile, string collectionUrl, string buildNumber, string teamProject, string platform, string flavor)
        {
            string argument = string.Format("/publish:\"{0}\" /publishresultsfile:\"{1}\" /publishbuild:\"{2}\" /teamproject:\"{3}\" /platform:\"{4}\" /flavor:\"{5}\"", collectionUrl, resultTrxFile, buildNumber, teamProject, platform, flavor);
            this.RunProcess(Environment.ExpandEnvironmentVariables(@"%VS100COMNTOOLS%\..\IDE\MSTest.exe"), null, argument);
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