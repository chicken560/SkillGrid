using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class nextlevel : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Optional: type the exact scene name to load. If empty the next build index (active + 1) will be used.")]
    public string nextSceneName = "";

    [Header("Transport")]
    [Tooltip("If true the trigger will only activate once.")]
    public bool oneShot = true;

    [Tooltip("Optional delay (seconds) before loading the scene after activation.")]
    public float loadDelay = 0.25f;

    private bool _activated;

    void Reset()
    {
        // Ensure there's a trigger collider so this works from the inspector without extra setup.
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            var sc = gameObject.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.5f;
        }
        else
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (_activated && oneShot) return;
        if (other == null) return;

        // Basic player detection: PlayerController component OR tag "Player" OR name "player"
        bool isPlayer = other.GetComponent<PlayerController>() != null ||
                        other.CompareTag("Player") ||
                        other.name.ToLower() == "player";

        if (!isPlayer) return;

        if (oneShot) _activated = true;
        if (loadDelay > 0f)
            StartCoroutine(DelayedLoad());
        else
            LoadNext();
    }

    private IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(loadDelay);
        LoadNext();
    }

    private void LoadNext()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning($"nextlevel: Scene '{nextSceneName}' is not in Build Settings or cannot be loaded.");
            }
            return;
        }

        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogWarning("nextlevel: No next scene in Build Settings (already at last index).");
        }
    }

    // Public API: allow other scripts (tentacles) to trigger loading programmatically.
    public void TriggerLoad()
    {
        if (_activated && oneShot) return;
        _activated = true;
        if (loadDelay > 0f)
            StartCoroutine(DelayedLoad());
        else
            LoadNext();
    }
}