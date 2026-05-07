using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Fade Settings")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1.0f;
    public bool fadeOnStart = true;

    private void Awake()
    {
        // Ensure the fade layer is on top of everything else at the start
        if (fadeGroup != null)
        {
            fadeGroup.gameObject.SetActive(true);
        }
    }

    void Start()
    {
        if (fadeGroup == null) return;

        if (fadeOnStart)
        {
            StartCoroutine(Fade(1f, 0f)); // Fade from Black to Clear
        }
        else
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }
    }

    public void LoadScene(string sceneName)
    {
        StopAllCoroutines(); // Prevents overlapping fades
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        // 1. Start loading the scene in the background immediately
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        loadOp.allowSceneActivation = false; // Prevents the scene from switching yet

        // 2. Perform the Fade Out (Clear to Black)
        yield return StartCoroutine(Fade(0f, 1f));

        // 3. Optional: Small buffer to ensure the screen is fully black 
        // before the "blink" of a new scene loading occurs.
        yield return new WaitForSeconds(0.2f);

        // 4. Activate the new scene
        loadOp.allowSceneActivation = true;
    }

    // A single reusable method for both Fading In and Out
    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        fadeGroup.blocksRaycasts = true; // Stop user clicks during transition
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime; // Use unscaled so it works even if game is paused
            fadeGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = endAlpha;

        // Only stop blocking raycasts if we are now transparent
        fadeGroup.blocksRaycasts = (endAlpha == 1f);
    }
}