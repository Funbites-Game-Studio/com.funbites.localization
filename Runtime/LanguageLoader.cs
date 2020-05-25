using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Funbites.Localization {
    [CreateAssetMenu(menuName ="Localization/Language Loader")]
    public class LanguageLoader : ScriptableObject {
        [Serializable]
        public class LanguageDataDefinition {
            public string Language => m_language;
            public AssetReferenceLanguageData DataAssetReference => m_dataAssetReference;
            [SerializeField, ValueDropdown("GetAllCulturesNames"), HideLabel]
            private string m_language = DefaultLanguage;
            [SerializeField, Required]
            private AssetReferenceLanguageData m_dataAssetReference = null;
            private static IEnumerable GetAllCulturesNames() {
                var result = new ValueDropdownList<string>();
                var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                foreach (var culture in allCultures) {
                    result.Add($"{culture.DisplayName}->'{culture.Name}'", culture.Name);
                }
                return result;
            }
        }
        public const string DefaultLanguage = "en";
        [InfoBox("The first language is the default one and it always set when the requested language was not found.")]
        [SerializeField]
        private LanguageDataDefinition[] m_availableLanguages = null;
        
        public AssetReferenceLanguageData FindAssetReferenceForLanguage(string language) {
            if (m_availableLanguages.Length == 0)
                throw new Exception("No language file was registred.");
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
                Debug.LogError($"Requested language was not found:{language}, using the default: {foundLanguageDef.Language}");
            }
            return foundLanguageDef.DataAssetReference;
        }
    }
}