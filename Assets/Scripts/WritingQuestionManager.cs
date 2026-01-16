using System.Collections.Generic;
using UnityEngine;

public class WritingQuestionManager : MonoBehaviour
{
    public static WritingQuestionManager Instance { get; private set; }

    private List<WritingQuestionData> questions = new List<WritingQuestionData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeQuestions();
    }

    private void InitializeQuestions()
    {
        questions.Add(new WritingQuestionData(
            "Con ___ đang bơi dưới nước.",
            "lươn"
        ));

        questions.Add(new WritingQuestionData(
            "Em bé ___ ngủ trên giường.",
            "nằm"
        ));

        questions.Add(new WritingQuestionData(
            "Anh ấy đang ___ bài tập.",
            "làm"
        ));

        questions.Add(new WritingQuestionData(
            "Buổi sáng, mặt trời ___ lên rất đẹp.",
            "nắng"
        ));

        questions.Add(new WritingQuestionData(
            "Con mèo ___ trên chiếc ghế nhỏ.",
            "nằm"
        ));

        questions.Add(new WritingQuestionData(
            "Những chiếc lá rơi xuống ___ đất.",
            "nền"
        ));

        questions.Add(new WritingQuestionData(
            "Em hãy ___ nghe cô giáo giảng bài.",
            "lắng"
        ));

        questions.Add(new WritingQuestionData(
            "Con chim non đang ___ trong tổ.",
            "nở"
        ));

        questions.Add(new WritingQuestionData(
            "Người nông dân ___ việc trên ruộng lúa.",
            "làm"
        ));

        questions.Add(new WritingQuestionData(
            "Con trâu đứng dưới ___ cây.",
            "lùm"
        ));

        questions.Add(new WritingQuestionData(
            "Mẹ đang ___ bữa trưa cho gia đình.",
            "nấu"
        ));

        questions.Add(new WritingQuestionData(
            "Cô giáo rất ___ sự với học sinh.",
            "lịch"
        ));

        questions.Add(new WritingQuestionData(
            "Em hãy ___ nghỉ sau giờ học.",
            "nằm"
        ));

        questions.Add(new WritingQuestionData(
            "Những ___ gió nhẹ thổi qua cánh đồng.",
            "làn"
        ));

        questions.Add(new WritingQuestionData(
            "Anh Nam ___ việc rất chăm chỉ.",
            "làm"
        ));

        questions.Add(new WritingQuestionData(
            "Mặt hồ ___ lẽ dưới ánh nắng.",
            "lặng"
        ));

        questions.Add(new WritingQuestionData(
            "Cô giáo luôn ___ sự và nhẹ nhàng.",
            "lịch"
        ));

        questions.Add(new WritingQuestionData(
            "Con cá nhỏ bơi trong ___ nước trong xanh.",
            "làn"
        ));

        questions.Add(new WritingQuestionData(
            "Em bé ___ nghe bà kể chuyện.",
            "nằm"
        ));

        questions.Add(new WritingQuestionData(
            "Người thợ ___ nghề đang làm cửa gỗ.",
            "lành"
        ));
    }

    public WritingQuestionData GetRandomQuestion()
    {
        if (questions.Count == 0)
        {
            InitializeQuestions();
        }
        
        int randomIndex = Random.Range(0, questions.Count);
        return questions[randomIndex];
    }
}

