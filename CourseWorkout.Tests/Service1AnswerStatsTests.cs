using System.Collections.Generic;
using System.Threading.Tasks;
using CourseWorkout.Models;
using CourseWorkout.Services;
using Moq;
using Xunit;

namespace CourseWorkout.Tests;

/// <summary>
/// Service1 答题统计功能测试
/// </summary>
public class Service1AnswerStatsTests
{
    [Fact]
    public async Task GetAnswerStats_With10Questions8Correct_ShouldReturn80Percent()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = CreateTestQuestions(10);
        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);
        await service1.InitializeAsync("test.txt");
        
        // 先获取第一页
        service1.GetPageQuestions(1);

        // 提交10道题的答案，8道正确，2道错误
        for (int i = 1; i <= 8; i++)
        {
            await service1.SaveAnswerAsync(i, "a", 5); // 正确答案
        }
        for (int i = 9; i <= 10; i++)
        {
            await service1.SaveAnswerAsync(i, "b", 5); // 错误答案
        }

        // Act
        var (accuracy, answeredCount, totalCount) = service1.GetAnswerStats();

        // Assert
        Assert.Equal(80.0, accuracy, 2);
        Assert.Equal(10, answeredCount);
        Assert.Equal(10, totalCount);
    }

    [Fact]
    public async Task GetAnswerStats_WithNoAnswers_ShouldReturn0Percent()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = CreateTestQuestions(10);
        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);
        await service1.InitializeAsync("test.txt");
        service1.GetPageQuestions(1);

        // Act
        var (accuracy, answeredCount, totalCount) = service1.GetAnswerStats();

        // Assert
        Assert.Equal(0.0, accuracy);
        Assert.Equal(0, answeredCount);
        Assert.Equal(10, totalCount);
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

