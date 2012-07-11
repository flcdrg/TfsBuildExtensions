//-----------------------------------------------------------------------
// <copyright file="BuildControllerViewModel.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildManager.Views.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Microsoft.TeamFoundation.Build.Client;

    public class BuildControllerViewModel : ViewModelBase
    {
        public BuildControllerViewModel(IBuildController controller)
        {
            this.Name = controller.Name;
            this.Tags = string.Empty;
            foreach (var tag in controller.Tags)
            {
                this.Tags += tag + "\n";
            }

            this.Tags = this.Tags.TrimEnd('\n');
            if (string.IsNullOrEmpty(this.Tags))
            {
                this.Tags = "<No Tags>";
            }
        }

        public string Name { get; set; }

        public string Tags { get; set; }
    }

    public class DropLocationViewModel : ViewModelBase
    {
        public DropLocationViewModel()
        {
            this.ModeReplace = true;
            this.Macros = new Dictionary<string, string>();
        }

        public string SearchTxt { get; set; }

        public string ReplaceTxt { get; set; }

        public bool UpdateExistingBuilds { get; set; }

        public bool ModeReplace { get; set; }

        public string SetDropLocation { get; set; }

        public Dictionary<string, string> Macros { get; private set; }

        public void AddMacro(string macro, string value)
        {
            this.Macros.Add(macro, value);
        }
    }

    public class BuildControllerListViewModel : ViewModelBase
    {
        public BuildControllerListViewModel(IEnumerable<IBuildController> controllers)
        {
            this.BuildControllers = new ObservableCollection<BuildControllerViewModel>();
            foreach (var c in controllers)
            {
                this.BuildControllers.Add(new BuildControllerViewModel(c));
            }
        }

        public ObservableCollection<BuildControllerViewModel> BuildControllers { get; private set; }
    }

    public class TeamProjectViewModel : ViewModelBase
    {
        public TeamProjectViewModel(string project)
        {
            this.Name = project;
        }

        public string Name { get; set; }
    }

    public class TeamProjectListViewModel : ViewModelBase
    {
        public TeamProjectListViewModel(IEnumerable<string> projects)
        {
            this.TeamProjects = new ObservableCollection<TeamProjectViewModel>();
            foreach (var p in projects)
            {
                this.TeamProjects.Add(new TeamProjectViewModel(p));
            }
        }

        public ObservableCollection<TeamProjectViewModel> TeamProjects { get; private set; }
    }

    public class BuildQualityViewModel : ViewModelBase
    {
        public BuildQualityViewModel(string project)
        {
            this.Name = project;
        }

        public string Name { get; set; }
    }

    public class BuildQualityListViewModel : ViewModelBase
    {
        public BuildQualityListViewModel(IEnumerable<string> projects)
        {
            this.BuildQualities = new ObservableCollection<BuildQualityViewModel>();
            foreach (var p in projects)
            {
                this.BuildQualities.Add(new BuildQualityViewModel(p));
            }
        }

        public ObservableCollection<BuildQualityViewModel> BuildQualities { get; private set; }
    }
}
