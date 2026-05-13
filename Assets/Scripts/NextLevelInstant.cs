using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class NextLevelInstant : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Optional: type the exact scene name to load. If empty the next build index (active + 1) will be used.")]
    public string nextSceneName = "";

    [Header("Behavior")]
    [Tooltip("If true the trigger only works once.")]
    public bool oneShot = true;

    private bool _activated;

    void Reset()
    {
        // Ensure there's a trigger collider so the component works out-of-the-box.
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

        // Detect player by common methods: PlayerController component, "Player" tag or name "player"
        bool isPlayer = other.GetComponent<PlayerController>() != null ||
                        other.CompareTag("Player") ||
                        other.name.ToLower() == "player";

        if (!isPlayer) return;

        // Mark activated if oneShot to avoid re-entry
        if (oneShot) _activated = true;

        LoadNext();
    }

    // Public API for scripts to trigger the load programmatically
    public void TriggerLoad()
    {
        if (_activated && oneShot) return;
        if (oneShot) _activated = true;
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
                Debug.LogWarning($"NextLevelInstant: Scene '{nextSceneName}' is not in Build Settings or cannot be loaded.");
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
            Debug.LogWarning("NextLevelInstant: No next scene in Build Settings (already at last index).");
        }
    }
}