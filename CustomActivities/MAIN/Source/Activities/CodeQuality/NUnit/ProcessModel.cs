// <copyright file="ProcessModel.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>

namespace TfsBuildExtensions.Activities.CodeQuality
{
    /// <summary>
    /// Controls how NUnit loads tests in processes
    /// </summary>
    public enum ProcessModel
    {
        /// <summary>
        /// Default process model
        /// </summary>
        Default,

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