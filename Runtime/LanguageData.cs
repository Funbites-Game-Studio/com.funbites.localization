namespace Funbites.Localization {
    [UnityEngine.CreateAssetMenu(menuName = "Funbites/Localization/Language Data")]
    public class LanguageData : UnityEngine.ScriptableObject, UnityEngine.ISerializationCallbackReceiver
    {
        
        [UnityEngine.SerializeField, Sirenix.OdinInspector.ReadOnly]
        private string[] _data = null;

        internal System.Collections.Generic.Dictionary<string, string> GetData() {
            int numberOfElements = _data.Length / 2;
            var result = new System.Collections.Generic.Dictionary<string, string>(numberOfElements);
            for (int i = 0; i < numberOfElements; i++) {
                result.Add(_data[i * 2], _data[i * 2 + 1]);
            }
            return result;
        }
#if UNITY_EDITOR
        [Sirenix.OdinInspector.ShowInInspector] 
        [Sirenix.OdinInspector.InfoBox("Don't edit while the game is playing", 
            InfoMessageType = Sirenix.OdinInspector.InfoMessageType.Error, VisibleIf = "IsApplicationPlaying")]
        private System.Collections.Generic.SortedDictionary<string, string> dataDic;
        private bool IsApplicationPlaying => UnityEngine.Application.isPlaying;

        [Sirenix.OdinInspector.Button(Style = Sirenix.OdinInspector.ButtonStyle.CompactBox, Expanded = true)]
        private void Synchronize(LanguageData otherLanguage)
        {
            if (otherLanguage == null)
            {
                UnityEditor.EditorUtility.DisplayDialog("Synchronization - Error", "Please, select a file", "Ok");
                return;
            }
            if (otherLanguage == this)
            {
                UnityEditor.EditorUtility.DisplayDialog("Synchronization - Error", "You are trying to synchronize with the same file.", "Ok");
                return;
            }
            UnityEditor.AssetDatabase.SaveAssets();
            var removedKeys = GetKeys();
            var addedKeys = otherLanguage.GetKeys();
            var otherKeys = otherLanguage.GetKeys();
            addedKeys.ExceptWith(removedKeys);
            removedKeys.ExceptWith(otherKeys);
            bool hasRemovedKeys = removedKeys.Count > 0;
            bool hasAddedKeys = addedKeys.Count > 0;
            if (!hasRemovedKeys && !hasAddedKeys)
            {
                UnityEditor.EditorUtility.DisplayDialog("Synchronization", "File is syncronized", "Ok");
                return;
            }

            if (hasRemovedKeys)
            {


                if (UnityEditor.EditorUtility.DisplayDialog("Synchronization - Removing",
                    BuildSynchonizationMessage($"The following {removedKeys.Count.ToString()} key(s) were not present in the other language {otherLanguage.name} but it's in this language {name}:",
                    removedKeys, "Do you want to remove from this file?"), "Yes", "No"))
                {
                    foreach (string key in removedKeys)
                    {
                        dataDic.Remove(key);
                    }
                }
            }
            if (hasAddedKeys)
            {
                if (UnityEditor.EditorUtility.DisplayDialog("Synchronization - Adding",
                    BuildSynchonizationMessage($"The following {addedKeys.Count.ToString()} key(s) were present in the other language {otherLanguage.name} but  it's not in this language {name}:",
                    addedKeys, "Do you want to add from this file?"), "Yes", "No"))
                {
                    foreach (string key in addedKeys)
                    {
                        dataDic.Add(key, otherLanguage.dataDic[key]);
                    }
                }
            }
            UnityEditor.AssetDatabase.SaveAssets();
        }

        private const int maxCharPerLine = 200;
        private string BuildSynchonizationMessage(string statement, System.Collections.Generic.HashSet<string> elements, string question)
        {
            var message = new System.Text.StringBuilder();
            message.Append(statement);
            message.Append("\n");
            int charCountPerLine = 0;
            foreach (string key in elements)
            {
                message.Append(key);
                charCountPerLine += key.Length + 1;
                if (charCountPerLine < maxCharPerLine)
                {
                    message.Append(";");
                } else
                {
                    message.Append(";\n");
                    charCountPerLine = 0;
                }
            }
            message.Append(question);
            message.Append("\n");
            return message.ToString();
        }

        private static readonly char[] commaSeparetor = new char[] { ',' };
        [Sirenix.OdinInspector.Button]
        private void AddKeysInBatchWithCommaSeparetor(string keys, string prefixes = "", string suffixes = "")
        {
            AddKeysInBatchWithSeparetor(keys, prefixes, suffixes, commaSeparetor);
        }

        [Sirenix.OdinInspector.Button]
        private void AddKeysInBatchWithSeparetor(string keys, string prefixes, string suffixes, char[] separetor)
        {
            string[] keysArray = keys.Split(separetor);
            string[] prefixesArray = prefixes.Split(separetor);
            string[] suffixesArray = suffixes.Split(separetor);
            var sb = new System.Text.StringBuilder();
            string newKey, curKey;
            foreach (var key in keysArray)
            {
                sb.Clear();
                curKey = key.Trim();
                foreach (var prefix in prefixesArray)
                {
                    sb.Append(prefix.Trim());
                    sb.Append(key);
                    foreach (var suffix in suffixesArray)
                    {
                        sb.Append(suffix.Trim());
                        newKey = System.Text.RegularExpressions.Regex.Replace(sb.ToString().ToUpperInvariant(), allSpacesPattern, "");
                        if (dataDic.ContainsKey(newKey))
                        {
                            UnityEngine.Debug.LogError($"Key {newKey} is already on language data");
                        } else
                        {
                            dataDic.Add(newKey, newKey);
                        }
                        sb.Remove(sb.Length - suffix.Length, suffix.Length);
                    }
                    sb.Clear();
                }
            }
        }

        private System.Collections.Generic.HashSet<string> GetKeys() {
            var result = new System.Collections.Generic.HashSet<string>();
            for (int i = 0; i < _data.Length; i += 2) {
                result.Add(_data[i]);
            }
            return result;
        }

#endif
        public void OnAfterDeserialize() {
#if UNITY_EDITOR
            int numberOfElements = _data.Length / 2;
            dataDic = new System.Collections.Generic.SortedDictionary<string, string>();
            int k = 0;
            string key;
            string value;
            for (int i = 0; i < numberOfElements; i++) {
                key = _data[k];
                k++;
                value = _data[k];
                k++;
                dataDic.Add(key, value);
            }
#endif
        }
        private const string allSpacesPattern = @"\s+";
        public void OnBeforeSerialize() {
#if UNITY_EDITOR
            if (dataDic == null) {
                _data = new string[0];
                dataDic = new System.Collections.Generic.SortedDictionary<string, string>();
                return;
            }
            _data = new string[dataDic.Count * 2];
            int i = 0;
            foreach (var datum in dataDic) {
                _data[i++] = System.Text.RegularExpressions.Regex.Replace(datum.Key.ToUpperInvariant(), allSpacesPattern, "");
                _data[i++] = datum.Value;
            }
#endif
        }

        
    }
}