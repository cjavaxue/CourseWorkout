using System.Collections.Generic;
using System.Threading.Tasks;
using CourseWorkout.Models;
using CourseWorkout.Services;
using Moq;
using Xunit;

namespace CourseWorkout.Tests;

/// <summary>
/// Service1 错题本功能测试
/// </summary>
public class Service1WrongQuestionsTests
{
    [Fact]
    public async Task SaveAnswerAsync_WithWrongAnswer_ShouldMarkQuestionAsWrong()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = CreateTestQuestions(5);
        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);
        await service1.InitializeAsync("test.txt");

        // Act
        await service1.SaveAnswerAsync(1, "b", 3); // 错误答案（正确答案是 a）

        // Assert
        var wrongQuestions = service1.GetWrongQuestions();
        Assert.Single(wrongQuestions);
        Assert.True(wrongQuestions[0].IsMarkedAsWrong);
        Assert.Equal(1, wrongQuestions[0].Id);
    }

    [Fact]
    public async Task SaveAnswerAsync_WithCorrectAnswer_ShouldNotMarkQuestionAsWrong()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = CreateTestQuestions(5);
        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);
        await service1.InitializeAsync("test.txt");

        // Act
        await service1.SaveAnswerAsync(1, "a", 2); // 正确答案

        // Assert
        var wrongQuestions = service1.GetWrongQuestions();
        Assert.Empty(wrongQuestions);
    }

    [Fact]
    public async Task ClearWrongMarks_ShouldClearAllWrongMarks()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = CreateTestQuestions(5);
        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);
        await service1.InitializeAsync("test.txt");

        // 先标记一些错题
        await service1.SaveAnswerAsync(1, "b", 1);
        await service1.SaveAnswerAsync(2, "b", 1);
        Assert.Equal(2, service1.GetWrongQuestions().Count);

        // Act
        service1.ClearWrongMarks();

        // Assert
        var wrongQuestions = service1.GetWrongQuestions();
        Assert.Empty(wrongQuestions);
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

