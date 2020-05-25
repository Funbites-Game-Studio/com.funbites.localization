using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Funbites.Localization {
    
    public class TMP_Text_MultiLanguage : TMP_Text_Base {
        [SerializeField, Required, ValidateInput("CheckKey", "Key was not found in language manager default language")]
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
            text.SetMultilanguageText(m_key);
        }
    }
}