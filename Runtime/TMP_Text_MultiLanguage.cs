﻿namespace Funbites.Localization {
    public class TMP_Text_MultiLanguage : TMP_Text_Base {
        [UnityEngine.SerializeField]
        [Sirenix.OdinInspector.Required]
        [Sirenix.OdinInspector.ValidateInput("CheckKey", "Key was not found in language manager default language")]
        private string m_key = "KEY";
        
        private static bool CheckKey(string key) {
            if (string.IsNullOrEmpty(key)) return true;
            LanguageManager lm = LanguageManager.Instance;
            if (!lm.HasLoadedLanguage) {
                if (!lm.IsLoading) {
                    lm.LoadLanguage(null, null);
                }
                return true;
            }
            return lm.ContainsKey(key);
        }


        public override void UpdateText() {
            if (text == null) text = GetComponent<TMPro.TMP_Text>();
            text.SetMultilanguageText(m_key);
        }

        public void UpdateText(string newKey)
        {
            if (!CheckKey(newKey))
                throw new System.Exception("Key not found: " + newKey);
            m_key = newKey;
            UpdateText();
        }
    }
}