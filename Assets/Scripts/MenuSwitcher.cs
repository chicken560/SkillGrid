using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject AnyMenu;
    public GameObject pasusemenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void GoToSettings()
    {
        pasusemenu.SetActive(false);
        AnyMenu.SetActive(true);
    }
}
