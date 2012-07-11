//-----------------------------------------------------------------------
// <copyright file="TriggerWindow.xaml.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildManager.Views
{
    using System.Windows;
    using TfsBuildManager.Views.ViewModels;

    /// <summary>
    /// Interaction logic for DropLocationWindow.xaml
    /// </summary>
    public partial class TriggerWindow
    {
        public TriggerWindow(TriggerViewModel trigger)
        {
            this.Trigger = trigger;
            this.InitializeComponent();
            this.DataContext = this.Trigger;
        }

        public TriggerViewModel Trigger { get; private set; }

        private void OnOK(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void rdoTriggerRolling_Checked(object sender, RoutedEventArgs e)
        {
            this.checkboxRolling.IsEnabled = true;
            this.labelMinutes.IsEnabled = true;
            this.textboxMinutes.IsEnabled = true;
        }

        private void rdoTriggerRolling_Unchecked(object sender, RoutedEventArgs e)
        {
            this.checkboxRolling.IsEnabled = false;
            this.labelMinutes.IsEnabled = false;
            this.textboxMinutes.IsEnabled = false;
        }

        private void rdoTriggerGated_Checked(object sender, RoutedEventArgs e)
        {
            this.checkboxGated.IsEnabled = true;
            this.labelSubmissions.IsEnabled = true;
            this.textboxSubmissions.IsEnabled = true;
        }

        private void rdoTriggerGated_Unchecked(object sender, RoutedEventArgs e)
        {
            this.checkboxGated.IsEnabled = false;
            this.labelSubmissions.IsEnabled = false;
            this.textboxSubmissions.IsEnabled = false;
        }
    }
}
