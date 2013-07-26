using System;
using System.Linq;
using System.Windows;

namespace Config
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainViewModel();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)DataContext).Save();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}