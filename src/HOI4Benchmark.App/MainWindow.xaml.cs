using System.Windows;
using HOI4Benchmark.App.ViewModels;

namespace HOI4Benchmark.App;

public partial class MainWindow : Window
{
    public MainWindow(
        MainWindowViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}