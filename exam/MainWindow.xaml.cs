// Файл: MainWindow.xaml.cs
using System.Windows;

namespace ParabolaAnimation
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            Loaded += async (s, e) => await _viewModel.InitializeAsync();
        }
    }
}