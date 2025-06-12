using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameControl2 : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject Player1;
    public GameObject Player2;
    public Transform[] waypoints;
    public float moveSpeed = 5f;
    public Vector3 player1Offset = new Vector3(-0.5f, 0f, 0f);


    [Header("Quiz Settings")]
    public GameObject quizCanvas;
    public TMP_Text questionText;
    public Button[] answerButtons;
    public TMP_Text[] answerTexts;
    public TMP_Text currentPlayerText;

    [Header("Scoring")]
    public TMP_Text player1ScoreText;
    public TMP_Text player2ScoreText;
    public int player1Score = 0;
    public int player2Score = 0;
    public int pointsForCorrectAnswer = 10;

    [Header("Answer Feedback")]
    public Color correctAnswerColor = Color.green;
    public Color wrongAnswerColor = Color.red;
    public float feedbackDelay = 1f;

    [Header("Game Completion")]
    public GameObject gameCompletionPanel;
    public TMP_Text finalScoreText;
    public Button restartButton;

    [Header("Game UI")]
    public GameObject scoreCanvas;
    public GameObject nextButtonCanvas;

    [System.Serializable]
    public class QuizQuestion
    {
        public string question;
        public string[] answers = new string[4];
        public int correctAnswerIndex;
        public string category;
        public string difficulty;
        public bool isBonus = false;
    }

    public List<QuizQuestion> quizQuestions = new List<QuizQuestion>();

    private int player1WaypointIndex = 0;
    private int player2WaypointIndex = 0;
    private int player1QuestionCount = 0;
    private int player2QuestionCount = 0;

    private bool isMoving = false;
    private int currentQuestionIndex = 0;
    private bool isPlayer1Turn = true;

    public static GameControl2 Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned!");
            return;
        }

        Player1.transform.position = waypoints[0].position + player1Offset;
        if (Player2 != null) Player2.transform.position = waypoints[0].position;

        if (quizCanvas != null) quizCanvas.SetActive(false);
        if (gameCompletionPanel != null)
        {
            gameCompletionPanel.SetActive(false);
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
        }

        UpdateScoreDisplay();
        UpdateCurrentPlayerText();

        if (quizQuestions.Count == 0)
            AddSampleQuestions();

        quizQuestions = ShuffleList(quizQuestions);
    }

    private List<QuizQuestion> ShuffleList(List<QuizQuestion> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            QuizQuestion temp = list[i];
            int rand = Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
        }
        return list;
    }

    private void UpdateScoreDisplay()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = $"Player 1: {player1Score}";
        if (player2ScoreText != null)
            player2ScoreText.text = $"Player 2: {player2Score}";
    }

    private void UpdateCurrentPlayerText()
    {
        if (currentPlayerText != null)
            currentPlayerText.text = isPlayer1Turn ? "Player 1's Turn" : "Player 2's Turn";
    }

    public void SetQuizQuestions(List<QuizQuestion> questions)
    {
        if (questions != null && questions.Count > 0)
            quizQuestions = ShuffleList(questions);
    }

    public void MoveToNextWaypoint()
    {
        ShowQuizQuestion();
    }

    private void ShowQuizQuestion()
    {
        if (quizCanvas == null || questionText == null || answerButtons.Length < 4)
        {
            Debug.LogError("Quiz UI components not assigned!");
            return;
        }

        if (scoreCanvas != null) scoreCanvas.SetActive(false);
        if (nextButtonCanvas != null) nextButtonCanvas.SetActive(false);
        quizCanvas.SetActive(true);

        QuizQuestion currentQuestion = quizQuestions[currentQuestionIndex % quizQuestions.Count];
        questionText.text = currentQuestion.question;

        // Determine if this is a bonus question for the active player
        int questionCount = isPlayer1Turn ? player1QuestionCount : player2QuestionCount;
        bool isBonus = questionCount == 4 || questionCount == 8 || questionCount == 12 || questionCount == 18;

        if (isBonus)
        {
            questionText.text += "\n<size=75%><color=yellow>[Întrebare Bonus – punctaj dublu!]</color></size>";
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < currentQuestion.answers.Length)
            {
                if (answerTexts != null && answerTexts.Length > i)
                {
                    answerTexts[i].text = currentQuestion.answers[i];
                    ConfigureTextToFitButton(answerTexts[i]);
                }

                int answerIndex = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => CheckAnswer(answerIndex, isBonus));
                answerButtons[i].gameObject.SetActive(true);
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        currentQuestionIndex++;
        if (isPlayer1Turn) player1QuestionCount++;
        else player2QuestionCount++;
    }

    private void ConfigureTextToFitButton(TMP_Text textComponent)
    {
        if (textComponent == null) return;
        textComponent.enableAutoSizing = true;
        textComponent.fontSizeMin = 10;
        textComponent.fontSizeMax = 24;
        textComponent.enableWordWrapping = true;
        textComponent.overflowMode = TextOverflowModes.Ellipsis;
        textComponent.alignment = TextAlignmentOptions.Center;
    }

    private void CheckAnswer(int selectedAnswerIndex, bool isBonus)
    {
        QuizQuestion currentQuestion = quizQuestions[(currentQuestionIndex - 1) % quizQuestions.Count];

        Image correctImage = answerButtons[currentQuestion.correctAnswerIndex].GetComponent<Image>();
        if (correctImage != null)
            correctImage.color = correctAnswerColor;

        if (selectedAnswerIndex == currentQuestion.correctAnswerIndex)
        {
            int earnedPoints = isBonus ? pointsForCorrectAnswer * 2 : pointsForCorrectAnswer;

            if (isPlayer1Turn)
                player1Score += earnedPoints;
            else
                player2Score += earnedPoints;

            UpdateScoreDisplay();
        }
        else
        {
            Image wrongImage = answerButtons[selectedAnswerIndex].GetComponent<Image>();
            if (wrongImage != null)
                wrongImage.color = wrongAnswerColor;
        }

        foreach (Button btn in answerButtons)
            btn.interactable = false;

        StartCoroutine(ProceedAfterDelay());
    }

    private IEnumerator ProceedAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDelay);

        foreach (Button btn in answerButtons)
        {
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = Color.white;
                btn.interactable = true;
            }
        }

        quizCanvas.SetActive(false);
        if (scoreCanvas != null) scoreCanvas.SetActive(true);
        if (nextButtonCanvas != null) nextButtonCanvas.SetActive(true);

        MoveCurrentPlayerToNextWaypoint();
        isPlayer1Turn = !isPlayer1Turn;
        UpdateCurrentPlayerText();
    }

    private void MoveCurrentPlayerToNextWaypoint()
    {
        if (isMoving) return;

        GameObject currentPlayer = isPlayer1Turn ? Player1 : Player2;
        int nextIndex = isPlayer1Turn ? player1WaypointIndex + 1 : player2WaypointIndex + 1;

        if (nextIndex >= waypoints.Length)
        {
            ShowGameCompletionPanel();
            return;
        }

        if (isPlayer1Turn)
        {
            player1WaypointIndex = nextIndex;
            StartCoroutine(MovePlayerToWaypoint(currentPlayer, waypoints[player1WaypointIndex]));
        }
        else
        {
            player2WaypointIndex = nextIndex;
            StartCoroutine(MovePlayerToWaypoint(currentPlayer, waypoints[player2WaypointIndex]));
        }
    }

    private IEnumerator MovePlayerToWaypoint(GameObject player, Transform targetWaypoint)
    {
        isMoving = true;

        Vector3 start = player.transform.position;
        Vector3 end = targetWaypoint.position;
        float distance = Vector3.Distance(start, end);
        float startTime = Time.time;

        while (player.transform.position != end)
        {
            float dist = (Time.time - startTime) * moveSpeed;
            float frac = dist / distance;
            player.transform.position = Vector3.Lerp(start, end, frac);
            if (frac >= 1f) break;
            yield return null;
        }

        player.transform.position = end;
        isMoving = false;
    }

    private void ShowGameCompletionPanel()
    {
        if (!gameCompletionPanel) return;

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Felicitări!\nPlayer 1: {player1Score}\nPlayer 2: {player2Score}";
            if (player1Score > player2Score)
                finalScoreText.text += "\n\nPlayer 1 Wins!";
            else if (player2Score > player1Score)
                finalScoreText.text += "\n\nPlayer 2 Wins!";
            else
                finalScoreText.text += "\n\nIt's a tie!";
        }

        if (scoreCanvas != null) scoreCanvas.SetActive(false);
        if (nextButtonCanvas != null) nextButtonCanvas.SetActive(false);
        gameCompletionPanel.SetActive(true);
    }

    public void RestartGame()
    {
        ResetGameState();
    }

    private void ResetGameState()
    {
        player1Score = 0;
        player2Score = 0;
        player1WaypointIndex = 0;
        player2WaypointIndex = 0;
        player1QuestionCount = 0;
        player2QuestionCount = 0;
        currentQuestionIndex = 0;
        isPlayer1Turn = true;

        if (Player1 != null) Player1.transform.position = waypoints[0].position;
        if (Player2 != null) Player2.transform.position = waypoints[0].position;

        if (gameCompletionPanel != null) gameCompletionPanel.SetActive(false);
        if (scoreCanvas != null) scoreCanvas.SetActive(true);
        if (nextButtonCanvas != null) nextButtonCanvas.SetActive(true);

        UpdateScoreDisplay();
        UpdateCurrentPlayerText();
    }



private void AddSampleQuestions()
    {
        quizQuestions.Clear();
        QuizQuestion q;

        q = new QuizQuestion();
        q.question = "Ce echipă a câștigat Cupa Mondială FIFA în 2018?";
        q.answers = new string[] { "Germania", "Brazilia", "Franța", "Croația" };
        q.correctAnswerIndex = 2;
        q.category = "Sport";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este cel mai mare ocean?";
        q.answers = new string[] { "Atlantic", "Indian", "Pacific", "Arctic" };
        q.correctAnswerIndex = 2;
        q.category = "Geografie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "În ce an a căzut Zidul Berlinului?";
        q.answers = new string[] { "1987", "1989", "1990", "1991" };
        q.correctAnswerIndex = 1;
        q.category = "Istorie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este capitala Canadei?";
        q.answers = new string[] { "Toronto", "Montreal", "Vancouver", "Ottawa" };
        q.correctAnswerIndex = 3;
        q.category = "Geografie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce planetă este cea mai apropiată de Soare?";
        q.answers = new string[] { "Venus", "Pământ", "Mercur", "Marte" };
        q.correctAnswerIndex = 2;
        q.category = "Știință";
        q.difficulty = "hard";
        q.isBonus = true;
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce țară are cel mai mare număr de locuitori?";
        q.answers = new string[] { "SUA", "India", "China", "Rusia" };
        q.correctAnswerIndex = 2;
        q.category = "Geografie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este capitala Egiptului?";
        q.answers = new string[] { "Alexandria", "Giza", "Luxor", "Cairo" };
        q.correctAnswerIndex = 3;
        q.category = "Geografie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este capitala Franței?";
        q.answers = new string[] { "Madrid", "Berlin", "Paris", "Roma" };
        q.correctAnswerIndex = 2;
        q.category = "Geografie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a fost primul președinte al SUA?";
        q.answers = new string[] { "Thomas Jefferson", "George Washington", "Abraham Lincoln", "John Adams" };
        q.correctAnswerIndex = 1;
        q.category = "Istorie";
        q.difficulty = "hard";
        q.isBonus = true;
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a scris 'Romeo și Julieta'?";
        q.answers = new string[] { "Shakespeare", "Goethe", "Homer", "Cervantes" };
        q.correctAnswerIndex = 0;
        q.category = "Artă";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "În ce an a aderat România la NATO?";
        q.answers = new string[] { "2000", "2002", "2004", "2007" };
        q.correctAnswerIndex = 2;
        q.category = "Politică";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce compozitor a scris Simfonia a 9-a?";
        q.answers = new string[] { "Mozart", "Beethoven", "Bach", "Haydn" };
        q.correctAnswerIndex = 1;
        q.category = "Artă";
        q.difficulty = "hard";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce pictor român este celebru pentru 'Coloana Infinitului'?";
        q.answers = new string[] { "Tonitza", "Brâncuși", "Grigorescu", "Luchian" };
        q.correctAnswerIndex = 1;
        q.category = "Artă";
        q.difficulty = "hard";
        q.isBonus = true;
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "În ce an a avut loc Revoluția Franceză?";
        q.answers = new string[] { "1789", "1804", "1848", "1917" };
        q.correctAnswerIndex = 0;
        q.category = "Istorie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este simbolul chimic al Aurului?";
        q.answers = new string[] { "Ag", "Au", "Pt", "Pb" };
        q.correctAnswerIndex = 1;
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a câștigat Balonul de Aur în 2021?";
        q.answers = new string[] { "Messi", "Lewandowski", "Cristiano Ronaldo", "Mbappe" };
        q.correctAnswerIndex = 0;
        q.category = "Sport";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a pictat 'Noaptea înstelată'?";
        q.answers = new string[] { "Van Gogh", "Monet", "Picasso", "Dali" };
        q.correctAnswerIndex = 0;
        q.category = "Artă";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cât este 12 x 12?";
        q.answers = new string[] { "124", "132", "144", "156" };
        q.correctAnswerIndex = 2;
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce element are simbolul chimic 'Fe'?";
        q.answers = new string[] { "Fier", "Fluor", "Fermiu", "Fosfor" };
        q.correctAnswerIndex = 0;
        q.category = "Știință";
        q.difficulty = "hard";
        q.isBonus = true;
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a fost conducătorul revoluției bolșevice din 1917?";
        q.answers = new string[] { "Lenin", "Stalin", "Troțki", "Gorbaciov" };
        q.correctAnswerIndex = 0;
        q.category = "Istorie";
        q.difficulty = "hard";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce țară a inventat hârtia?";
        q.answers = new string[] { "Egipt", "China", "Grecia", "India" };
        q.correctAnswerIndex = 1;
        q.category = "Istorie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a descoperit penicilina?";
        q.answers = new string[] { "Einstein", "Pasteur", "Newton", "Fleming" };
        q.correctAnswerIndex = 3;
        q.category = "Știință";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cât este √144?";
        q.answers = new string[] { "10", "11", "12", "13" };
        q.correctAnswerIndex = 2;
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce oraș găzduiește sediul ONU?";
        q.answers = new string[] { "Londra", "Geneva", "New York", "Paris" };
        q.correctAnswerIndex = 2;
        q.category = "Politică";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce limbă are cei mai mulți vorbitori nativi?";
        q.answers = new string[] { "Engleză", "Spaniolă", "Hindi", "Chineză" };
        q.correctAnswerIndex = 3;
        q.category = "Cultură generală";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a scris 'Divina Comedie'?";
        q.answers = new string[] { "Virgil", "Homer", "Dante", "Ovidiu" };
        q.correctAnswerIndex = 2;
        q.category = "Artă";
        q.difficulty = "hard";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce țară este cunoscută pentru turnul său înclinat?";
        q.answers = new string[] { "Italia", "Grecia", "Franța", "Turcia" };
        q.correctAnswerIndex = 0;
        q.category = "Geografie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este cel mai lung fluviu din lume?";
        q.answers = new string[] { "Nil", "Amazon", "Mississippi", "Yangtze" };
        q.correctAnswerIndex = 1;
        q.category = "Geografie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Câte planete sunt în sistemul solar?";
        q.answers = new string[] { "7", "8", "9", "10" };
        q.correctAnswerIndex = 1;
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a fost Napoleon Bonaparte?";
        q.answers = new string[] { "Pictor", "General francez", "Filosof", "Astronom" };
        q.correctAnswerIndex = 1;
        q.category = "Istorie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este cel mai înalt munte din lume?";
        q.answers = new string[] { "K2", "Everest", "Kilimanjaro", "Alpii" };
        q.correctAnswerIndex = 1;
        q.category = "Geografie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a inventat becul electric?";
        q.answers = new string[] { "Nikola Tesla", "Thomas Edison", "Isaac Newton", "Galileo Galilei" };
        q.correctAnswerIndex = 1;
        q.category = "Știință";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce țară este cunoscută pentru Marele Zid?";
        q.answers = new string[] { "Japonia", "India", "China", "Coreea de Sud" };
        q.correctAnswerIndex = 2;
        q.category = "Istorie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este capitala Australiei?";
        q.answers = new string[] { "Sydney", "Melbourne", "Canberra", "Perth" };
        q.correctAnswerIndex = 2;
        q.category = "Geografie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a pictat 'Cina cea de Taină'?";
        q.answers = new string[] { "Michelangelo", "Leonardo da Vinci", "Rafael", "Rembrandt" };
        q.correctAnswerIndex = 1;
        q.category = "Artă";
        q.difficulty = "hard";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce organ este responsabil pentru pomparea sângelui în corp?";
        q.answers = new string[] { "Plămânul", "Creierul", "Inima", "Ficatul" };
        q.correctAnswerIndex = 2;
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a scris 'Enigma Otiliei'?";
        q.answers = new string[] { "Ion Luca Caragiale", "George Călinescu", "Mircea Eliade", "Marin Preda" };
        q.correctAnswerIndex = 1;
        q.category = "Artă";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este limba oficială în Brazilia?";
        q.answers = new string[] { "Spaniolă", "Portugheză", "Engleză", "Franceză" };
        q.correctAnswerIndex = 1;
        q.category = "Geografie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce element chimic are simbolul O?";
        q.answers = new string[] { "Aur", "Oxigen", "Ozon", "Osmiu" };
        q.correctAnswerIndex = 1;
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a compus 'Lacul lebedelor'?";
        q.answers = new string[] { "Mozart", "Tchaikovsky", "Bach", "Beethoven" };
        q.correctAnswerIndex = 1;
        q.category = "Artă";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a descoperit legea gravitației?";
        q.answers = new string[] { "Galileo", "Newton", "Einstein", "Kepler" };
        q.correctAnswerIndex = 1;
        q.category = "Știință";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este cel mai mare deșert din lume?";
        q.answers = new string[] { "Gobi", "Kalahari", "Sahara", "Antarctica" };
        q.correctAnswerIndex = 3;
        q.category = "Geografie";
        q.difficulty = "hard";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "În ce an a avut loc Revoluția Română?";
        q.answers = new string[] { "1985", "1989", "1991", "1990" };
        q.correctAnswerIndex = 1;
        q.category = "Istorie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a fost autorul romanului 'Moromeții'?";
        q.answers = new string[] { "Marin Preda", "Liviu Rebreanu", "Mihail Sadoveanu", "George Călinescu" };
        q.correctAnswerIndex = 0;
        q.category = "Artă";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este cea mai mică planetă din sistemul solar?";
        q.answers = new string[] { "Venus", "Marte", "Pluto", "Mercur" };
        q.correctAnswerIndex = 3;
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce țară a inventat sushi?";
        q.answers = new string[] { "Coreea de Sud", "China", "Japonia", "Thailanda" };
        q.correctAnswerIndex = 2;
        q.category = "Cultură generală";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Cine a compus 'Simfonia Destinului'?";
        q.answers = new string[] { "Beethoven", "Mozart", "Vivaldi", "Chopin" };
        q.correctAnswerIndex = 0;
        q.category = "Artă";
        q.difficulty = "hard";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce țară a fost prima care a trimis un om în spațiu?";
        q.answers = new string[] { "SUA", "URSS", "Germania", "China" };
        q.correctAnswerIndex = 1;
        q.category = "Istorie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Care este capitala Portugaliei?";
        q.answers = new string[] { "Lisabona", "Madrid", "Barcelona", "Porto" };
        q.correctAnswerIndex = 0;
        q.category = "Geografie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        q = new QuizQuestion();
        q.question = "Ce proces natural transformă dioxidul de carbon în oxigen?";
        q.answers = new string[] { "Respirația", "Fotosinteza", "Fermentația", "Evaporarea" };
        q.correctAnswerIndex = 1;
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);


        Debug.Log($"Total questions loaded: {quizQuestions.Count}");
    }
}
