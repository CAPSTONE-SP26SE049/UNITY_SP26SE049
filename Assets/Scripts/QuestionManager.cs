using System.Collections.Generic;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    public static QuestionManager Instance { get; private set; }

    private List<QuestionData> questions = new List<QuestionData>();

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
        // Nhóm câu hỏi 1
        questions.Add(new QuestionData(
            "Con ___ đang bơi dưới nước.",
            new string[] { "lươn", "nươn", "lương", "nương" },
            0 // A. lươn
        ));

        questions.Add(new QuestionData(
            "Em bé ___ bi trên sân.",
            new string[] { "năn", "lăn", "lan", "nan" },
            1 // B. lăn
        ));

        questions.Add(new QuestionData(
            "Buổi sáng, mặt trời ___ lên rất đẹp.",
            new string[] { "lắng", "nắng", "lắng lên", "nắng lên" },
            3 // D. nắng lên
        ));

        questions.Add(new QuestionData(
            "Con mèo đang ___ trên ghế.",
            new string[] { "nàm", "làm", "nằm", "lằm" },
            2 // C. nằm
        ));

        questions.Add(new QuestionData(
            "Anh ấy đang ___ bài cho em.",
            new string[] { "nàm", "làm", "lam", "nam" },
            1 // B. làm
        ));

        questions.Add(new QuestionData(
            "Chiếc lá rơi xuống ___ đất.",
            new string[] { "nền", "lền", "lên", "nền" },
            3 // D. nền
        ));

        questions.Add(new QuestionData(
            "Con chim đang ___ trên cành cây.",
            new string[] { "nổ", "lổ", "nở", "lở" },
            2 // C. nở
        ));

        questions.Add(new QuestionData(
            "Bé ___ từng bước nhỏ.",
            new string[] { "nắng", "lắng", "nắng nghe", "lắng nghe" },
            3 // D. lắng nghe
        ));

        questions.Add(new QuestionData(
            "Em ___ bài rất cẩn thận.",
            new string[] { "nàm", "làm", "nam", "lam" },
            1 // B. làm
        ));

        questions.Add(new QuestionData(
            "Con trâu đang đứng dưới ___ cây.",
            new string[] { "nùm", "lùm", "nồm", "lồm" },
            1 // B. lùm
        ));

        // Nhóm câu hỏi 2
        questions.Add(new QuestionData(
            "Em bé đang ___ ngủ trên giường.",
            new string[] { "nàm", "làm", "nằm", "lằm" },
            2 // C. nằm
        ));

        questions.Add(new QuestionData(
            "Mẹ đang ___ cơm trong bếp.",
            new string[] { "nấu", "lấu", "nau", "lau" },
            0 // A. nấu
        ));

        questions.Add(new QuestionData(
            "Chiếc thuyền đang trôi trên ___ nước.",
            new string[] { "nàn", "làn", "nan", "lan" },
            1 // B. làn
        ));

        questions.Add(new QuestionData(
            "Em hãy ___ kỹ trước khi trả lời.",
            new string[] { "nắng", "lắng", "nắng nghe", "lắng nghe" },
            3 // D. lắng nghe
        ));

        questions.Add(new QuestionData(
            "Người nông dân đang cày trên ___ ruộng.",
            new string[] { "nảnh", "lãnh", "nảnh ruộng", "mảnh ruộng" },
            3 // D. mảnh ruộng
        ));

        questions.Add(new QuestionData(
            "Cô giáo rất hiền và ___.",
            new string[] { "nịch sự", "lịch sự", "nịch sư", "lịch sư" },
            1 // B. lịch sự
        ));

        questions.Add(new QuestionData(
            "Bầu trời hôm nay rất trong và ___.",
            new string[] { "nắng", "lắng", "nắng trong", "lắng trong" },
            0 // A. nắng
        ));

        questions.Add(new QuestionData(
            "Con cá đang bơi trong ___ nước.",
            new string[] { "nàn", "làn", "lan", "nan" },
            1 // B. làn
        ));

        questions.Add(new QuestionData(
            "Em ___ bài rất cẩn thận.",
            new string[] { "nàm", "làm", "nam", "lam" },
            1 // B. làm
        ));

        questions.Add(new QuestionData(
            "Những chiếc lá rơi xuống ___ đất.",
            new string[] { "nền", "lền", "lên", "lân" },
            0 // A. nền
        ));
    }

    public QuestionData GetRandomQuestion()
    {
        if (questions.Count == 0)
        {
            InitializeQuestions();
        }
        
        int randomIndex = Random.Range(0, questions.Count);
        return questions[randomIndex];
    }
}

