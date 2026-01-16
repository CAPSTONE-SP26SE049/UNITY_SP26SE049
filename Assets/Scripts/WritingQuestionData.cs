using System;
using UnityEngine;

[Serializable]
public class WritingQuestionData
{
    public string question; // Câu hỏi có chỗ trống (dùng ___ để đánh dấu)
    public string correctAnswer; // Đáp án đúng

    public WritingQuestionData(string question, string correctAnswer)
    {
        this.question = question;
        this.correctAnswer = correctAnswer;
    }
}

