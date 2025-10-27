using Duckov.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    [HarmonyPatch]
    public class TextHelper
    {
        private static GameObject textTemplate;

        private static ModBehaviour modBehaviour;

        public static void Initialize(ModBehaviour behaviour)
        {
            modBehaviour = behaviour;
        }

        public static void Release()
        {
            GameObject.Destroy(textTemplate);
            textTemplate = null;

            modBehaviour = null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthBarManager), "Awake")]
        public static void CreateTextTemplate()
        {
            if (textTemplate) return;

            textTemplate = GameObject.Instantiate(HealthBarManager.Instance.healthBarPrefab.transform.Find("Horizontal").gameObject, modBehaviour.transform);
            textTemplate.name = "TextTemplate";
            GameObject.Destroy(textTemplate.transform.Find("Image").gameObject);
            GameObject text = textTemplate.transform.Find("NameText").gameObject;
            text.name = "Text";
            text.SetActive(true);
            textTemplate.SetActive(false);
        }

        public static GameObject GetText(TextConfigure configure)
        {
            GameObject newText = GameObject.Instantiate(textTemplate);
            if (configure.parent != null) newText.transform.SetParent(configure.parent);
            if (configure.localPosition.HasValue) newText.transform.localPosition = configure.localPosition.Value;
            if (configure.localRotation.HasValue) newText.transform.localEulerAngles = configure.localRotation.Value;
            if (configure.localScale.HasValue) newText.transform.localScale = configure.localScale.Value;
            if (!string.IsNullOrWhiteSpace(configure.textTemplateName)) newText.name = configure.textTemplateName;
            GameObject text = newText.transform.Find("Text").gameObject;
            if (!string.IsNullOrWhiteSpace(configure.textName)) text.name = configure.textName;
            text.GetComponent<TextMeshProUGUI>().text = string.IsNullOrWhiteSpace(configure.initText) ? string.Empty : configure.initText;
            newText.SetActive(configure.active);
            return newText;
        }

        public struct TextConfigure
        {
            public bool active;

            public Transform parent;

            public Vector3? localPosition;

            public Vector3? localRotation;

            public Vector3? localScale;

            public string textTemplateName;

            public string textName;

            public string initText;
        }
    }
}
