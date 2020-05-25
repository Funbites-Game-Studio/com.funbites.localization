namespace Funbites.Localization {
    [UnityEngine.CreateAssetMenu(menuName ="Localization/Language Loader")]
    public class LanguageLoader : UnityEngine.ScriptableObject
    {
        public const string DefaultLanguage = "en";
#if UNITY_EDITOR
        private static Sirenix.OdinInspector.ValueDropdownList<string> allCulturesCache = null;

        private static System.Collections.IEnumerable GetAllCulturesNames()
        {
            if (allCulturesCache == null)
            {
                allCulturesCache = new Sirenix.OdinInspector.ValueDropdownList<string>();
                var allCultures = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
                foreach (var culture in allCultures)
                {
                    allCulturesCache.Add($"{culture.DisplayName}->'{culture.Name}'", culture.Name);
                }
            }
            return allCulturesCache;
        }
#endif
        [System.Serializable]
        public class LanguageDataDefinition {
            public string Language => m_language;
            public AssetReferenceLanguageData DataAssetReference => m_dataAssetReference;
            [UnityEngine.SerializeField]
            [Sirenix.OdinInspector.ValueDropdown("@Funbites.Localization.LanguageLoader.GetAllCulturesNames()")]
            [Sirenix.OdinInspector.HideLabel]
            private string m_language = DefaultLanguage;
            [UnityEngine.SerializeField, Sirenix.OdinInspector.Required]
            private AssetReferenceLanguageData m_dataAssetReference = null;
            
        }
        
        [Sirenix.OdinInspector.InfoBox("The first language is the default one and it always set when the requested language was not found.")]
        [UnityEngine.SerializeField]
        private LanguageDataDefinition[] m_availableLanguages = null;
        
        public AssetReferenceLanguageData FindAssetReferenceForLanguage(string language) {
            if (m_availableLanguages.Length == 0)
                throw new System.Exception("No language file was registred.");
            bool foundLanguage = false;
            LanguageDataDefinition foundLanguageDef = m_availableLanguages[0];
            if (!string.IsNullOrEmpty(language) && language.Length >= 2) {
                foreach (var languageDef in m_availableLanguages) {
                    if (languageDef.Language == language) {
                        foundLanguageDef = languageDef;
                        foundLanguage = true;
                        break;
                    }
                }
                if (!foundLanguage && language.Length > 2) {
                    string shortLanguage = language.Substring(0, 2);
                    foreach (var languageDef in m_availableLanguages) {
                        if (languageDef.Language == shortLanguage) {
                            foundLanguageDef = languageDef;
                            foundLanguage = true;
                            break;
                        }
                    }
                }
            }
            if (!foundLanguage) {
                UnityEngine.Debug.LogError($"Requested language was not found:{language}, using the default: {foundLanguageDef.Language}");
            }
            return foundLanguageDef.DataAssetReference;
        }
    }
}