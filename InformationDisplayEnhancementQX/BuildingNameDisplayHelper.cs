using Duckov.Buildings;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    public class BuildingNameDisplayHelper : MonoBehaviour
    {
        private string _localizationKey;

        private void Awake()
        {
            var building = GetComponentInParent<Building>();
            if (building != null)
            {
                _localizationKey = building.DisplayNameKey;

                if (TryGetComponent<TextMeshPro>(out var textMeshPro))
                {
                    textMeshPro.text = building.DisplayName;
                }
            }
        }

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
                textMeshPro.text = _localizationKey?.ToPlainText();
            }
        }
    }
}
