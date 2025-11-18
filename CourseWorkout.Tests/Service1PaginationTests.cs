using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseWorkout.Models;
using CourseWorkout.Services;
using Moq;
using Xunit;

namespace CourseWorkout.Tests;

/// <summary>
/// Service1 分页功能测试
/// </summary>
public class Service1PaginationTests
{
    [Fact]
    public async Task GetPageQuestions_With100Questions_ShouldReturn10Pages()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = new List<Question>();
        
        // 创建100道题
        for (int i = 1; i <= 100; i++)
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

        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);
        await service1.InitializeAsync("test.txt");

        // Act
        var (pageQuestions, totalPages) = service1.GetPageQuestions(1);

        // Assert
        Assert.Equal(10, totalPages);
        Assert.Equal(10, pageQuestions.Count);
        Assert.Equal(1, pageQuestions.First().Id);
        Assert.Equal(10, pageQuestions.Last().Id);
    }

    [Fact]
    public async Task GetPageQuestions_WithCurrentPageLessThan1_ShouldReturnFirstPage()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = CreateTestQuestions(25);
        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);
        await service1.InitializeAsync("test.txt");

        // Act
        var (pageQuestions, totalPages) = service1.GetPageQuestions(0);

        // Assert
        Assert.Equal(1, pageQuestions.First().Id);
    }

    [Fact]
    public async Task GetPageQuestions_WithCurrentPageGreaterThanTotalPages_ShouldReturnLastPage()
    {
        // Arrange
        var mockService2 = new Mock<IService2>();
        var questions = CreateTestQuestions(25);
        mockService2.Setup(s => s.LoadQuestionsAsync(It.IsAny<string>()))
            .ReturnsAsync(questions);

        var service1 = new Service1(mockService2.Object);
        await service1.InitializeAsync("test.txt");

        // Act
        var (pageQuestions, totalPages) = service1.GetPageQuestions(100);

        // Assert
        Assert.Equal(3, totalPages);
        Assert.Equal(21, pageQuestions.First().Id);
        Assert.Equal(25, pageQuestions.Last().Id);
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

