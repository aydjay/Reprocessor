using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json;

namespace Reprocessor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string settingsFile = @"C:\temp\settings.json";
        internal MainWindowViewModel ViewModel;

        public MainWindow()
        {
            if (File.Exists(settingsFile))
            {
                var data = File.ReadAllText(settingsFile);
                ViewModel = JsonConvert.DeserializeObject<MainWindowViewModel>(data);
            }
            else
            {
                ViewModel = new MainWindowViewModel();
            }

            InitializeComponent();

            DataContext = ViewModel;


            if (ViewModel.SearchCommand.CanExecute(null))
            {
                ViewModel.SearchCommand.Execute(null);
            }
        }

        //private bool UserFilter(object item)
        //{
        //    if (string.IsNullOrEmpty(txtInput.Text))
        //        return true;

        //    var dataRow = item as DataRow;
        //    return dataRow != null && (dataRow["typeName"].ToString().IndexOf(ViewModel.UserInput, StringComparison.OrdinalIgnoreCase) >= 0);
        //}

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            //Serialise settings
            var settings = JsonConvert.SerializeObject(ViewModel, Formatting.Indented);

            File.WriteAllText(settingsFile, settings);
        }

        //private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        //{
        //    ViewModel.GetItems(txtInput.Text);
        //}

        //private void Evale_OnClick(object sender, RoutedEventArgs e)
        //{
        //    foreach (DataRow eveItem in ViewModel.EveItems.Rows)
        //    {
        //        ViewModel.GetReprocessData(eveItem["typeID"].ToString());
        //    }
        //}
    }
}