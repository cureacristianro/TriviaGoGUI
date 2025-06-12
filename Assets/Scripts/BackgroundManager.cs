using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public GameObject[] backgrounds;        // All backgrounds (Main, 1, 2, 3)
    public GameObject canvasToHide;         // Canvas to hide (e.g., menu panel)
    public GameObject canvasToShow;         // Canvas to show (e.g., background tools/options)

    public void ActivateBackground(int index)
    {
        // Activate the selected background and deactivate others
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] != null)
                backgrounds[i].SetActive(i == index);
        }

        // Hide the specified canvas
        if (canvasToHide != null)
            canvasToHide.SetActive(false);

        // Show the specified canvas
        if (canvasToShow != null)
            canvasToShow.SetActive(true);
    }
}
