using ProjetoFinal.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoFinal.UI.Settings
{
    [RequireComponent(typeof(Slider))]
    public sealed class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private TMP_Text valueLabel;

        private Slider slider;

        private void Awake()
        {
            slider = GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.onValueChanged.AddListener(OnSliderChanged);
        }

        private void OnEnable()
        {
            slider.SetValueWithoutNotify(AudioSettingsService.Master.Value);
            UpdateLabel(slider.value);
        }

        private void OnSliderChanged(float value)
        {
            AudioSettingsService.Master.Set(value);
            UpdateLabel(value);
        }

        private void UpdateLabel(float value)
        {
            if (valueLabel != null)
                valueLabel.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }
    }
}
