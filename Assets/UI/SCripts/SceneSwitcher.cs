using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for UI elements
using System.Collections; // Required for Coroutines

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Settings")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1.0f;

    // This is the method your Button will call
    public void LoadScene(string sceneName)
    {
        // Instead of loading immediately, we start the Fade process
        StartCoroutine(FadeAndExit(sceneName));
    }

    IEnumerator FadeAndExit(string sceneName)
    {
        float timer = 0;

        // Ensure the fade overlay blocks clicks so user can't spam the button
        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true;

            // Loop until the timer reaches the duration
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                // Gradually increase alpha from 0 to 1
                fadeGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
                yield return null; // Wait for the next frame
            }
        }

        // Now that the screen is dark, load the scene
        SceneManager.LoadScene(sceneName);
    }
}