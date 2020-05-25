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
        private void Synchronize(LanguageData otherLanguage) {
            if (otherLanguage == null) {
                UnityEditor.EditorUtility.DisplayDialog("Synchronization - Error", "Please, select a file", "Ok");
                return;
            }
            if (otherLanguage == this) {
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
            if (!hasRemovedKeys && !hasAddedKeys) {
                UnityEditor.EditorUtility.DisplayDialog("Synchronization", "File is syncronized", "Ok");
                return;
            }
            
            if (hasRemovedKeys) {
                var message = new System.Text.StringBuilder($"The following {removedKeys.Count} key(s) were not present in the other language {otherLanguage.name} but it's in this language {name}:\n");
                foreach (string key in removedKeys) {
                    message.Append(key);
                    message.Append("\n");
                }
                message.Append("Do you want to remove from this file?");
                if (UnityEditor.EditorUtility.DisplayDialog("Synchronization - Removing", message.ToString(), "Yes", "No")) {
                    foreach (string key in removedKeys) {
                        dataDic.Remove(key);
                    }
                }
            }
            if (hasAddedKeys) {
                var message = new System.Text.StringBuilder($"The following {addedKeys.Count} key(s) were present in the other language {otherLanguage.name} but  it's not in this language {name}:\n");
                foreach (string key in addedKeys) {
                    message.Append(key);
                    message.Append("\n");
                }
                message.Append("Do you want to add from this file?");
                if (UnityEditor.EditorUtility.DisplayDialog("Synchronization - Adding", message.ToString(), "Yes", "No")) {
                    foreach (string key in addedKeys) {
                        dataDic.Add(key, otherLanguage.dataDic[key]);
                    }
                }
            }
            UnityEditor.AssetDatabase.SaveAssets();
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
                _data[i++] = System.Text.RegularExpressions.Regex.Replace(datum.Key.ToUpperInvariant(), @"s", "");
                _data[i++] = datum.Value;
            }
#endif
        }

        
    }
}