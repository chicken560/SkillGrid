using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ClickToFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 1.0f;
    public string nextSceneName;

    // This is called when you click your button
    public void StartTransition()
    {
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        float time = 0;

        // Fades the image from 1 (solid) to 0 (invisible)
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, time / fadeDuration);
            yield return null;
        }

        // Once the image is invisible, the new scene loads
        SceneManager.LoadScene(nextSceneName);
    }
}
