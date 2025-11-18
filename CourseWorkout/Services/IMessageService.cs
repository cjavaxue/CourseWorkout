using System.Threading.Tasks;

namespace CourseWorkout.Services;

/// <summary>
/// 消息服务接口
/// </summary>
public interface IMessageService
{
    /// <summary>
    /// 显示消息框
    /// </summary>
    Task ShowMessageAsync(string title, string message);
}

