using System;

namespace CourseWorkout.Models;

/// <summary>
/// 答题记录实体类
/// </summary>
public class AnswerRecord
{
    /// <summary>
    /// 题目ID
    /// </summary>
    public int QuestionId { get; set; }

    /// <summary>
    /// 用户答案（a-d之一）
    /// </summary>
    public string UserAnswer { get; set; } = string.Empty;

    /// <summary>
    /// 提交时间
    /// </summary>
    public DateTime SubmitTime { get; set; }

    /// <summary>
    /// 是否正确
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// 本题耗时（秒）
    /// </summary>
    public int SpentSeconds { get; set; }

    /// <summary>
    /// 答题模式
    /// </summary>
    public Mode Mode { get; set; }
}

