using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWorkout.Models;
using CourseWorkout.ViewModels;

namespace CourseWorkout.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnAnswerChecked(object? sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton && 
            radioButton.DataContext is Question question &&
            radioButton.Tag is string answer &&
            DataContext is MainWindowViewModel viewModel)
        {
            viewModel.OnAnswerSelected(question.Id, answer);
        }
    }

    private void OnAnswerUnchecked(object? sender, RoutedEventArgs e)
    {
        // 不需要处理取消选择
    }
}
