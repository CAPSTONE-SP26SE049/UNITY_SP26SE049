using System;
using UnityEngine;

[Serializable]
public class SpeakingQuestionData
{
    public string question; // Câu hỏi/instruction (ví dụ: "Đọc to từ: "nắng"")
    public string targetText; // Text đúng cần phát âm (ví dụ: "nắng")
    public string goal; // Mục tiêu (ví dụ: "Phát âm N đầu từ")

    public SpeakingQuestionData(string question, string targetText, string goal)
    {
        this.question = question;
        this.targetText = targetText;
        this.goal = goal;
    }
}

