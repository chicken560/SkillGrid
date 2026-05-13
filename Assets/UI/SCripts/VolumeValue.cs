using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeValue : MonoBehaviour
{
    public static VolumeValue Instance; // Singleton instance

    [Header("UI References")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;

    private void Awake()
    {
        // Ensure only one instance of this object exists across all scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Don't delete this when loading new scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Load saved volume (default to 0.5f if no save exists)
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }

        ApplyVolume(savedVolume);

        // Add listener for UI interaction
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    private void OnSliderChanged(float value)
    {
        ApplyVolume(value);

        // Save the volume so it's remembered next time the game opens
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    private void ApplyVolume(float value)
    {
        AudioListener.volume = value;

        if (volumeText != null)
        {
            volumeText.text = "Volume: " + Mathf.Round(value * 1) + "%";
        }
    }
}