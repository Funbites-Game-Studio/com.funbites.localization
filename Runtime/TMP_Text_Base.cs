using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Funbites.Localization {
    [RequireComponent(typeof(TMP_Text))]
    public abstract class TMP_Text_Base : MonoBehaviour {
        protected TMP_Text text;
        
        void OnEnable() {
            if (text == null) text = GetComponent<TMP_Text>();
            LanguageManager.Instance.OnUpdateLanguage += UpdateText;
            if (LanguageManager.Instance.HasLoadedLanguage) UpdateText();
        }

        // Update is called once per frame
        void OnDisable() {
            LanguageManager.Instance.OnUpdateLanguage -= UpdateText;
        }
        [Button]
        public abstract void UpdateText();

        
    }
}