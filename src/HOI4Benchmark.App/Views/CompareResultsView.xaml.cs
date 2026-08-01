using System.Windows;
using System.Windows.Controls;
using HOI4Benchmark.App.ViewModels;

namespace HOI4Benchmark.App.Views;

public partial class CompareResultsView : UserControl
{
    private readonly CompareResultsViewModel _viewModel;
    private bool _isLoaded;

    public CompareResultsView(
        CompareResultsViewModel viewModel)
    {
        _viewModel = viewModel
            ?? throw new ArgumentNullException(
                nameof(viewModel));

        InitializeComponent();

        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(
        object sender,
        RoutedEventArgs e)
    {
        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;

        await _viewModel.LoadAsync();
    }
}