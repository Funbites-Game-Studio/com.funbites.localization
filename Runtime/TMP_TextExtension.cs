using TMPro;
using UnityEngine;

namespace Funbites.Localization {
    public static class TMP_TextExtension {
        public static void SetMultilanguageText(this TMP_Text text, string key) {
            string value = GetValue(text, key);
            if (value != null) {
                text.text = value;
            }
        }

        public static void SetMultilanguageText(this TMP_Text text, string key, string argument) {
            string value = GetValue(text, key);
            if (value != null) {
                text.text = string.Format(value, argument);
            }
        }

        public static void SetMultilanguageText(this TMP_Text text, string key, params object[] args) {
            string value = GetValue(text, key);
            if (value != null) {
                text.text = string.Format(value, args);
            }
        }

        private static string GetValue(TMP_Text text, string key) {
            string value;
            if (LanguageManager.Instance.TryToGetValue(key, out value)) {
                return value;
            } else {
                Debug.LogError($"Key {key} was not found in Language manager while trying to load on {text.name}", text);
                return null;
            }
        }
    }
}