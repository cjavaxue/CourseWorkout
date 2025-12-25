using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CourseWorkout.Models;

namespace CourseWorkout.Services;

/// <summary>
/// Service2 实现：读取本地 txt 文件
/// </summary>
public class Service2 : IService2
{
    /// <summary>
    /// 从指定路径加载题目列表
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>题目列表</returns>
    /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
    /// <exception cref="IOException">文件读取失败时抛出</exception>
    public async Task<List<Question>> LoadQuestionsAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("请检查Resources文件夹下是否存在Questions.txt文件", filePath);
            }

            var questions = new List<Question>();
            var lines = await File.ReadAllLinesAsync(filePath, Encoding.UTF8);

            Question? currentQuestion = null;
            int questionId = 0;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // 跳过空行和注释行
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    // 如果遇到空行且当前题目已完整，则保存题目
                    if (currentQuestion != null && currentQuestion.Options.Count == 4 && !string.IsNullOrEmpty(currentQuestion.CorrectAnswer))
                    {
                        questions.Add(currentQuestion);
                        currentQuestion = null;
                    }
                    continue;
                }

                if (trimmedLine.StartsWith("//"))
                {
                    continue;
                }

                // 检查是否是题号行（以数字开头，后跟点）
                if (char.IsDigit(trimmedLine[0]) && trimmedLine.Contains('.'))
                {
                    // 如果之前有未完成的题目，先保存
                    if (currentQuestion != null && currentQuestion.Options.Count == 4 && !string.IsNullOrEmpty(currentQuestion.CorrectAnswer))
                    {
                        questions.Add(currentQuestion);
                    }

                    // 创建新题目
                    var dotIndex = trimmedLine.IndexOf('.');
                    if (dotIndex > 0 && int.TryParse(trimmedLine.Substring(0, dotIndex), out int id))
                    {
                        questionId = id;
                    }

                    currentQuestion = new Question
                    {
                        Id = questionId,
                        Title = trimmedLine.Substring(dotIndex + 1).Trim(),
                        Options = new Dictionary<string, string>()
                    };
                }
                // 检查是否是选项行（以 a. b. c. d. 开头）
                else if (currentQuestion != null && trimmedLine.Length >= 2 && 
                         (trimmedLine.StartsWith("a.", StringComparison.OrdinalIgnoreCase) ||
                          trimmedLine.StartsWith("b.", StringComparison.OrdinalIgnoreCase) ||
                          trimmedLine.StartsWith("c.", StringComparison.OrdinalIgnoreCase) ||
                          trimmedLine.StartsWith("d.", StringComparison.OrdinalIgnoreCase)))
                {
                    var optionKey = trimmedLine.Substring(0, 1).ToLower();
                    var optionValue = trimmedLine.Substring(2).Trim();
                    currentQuestion.Options[optionKey] = optionValue;
                }
                // 检查是否是答案行（以 ANSWER: 开头）
                else if (currentQuestion != null && trimmedLine.StartsWith("ANSWER:", StringComparison.OrdinalIgnoreCase))
                {
                    var answer = trimmedLine.Substring(7).Trim().ToLower();
                    if (answer.Length > 0)
                    {
                        currentQuestion.CorrectAnswer = answer.Substring(0, 1);
                    }
                }
                // 检查是否是难度行（DIFFICULTY:）
                else if (currentQuestion != null && trimmedLine.StartsWith("DIFFICULTY:", StringComparison.OrdinalIgnoreCase))
                {
                    var diffText = trimmedLine.Substring("DIFFICULTY:".Length).Trim().ToLower();
                    if (Enum.TryParse<Difficulty>(diffText, true, out var difficulty))
                    {
                        currentQuestion.Difficulty = difficulty;
                    }
                }
            }

            // 保存最后一个题目
            if (currentQuestion != null && currentQuestion.Options.Count == 4 && !string.IsNullOrEmpty(currentQuestion.CorrectAnswer))
            {
                questions.Add(currentQuestion);
            }

            return questions;
        }
        catch (FileNotFoundException ex)
        {
            throw new FileNotFoundException("请检查Resources文件夹下是否存在Questions.txt文件", ex.FileName, ex);
        }
        catch (IOException ex)
        {
            throw new IOException("请检查Resources文件夹下是否存在Questions.txt文件", ex);
        }
    }
}

