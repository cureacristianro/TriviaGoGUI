using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Added for Button functionality

public class QuestionButton : MonoBehaviour
{
    private Button button;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Button component attached to this GameObject
        button = GetComponent<Button>();

        // Add a listener to the button click event
        if (button != null)
        {
            button.onClick.AddListener(ButtonClicked);
        }
        else
        {
            Debug.LogError("No Button component found on this GameObject");
        }
    }

    // Function that will be called when the button is clicked
    void ButtonClicked()
    {

        // Call the MoveToNextWaypoint method in GameControl
        if (GameControl.Instance != null)
        {
            GameControl.Instance.MoveToNextWaypoint();
        }
        else
        {
            Debug.LogError("GameControl instance not found!");
        }
    }

}