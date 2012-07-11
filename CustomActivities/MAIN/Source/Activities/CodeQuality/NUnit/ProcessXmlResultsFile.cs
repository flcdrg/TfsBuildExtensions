using System.Activities;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Xml;

using Microsoft.TeamFoundation.Build.Client;

namespace TfsBuildExtensions.Activities.CodeQuality
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    internal sealed class ProcessXmlResultsFile : BaseTestResultsCodeActivity
    {
        public OutArgument<int> Failures { get; set; }

        /// <summary>
        /// Gets the NotRun count
        /// </summary>
        public OutArgument<int> NotRun { get; set; }

        /// <summary>
        /// Gets the Total count
        /// </summary>
        public OutArgument<int> Total { get; set; }

        /// <summary>
        /// Gets the Errors count
        /// </summary>
        public OutArgument<int> Errors { get; set; }

        /// <summary>
        /// Gets the Inconclusive count
        /// </summary>
        public OutArgument<int> Inconclusive { get; set; }

        /// <summary>
        /// Gets the Ignored count
        /// </summary>
        public OutArgument<int> Ignored { get; set; }

        /// <summary>
        /// Gets the Skipped count
        /// </summary>
        public OutArgument<int> Skipped { get; set; }

        /// <summary>
        /// Gets the Invalid count
        /// </summary>
        public OutArgument<int> Invalid { get; set; }

        protected override void InternalExecute()
        {
            string folder = this.WorkingDirectory.Get(this.ActivityContext);

            string filename = Path.Combine(folder, this.GetResultFileName(this.ActivityContext));

            this.LogBuildMessage("Processing " + filename, BuildMessageImportance.High);
            if (File.Exists(filename))
            {
                var doc = new XmlDocument();
                try
                {
                    doc.Load(filename);
                }
                catch (Exception ex)
                {
                    this.LogBuildError(ex.Message);
                    return;
                }

                XmlNode root = doc.DocumentElement;
                if (root == null)
                {
                    this.LogBuildError("Failed to load the OutputXmlFile");
                    return;
                }

                this.Total.Set(this.ActivityContext, GetAttributeInt32Value("total", root));
                this.NotRun.Set(this.ActivityContext, GetAttributeInt32Value("not-run", root));
                this.Errors.Set(this.ActivityContext, GetAttributeInt32Value("errors", root));
                this.Failures.Set(this.ActivityContext, GetAttributeInt32Value("failures", root));
                this.Inconclusive.Set(this.ActivityContext, GetAttributeInt32Value("inconclusive", root));
                this.Ignored.Set(this.ActivityContext, GetAttributeInt32Value("ignored", root));
                this.Skipped.Set(this.ActivityContext, GetAttributeInt32Value("skipped", root));
                this.Invalid.Set(this.ActivityContext, GetAttributeInt32Value("invalid", root));

                if (this.Errors.Get(this.ActivityContext) > 0 || this.Failures.Get(this.ActivityContext) > 0)
                {
                    var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                    buildDetail.Status = BuildStatus.PartiallySucceeded;
                }
            }
        }

        private static int GetAttributeInt32Value(string name, XmlNode node)
        {
            if (node.Attributes[name] != null)
            {
                return Convert.ToInt32(node.Attributes[name].Value, CultureInfo.InvariantCulture);
            }

            return 0;
        }
    }
}