namespace Funbites.Localization {
    public class LanguageManager {
        #region Singleton
        private static LanguageManager _instance;
        private static bool _hasInstance = false;

        public static LanguageManager Instance {
            get {
                if (!_hasInstance) {
                    _hasInstance = true;
                    _instance = new LanguageManager();
                }
                return _instance;
            }
        }
        #endregion


        private LanguageLoader languageLoader = null;
        public bool IsLoading { get; private set; } = false;
        public bool HasLoadedLanguage { get; private set; } = false;
        public string LoadedLanguage { get; private set; } = null;
        public System.Action OnUpdateLanguage { get; internal set; } = null;

        private System.Collections.Generic.Dictionary<string, string> loadedData;
        private LanguageManager() {
            loadedData = null;
            HasLoadedLanguage = false;
            IsLoading = false;
            languageLoader = null;
        }

        public string FindDeviceCultureName()
        {
            string result = System.Globalization.CultureInfo.CurrentCulture.Name;
#if UNITY_EDITOR

#elif UNITY_ANDROID
            using (UnityEngine.AndroidJavaClass cls = new UnityEngine.AndroidJavaClass("java.util.Locale")) {
                using (UnityEngine.AndroidJavaObject locale = cls.CallStatic<UnityEngine.AndroidJavaObject>("getDefault")) {
                    result = locale.Call<string>("getLanguage")+"-"+locale.Call<string>("getCountry");
                }
            }
#elif UNITY_IOS
            
#else
            
#endif
            return result;
        }

        private const string LanguageManagerLoaderAddress = "_LANGUAGE_MANAGER_LOADER";

        public void LoadLanguage(UnityEngine.MonoBehaviour caller, string language) {
#if UNITY_EDITOR
            if (UnityEngine.Application.isEditor && !UnityEngine.Application.isPlaying) {
                IsLoading = true;
                try {
                    var aaSettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.GetSettings(true);
                    UnityEditor.AddressableAssets.Settings.AddressableAssetEntry foundEntry = null;
                    var assetsList = new System.Collections.Generic.List<UnityEditor.AddressableAssets.Settings.AddressableAssetEntry>(2048);
                    aaSettings.GetAllAssets(assetsList, false);
                    foreach (var entry in assetsList) {
                        if (entry.address == LanguageManagerLoaderAddress) {
                            foundEntry = entry;
                            break;
                        }
                    }
                    languageLoader = UnityEditor.AssetDatabase.LoadAssetAtPath<LanguageLoader>(UnityEditor.AssetDatabase.GUIDToAssetPath(foundEntry.guid));
                    var languageAssetReference = languageLoader.FindAssetReferenceForLanguage(language);
                    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(languageAssetReference.RuntimeKey.ToString());
                    var type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(path);
                    var loadedLanguageData = UnityEditor.AssetDatabase.LoadAssetAtPath<LanguageData>(path);
                    loadedData = loadedLanguageData.GetData();
                    HasLoadedLanguage = true;
                    LoadedLanguage = language;
                } catch (System.Exception e) {
                    IsLoading = false;
                    throw e;
                }
            }
#endif
            if (UnityEngine.Application.isPlaying)
                caller.StartCoroutine(LoadLanguageCoroutine(language));
        }

        private System.Collections.IEnumerator LoadLanguageCoroutine(string language) {
            IsLoading = true;
            HasLoadedLanguage = false;
            if (languageLoader == null) {
                var languageLoaderAsyncOperation = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<LanguageLoader>(LanguageManagerLoaderAddress);
                while (!languageLoaderAsyncOperation.IsDone) yield return null;
                languageLoader = languageLoaderAsyncOperation.Result;
                UnityEngine.AddressableAssets.Addressables.Release(languageLoaderAsyncOperation);
            }
            if (languageLoader != null) {
                var languageAssetReference = languageLoader.FindAssetReferenceForLanguage(language);
                var languageLoading = languageAssetReference.LoadAssetAsync();
                while (!languageLoading.IsDone) yield return null;
                loadedData = languageLoading.Result.GetData();
                UnityEngine.AddressableAssets.Addressables.Release(languageLoading);
                HasLoadedLanguage = true;
                LoadedLanguage = language;
                IsLoading = false;
                OnUpdateLanguage?.Invoke();
            }
            IsLoading = false;
        }


        public bool ContainsKey(string key) {
            return loadedData.ContainsKey(key);
        }

        public bool TryToGetValue(string key, out string value) {
            if (!HasLoadedLanguage) {
                value = null;
                return false;
            }
            return loadedData.TryGetValue(key, out value);
        }

    }
}