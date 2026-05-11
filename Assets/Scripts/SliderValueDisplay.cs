using UnityEngine;
using UnityEngine.UI; // Required for the Slider
using TMPro; // Required for TextMeshPro

public class SliderValueDisplay : MonoBehaviour
{
    [SerializeField] private Slider mySlider;
    [SerializeField] private TextMeshProUGUI valueText;

    void Start()
    {
        // Update the text immediately on start
        UpdateText(mySlider.value);

        // Add a listener that triggers whenever the slider moves
        mySlider.onValueChanged.AddListener((val) => {
            UpdateText(val);
        });
    }

    void UpdateText(float value)
    {
        // "f0" removes decimals. Use "f2" if you want two decimal places.
        valueText.text = value.ToString("f0");
    }
}