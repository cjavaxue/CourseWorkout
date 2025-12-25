using System.Collections.Generic;
using System.Threading.Tasks;
using CourseWorkout.Models;

namespace CourseWorkout.Services;

/// <summary>
/// IService1：题目管理、答题记录、错题本功能（可测试）
/// </summary>
public interface IService1
{
    /// <summary>
    /// 初始化：加载题目并缓存，格式错误时抛 ArgumentException 并说明错误题目标号
    /// </summary>
    /// <param name="txtFilePath">txt文件路径</param>
    /// <exception cref="ArgumentException">题目格式错误时抛出</exception>
    Task InitializeAsync(string txtFilePath);

    /// <summary>
    /// 分页获取题目，currentPage 小于1时默认返回第1页，大于总页数时返回最后一页
    /// </summary>
    /// <param name="currentPage">当前页码（从1开始）</param>
    /// <returns>当前页题目列表和总页数</returns>
    (List<Question> PageQuestions, int TotalPages) GetPageQuestions(int currentPage);

    /// <summary>
    /// 保存用户答案，userAnswer 非 a-d 时抛 ArgumentException
    /// </summary>
    /// <param name="questionId">题目ID</param>
    /// <param name="userAnswer">用户答案（a-d）</param>
    /// <param name="spentSeconds">本题耗时（秒）</param>
    /// <exception cref="ArgumentException">userAnswer 非 a-d 时抛出</exception>
    Task SaveAnswerAsync(int questionId, string userAnswer, int spentSeconds);

    /// <summary>
    /// 获取答题统计，CurrentPageAccuracy 范围 0-100（百分比）
    /// </summary>
    /// <returns>当前页正确率（百分比）、已答题数、总题数</returns>
    (double CurrentPageAccuracy, int AnsweredCount, int TotalQuestionCount) GetAnswerStats();

    /// <summary>
    /// 获取错题列表
    /// </summary>
    /// <returns>错题列表</returns>
    List<Question> GetWrongQuestions();

    /// <summary>
    /// 清除所有错题标记
    /// </summary>
    void ClearWrongMarks();

    /// <summary>
    /// 切换收藏状态
    /// </summary>
    /// <param name="questionId">题目ID</param>
    void ToggleFavorite(int questionId);

    /// <summary>
    /// 获取收藏题目列表
    /// </summary>
    /// <returns>收藏题目</returns>
    List<Question> GetFavoriteQuestions();

    /// <summary>
    /// 设置题目难度
    /// </summary>
    /// <param name="questionId">题目ID</param>
    /// <param name="difficulty">难度</param>
    void SetDifficulty(int questionId, Difficulty difficulty);

    /// <summary>
    /// 按难度筛选题目
    /// </summary>
    /// <param name="difficulty">目标难度，null 表示全部</param>
    /// <returns>符合筛选的题目</returns>
    List<Question> FilterByDifficulty(Difficulty? difficulty);

    /// <summary>
    /// 按难度统计正确率（百分比）
    /// </summary>
    /// <returns>三种难度的正确率</returns>
    (double Easy, double Medium, double Hard) GetDifficultyAccuracy();

    /// <summary>
    /// 设置当前模式
    /// </summary>
    /// <param name="mode">练习/考试</param>
    void SetCurrentMode(Mode mode);

    /// <summary>
    /// 获取考试报告
    /// </summary>
    /// <returns>考试报告</returns>
    ExamReport GetExamReport();

    /// <summary>
    /// 获取耗时统计
    /// </summary>
    /// <returns>当前页平均耗时、全局平均耗时（秒）</returns>
    (double CurrentPageAvgSeconds, double GlobalAvgSeconds) GetTimeStats();
}

