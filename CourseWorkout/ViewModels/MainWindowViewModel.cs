using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
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

    [ObservableProperty]
    private bool _isShowingFavorites = false;

    [ObservableProperty]
    private Difficulty? _currentDifficultyFilter = null;

    [ObservableProperty]
    private Mode _currentMode = Mode.Practice;

    [ObservableProperty]
    private int _examRemainingSeconds = 0;

    [ObservableProperty]
    private double _currentPageAvgSeconds = 0;

    [ObservableProperty]
    private double _globalAvgSeconds = 0;

    /// <summary>
    /// 单题耗时（最近一次提交）
    /// </summary>
    //public Dictionary<int, int> QuestionSpentSeconds { get; private set; } = new();
    // 改为公共setter并支持属性变更通知
    private Dictionary<int, int> _questionSpentSeconds = new();
    public Dictionary<int, int> QuestionSpentSeconds 
    { 
        get => _questionSpentSeconds; 
        set => SetProperty(ref _questionSpentSeconds, value); 
    }

    /// <summary>
    /// 用户选择的答案字典，Key=题号，Value=用户选的 a-d
    /// </summary>
    public Dictionary<int, string> UserSelectedAnswers { get; } = new();

    private readonly Dictionary<int, DateTime> _questionStartTimes = new();
    private CancellationTokenSource? _examCts;

    public MainWindowViewModel(IService1 service1, IMessageService messageService)
    {
        _service1 = service1;
        _messageService = messageService;
        _service1.SetCurrentMode(CurrentMode);
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
            List<Question> pageQuestions;
            int totalPages;

            if (IsShowingFavorites)
            {
                pageQuestions = _service1.GetFavoriteQuestions();
                totalPages = 1;
                CurrentPage = 1;
            }
            else if (CurrentDifficultyFilter != null)
            {
                pageQuestions = _service1.FilterByDifficulty(CurrentDifficultyFilter);
                totalPages = 1;
                CurrentPage = 1;
            }
            else
            {
                var result = _service1.GetPageQuestions(page);
                pageQuestions = result.PageQuestions;
                totalPages = result.TotalPages;
                CurrentPage = page;
            }

            TotalPages = totalPages;

            CurrentPageQuestions.Clear();
            _questionStartTimes.Clear();
            foreach (var question in pageQuestions)
            {
                CurrentPageQuestions.Add(question);
                _questionStartTimes[question.Id] = DateTime.Now;
            }

            UpdateButtonStates();
            UpdateStats();
            UpdateTimeStats();
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
    /// 更新耗时统计
    /// </summary>
    private void UpdateTimeStats()
    {
        var (pageAvg, globalAvg) = _service1.GetTimeStats();
        CurrentPageAvgSeconds = pageAvg;
        GlobalAvgSeconds = globalAvg;
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
                    var spent = 0;
                    if (_questionStartTimes.TryGetValue(question.Id, out var start))
                    {
                        spent = (int)Math.Max(0, (DateTime.Now - start).TotalSeconds);
                    }
                    QuestionSpentSeconds[question.Id] = spent;
                    await _service1.SaveAnswerAsync(question.Id, userAnswer, spent);
                }
            }

            UpdateStats();
            UpdateTimeStats();
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

    /// <summary>
    /// 切换收藏
    /// </summary>
    [RelayCommand]
    private void ToggleFavorite(int questionId)
    {
        try
        {
            _service1.ToggleFavorite(questionId);
            if (IsShowingFavorites)
            {
                LoadPage(1);
            }
        }
        catch (Exception ex)
        {
            _ = _messageService.ShowMessageAsync("错误", ex.Message);
        }
    }

    /// <summary>
    /// 查看/退出收藏列表
    /// </summary>
    [RelayCommand]
    private void ViewFavorites()
    {
        IsShowingFavorites = !IsShowingFavorites;
        LoadPage(1);
    }

    /// <summary>
    /// 设置难度
    /// </summary>
    [RelayCommand]
    private void SetDifficulty((int QuestionId, Difficulty Difficulty) param)
    {
        try
        {
            _service1.SetDifficulty(param.QuestionId, param.Difficulty);
        }
        catch (Exception ex)
        {
            _ = _messageService.ShowMessageAsync("错误", ex.Message);
        }
    }

    /// <summary>
    /// 按难度筛选
    /// </summary>
    [RelayCommand]
    private void FilterByDifficulty(Difficulty? difficulty)
    {
        CurrentDifficultyFilter = difficulty;
        LoadPage(1);
    }

    /// <summary>
    /// 模式切换
    /// </summary>
    [RelayCommand]
    private async Task SwitchMode()
    {
        CurrentMode = CurrentMode == Mode.Practice ? Mode.Exam : Mode.Practice;
        _service1.SetCurrentMode(CurrentMode);

        _examCts?.Cancel();
        if (CurrentMode == Mode.Exam)
        {
            ExamRemainingSeconds = 1800;
            _examCts = new CancellationTokenSource();
            _ = RunExamCountdownAsync(_examCts.Token);
            await _messageService.ShowMessageAsync("提示", "已切换到考试模式，倒计时 30 分钟。");
        }
        else
        {
            ExamRemainingSeconds = 0;
            await _messageService.ShowMessageAsync("提示", "已切换到练习模式。");
        }
    }

    private async Task RunExamCountdownAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && ExamRemainingSeconds > 0)
            {
                await Task.Delay(1000, token);
                ExamRemainingSeconds--;
            }

            if (!token.IsCancellationRequested && ExamRemainingSeconds <= 0)
            {
                await SubmitPageAnswers();
                var report = _service1.GetExamReport();
                await _messageService.ShowMessageAsync("考试结束", $"得分：{report.Score}，用时：{report.SpentSeconds}秒，最高分：{report.BestScore}");
            }
        }
        catch (TaskCanceledException)
        {
            // ignore
        }
    }
}
