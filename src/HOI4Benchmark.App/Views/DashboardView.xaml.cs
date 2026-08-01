using System.Windows;
using System.Windows.Controls;
using HOI4Benchmark.App.ViewModels;

namespace HOI4Benchmark.App.Views;

public partial class DashboardView : UserControl
{
    private readonly DashboardViewModel _viewModel;

    public DashboardView(DashboardViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;

        InitializeComponent();

        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        await _viewModel.LoadAsync();
    }
}