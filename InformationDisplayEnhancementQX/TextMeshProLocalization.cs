using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    public class TextMeshProLocalization : MonoBehaviour
    {
        public string LocalizationKey { get; set; }

        private void OnEnable()
        {
            LocalizationManager.OnSetLanguage += OnSetLanguage;
            OnSetLanguage(LocalizationManager.CurrentLanguage);
        }

        private void OnDisable()
        {
            LocalizationManager.OnSetLanguage -= OnSetLanguage;
        }

        private void OnSetLanguage(SystemLanguage currentLanguage)
        {
            if (TryGetComponent<TextMeshPro>(out var textMeshPro))
            {
                textMeshPro.text = LocalizationKey?.ToPlainText();
            }
        }
    }
}
