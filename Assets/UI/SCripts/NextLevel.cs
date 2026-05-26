using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private int sceneBuildIndex = -1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GoToSceneByName();
        }
    }

    // Call from a Button OnClick() (no args) -> choose one of the two inspector-configured methods
    public void GoToSceneByName()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("NextLevel: sceneName is empty.");
        }
    }

    public void GoToSceneByIndex()
    {
        if (sceneBuildIndex >= 0)
        {
            SceneManager.LoadScene(sceneBuildIndex);
        }
        else
        {
            Debug.LogWarning("NextLevel: sceneBuildIndex not set.");
        }
    }

    // Alternative: call this from a Button OnClick(string) and pass the scene name directly
    public void GoToScene(string name)
    {
        if (!string.IsNullOrEmpty(name))
            SceneManager.LoadScene(name);
        else
            Debug.LogWarning("NextLevel.GoToScene called with empty name.");
    }
}
