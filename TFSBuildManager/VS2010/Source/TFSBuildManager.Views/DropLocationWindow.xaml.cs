//-----------------------------------------------------------------------
// <copyright file="DropLocationWindow.xaml.cs">(c) http://TfsBuildExtensions.codeplex.com/. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace TfsBuildManager.Views
{
    using System.Collections.Generic;
    using System.Windows;

    using TfsBuildManager.Views.ViewModels;

    /// <summary>
    /// Interaction logic for DropLocationWindow.xaml
    /// </summary>
    public partial class DropLocationWindow : Window
    {
        private readonly DropLocationViewModel model;

        public DropLocationWindow(DropLocationViewModel model)
        {
            this.model = model;
            this.InitializeComponent();
            this.DataContext = this.model;
        }

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

        private void LstMacrosMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string s = ((KeyValuePair<string, string>)lstMacros.SelectedItem).Key;
            IDataObject old = Clipboard.GetDataObject();
            Clipboard.SetText(s);

            txtSetDropLocation.BeginChange();
            txtSetDropLocation.Paste();
            txtSetDropLocation.EndChange();
            Clipboard.SetDataObject(old);

            txtSetDropLocation.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
        }
    }
}
