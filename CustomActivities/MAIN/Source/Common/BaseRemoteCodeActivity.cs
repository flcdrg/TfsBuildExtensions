//-----------------------------------------------------------------------
// <copyright file="BaseRemoteCodeActivity.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
// <autogenerated/>
// TODO: remove this autogenerated tag.
namespace TfsBuildExtensions.Activities
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Management;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.Build.Workflow.Activities;
    using Microsoft.TeamFoundation.Build.Workflow.Services;

    /// <summary>
    /// Provides a base class to all Activities which support remoting
    /// </summary>
    public abstract class BaseRemoteCodeActivity : CodeActivity, IBaseActivityMinimumArguments
    {
        private bool failingbuild;
        private InArgument<bool> logExceptionStack = true;
        private InArgument<string> machineName = Environment.MachineName;

        /// <summary>
        /// Set to true to fail the build if the activity logs any errors. Default is false
        /// </summary>
        [Description("Set to true to fail the build if errors are logged")]
        public InArgument<bool> FailBuildOnError { get; set; }

        /// <summary>
        /// Set to true to fail the build if the activity logs any errors. Default is false
        /// </summary>
        [Description("Set to true to make all warnings errors")]
        public InArgument<bool> TreatWarningsAsErrors { get; set; }

        /// <summary>
        /// Set to true to ignore any unhandled exceptions thrown by activities. Default is false
        /// </summary>
        [Description("Set to true to ignore unhandled exceptions")]
        public InArgument<bool> IgnoreExceptions { get; set; }

        /// <summary>
        /// Set to true to log the entire stack in the event of an exception. Default is true
        /// <para></para>
        /// <remarks>This parameter is ignored, if <see cref="FailBuildOnError"/> is true or <see cref="TreatWarningsAsErrors"/> is true </remarks>
        /// </summary>
        [Description("Set to true to log the entire stack in the event of an exception")]
        public InArgument<bool> LogExceptionStack
        {
            get { return this.logExceptionStack; }
            set { this.logExceptionStack = value; }
        }

        /// <summary>
        /// Variable to hold CodeActivityContext
        /// </summary>
        protected CodeActivityContext ActivityContext { get; set; }

        /// <summary>
        /// Variable to hold authenticationLevel
        /// </summary>
        private AuthenticationLevel authLevel = System.Management.AuthenticationLevel.Default;

        /// <summary>
        /// Sets the MachineName. Defaults to Environment.MachineName
        /// </summary>
        public InArgument<string> MachineName
        {
            get { return this.machineName; }
            set { this.machineName = value; }
        }

        /// <summary>
        /// Sets the UserName
        /// </summary>
        public InArgument<string> UserName { get; set; }

        /// <summary>
        /// Sets the UserPassword.
        /// </summary>
        public InArgument<string> UserPassword { get; set; }

        /// <summary>
        /// Sets the authority to be used to authenticate the specified user.
        /// </summary>
        public InArgument<string> Authority { get; set; }

        /// <summary>
        /// Sets the authentication level to be used to connect to WMI. Default is Default. Also supports: Call, Connect, None, Packet, PacketIntegrity, PacketPrivacy, Unchanged
        /// </summary>
        public InArgument<string> AuthenticationLevel { get; set; }

        internal ManagementScope Scope { get; set; }

        internal void GetManagementScope(string wmiNamespace)
        {
            if (string.Compare(this.MachineName.Get(this.ActivityContext), Environment.MachineName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.Scope = new ManagementScope("\\\\" + this.MachineName.Get(this.ActivityContext) + wmiNamespace);
            }
            else
            {
                ConnectionOptions options = new ConnectionOptions
                {
                    Authentication = this.authLevel,
                    Username = this.UserName.Get(this.ActivityContext),
                    Password = this.UserPassword.Get(this.ActivityContext),
                    Authority = this.Authority.Get(this.ActivityContext)
                };
                this.Scope = new ManagementScope("\\\\" + this.MachineName.Get(this.ActivityContext) + wmiNamespace, options);
            }
        }

        internal void GetManagementScope(string wmiNamespace, ConnectionOptions options)
        {
            this.Scope = new ManagementScope("\\\\" + this.MachineName.Get(this.ActivityContext) + wmiNamespace, options);
        }

        /// <summary>
        /// Executes the logic for this workflow activity
        /// </summary>
        /// <param name="context">CodeActivityContext</param>
        protected override void Execute(CodeActivityContext context)
        {
            this.ActivityContext = context;
            try
            {
                if (!string.IsNullOrEmpty(context.GetValue(this.AuthenticationLevel)))
                {
                    this.authLevel = (AuthenticationLevel)Enum.Parse(typeof(AuthenticationLevel), context.GetValue(this.AuthenticationLevel));
                }

                this.InternalExecute();
            }
            catch (Exception ex)
            {
                if (!this.failingbuild)
                {
                    if (this.LogExceptionStack.Get(context))
                    {
                        string innerException = string.Empty;
                        if (ex.InnerException != null)
                        {
                            innerException = string.Format("Inner Exception: {0}", ex.InnerException.Message);
                        }

                        this.LogBuildError(string.Format("Error: {0}. Stack Trace: {1}. {2}", ex.Message, ex.StackTrace, innerException));
                    }
                }

                if (this.IgnoreExceptions.Get(context) != true)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// InternalExecute method which activities should implement
        /// </summary>
        protected abstract void InternalExecute();

        /// <summary>
        /// Logs a message as a build error
        /// Also can fail the build if the FailBuildOnError flag is set
        /// </summary>
        /// <param name="errorMessage">Message to save</param>
        protected void LogBuildError(string errorMessage)
        {
            Debug.WriteLine(string.Format("BuildError: {0}", errorMessage));
            if (this.FailBuildOnError.Get(this.ActivityContext))
            {
                this.failingbuild = true;
                var buildDetail = this.ActivityContext.GetExtension<IBuildDetail>();
                buildDetail.Status = BuildStatus.Failed;
                buildDetail.Save();
                throw new Exception(errorMessage);
            }

            this.ActivityContext.TrackBuildError(errorMessage);
        }

        /// <summary>
        /// Logs a message as a build warning
        /// </summary>
        /// <param name="warningMessage">Message to save</param>
        protected void LogBuildWarning(string warningMessage)
        {
            if (this.TreatWarningsAsErrors.Get(this.ActivityContext))
            {
                this.LogBuildError(warningMessage);
            }
            else
            {
                this.ActivityContext.TrackBuildWarning(warningMessage);
                Debug.WriteLine(string.Format("BuildWarning: {0}", warningMessage));
            }
        }

        /// <summary>
        /// Logs a generical build message
        /// </summary>
        /// <param name="message">The message to save</param>
        /// <param name="importance">The verbosity importance of the message</param>
        protected void LogBuildMessage(string message, BuildMessageImportance importance = BuildMessageImportance.Normal)
        {
            this.ActivityContext.TrackBuildMessage(message, importance);
            Debug.WriteLine(string.Format("BuildMessage: {0}", message));
        }

        /// <summary>
        /// Logs a link to the build log
        /// </summary>
        /// <param name="message">Message to save as link name</param>
        /// <param name="uri">Uri for link</param>
        protected void LogBuildLink(string message, Uri uri)
        {
            IActivityTracking currentTracking = this.ActivityContext.GetExtension<IBuildLoggingExtension>().GetActivityTracking(this.ActivityContext);
            var link = currentTracking.Node.Children.AddExternalLink(message, uri);
            link.Save();
            Debug.WriteLine(string.Format("BuildLink: {0}, Uri: {1}", message, uri));
        }

        /// <summary>
        /// Add a text node to the build log
        /// </summary>
        /// <param name="text">Display text</param>
        /// <param name="parent">Parent node in the build log</param>
        /// <returns>The new node containing the supplied text</returns>
        protected static IBuildInformationNode AddTextNode(string text, IBuildInformationNode parent)
        {
            IBuildInformationNode childNode = parent.Children.CreateNode();
            childNode.Type = parent.Type;
            childNode.Fields.Add("DisplayText", text);
            childNode.Save();
            return childNode;
        }

        /// <summary>
        /// Add a hyperlink to the
        /// </summary>
        /// <param name="text">Display text of the hyperlink</param>
        /// <param name="uri">Uri of the hyperlink</param>
        /// <param name="parent">Parent node in the build log</param>
        /// <returns>The new external link containing the supplied hyperlink</returns>
        protected static IExternalLink AddLinkNode(string text, Uri uri, IBuildInformationNode parent)
        {
            var link = parent.Children.AddExternalLink(text, uri);
            link.Save();
            return link;
        }
    }
}