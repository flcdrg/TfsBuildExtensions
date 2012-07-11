using System.Activities;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TfsBuildExtensions.Activities.CodeQuality
{
    internal abstract class BaseTestResultsCodeActivity : BaseCodeActivity
    {
        public InArgument<string> OutputXmlFile { get; set; }

        public InArgument<string> WorkingDirectory { get; set; }

        protected string GetResultFileName(ActivityContext context)
        {
            string filename = "TestResult.xml";
            if (this.OutputXmlFile.Get(context) != null && !string.IsNullOrEmpty(this.OutputXmlFile.Get(context)))
            {
                filename = this.OutputXmlFile.Get(context);
            }

            return filename;
        }
    }
}