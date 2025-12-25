using Avalonia.Controls;
using Avalonia.Interactivity;
using CourseWorkout.Models;
using CourseWorkout.ViewModels;
using System;
// 新增：引入DI命名空间
using Microsoft.Extensions.DependencyInjection;

namespace CourseWorkout.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // 从全局ServiceProvider获取ViewModel实例（需先配置App.axaml.cs）
        if (App.ServiceProvider != null)
        {
            DataContext = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        }
        else
        {
            // 兜底：若DI未配置，手动提示错误（可选）
            throw new InvalidOperationException("请先配置App的依赖注入容器！");
        }
    }
    
    // 构造函数（依赖注入模式，优先用带参数的构造函数）
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        // 核心：将ViewModel赋值给窗口的DataContext
        DataContext = viewModel;
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

    private void OnDifficultyChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo && combo.Tag is int id && DataContext is MainWindowViewModel vm)
        {
            if (combo.SelectedItem is ComboBoxItem item)
            {
                var tag = item.Tag;
                Difficulty? difficulty = tag switch
                {
                    Difficulty d => d,
                    null => null,
                    _ => null
                };
                if (difficulty != null)
                {
                    vm.SetDifficultyCommand.Execute((id, difficulty.Value));
                }
            }
        }
    }

    private void OnDifficultyFilterChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo && DataContext is MainWindowViewModel vm)
        {
            if (combo.SelectedItem is ComboBoxItem item)
            {
                var tag = item.Tag;
                Difficulty? difficulty = tag switch
                {
                    Difficulty d => d,
                    null => null,
                    _ => null
                };
                vm.FilterByDifficultyCommand.Execute(difficulty);
            }
        }
    }
}
