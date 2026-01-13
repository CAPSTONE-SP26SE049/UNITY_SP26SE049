using System;
using UnityEngine;

[Serializable]
public class ListeningQuestionData
{
    public string audioFileName; // Tên file audio (L1, L2, ..., L20)
    public string sentence;      // Câu nghe (hiển thị trên UI)
    public string[] options; // 4 đáp án: A, B, C, D
    public int correctAnswerIndex; // 0 = A, 1 = B, 2 = C, 3 = D

    public ListeningQuestionData(string audioFileName, string sentence, string[] options, int correctAnswerIndex)
    {
        this.audioFileName = audioFileName;
        this.sentence = sentence;
        this.options = options;
        this.correctAnswerIndex = correctAnswerIndex;
    }
}

