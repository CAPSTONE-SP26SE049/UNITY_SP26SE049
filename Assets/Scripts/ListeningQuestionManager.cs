using System.Collections.Generic;
using UnityEngine;

public class ListeningQuestionManager : MonoBehaviour
{
    public static ListeningQuestionManager Instance { get; private set; }

    private List<ListeningQuestionData> questions = new List<ListeningQuestionData>();

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
        questions.Add(new ListeningQuestionData(
            "L1",
            "Con ___ đang bơi dưới nước.",
            new string[] { "nươn", "lươn", "lương", "nương" },
            1 // B. lươn
        ));

        questions.Add(new ListeningQuestionData(
            "L2",
            "Em bé ___ ngủ trên giường.",
            new string[] { "lằm", "nàm", "nằm", "làm" },
            2 // C. nằm
        ));

        questions.Add(new ListeningQuestionData(
            "L3",
            "Anh ấy đang ___ bài tập.",
            new string[] { "nàm", "làm", "lam", "nam" },
            1 // B. làm
        ));

        questions.Add(new ListeningQuestionData(
            "L4",
            "Buổi sáng, trời rất ___.",
            new string[] { "lắng", "nắng", "lắng nắng", "nắng lắng" },
            1 // B. nắng
        ));

        questions.Add(new ListeningQuestionData(
            "L5",
            "Con mèo ___ trên chiếc ghế.",
            new string[] { "lằm", "làm", "nàm", "nằm" },
            3 // D. nằm
        ));

        questions.Add(new ListeningQuestionData(
            "L6",
            "Những chiếc lá rơi xuống ___ đất.",
            new string[] { "lền", "nền", "lên", "nân" },
            1 // B. nền
        ));

        questions.Add(new ListeningQuestionData(
            "L7",
            "Em hãy ___ nghe cô giáo.",
            new string[] { "nắng", "lắng", "nắng nghe", "lắng nhe" },
            1 // B. lắng
        ));

        questions.Add(new ListeningQuestionData(
            "L8",
            "Con chim non đang ___ trong tổ.",
            new string[] { "lở", "nở", "lổ", "nổ" },
            1 // B. nở
        ));

        questions.Add(new ListeningQuestionData(
            "L9",
            "Người nông dân ___ việc trên ruộng.",
            new string[] { "nàm", "làm", "nam", "lam" },
            1 // B. làm
        ));

        questions.Add(new ListeningQuestionData(
            "L10",
            "Con trâu đứng dưới ___ cây.",
            new string[] { "nùm", "lùm", "nồm", "lồm" },
            1 // B. lùm
        ));

        questions.Add(new ListeningQuestionData(
            "L11",
            "Mẹ đang ___ bữa trưa.",
            new string[] { "nấu", "lấu", "nau", "lau" },
            0 // A. nấu
        ));

        questions.Add(new ListeningQuestionData(
            "L12",
            "Cô giáo rất ___ sự.",
            new string[] { "nịch", "lịch", "nịch sự", "lịch sư" },
            1 // B. lịch
        ));

        questions.Add(new ListeningQuestionData(
            "L13",
            "Em hãy ___ nghỉ một lát.",
            new string[] { "lằm", "nàm", "nằm", "làm" },
            2 // C. nằm
        ));

        questions.Add(new ListeningQuestionData(
            "L14",
            "Những ___ gió nhẹ thổi qua.",
            new string[] { "nàn", "làn", "lan", "nan" },
            1 // B. làn
        ));

        questions.Add(new ListeningQuestionData(
            "L15",
            "Anh Nam ___ việc rất chăm chỉ.",
            new string[] { "nàm", "làm", "nam", "lam" },
            1 // B. làm
        ));

        questions.Add(new ListeningQuestionData(
            "L16",
            "Mặt hồ ___ lẽ vào buổi sáng.",
            new string[] { "nặng", "lặng", "nặng lẽ", "lặng lẽ" },
            1 // B. lặng
        ));

        questions.Add(new ListeningQuestionData(
            "L17",
            "Cô giáo luôn ___ sự với học sinh.",
            new string[] { "nịch", "lịch", "nịch sự", "lịch sư" },
            1 // B. lịch
        ));

        questions.Add(new ListeningQuestionData(
            "L18",
            "Con cá nhỏ bơi trong ___ nước.",
            new string[] { "nàn", "làn", "lan", "nan" },
            1 // B. làn
        ));

        questions.Add(new ListeningQuestionData(
            "L19",
            "Em bé ___ nghe bà kể chuyện.",
            new string[] { "lằm", "nàm", "nằm", "làm" },
            2 // C. nằm
        ));

        questions.Add(new ListeningQuestionData(
            "L20",
            "Người thợ ___ nghề đang làm cửa gỗ.",
            new string[] { "nành", "lành", "nành nghề", "làn nghề" },
            1 // B. lành
        ));
    }

    public ListeningQuestionData GetRandomQuestion()
    {
        if (questions.Count == 0)
        {
            InitializeQuestions();
        }
        
        int randomIndex = Random.Range(0, questions.Count);
        return questions[randomIndex];
    }
}

