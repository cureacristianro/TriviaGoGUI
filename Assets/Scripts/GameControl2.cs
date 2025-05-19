using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For Button functionality
using TMPro; // Added for TextMeshPro functionality
using UnityEngine.SceneManagement; // For scene reloading

public class GameControl2 : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject Player1; // Reference to the Player1 game object
    public GameObject Player2; // Reference to the Player2 game object
    public Transform[] waypoints; // Array to store all 24 waypoint transforms
    public float moveSpeed = 5f; // Speed at which the player moves between waypoints

    [Header("Quiz Settings")]
    public GameObject quizCanvas; // Canvas to display questions and answers
    public TMP_Text questionText; // TextMeshPro component to display the question
    public Button[] answerButtons; // Array of 4 buttons for answers
    public TMP_Text[] answerTexts; // Array of TextMeshPro components for answers
    public TMP_Text currentPlayerText; // Text showing which player's turn it is

    [Header("Scoring")]
    public TMP_Text player1ScoreText; // TextMeshPro component to display Player1's score
    public TMP_Text player2ScoreText; // TextMeshPro component to display Player2's score
    public int player1Score = 0; // Current score of Player1
    public int player2Score = 0; // Current score of Player2
    public int pointsForCorrectAnswer = 10; // Points awarded for a correct answer

    [Header("Answer Feedback")]
    public Color correctAnswerColor = Color.green;
    public Color wrongAnswerColor = Color.red;
    public float feedbackDelay = 1f; // Time in seconds to show feedback before moving on

    [Header("Game Completion")]
    public GameObject gameCompletionPanel; // Panel to show when game is completed
    public TMP_Text finalScoreText; // Text to show the final score
    public Button restartButton; // Button to restart the game

    [Header("Game UI")]
    public GameObject scoreCanvas; // Canvas showing the current score during gameplay
    public GameObject nextButtonCanvas; // Canvas containing the Next button

    [System.Serializable]
    public class QuizQuestion
    {
        public string question;
        public string[] answers = new string[4];
        public int correctAnswerIndex;
        public string category; // Added category field
        public string difficulty; // Added difficulty field
    }

    public List<QuizQuestion> quizQuestions = new List<QuizQuestion>();

    private int player1WaypointIndex = 0; // Current waypoint index for Player1
    private int player2WaypointIndex = 0; // Current waypoint index for Player2
    private bool isMoving = false; // Flag to check if a player is currently moving
    private int currentQuestionIndex = 0; // Current question being displayed
    private bool isPlayer1Turn = true; // Flag to track whose turn it is (true for Player1, false for Player2)

    // Singleton instance to allow access from other scripts
    public static GameControl2 Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GameControl Start method called");
        if (waypoints.Length == 0)
        {
            Debug.LogError("No waypoints assigned to the GameControl script!");
            return;
        }

        // Set initial position of players to the first waypoint
        if (Player1 != null)
        {
            Player1.transform.position = waypoints[0].position;
        }
        else
        {
            Debug.LogError("Player1 game object not assigned!");
        }

        if (Player2 != null)
        {
            Player2.transform.position = waypoints[0].position;
        }
        else
        {
            Debug.LogError("Player2 game object not assigned!");
        }

        // Make sure quiz canvas is hidden at start
        if (quizCanvas != null)
        {
            quizCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("Quiz canvas not assigned!");
        }

        // Make sure game completion panel is hidden at start
        if (gameCompletionPanel != null)
        {
            gameCompletionPanel.SetActive(false);

            // Set up the restart button
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
        }
        else
        {
            Debug.LogError("Game completion panel not assigned!");
        }

        // Initialize score display
        UpdateScoreDisplay();

        // Set initial player turn text
        UpdateCurrentPlayerText();

        // Add sample questions if none exist (you can replace with your own questions)
        if (quizQuestions.Count == 0)
        {
            AddSampleQuestions();
        }
    }

    // Update the score UI display
    private void UpdateScoreDisplay()
    {
        if (player1ScoreText != null)
        {
            player1ScoreText.text = $"Player 1: {player1Score}";
        }
        
        if (player2ScoreText != null)
        {
            player2ScoreText.text = $"Player 2: {player2Score}";
        }
    }

    // Update the text showing which player's turn it is
    private void UpdateCurrentPlayerText()
    {
        if (currentPlayerText != null)
        {
            currentPlayerText.text = isPlayer1Turn ? "Player 1's Turn" : "Player 2's Turn";
        }
    }

    // Public method to set quiz questions (called by QuestionDatabaseManager)
    public void SetQuizQuestions(List<QuizQuestion> questions)
    {
        if (questions != null && questions.Count > 0)
        {
            quizQuestions = questions;
            Debug.Log($"Quiz questions updated. Total: {quizQuestions.Count}");
        }
    }

    // Public method that will be called from QuestionButton.cs
    public void MoveToNextWaypoint()
    {
        Debug.Log("MoveToNextWaypoint called");

        // Show the quiz canvas instead of moving immediately
        ShowQuizQuestion();
    }

    // Show a question on the canvas
    private void ShowQuizQuestion()
    {
        if (quizCanvas == null || questionText == null || answerButtons.Length < 4)
        {
            Debug.LogError("Quiz UI components not properly assigned!");
            return;
        }

        // Hide the in-game UI elements while answering questions
        if (scoreCanvas != null)
        {
            scoreCanvas.SetActive(false);
        }

        if (nextButtonCanvas != null)
        {
            nextButtonCanvas.SetActive(false);
        }

        // Show the quiz canvas
        quizCanvas.SetActive(true);

        // Make sure we have questions
        if (quizQuestions.Count == 0)
        {
            questionText.text = "Error: No questions available!";
            return;
        }

        // Get the current question (cycle through available questions)
        QuizQuestion currentQuestion = quizQuestions[currentQuestionIndex % quizQuestions.Count];

        // Set the question text
        questionText.text = currentQuestion.question;

        // Set up answer buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < currentQuestion.answers.Length)
            {
                // Set answer text
                if (answerTexts != null && answerTexts.Length > i)
                {
                    // Set the text
                    answerTexts[i].text = currentQuestion.answers[i];

                    // Configure text to fit within button
                    ConfigureTextToFitButton(answerTexts[i]);
                }

                // Store the answer index for the button
                int answerIndex = i;

                // Clear previous listeners to prevent stacking
                answerButtons[i].onClick.RemoveAllListeners();

                // Add click listener
                answerButtons[i].onClick.AddListener(() => CheckAnswer(answerIndex));

                // Make sure button is active
                answerButtons[i].gameObject.SetActive(true);
            }
            else
            {
                // Hide unused buttons
                answerButtons[i].gameObject.SetActive(false);
            }
        }

        // Increment question index for next time
        currentQuestionIndex++;
    }

    // Configure text to fit inside button boundaries
    private void ConfigureTextToFitButton(TMP_Text textComponent)
    {
        if (textComponent == null)
            return;

        // Option 1: Enable text auto-sizing to fit in button
        textComponent.enableAutoSizing = true;
        textComponent.fontSizeMin = 10; // Minimum readable size
        textComponent.fontSizeMax = 24; // Maximum size (adjust as needed)

        // Option 2: Enable word wrapping
        textComponent.enableWordWrapping = true;

        // Option 3: Set overflow mode to ellipsis if text is still too long
        textComponent.overflowMode = TextOverflowModes.Ellipsis;

        // Option 4: Make sure text alignment is centered for better appearance
        textComponent.alignment = TextAlignmentOptions.Center;
    }

    // Check if the selected answer is correct
    private void CheckAnswer(int selectedAnswerIndex)
    {
        // Get the current question
        QuizQuestion currentQuestion = quizQuestions[(currentQuestionIndex - 1) % quizQuestions.Count];

        // Highlight the correct answer in green
        Image correctButtonImage = answerButtons[currentQuestion.correctAnswerIndex].GetComponent<Image>();
        if (correctButtonImage != null)
        {
            correctButtonImage.color = correctAnswerColor;
        }

        // Check if answer is correct
        if (selectedAnswerIndex == currentQuestion.correctAnswerIndex)
        {
            Debug.Log("Correct answer!");

            // Add points to the score for the current player
            if (isPlayer1Turn)
            {
                player1Score += pointsForCorrectAnswer;
            }
            else
            {
                player2Score += pointsForCorrectAnswer;
            }

            // Update the score display
            UpdateScoreDisplay();
        }
        else
        {
            Debug.Log("Wrong answer! No points awarded.");

            // Highlight the selected wrong answer in red
            Image wrongButtonImage = answerButtons[selectedAnswerIndex].GetComponent<Image>();
            if (wrongButtonImage != null)
            {
                wrongButtonImage.color = wrongAnswerColor;
            }
        }

        // Disable all buttons to prevent multiple selections
        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }

        // Wait for feedbackDelay seconds and then proceed
        StartCoroutine(ProceedAfterDelay());
    }

    // Coroutine to wait before proceeding to the next waypoint
    private IEnumerator ProceedAfterDelay()
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(feedbackDelay);

        // Reset button colors for next question
        foreach (Button button in answerButtons)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.white; // Reset to default color
                button.interactable = true; // Make buttons interactable again
            }
        }

        // Hide quiz canvas
        quizCanvas.SetActive(false);

        // Show the in-game UI elements again after answering the question
        if (scoreCanvas != null && !gameCompletionPanel.activeSelf)
        {
            scoreCanvas.SetActive(true);
        }

        if (nextButtonCanvas != null && !gameCompletionPanel.activeSelf)
        {
            nextButtonCanvas.SetActive(true);
        }

        // Move appropriate player to next waypoint
        MoveCurrentPlayerToNextWaypoint();
        
        // Switch turns
        isPlayer1Turn = !isPlayer1Turn;
        
        // Update the player turn text
        UpdateCurrentPlayerText();
    }

    // Move the current player to the next waypoint
    private void MoveCurrentPlayerToNextWaypoint()
    {
        if (isMoving)
            return;
            
        GameObject currentPlayer = isPlayer1Turn ? Player1 : Player2;
        int nextIndex;
        
        if (currentPlayer == null)
            return;
            
        // Use the appropriate waypoint index based on which player's turn it is
        if (isPlayer1Turn)
        {
            nextIndex = player1WaypointIndex + 1;
            
            // Check if we've reached the end of the waypoints
            if (nextIndex >= waypoints.Length)
            {
                Debug.Log("Player 1 reached the final waypoint! Game complete!");
                ShowGameCompletionPanel();
                return;
            }
            
            // Update the waypoint index
            player1WaypointIndex = nextIndex;
            
            // Start the movement coroutine
            StartCoroutine(MovePlayerToWaypoint(currentPlayer, waypoints[player1WaypointIndex]));
            Debug.Log("Moving Player 1 to waypoint " + player1WaypointIndex);
        }
        else
        {
            nextIndex = player2WaypointIndex + 1;
            
            // Check if we've reached the end of the waypoints
            if (nextIndex >= waypoints.Length)
            {
                Debug.Log("Player 2 reached the final waypoint! Game complete!");
                ShowGameCompletionPanel();
                return;
            }
            
            // Update the waypoint index
            player2WaypointIndex = nextIndex;
            
            // Start the movement coroutine
            StartCoroutine(MovePlayerToWaypoint(currentPlayer, waypoints[player2WaypointIndex]));
            Debug.Log("Moving Player 2 to waypoint " + player2WaypointIndex);
        }
    }

    // Coroutine to smoothly move the player to the target waypoint
    private IEnumerator MovePlayerToWaypoint(GameObject player, Transform targetWaypoint)
    {
        isMoving = true;

        Vector3 startPosition = player.transform.position;
        Vector3 targetPosition = targetWaypoint.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (player.transform.position != targetPosition)
        {
            float distCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distCovered / journeyLength;

            player.transform.position = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            if (fractionOfJourney >= 1.0f)
                break;

            yield return null;
        }

        // Ensure the player is exactly at the waypoint position
        player.transform.position = targetPosition;
        isMoving = false;
    }

    // Show the game completion panel with final scores
    private void ShowGameCompletionPanel()
    {
        if (gameCompletionPanel != null)
        {
            // Update the final score text
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Felicitări!\nPlayer 1 Score: {player1Score}\nPlayer 2 Score: {player2Score}";
                
                // Determine the winner
                if (player1Score > player2Score)
                {
                    finalScoreText.text += "\n\nPlayer 1 Wins!";
                }
                else if (player2Score > player1Score)
                {
                    finalScoreText.text += "\n\nPlayer 2 Wins!";
                }
                else
                {
                    finalScoreText.text += "\n\nIt's a tie!";
                }
            }

            // Hide the in-game UI elements
            if (scoreCanvas != null)
            {
                scoreCanvas.SetActive(false);
            }

            if (nextButtonCanvas != null)
            {
                nextButtonCanvas.SetActive(false);
            }

            // Show the panel
            gameCompletionPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Game completion panel not assigned!");
        }
    }

    // Reset the game state to start a new game
    private void ResetGameState()
    {
        // Reset scores
        ResetScores();

        // Reset waypoint indices
        player1WaypointIndex = 0;
        player2WaypointIndex = 0;

        // Reset question index
        currentQuestionIndex = 0;
        
        // Reset to Player 1's turn
        isPlayer1Turn = true;
        
        // Update player turn text
        UpdateCurrentPlayerText();

        // Reset player positions to the first waypoint
        if (Player1 != null && waypoints.Length > 0)
        {
            Player1.transform.position = waypoints[0].position;
        }
        
        if (Player2 != null && waypoints.Length > 0)
        {
            Player2.transform.position = waypoints[0].position;
        }

        // Hide the game completion panel
        if (gameCompletionPanel != null)
        {
            gameCompletionPanel.SetActive(false);
        }

        // Show the in-game UI elements again
        if (scoreCanvas != null)
        {
            scoreCanvas.SetActive(true);
        }

        if (nextButtonCanvas != null)
        {
            nextButtonCanvas.SetActive(true);
        }
    }

    public void ResetScores()
    {
        player1Score = 0;
        player2Score = 0;
        UpdateScoreDisplay();
    }

    // Restart the game
    public void RestartGame()
    {
        Debug.Log("Restarting game...");

        // Option 1: Reset the current game state
        ResetGameState();

        // Option 2: Reload the current scene (commented out as an alternative approach)
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void AddSampleQuestions()
    {
        // Clear any existing questions
        quizQuestions.Clear();

        // Question 1
        QuizQuestion q = new QuizQuestion();
        q.question = "Ce echipă a câștigat Cupa Mondială FIFA în 2018?";
        q.answers = new string[] { "Germania", "Brazilia", "Franța", "Croația" };
        q.correctAnswerIndex = 2; // Franța
        q.category = "Sport";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        // Question 2
        q = new QuizQuestion();
        q.question = "Care este cel mai mare ocean?";
        q.answers = new string[] { "Atlantic", "Indian", "Pacific", "Arctic" };
        q.correctAnswerIndex = 2; // Pacific
        q.category = "Geografie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        // Question 3
        q = new QuizQuestion();
        q.question = "În ce an a căzut Zidul Berlinului?";
        q.answers = new string[] { "1987", "1989", "1990", "1991" };
        q.correctAnswerIndex = 1; // 1989
        q.category = "Istorie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        // Question 4
        q = new QuizQuestion();
        q.question = "Care este capitala Canadei?";
        q.answers = new string[] { "Toronto", "Montreal", "Vancouver", "Ottawa" };
        q.correctAnswerIndex = 3; // Ottawa
        q.category = "Geografie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        // Question 5
        q = new QuizQuestion();
        q.question = "Ce planetă este cea mai apropiată de Soare?";
        q.answers = new string[] { "Venus", "Pământ", "Mercur", "Marte" };
        q.correctAnswerIndex = 2; // Mercur
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        // Question 6
        q = new QuizQuestion();
        q.question = "Ce țară are cel mai mare număr de locuitori?";
        q.answers = new string[] { "SUA", "India", "China", "Rusia" };
        q.correctAnswerIndex = 2; // China
        q.category = "Geografie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        // Question 7
        q = new QuizQuestion();
        q.question = "Care este capitala Egiptului?";
        q.answers = new string[] { "Alexandria", "Giza", "Luxor", "Cairo" };
        q.correctAnswerIndex = 3; // Cairo
        q.category = "Geografie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        // Question 8
        q = new QuizQuestion();
        q.question = "Care este capitala Franței?";
        q.answers = new string[] { "Madrid", "Berlin", "Paris", "Roma" };
        q.correctAnswerIndex = 2; // Paris
        q.category = "Geografie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        // Question 9
        q = new QuizQuestion();
        q.question = "Cine a fost primul președinte al SUA?";
        q.answers = new string[] { "Thomas Jefferson", "George Washington", "Abraham Lincoln", "John Adams" };
        q.correctAnswerIndex = 1; // George Washington
        q.category = "Istorie";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        // Question 10
        q = new QuizQuestion();
        q.question = "Cine a scris 'Romeo și Julieta'?";
        q.answers = new string[] { "Shakespeare", "Goethe", "Homer", "Cervantes" };
        q.correctAnswerIndex = 0; // Shakespeare
        q.category = "Artă";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        // Question 11
        q = new QuizQuestion();
        q.question = "În ce an a aderat România la NATO?";
        q.answers = new string[] { "2000", "2002", "2004", "2007" };
        q.correctAnswerIndex = 2; // 2004
        q.category = "Politică";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        // Question 12
        q = new QuizQuestion();
        q.question = "Ce compozitor a scris Simfonia a 9-a?";
        q.answers = new string[] { "Mozart", "Beethoven", "Bach", "Haydn" };
        q.correctAnswerIndex = 1; // Beethoven
        q.category = "Artă";
        q.difficulty = "hard";
        quizQuestions.Add(q);

        // Question 13
        q = new QuizQuestion();
        q.question = "Ce pictor român este celebru pentru 'Coloana Infinitului'?";
        q.answers = new string[] { "Tonitza", "Brâncuși", "Grigorescu", "Luchian" };
        q.correctAnswerIndex = 1; // Brâncuși
        q.category = "Artă";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        // Question 14
        q = new QuizQuestion();
        q.question = "În ce an a avut loc Revoluția Franceză?";
        q.answers = new string[] { "1789", "1804", "1848", "1917" };
        q.correctAnswerIndex = 0; // 1789
        q.category = "Istorie";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        // Question 15
        q = new QuizQuestion();
        q.question = "Care este simbolul chimic al Aurului?";
        q.answers = new string[] { "Ag", "Au", "Pt", "Pb" };
        q.correctAnswerIndex = 1; // Au
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        // Question 16
        q = new QuizQuestion();
        q.question = "Cine a câștigat Balonul de Aur în 2021?";
        q.answers = new string[] { "Messi", "Lewandowski", "Cristiano Ronaldo", "Mbappe" };
        q.correctAnswerIndex = 0; // Messi
        q.category = "Sport";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        // Question 17
        q = new QuizQuestion();
        q.question = "Cine a pictat 'Noaptea înstelată'?";
        q.answers = new string[] { "Van Gogh", "Monet", "Picasso", "Dali" };
        q.correctAnswerIndex = 0; // Van Gogh
        q.category = "Artă";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        // Question 18
        q = new QuizQuestion();
        q.question = "Cât este 12 x 12?";
        q.answers = new string[] { "124", "132", "144", "156" };
        q.correctAnswerIndex = 2; // 144
        q.category = "Știință";
        q.difficulty = "easy";
        quizQuestions.Add(q);

        // Question 19
        q = new QuizQuestion();
        q.question = "Ce element are simbolul chimic 'Fe'?";
        q.answers = new string[] { "Fier", "Fluor", "Fermiu", "Fosfor" };
        q.correctAnswerIndex = 0; // Fier
        q.category = "Știință";
        q.difficulty = "medium";
        quizQuestions.Add(q);

        // Question 20
        q = new QuizQuestion();
        q.question = "Cine a fost conducătorul revoluției bolșevice din 1917?";
        q.answers = new string[] { "Lenin", "Stalin", "Troțki", "Gorbaciov" };
        q.correctAnswerIndex = 0; // Lenin
        q.category = "Istorie";
        q.difficulty = "hard";
        quizQuestions.Add(q);
    }
}
