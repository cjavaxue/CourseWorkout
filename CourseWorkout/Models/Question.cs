using System.Collections.Generic;

namespace CourseWorkout.Models;

/// <summary>
/// 题目实体类
/// </summary>
public class Question
{
    /// <summary>
    /// 题目ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 题目标题（题干）
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 选项字典，Key为"a"/"b"/"c"/"d"，Value为选项内容
    /// </summary>
    public Dictionary<string, string> Options { get; set; } = new();

    /// <summary>
    /// 正确答案（a-d之一）
    /// </summary>
    public string CorrectAnswer { get; set; } = string.Empty;

    /// <summary>
    /// 是否标记为错题
    /// </summary>
    public bool IsMarkedAsWrong { get; set; }

    /// <summary>
    /// 是否收藏/重点标记
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// 难度（可选）
    /// </summary>
    public Difficulty? Difficulty { get; set; }
    
    public int SpentSeconds { get; set; } // 单题耗时
}

