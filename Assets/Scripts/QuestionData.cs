using System;
using UnityEngine;

[Serializable]
public class QuestionData
{
    public string question;
    public string[] options; // A, B, C, D
    public int correctAnswerIndex; // 0 = A, 1 = B, 2 = C, 3 = D

    public QuestionData(string question, string[] options, int correctAnswerIndex)
    {
        this.question = question;
        this.options = options;
        this.correctAnswerIndex = correctAnswerIndex;
    }
}

