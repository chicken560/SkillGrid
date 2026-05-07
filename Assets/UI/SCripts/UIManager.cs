using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject startMenuPanel;
    public GameObject settingsMenuPanel;

    // This runs automatically as soon as the game starts
    void Start()
    {
        // Force the correct UI state at launch
        startMenuPanel.SetActive(true);
        settingsMenuPanel.SetActive(false);
    }

    public void GoToSettings()
    {
        startMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
    }

    public void GoToStart()
    {
        settingsMenuPanel.SetActive(false);
        startMenuPanel.SetActive(true);
    }
}