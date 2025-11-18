using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseWorkout.Models;

namespace CourseWorkout.Services;

/// <summary>
/// Service1 实现：题目管理、答题记录、错题本功能
/// </summary>
public class Service1 : IService1
{
    private readonly IService2 _service2;
    private List<Question> _allQuestions = new();
    private List<AnswerRecord> _answerRecords = new();
    private int _currentPage = 1;
    private const int PageSize = 10;

    public Service1(IService2 service2)
    {
        _service2 = service2;
    }

    /// <summary>
    /// 初始化：加载题目并缓存，格式错误时抛 ArgumentException 并说明错误题目标号
    /// </summary>
    public async Task InitializeAsync(string txtFilePath)
    {
        var questions = await _service2.LoadQuestionsAsync(txtFilePath);

        // 去重（按ID）
        var uniqueQuestions = questions
            .GroupBy(q => q.Id)
            .Select(g => g.First())
            .ToList();

        // 校验格式合法性
        foreach (var question in uniqueQuestions)
        {
            // 检查是否有4个选项
            if (question.Options == null || question.Options.Count != 4)
            {
                throw new ArgumentException($"题目格式错误：第{question.Id}题缺少选项");
            }

            // 检查选项是否包含 a, b, c, d
            var requiredKeys = new[] { "a", "b", "c", "d" };
            foreach (var key in requiredKeys)
            {
                if (!question.Options.ContainsKey(key))
                {
                    throw new ArgumentException($"题目格式错误：第{question.Id}题缺少选项{key}");
                }
            }

            // 检查正确答案是否为 a-d 之一
            if (string.IsNullOrEmpty(question.CorrectAnswer) || 
                !new[] { "a", "b", "c", "d" }.Contains(question.CorrectAnswer.ToLower()))
            {
                throw new ArgumentException($"题目格式错误：第{question.Id}题答案格式不正确");
            }

            question.CorrectAnswer = question.CorrectAnswer.ToLower();
        }

        _allQuestions = uniqueQuestions;
        _answerRecords = new List<AnswerRecord>();
    }

    /// <summary>
    /// 分页获取题目，currentPage 小于1时默认返回第1页，大于总页数时返回最后一页
    /// </summary>
    public (List<Question> PageQuestions, int TotalPages) GetPageQuestions(int currentPage)
    {
        if (_allQuestions.Count == 0)
        {
            return (new List<Question>(), 0);
        }

        var totalPages = (int)Math.Ceiling(_allQuestions.Count / (double)PageSize);

        // 边界处理
        if (currentPage < 1)
        {
            currentPage = 1;
        }
        else if (currentPage > totalPages)
        {
            currentPage = totalPages;
        }

        _currentPage = currentPage;

        var skip = (currentPage - 1) * PageSize;
        var pageQuestions = _allQuestions.Skip(skip).Take(PageSize).ToList();

        // 恢复用户已选择的答案
        foreach (var question in pageQuestions)
        {
            var record = _answerRecords.FirstOrDefault(r => r.QuestionId == question.Id);
            if (record != null)
            {
                // 这里不直接设置，因为答案记录在 ViewModel 中管理
            }
        }

        return (pageQuestions, totalPages);
    }

    /// <summary>
    /// 保存用户答案，userAnswer 非 a-d 时抛 ArgumentException
    /// </summary>
    public Task SaveAnswerAsync(int questionId, string userAnswer)
    {
        if (string.IsNullOrEmpty(userAnswer) || !new[] { "a", "b", "c", "d" }.Contains(userAnswer.ToLower()))
        {
            throw new ArgumentException("用户答案必须是 a、b、c 或 d 之一");
        }

        userAnswer = userAnswer.ToLower();
        var question = _allQuestions.FirstOrDefault(q => q.Id == questionId);
        if (question == null)
        {
            throw new ArgumentException($"题目ID {questionId} 不存在");
        }

        var isCorrect = question.CorrectAnswer.Equals(userAnswer, StringComparison.OrdinalIgnoreCase);

        // 更新或创建答题记录
        var existingRecord = _answerRecords.FirstOrDefault(r => r.QuestionId == questionId);
        if (existingRecord != null)
        {
            existingRecord.UserAnswer = userAnswer;
            existingRecord.SubmitTime = DateTime.Now;
            existingRecord.IsCorrect = isCorrect;
        }
        else
        {
            _answerRecords.Add(new AnswerRecord
            {
                QuestionId = questionId,
                UserAnswer = userAnswer,
                SubmitTime = DateTime.Now,
                IsCorrect = isCorrect
            });
        }

        // 更新错题标记
        question.IsMarkedAsWrong = !isCorrect;

        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取答题统计，CurrentPageAccuracy 范围 0-100（百分比）
    /// </summary>
    public (double CurrentPageAccuracy, int AnsweredCount, int TotalQuestionCount) GetAnswerStats()
    {
        var totalQuestionCount = _allQuestions.Count;
        var answeredCount = _answerRecords.Count;

        // 计算当前页正确率（不改变当前页状态）
        var totalPages = (int)Math.Ceiling(_allQuestions.Count / (double)PageSize);
        var currentPage = _currentPage;
        if (currentPage < 1) currentPage = 1;
        else if (currentPage > totalPages) currentPage = totalPages;

        var skip = (currentPage - 1) * PageSize;
        var pageQuestions = _allQuestions.Skip(skip).Take(PageSize).ToList();
        
        var pageAnsweredCount = 0;
        var pageCorrectCount = 0;

        foreach (var question in pageQuestions)
        {
            var record = _answerRecords.FirstOrDefault(r => r.QuestionId == question.Id);
            if (record != null)
            {
                pageAnsweredCount++;
                if (record.IsCorrect)
                {
                    pageCorrectCount++;
                }
            }
        }

        var currentPageAccuracy = pageAnsweredCount > 0 
            ? Math.Round((double)pageCorrectCount / pageAnsweredCount * 100, 2) 
            : 0.0;

        return (currentPageAccuracy, answeredCount, totalQuestionCount);
    }

    /// <summary>
    /// 获取错题列表
    /// </summary>
    public List<Question> GetWrongQuestions()
    {
        return _allQuestions.Where(q => q.IsMarkedAsWrong).ToList();
    }

    /// <summary>
    /// 清除所有错题标记
    /// </summary>
    public void ClearWrongMarks()
    {
        foreach (var question in _allQuestions)
        {
            question.IsMarkedAsWrong = false;
        }
    }
}

