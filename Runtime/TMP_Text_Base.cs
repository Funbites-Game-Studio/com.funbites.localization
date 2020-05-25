namespace Funbites.Localization {
    [UnityEngine.RequireComponent(typeof(TMPro.TMP_Text))]
    [UnityEngine.DisallowMultipleComponent]
    public abstract class TMP_Text_Base : UnityEngine.MonoBehaviour
    {
        protected TMPro.TMP_Text text;
        
        void OnEnable() {
            if (text == null) text = GetComponent<TMPro.TMP_Text>();
            LanguageManager.Instance.OnUpdateLanguage += UpdateText;
            if (LanguageManager.Instance.HasLoadedLanguage) UpdateText();
        }

        void OnDisable() {
            LanguageManager.Instance.OnUpdateLanguage -= UpdateText;
        }

        [Sirenix.OdinInspector.Button]
        public abstract void UpdateText();

        
    }
}