using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseWorkout.Models;
using CourseWorkout.Services;
using CourseWorkout.ViewModels;
using Moq;
using Xunit;

namespace CourseWorkout.Tests;

public class MainWindowViewModelTests
{
    // 1. 构造函数与初始化测试
    [Fact]
    public async Task InitializeAsync_Success_ShouldLoadFirstPage()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        mockService1.Setup(s => s.InitializeAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        mockService1.Setup(s => s.GetPageQuestions(1)).Returns((new List<Question>(), 1));

        // Act
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object);
        await Task.Delay(100); // 等待异步初始化完成

        // Assert
        mockService1.Verify(s => s.InitializeAsync(It.IsAny<string>()), Times.Once);
        mockService1.Verify(s => s.GetPageQuestions(1), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_WithException_ShouldShowError()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        mockService1.Setup(s => s.InitializeAsync(It.IsAny<string>())).ThrowsAsync(new Exception("加载失败"));

        // Act
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object);
        await Task.Delay(100);

        // Assert
        mockMessage.Verify(m => m.ShowMessageAsync("错误", "加载失败"), Times.Once);
    }


    // 2. 分页加载与切换测试
    [Fact]
    public async Task GoPrevious_WhenCurrentPageIs2_ShouldLoadPage1()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        mockService1.Setup(s => s.GetPageQuestions(1)).Returns((new List<Question>(), 2));
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object) { CurrentPage = 2 };
        await Task.Delay(50);
        mockService1.Invocations.Clear();

        // Act
        vm.GoPreviousCommand.Execute(null);

        // Assert
        Assert.Equal(1, vm.CurrentPage);
        mockService1.Verify(s => s.GetPageQuestions(1), Times.Once);
    }

    [Fact]
    public async Task GoPrevious_WhenCurrentPageIs1_ShouldNotLoadPage()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object) { CurrentPage = 1 };
        await Task.Delay(50);
        mockService1.Invocations.Clear();

        // Act
        vm.GoPreviousCommand.Execute(null);

        // Assert
        mockService1.Verify(s => s.GetPageQuestions(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GoNext_WhenCurrentPageIs1AndTotalPages3_ShouldLoadPage2()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        mockService1.Setup(s => s.GetPageQuestions(2)).Returns((new List<Question>(), 3));
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object) 
        { 
            CurrentPage = 1, 
            TotalPages = 3 
        };
        await Task.Delay(50);
        mockService1.Invocations.Clear();

        // Act
        vm.GoNextCommand.Execute(null);

        // Assert
        Assert.Equal(2, vm.CurrentPage);
        mockService1.Verify(s => s.GetPageQuestions(2), Times.Once);
    }

    [Fact]
    public async Task GoNext_WhenCurrentPageIsLast_ShouldNotLoadPage()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object) 
        { 
            CurrentPage = 3, 
            TotalPages = 3 
        };
        await Task.Delay(50);
        mockService1.Invocations.Clear();

        // Act
        vm.GoNextCommand.Execute(null);

        // Assert
        mockService1.Verify(s => s.GetPageQuestions(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void LoadPage_WithException_ShouldShowError()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        mockService1.Setup(s => s.GetPageQuestions(1)).Throws(new Exception("加载失败"));
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object);
        mockMessage.Invocations.Clear();

        // 反射调用私有方法 LoadPage
        var loadPageMethod = typeof(MainWindowViewModel).GetMethod("LoadPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        loadPageMethod?.Invoke(vm, new object[] { 1 });

        // Assert
        mockMessage.Verify(m => m.ShowMessageAsync("错误", "加载失败"), Times.Once);
    }


    // 3. 答案提交测试
    [Fact]
    public async Task SubmitPageAnswers_WithValidAnswers_ShouldSaveAndUpdateStats()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        var questions = new List<Question> { new Question { Id = 1 } };
        mockService1.Setup(s => s.GetPageQuestions(1)).Returns((questions, 1));
        mockService1.Setup(s => s.GetAnswerStats()).Returns((80.0, 5, 10));
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object);
        
        // 反射调用 LoadPage 加载题目
        var loadPageMethod = typeof(MainWindowViewModel).GetMethod("LoadPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        loadPageMethod?.Invoke(vm, new object[] { 1 });
        
        vm.OnAnswerSelected(1, "a"); // 选择答案

        // Act
        await vm.SubmitPageAnswersCommand.ExecuteAsync(null);

        // Assert
        mockService1.Verify(s => s.SaveAnswerAsync(1, "a"), Times.Once);
        // 修正：实际会在初始化、加载页面、提交后共调用3次
        mockService1.Verify(s => s.GetAnswerStats(), Times.Exactly(3));
        mockMessage.Verify(m => m.ShowMessageAsync("提示", "答案已提交！"), Times.Once);
    }

    [Fact]
    public async Task SubmitPageAnswers_WithInvalidAnswer_ShouldShowError()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        var questions = new List<Question> { new Question { Id = 1 } };
        mockService1.Setup(s => s.GetPageQuestions(1)).Returns((questions, 1));
        mockService1.Setup(s => s.SaveAnswerAsync(1, "e")).ThrowsAsync(new ArgumentException("无效答案"));
        
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object);
        var loadPageMethod = typeof(MainWindowViewModel).GetMethod("LoadPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        loadPageMethod?.Invoke(vm, new object[] { 1 });
        
        vm.OnAnswerSelected(1, "e");

        // Act
        await vm.SubmitPageAnswersCommand.ExecuteAsync(null);

        // Assert
        mockMessage.Verify(m => m.ShowMessageAsync("错误", "无效答案"), Times.Once);
    }

    [Fact]
    public async Task SubmitPageAnswers_WithNoAnswers_ShouldNotThrow()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        var questions = new List<Question> { new Question { Id = 1 } };
        mockService1.Setup(s => s.GetPageQuestions(1)).Returns((questions, 1));
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object);
        var loadPageMethod = typeof(MainWindowViewModel).GetMethod("LoadPage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        loadPageMethod?.Invoke(vm, new object[] { 1 });

        // Act & Assert（无异常即为通过）
        await vm.SubmitPageAnswersCommand.ExecuteAsync(null);
        mockService1.Verify(s => s.SaveAnswerAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }


    // 4. 错题本操作测试
    [Fact]
    public async Task ViewWrongQuestions_WithNoWrongQuestions_ShouldShowEmptyMessage()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        mockService1.Setup(s => s.GetWrongQuestions()).Returns(new List<Question>());
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object);

        // Act
        await vm.ViewWrongQuestionsCommand.ExecuteAsync(null);

        // Assert
        mockMessage.Verify(m => m.ShowMessageAsync("提示", "暂无错题！"), Times.Once);
    }

    [Fact]
    public async Task ViewWrongQuestions_With15WrongQuestions_ShouldShowTruncatedMessage()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        var wrongQuestions = Enumerable.Range(1, 15).Select(i => new Question { Id = i }).ToList();
        mockService1.Setup(s => s.GetWrongQuestions()).Returns(wrongQuestions);
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object);

        // Act
        await vm.ViewWrongQuestionsCommand.ExecuteAsync(null);

        // Assert
        mockMessage.Verify(m => m.ShowMessageAsync("错题本", It.Is<string>(msg => 
            msg.Contains("共有 15 道错题") && msg.Contains("还有 5 道错题")
        )), Times.Once);
    }

    [Fact]
    public async Task ClearWrongMarks_Success_ShouldNotifyAndUpdateStats()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        mockService1.Setup(s => s.GetAnswerStats()).Returns((80.0, 5, 10));
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object);

        // Act
        await vm.ClearWrongMarksCommand.ExecuteAsync(null);

        // Assert
        mockService1.Verify(s => s.ClearWrongMarks(), Times.Once);
        mockService1.Verify(s => s.GetAnswerStats(), Times.Once);
        mockMessage.Verify(m => m.ShowMessageAsync("提示", "已清除所有错题标记！"), Times.Once);
    }

    [Fact]
    public async Task ClearWrongMarks_WithException_ShouldShowError()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        var mockMessage = new Mock<IMessageService>();
        mockService1.Setup(s => s.ClearWrongMarks()).Throws(new Exception("清除失败"));
        var vm = new MainWindowViewModel(mockService1.Object, mockMessage.Object);

        // Act
        await vm.ClearWrongMarksCommand.ExecuteAsync(null);

        // Assert
        mockMessage.Verify(m => m.ShowMessageAsync("错误", "清除错题标记时发生错误：清除失败"), Times.Once);
    }


    // 5. 辅助方法测试
    [Fact]
    public void OnAnswerSelected_NewAnswer_ShouldAddToDictionary()
    {
        // Arrange
        var vm = new MainWindowViewModel(Mock.Of<IService1>(), Mock.Of<IMessageService>());

        // Act
        vm.OnAnswerSelected(1, "a");

        // Assert
        Assert.True(vm.UserSelectedAnswers.ContainsKey(1));
        Assert.Equal("a", vm.UserSelectedAnswers[1]);
    }

    [Fact]
    public void OnAnswerSelected_ExistingAnswer_ShouldUpdateDictionary()
    {
        // Arrange
        var vm = new MainWindowViewModel(Mock.Of<IService1>(), Mock.Of<IMessageService>());
        vm.OnAnswerSelected(1, "a");

        // Act
        vm.OnAnswerSelected(1, "b");

        // Assert
        Assert.Equal("b", vm.UserSelectedAnswers[1]);
    }

    [Fact]
    public void UpdateButtonStates_WhenCurrentPage1AndTotalPages3_ShouldSetCanGoPreviousFalse()
    {
        // Arrange
        var vm = new MainWindowViewModel(Mock.Of<IService1>(), Mock.Of<IMessageService>())
        {
            CurrentPage = 1,
            TotalPages = 3
        };

        // 反射调用私有方法 UpdateButtonStates
        var updateMethod = typeof(MainWindowViewModel).GetMethod("UpdateButtonStates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Act
        updateMethod?.Invoke(vm, null);

        // Assert
        Assert.False(vm.CanGoPrevious);
        Assert.True(vm.CanGoNext);
    }

    [Fact]
    public void UpdateStats_ShouldSyncWithServiceStats()
    {
        // Arrange
        var mockService1 = new Mock<IService1>();
        mockService1.Setup(s => s.GetAnswerStats()).Returns((75.5, 10, 20));
        var vm = new MainWindowViewModel(mockService1.Object, Mock.Of<IMessageService>());

        // 反射调用私有方法 UpdateStats
        var updateMethod = typeof(MainWindowViewModel).GetMethod("UpdateStats", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Act
        updateMethod?.Invoke(vm, null);

        // Assert
        Assert.Equal(75.5, vm.CurrentPageAccuracy);
        Assert.Equal(10, vm.GlobalAnsweredCount);
        Assert.Equal(20, vm.TotalQuestionCount);
        mockService1.Verify(s => s.GetAnswerStats(), Times.Once);
    }
}