//-----------------------------------------------------------------------
// <copyright file="BuildManagerViewModel.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildManager.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Microsoft.TeamFoundation.Build.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using TfsBuildManager.Repository;
    using TfsBuildManager.Views.ViewModels;

    public class BuildManagerViewModel : ViewModelBase
    {
        public const string AllItem = "All";

        private readonly Window owner;
        private readonly ITfsContext context;
        private readonly ITfsClientRepository repository;
        private readonly IMainView view;
        private bool includeDisabledBuildDefinitions;
        private DateFilter selectedBuildDateFilter;
        private BuildFilter selectedBuildFilter;
        private BuildView selectedBuildView;

        public BuildManagerViewModel(Window owner, ITfsClientRepository repository, IMainView view, IEnumerable<IBuildController> controllers, IEnumerable<TeamProject> teamProjects, ITfsContext context)
        {
            this.owner = owner;
            this.repository = repository;
            this.view = view;
            this.context = context;
            this.BuildDefinitions = new ObservableCollection<BuildDefinitionViewModel>();
            this.Builds = new ObservableCollection<BuildViewModel>();
            this.DeleteCommand = new DelegateCommand(this.OnDelete);
            this.OpenDropFolderCommand = new DelegateCommand(this.OnOpenDropfolder);
            this.RetainIndefinitelyCommand = new DelegateCommand(this.OnRetainIndefinitely);
            this.SetBuildQualityCommand = new DelegateCommand(this.OnSetBuildQuality);
            this.BuildNotesCommand = new DelegateCommand(this.OnBuildNotes);
            this.DeleteBuildCommand = new DelegateCommand(this.OnDeleteBuild);
            this.ShowDetailsCommand = new DelegateCommand(this.OnShowDetails);
            this.ShowQueuedDetailsCommand = new DelegateCommand(this.OnShowQueuedDetails);
            this.StopBuildCommand = new DelegateCommand(this.OnStopBuild);
            this.ChangeBuildTemplateCommand = new DelegateCommand(this.OnChangeBuildTemplate);
            this.SetDefaultBuildTemplateCommand = new DelegateCommand(this.OnSetDefaultBuildTemplate, this.OnCanSetDefaultBuildTemplate);
            this.AddBuildProcessTemplateCommand = new DelegateCommand(this.OnAddBuildProcessTemplate);
            this.RemoveBuildProcessTemplateCommand = new DelegateCommand(this.OnRemoveBuildProcessTemplate);
            this.EnableCommand = new DelegateCommand(this.OnEnable, this.OnCanEnable);
            this.DisableCommand = new DelegateCommand(this.OnDisable, this.OnCanDisable);
            this.SetRetentionPoliciesCommand = new DelegateCommand(this.OnSetRetentionsPolicies);
            this.ChangeBuildControllerCommand = new DelegateCommand(this.OnChangeBuildController);
            this.ChangeDefaultDropLocationCommand = new DelegateCommand(this.OnChangeDefaultDropLocation);

            this.CloneBuildsCommand = new DelegateCommand(this.OnCloneBuilds, this.OnCanCloneBuilds);
            this.QueueBuildsCommand = new DelegateCommand(this.OnQueueBuilds, this.OnCanQueueBuilds);
            this.EditBuildDefinitionCommand = new DelegateCommand(this.OnEditBuildDefinition, this.OnCanEditBuildDefinition);
            this.GenerateBuildResourcesCommand = new DelegateCommand(this.OnGenerateBuildResources);
            this.Controllers = new ObservableCollection<string>(controllers.Select(c => c.Name));
            this.RefreshCurrentView = new DelegateCommand(this.OnRefreshCurrentView);
            this.Controllers.Insert(0, AllItem);
            this.TeamProjects = new ObservableCollection<string>(teamProjects.Select(tp => tp.Name));
            this.TeamProjects.Insert(0, AllItem);
            this.SelectedBuildFilter = BuildFilter.Queued;
            this.includeDisabledBuildDefinitions = false;
            this.BuildViews = new ObservableCollection<BuildViewItem> { new BuildViewItem { Name = "Build Definitions", Value = BuildView.BuildDefinitions }, new BuildViewItem { Name = "Builds", Value = BuildView.Builds }, new BuildViewItem() { Name = "Build Process Templates", Value = BuildView.BuildProcessTemplates } };
            this.DateFilters = new DateFilterCollection();
            this.SelectedBuildView = BuildView.BuildDefinitions;
            this.selectedBuildDateFilter = DateFilter.Today;
            this.BuildProcessTemplatess = new ObservableCollection<BuildTemplateViewModel>();
        }

        public event EventHandler Refresh;

        public ICommand DeleteCommand { get; private set; }

        public ICommand DisableCommand { get; private set; }

        public ICommand EnableCommand { get; private set; }

        public ICommand ChangeBuildTemplateCommand { get; private set; }

        public ICommand SetDefaultBuildTemplateCommand { get; private set; }

        public ICommand AddBuildProcessTemplateCommand { get; private set; }

        public ICommand RemoveBuildProcessTemplateCommand { get; private set; }

        public ICommand OpenDropFolderCommand { get; private set; }

        public ICommand DeleteBuildCommand { get; private set; }

        public ICommand ShowDetailsCommand { get; private set; }

        public ICommand ShowQueuedDetailsCommand { get; private set; }

        public ICommand StopBuildCommand { get; private set; }

        public ICommand RetainIndefinitelyCommand { get; private set; }

        public ICommand SetBuildQualityCommand { get; private set; }

        public ICommand BuildNotesCommand { get; private set; }

        public ICommand ChangeBuildControllerCommand { get; private set; }

        public ICommand ChangeDefaultDropLocationCommand { get; private set; }

        public ICommand SetRetentionPoliciesCommand { get; private set; }

        public ICommand QueueBuildsCommand { get; private set; }

        public ICommand EditBuildDefinitionCommand { get; private set; }

        public ICommand CloneBuildsCommand { get; private set; }

        public ICommand GenerateBuildResourcesCommand { get; private set; }

        public ICommand RefreshCurrentView { get; private set; }

        public ObservableCollection<BuildDefinitionViewModel> BuildDefinitions { get; private set; }

        public ObservableCollection<BuildViewModel> Builds { get; private set; }

        public ObservableCollection<BuildTemplateViewModel> BuildProcessTemplatess { get; private set; }

        public ObservableCollection<string> Controllers { get; private set; }

        public ObservableCollection<string> TeamProjects { get; private set; }

        public ObservableCollection<BuildViewItem> BuildViews { get; private set; }

        public DateFilterCollection DateFilters { get; private set; }

        public BuildFilter SelectedBuildFilter
        {
            get
            {
                return this.selectedBuildFilter;
            }

            set
            {
                var old = this.selectedBuildFilter;
                this.selectedBuildFilter = value;
                if (value != old)
                {
                    this.NotifyPropertyChanged("SelectedBuildFilter");
                }
            }
        }

        public bool IncludeDisabledBuildDefinitions
        {
            get
            {
                return this.includeDisabledBuildDefinitions;
            }

            set
            {
                var old = this.includeDisabledBuildDefinitions;
                this.includeDisabledBuildDefinitions = value;
                if (value != old)
                {
                    this.NotifyPropertyChanged("includeDisabledBuildDefinitions");
                }
            }
        }

        public BuildView SelectedBuildView
        {
            get
            {
                return this.selectedBuildView;
            }

            set
            {
                var old = this.selectedBuildView;
                this.selectedBuildView = value;
                if (value != old)
                {
                    this.NotifyPropertyChanged("SelectedBuildView");
                    this.NotifyPropertyChanged("BuildDefinitionViewVisible");
                    this.NotifyPropertyChanged("BuildsViewVisible");
                    this.NotifyPropertyChanged("BuildProcessTemplateViewVisible");
                }
            }
        }

        public DateFilter SelectedDateFilter
        {
            get
            {
                return this.selectedBuildDateFilter;
            }

            set
            {
                var old = this.selectedBuildDateFilter;
                this.selectedBuildDateFilter = value;
                if (value != old)
                {
                    this.OnRefresh(new EventArgs());
                }
            }
        }

        public Visibility BuildDefinitionViewVisible
        {
            get { return this.SelectedBuildView == BuildView.BuildDefinitions ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility BuildsViewVisible
        {
            get { return this.SelectedBuildView == BuildView.Builds ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility BuildProcessTemplateViewVisible
        {
            get { return this.SelectedBuildView == BuildView.BuildProcessTemplates ? Visibility.Visible : Visibility.Collapsed; }
        }

        public void OnDelete()
        {
            try
            {
                var items = this.view.SelectedItems;
                DeleteOptions? options = this.PromptDeleteOptions();
                if (options.HasValue)
                {
                    using (new WaitCursor())
                    {
                        this.repository.DeleteBuildDefinitions(items.Select(b => b.Uri), options.Value);
                        this.OnRefresh(new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void OnSetDefaultBuildTemplate()
        {
            try
            {
                var items = this.view.SelectedBuildProcessTemplates;
                foreach (var item in items)
                {
                    this.repository.SetDefaultBuildProcessTemplate(item.ServerPath, item.TeamProject);
                }

                this.OnRefresh(new EventArgs());
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public bool OnCanSetDefaultBuildTemplate()
        {
            var items = this.view.SelectedBuildProcessTemplates.ToList();
            return items.Count() == 1 && items.First().TemplateType != "Default";
        }

        public void OnAddBuildProcessTemplate()
        {
            try
            {
                var items = this.view.SelectedBuildProcessTemplates;
                var projects = this.repository.AllTeamProjects.Select(tp => tp.Name).ToList();
                var viewModel = new TeamProjectListViewModel(projects);

                var wnd = new SelectTeamProject(viewModel, null);
                bool? res = wnd.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    using (new WaitCursor())
                    {
                        this.repository.AddBuildProcessTemplates(items.Select(pt => pt.ServerPath), wnd.SelectedTeamProjects.Select(tp => tp.Name), wnd.SetAsDefault);
                        this.OnRefresh(new EventArgs());
                    }

                    this.OnRefresh(new EventArgs());
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void OnRemoveBuildProcessTemplate()
        {
            try
            {
                var items = this.view.SelectedBuildProcessTemplates.ToList();
                var projects = this.repository.AllTeamProjects.Select(tp => tp.Name).ToList();
                var viewModel = new TeamProjectListViewModel(projects);

                var wnd = new SelectTeamProject(viewModel, this.view.SelectedTeamProject);
                wnd.cbSetAsDefault.Visibility = Visibility.Collapsed;
                bool? res = wnd.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    using (new WaitCursor())
                    {
                        this.repository.RemoveBuildProcessTemplates(items.Select(pt => pt.ServerPath), wnd.SelectedTeamProjects.Select(tp => tp.Name), this.ShouldRemove);
                        this.OnRefresh(new EventArgs());
                    }

                    this.OnRefresh(new EventArgs());
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void OnDisable()
        {
            try
            {
                var items = this.view.SelectedItems;
                using (new WaitCursor())
                {
                    this.repository.DisableBuildDefinitions(items.Select(b => b.Uri));
                    this.OnRefresh(new EventArgs());
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public bool OnCanDisable()
        {
            return !this.view.SelectedItems.Any(s => !s.Enabled);
        }

        public void OnOpenDropfolder()
        {
            try
            {
                var items = this.view.SelectedBuilds;
                using (new WaitCursor())
                {
                    if (!this.repository.OpenDropFolder(items.Select(b => b.DropLocation)))
                    {
                        MessageBox.Show("No valid drop folders were found for the selected builds", "Open Drop Folder", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void OnShowDetails()
        {
            foreach (var selectedBuild in this.view.SelectedBuilds)
            {
                this.context.ShowBuild(selectedBuild.Uri);
            }
        }

        public void OnShowQueuedDetails()
        {
            foreach (var selectedBuild in this.view.SelectedActiveBuilds)
            {
                this.context.ShowBuild(selectedBuild.Uri);
            }
        }

        public void OnDeleteBuild()
        {
            try
            {
                if (MessageBox.Show(this.owner, "Confirm Delete. ALL artefacts will be deleted.", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var items = this.view.SelectedBuilds;
                    using (new WaitCursor())
                    {
                        this.repository.DeleteBuilds(items.Select(b => b.FullBuildDetail));
                        this.OnRefresh(new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void OnStopBuild()
        {
            try
            {
                if (MessageBox.Show(this.owner, "Confirm stop.", "Stop", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var items = this.view.SelectedActiveBuilds;
                    using (new WaitCursor())
                    {
                        this.repository.StopBuilds(items.Select(b => b.QueuedBuildDetail));
                        this.OnRefresh(new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void OnBuildNotes()
        {
            try
            {
                var items = this.view.SelectedBuilds;
                var options = this.PromptNotesOptions();
                if (options != null)
                {
                    using (new WaitCursor())
                    {
                        this.repository.GenerateBuildNotes(items.Select(b => b.FullBuildDetail), options, this.context.SelectedProject);
                        this.OnRefresh(new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void OnRetainIndefinitely()
        {
            try
            {
                var items = this.view.SelectedBuilds;
                using (new WaitCursor())
                {
                    this.repository.RetainIndefinitely(items.Select(b => b.FullBuildDetail));
                    this.OnRefresh(new EventArgs());
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void OnSetBuildQuality()
        {
            try
            {
                var items = this.view.SelectedBuilds.ToList();
                var buildQualities = this.repository.GetBuildQualities(items.Select(bd => bd.TeamProject).ToList().Distinct());
                var wnd = new SelectBuildQuality(new BuildQualityListViewModel(buildQualities));
                bool? res = wnd.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    using (new WaitCursor())
                    {
                        this.repository.SetBuildQualities(items.Select(bd => bd.Uri), wnd.SelectedBuildQuality.Name);
                        this.OnRefresh(new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void OnEnable()
        {
            try
            {
                var items = this.view.SelectedItems;
                using (new WaitCursor())
                {
                    this.repository.EnableBuildDefinitions(items.Select(b => b.Uri));
                    this.OnRefresh(new EventArgs());
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public bool OnCanEnable()
        {
            return !this.view.SelectedItems.Any(s => s.Enabled);
        }

        public void AssignBuildDefinitions(IEnumerable<IBuildDefinition> builds)
        {
            try
            {
                this.BuildDefinitions.Clear();
                foreach (var b in builds.Select(b => new BuildDefinitionViewModel(b)))
                {
                    this.BuildDefinitions.Add(b);
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void AssignBuilds(IEnumerable<IBuildDetail> builds)
        {
            try
            {
                this.Builds.Clear();
                foreach (var b in builds.Where(b => b != null).Select(b => new BuildViewModel(b)))
                {
                    this.Builds.Add(b);
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void AssignBuildProcessTemplates(IEnumerable<IProcessTemplate> buildProcessTemplates)
        {
            try
            {
                this.BuildProcessTemplatess.Clear();
                foreach (var b in buildProcessTemplates.Where(b => b != null).Select(b => new BuildTemplateViewModel(b)))
                {
                    this.BuildProcessTemplatess.Add(b);
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        public void AssignBuilds(IEnumerable<IQueuedBuild> queuedBuilds)
        {
            try
            {
                this.Builds.Clear();
                foreach (var b in queuedBuilds.Where(b => b != null).Select(b => new BuildViewModel(b)))
                {
                    this.Builds.Add(b);
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        private static string GetCommonString(List<string> lst, bool caseSensitive = false)
        {
            string shortest = lst[0];
            for (int i = 0; i < lst.Count; i++)
            {
                string s = lst[i];
                if (s.Length < shortest.Length)
                {
                    shortest = s;
                }

                if (!caseSensitive)
                {
                    lst[i] = lst[i].ToUpper();
                }
            }

            for (int i = 0; i < shortest.Length; i++)
            {
                char c = lst[0].ElementAt(i);

                foreach (string s in lst)
                {
                    if (c != s.ElementAt(i))
                    {
                        return shortest.Substring(0, i);
                    }
                }
            }

            return shortest;
        }

        private void ShowNoBranchMessage(string project)
        {
            MessageBox.Show(this.owner, "Could not locate branch object for " + project, "Clone Build To Branch", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private void ShowInvalidActionMessage(string action, string message)
        {
            MessageBox.Show(this.owner, message, action, MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private bool ShouldRemove()
        {
            return MessageBox.Show(this.owner, "One or more of the selected build process templates are used by build definitions. Do you want to proceed?", "Remove Build Process Template", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        private IEnumerable<string> PromptNotesOptions()
        {
            try
            {
                var wnd = new BuildNotesOptionWnd();
                bool? action = wnd.ShowDialog();
                if (action.HasValue && action.Value)
                {
                    return wnd.Option;
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }

            return null;
        }

        private DeleteOptions? PromptDeleteOptions()
        {
            try
            {
                var wnd = new DeleteOptionsWindow();
                var action = wnd.ShowDialog();
                if (action.HasValue && action.Value)
                {
                    return wnd.Option;
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }

            return null;
        }

        private bool OnCanCloneBuilds()
        {
            try
            {
                return this.view.SelectedItems.Count() == 1;
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }

            return false;
        }

        private void OnRefreshCurrentView()
        {
            this.OnRefresh(new EventArgs());
        }

        private void OnGenerateBuildResources()
        {
            try
            {
                using (new WaitCursor())
                {
                    var dgml = this.repository.GenerateBuildResourcesDependencyGraph();
                    string tempFile = Path.GetTempFileName();
                    tempFile = Path.ChangeExtension(tempFile, ".dgml");
                    using (var sw = new StreamWriter(tempFile))
                    {
                        sw.Write(dgml);
                    }

                    Process.Start(tempFile);
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        private void OnCloneBuilds()
        {
            try
            {
                var items = this.view.SelectedItems.ToList();
                if (items.Count() != 1)
                {
                    return;
                }

                var item = items.First();
                using (new WaitCursor())
                {
                    var projects = this.repository.GetProjectsToBuild(item.Uri).ToList();
                    if (!projects.Any())
                    {
                        this.ShowInvalidActionMessage("Clone Build to Branch", "Could not locate any projects in the selected build(s)");
                        return;
                    }

                    var project = projects.First();
                    var branchObject = this.repository.GetBranchObjectForItem(project);
                    if (branchObject == null)
                    {
                        this.ShowNoBranchMessage(project);
                        return;
                    }

                    var childBranches = this.repository.GetChildBranchObjectsForItem(branchObject.ServerPath).ToList();
                    if (!childBranches.Any())
                    {
                        MessageBox.Show(this.owner, "No branch exist for " + branchObject.ServerPath, "Clone Build To Branch", MessageBoxButton.OK, MessageBoxImage.Stop);
                        return;
                    }

                    var dlg = new SelectTargetBranchWindow(item.Name, childBranches, item.TeamProject, this.repository);
                    bool? res = dlg.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        this.repository.CloneBuild(item.Uri, dlg.NewBuildDefinitionName, branchObject, dlg.SelectedTargetBranch);
                    }

                    this.OnRefresh(new EventArgs());
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        private void OnQueueBuilds()
        {
            try
            {
                var items = this.view.SelectedItems;
                using (new WaitCursor())
                {
                    this.repository.QueueBuilds(items.Select(b => b.Uri));
                    this.SelectedBuildView = BuildView.Builds;
                    this.SelectedBuildFilter = BuildFilter.Queued;
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        private void OnEditBuildDefinition()
        {
            try
            {
                if (this.selectedBuildView == BuildView.BuildDefinitions)
                {
                    var buildDefinitionUri = this.view.SelectedItems.First().Uri;
                    this.context.EditBuildDefinition(buildDefinitionUri);
                }
                else if (this.selectedBuildFilter == BuildFilter.Completed)
                {
                    var buildDefinitionUri = this.view.SelectedBuilds.First().BuildDefinitionUri;
                    this.context.EditBuildDefinition(buildDefinitionUri);
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        private bool OnCanEditBuildDefinition()
        {
            try
            {
                return this.view.SelectedItems.Count() == 1 || this.view.SelectedBuilds.Count() == 1;
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }

            return false;
        }

        private bool OnCanQueueBuilds()
        {
            try
            {
                return this.view.SelectedItems.Any(s => !s.HasProcess);
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }

            return false;
        }

        private void OnSetRetentionsPolicies()
        {
            try
            {
                var items = this.view.SelectedItems;
                var wnd = new RetentionPolicyWindow();
                bool? res = wnd.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    using (new WaitCursor())
                    {
                        this.repository.SetRetentionPolicies(items.Select(bd => bd.Uri), wnd.BuildRetentionPolicy);
                        this.OnRefresh(new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        private void OnRefresh(EventArgs e)
        {
            try
            {
                EventHandler handler = this.Refresh;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        private void OnChangeBuildTemplate()
        {
            try
            {
                var items = this.view.SelectedItems.ToList();
                var teamProjects = items.Select(i => i.TeamProject).Distinct();

                IEnumerable<IProcessTemplate> buildTemplates;
                using (new WaitCursor())
                {
                    buildTemplates = this.repository.GetBuildProcessTemplates(teamProjects);
                }

                var viewModel = new BuildTemplateListViewModel(buildTemplates);

                var wnd = new SelectBuildProcessTemplateWindow(viewModel);
                bool? res = wnd.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    using (new WaitCursor())
                    {
                        this.repository.AssignBuildProcessTemplate(items.Select(bd => bd.Uri), wnd.SelectedBuildTemplate.ServerPath);
                        this.OnRefresh(new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        private void OnChangeDefaultDropLocation()
        {
            try
            {
                var items = this.view.SelectedItems;
                var viewModel = new DropLocationViewModel();
                var lstDropLocations = from b in items orderby b.DefaultDropLocation.Length select b.DefaultDropLocation;

                viewModel.SearchTxt = GetCommonString(lstDropLocations.ToList());

                viewModel.ReplaceTxt = viewModel.SearchTxt;
                viewModel.SetDropLocation = string.Empty;
                viewModel.AddMacro("$(TeamProject)", "TO BE SET");
                viewModel.AddMacro("$(BuildDefinition)", "TO BE SET");
                viewModel.AddMacro("$(BuildServer)", "TO BE SET");
                viewModel.AddMacro("$(BuildType)", "TO BE SET");

                var wnd = new DropLocationWindow(viewModel);
                bool? res = wnd.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    using (new WaitCursor())
                    {
                        if (viewModel.ModeReplace)
                        {
                            this.repository.ChangeDefaultDropLocation(items.Select(bd => bd.Uri), viewModel.ReplaceTxt, viewModel.SearchTxt, viewModel.UpdateExistingBuilds);
                            this.OnRefresh(new EventArgs());
                        }
                        else
                        {
                            this.repository.SetDefaultDropLocation(items.Select(bd => bd.Uri), viewModel.SetDropLocation, viewModel.Macros, viewModel.UpdateExistingBuilds);
                            this.OnRefresh(new EventArgs());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }

        private void OnChangeBuildController()
        {
            try
            {
                var items = this.view.SelectedItems;
                var controllers = this.repository.Controllers;
                var viewModel = new BuildControllerListViewModel(controllers);

                var wnd = new BuildControllerWindow(viewModel);
                bool? res = wnd.ShowDialog();
                if (res.HasValue && res.Value)
                {
                    using (new WaitCursor())
                    {
                        this.repository.ChangeBuildController(items.Select(bd => bd.Uri), wnd.SelectedBuildController.Name);
                        this.OnRefresh(new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                this.view.DisplayError(ex);
            }
        }
    }
}
