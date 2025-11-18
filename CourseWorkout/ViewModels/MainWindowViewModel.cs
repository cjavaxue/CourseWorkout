using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CourseWorkout.Models;
using CourseWorkout.Services;

namespace CourseWorkout.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IService1 _service1;
    private readonly IMessageService _messageService;

    [ObservableProperty]
    private ObservableCollection<Question> _currentPageQuestions = new();

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 0;

    [ObservableProperty]
    private double _currentPageAccuracy = 0.0;

    [ObservableProperty]
    private int _globalAnsweredCount = 0;

    [ObservableProperty]
    private int _totalQuestionCount = 0;

    [ObservableProperty]
    private bool _canGoPrevious = false;

    [ObservableProperty]
    private bool _canGoNext = false;

    /// <summary>
    /// 用户选择的答案字典，Key=题号，Value=用户选的 a-d
    /// </summary>
    public Dictionary<int, string> UserSelectedAnswers { get; } = new();

    public MainWindowViewModel(IService1 service1, IMessageService messageService)
    {
        _service1 = service1;
        _messageService = messageService;
        InitializeAsync();
    }

    /// <summary>
    /// 初始化：加载题目
    /// </summary>
    private async void InitializeAsync()
    {
        try
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Questions.txt");
            await _service1.InitializeAsync(filePath);
            LoadPage(1);
        }
        catch (Exception ex)
        {
            await _messageService.ShowMessageAsync("错误", ex.Message);
        }
    }

    /// <summary>
    /// 加载指定页
    /// </summary>
    private async void LoadPage(int page)
    {
        try
        {
            var (pageQuestions, totalPages) = _service1.GetPageQuestions(page);
            CurrentPage = page;
            TotalPages = totalPages;

            CurrentPageQuestions.Clear();
            foreach (var question in pageQuestions)
            {
                CurrentPageQuestions.Add(question);
                // 恢复用户已选择的答案
                if (UserSelectedAnswers.ContainsKey(question.Id))
                {
                    // 答案已保存在字典中，UI 会通过绑定获取
                }
            }

            UpdateButtonStates();
            UpdateStats();
        }
        catch (Exception ex)
        {
            await _messageService.ShowMessageAsync("错误", ex.Message);
        }
    }

    /// <summary>
    /// 更新按钮状态
    /// </summary>
    private void UpdateButtonStates()
    {
        CanGoPrevious = CurrentPage > 1;
        CanGoNext = CurrentPage < TotalPages;
        GoPreviousCommand.NotifyCanExecuteChanged();
        GoNextCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 更新统计信息
    /// </summary>
    private void UpdateStats()
    {
        var (accuracy, answeredCount, totalCount) = _service1.GetAnswerStats();
        CurrentPageAccuracy = accuracy;
        GlobalAnsweredCount = answeredCount;
        TotalQuestionCount = totalCount;
    }

    /// <summary>
    /// 上一页命令
    /// </summary>
    [RelayCommand]
    private void GoPrevious()
    {
        if (CurrentPage > 1)
        {
            LoadPage(CurrentPage - 1);
        }
    }

    /// <summary>
    /// 下一页命令
    /// </summary>
    [RelayCommand]
    private void GoNext()
    {
        if (CurrentPage < TotalPages)
        {
            LoadPage(CurrentPage + 1);
        }
    }

    /// <summary>
    /// 提交当前页答案命令
    /// </summary>
    [RelayCommand]
    private async Task SubmitPageAnswers()
    {
        try
        {
            foreach (var question in CurrentPageQuestions)
            {
                if (UserSelectedAnswers.ContainsKey(question.Id))
                {
                    var userAnswer = UserSelectedAnswers[question.Id];
                    await _service1.SaveAnswerAsync(question.Id, userAnswer);
                }
            }

            UpdateStats();
            await _messageService.ShowMessageAsync("提示", "答案已提交！");
        }
        catch (ArgumentException ex)
        {
            await _messageService.ShowMessageAsync("错误", ex.Message);
        }
        catch (Exception ex)
        {
            await _messageService.ShowMessageAsync("错误", $"提交答案时发生错误：{ex.Message}");
        }
    }

    /// <summary>
    /// 查看错题本命令
    /// </summary>
    [RelayCommand]
    private async Task ViewWrongQuestions()
    {
        try
        {
            var wrongQuestions = _service1.GetWrongQuestions();
            if (wrongQuestions.Count == 0)
            {
                await _messageService.ShowMessageAsync("提示", "暂无错题！");
                return;
            }

            var message = $"共有 {wrongQuestions.Count} 道错题：\n";
            foreach (var question in wrongQuestions.Take(10))
            {
                message += $"第 {question.Id} 题\n";
            }
            if (wrongQuestions.Count > 10)
            {
                message += $"... 还有 {wrongQuestions.Count - 10} 道错题";
            }

            await _messageService.ShowMessageAsync("错题本", message);
        }
        catch (Exception ex)
        {
            await _messageService.ShowMessageAsync("错误", $"查看错题本时发生错误：{ex.Message}");
        }
    }

    /// <summary>
    /// 清除错题标记命令
    /// </summary>
    [RelayCommand]
    private async Task ClearWrongMarks()
    {
        try
        {
            _service1.ClearWrongMarks();
            await _messageService.ShowMessageAsync("提示", "已清除所有错题标记！");
            UpdateStats();
        }
        catch (Exception ex)
        {
            await _messageService.ShowMessageAsync("错误", $"清除错题标记时发生错误：{ex.Message}");
        }
    }

    /// <summary>
    /// 当用户选择答案时调用
    /// </summary>
    public void OnAnswerSelected(int questionId, string answer)
    {
        UserSelectedAnswers[questionId] = answer;
    }
}
