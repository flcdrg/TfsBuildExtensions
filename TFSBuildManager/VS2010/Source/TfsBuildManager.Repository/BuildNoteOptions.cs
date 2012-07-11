﻿//-----------------------------------------------------------------------
// <copyright file="BuildNoteOptions.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildManager.Repository
{
    using System;

    [Flags]
    public enum BuildNoteOptions
    {
        /// <summary>
        /// WorkItems Details
        /// </summary>
        WorkItemDetails = 1,

        /// <summary>
        /// Testresults
        /// </summary>
        TestResults = 2,
        
        /// <summary>
        /// Changesets Details
        /// </summary>
        ChangesetDetails = 3,

        /// <summary>
        /// Build Configuration Summary
        /// </summary>
        BuildConfigurationSummary = 4,

        /// <summary>
        /// Check in Build notes to TFS
        /// </summary>
        CheckIntoTfs = 5
    }
}
