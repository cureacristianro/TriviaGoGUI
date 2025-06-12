using UnityEngine;

public class SettingsMenuToggle : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject otherPanel; // e.g., Main Menu Panel

    public void ToggleSettings()
    {
        bool isActive = settingsPanel.activeSelf;

        // Toggle settings panel
        settingsPanel.SetActive(!isActive);

        // Hide or show the other panel based on settings panel visibility
        if (otherPanel != null)
        {
            otherPanel.SetActive(isActive); // Show the other panel when settings is hidden, and vice versa
        }
    }

    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
        if (otherPanel != null)
            otherPanel.SetActive(false);
    }

    public void HideSettings()
    {
        settingsPanel.SetActive(false);
        if (otherPanel != null)
            otherPanel.SetActive(true);
    }
}
