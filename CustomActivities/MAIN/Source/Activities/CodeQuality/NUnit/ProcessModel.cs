namespace TfsBuildExtensions.Activities.CodeQuality
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Controls how NUnit loads tests in processes
    /// </summary>
    public enum ProcessModel
    {
        /// <summary>
        /// All the tests are run in the nunit-console process
        /// </summary>
        Single, 

        /// <summary>
        /// A separate process is created to run the tests
        /// </summary>
        Separate, 

        /// <summary>
        /// A separate process is created for each test assembly, whether specified on the command line or listed in an NUnit project file
        /// </summary>
        Multiple
    }
}