// <copyright file="DomainUsage.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>

namespace TfsBuildExtensions.Activities.CodeQuality
{
    /// <summary>
    /// Controls of the creation of AppDomains for running tests
    /// </summary>
    public enum DomainUsage
    {
        /// <summary>
        /// Use multiple domains if multiple assemblies are listed on the command line. Otherwise a single domain is used
        /// </summary>
        Default, 

        /// <summary>
        /// No domain is created - the tests are run in the primary domain
        /// </summary>
        None, 

        /// <summary>
        /// A test domain is created
        /// </summary>
        Single, 

        /// <summary>
        /// A separate test domain is created for each assembly
        /// </summary>
        Multiple
    }
}