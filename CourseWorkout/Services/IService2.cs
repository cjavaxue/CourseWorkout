using System.Collections.Generic;
using System.Threading.Tasks;
using CourseWorkout.Models;

namespace CourseWorkout.Services;

/// <summary>
/// IService2：负责读取本地选择题 txt 文件（不可测试）
/// </summary>
public interface IService2
{
    /// <summary>
    /// 从指定路径加载题目列表
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>题目列表</returns>
    Task<List<Question>> LoadQuestionsAsync(string filePath);
}

