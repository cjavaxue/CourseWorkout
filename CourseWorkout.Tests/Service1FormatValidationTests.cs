using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseWorkout.Models;
using CourseWorkout.Services;
using Moq;
using Xunit;

namespace CourseWorkout.Tests;

/// <summary>
/// Service1 格式校验功能测试
/// </summary>
public class Service1FormatValidationTests
{
    [Fact]
    public async Task InitializeAsync_WithMissingOptions_ShouldThrowArgumentException()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = new List<Question>
        {
            new Question
            {
                Id = 1,
                Title = "Test Question",
                Options = new Dictionary<string, string>
                {
                    { "a", "Option A" },
                    { "b", "Option B" }
                    // 缺少 c 和 d
                },
                CorrectAnswer = "a"
            }
        };

        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service1.InitializeAsync("test.txt"));
        Assert.Contains("第1题缺少选项", exception.Message);
    }

    [Fact]
    public async Task InitializeAsync_WithInvalidAnswer_ShouldThrowArgumentException()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = new List<Question>
        {
            new Question
            {
                Id = 1,
                Title = "Test Question",
                Options = new Dictionary<string, string>
                {
                    { "a", "Option A" },
                    { "b", "Option B" },
                    { "c", "Option C" },
                    { "d", "Option D" }
                },
                CorrectAnswer = "e" // 无效答案
            }
        };

        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service1.InitializeAsync("test.txt"));
        Assert.Contains("第1题答案格式不正确", exception.Message);
    }

    [Fact]
    public async Task SaveAnswerAsync_WithInvalidAnswer_ShouldThrowArgumentException()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = CreateTestQuestions(1);
        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);
        await service1.InitializeAsync("test.txt");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service1.SaveAnswerAsync(1, "e"));
        Assert.Contains("用户答案必须是 a、b、c 或 d 之一", exception.Message);
    }

    private List<Question> CreateTestQuestions(int count)
    {
        var questions = new List<Question>();
        for (int i = 1; i <= count; i++)
        {
            questions.Add(new Question
            {
                Id = i,
                Title = $"Question {i}",
                Options = new Dictionary<string, string>
                {
                    { "a", "Option A" },
                    { "b", "Option B" },
                    { "c", "Option C" },
                    { "d", "Option D" }
                },
                CorrectAnswer = "a"
            });
        }
        return questions;
    }
}

