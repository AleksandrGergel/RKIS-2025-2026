using System.Windows;
using TodoApp.Desktop.ViewModels;

namespace TodoApp.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
