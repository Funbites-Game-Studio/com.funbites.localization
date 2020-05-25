using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
#endif
namespace Funbites.Localization {
    [CreateAssetMenu(menuName = "Localization/Language Data")]
    public class LanguageData : ScriptableObject, ISerializationCallbackReceiver {
        
        [SerializeField, ReadOnly]
        private string[] _data = null;

        internal Dictionary<string, string> GetData() {
            int numberOfElements = _data.Length / 2;
            Dictionary<string, string> result = new Dictionary<string, string>(numberOfElements);
            for (int i = 0; i < numberOfElements; i++) {
                result.Add(_data[i * 2], _data[i * 2 + 1]);
            }
            return result;
        }
#if UNITY_EDITOR
        [ShowInInspector, InfoBox("Don't edit while the game is playing", InfoMessageType =InfoMessageType.Error, VisibleIf = "IsApplicationPlaying")]
        private SortedDictionary<string, string> dataDic;
        private bool IsApplicationPlaying => Application.isPlaying;

        [Button(Style = ButtonStyle.CompactBox, Expanded = true)]
        private void Synchronize(LanguageData otherLanguage) {
            if (otherLanguage == null) {
                EditorUtility.DisplayDialog("Synchronization - Error", "Please, select a file", "Ok");
                return;
            }
            if (otherLanguage == this) {
                EditorUtility.DisplayDialog("Synchronization - Error", "You are trying to synchronize with the same file.", "Ok");
                return;
            }
            AssetDatabase.SaveAssets();
            HashSet<string> removedKeys = GetKeys();
            HashSet<string> addedKeys = otherLanguage.GetKeys();
            HashSet<string> otherKeys = otherLanguage.GetKeys();
            addedKeys.ExceptWith(removedKeys);
            removedKeys.ExceptWith(otherKeys);
            bool hasRemovedKeys = removedKeys.Count > 0;
            bool hasAddedKeys = addedKeys.Count > 0;
            if (!hasRemovedKeys && !hasAddedKeys) {
                EditorUtility.DisplayDialog("Synchronization", "File is syncronized", "Ok");
                return;
            }
            
            if (hasRemovedKeys) {
                StringBuilder message = new StringBuilder($"The following {removedKeys.Count} key(s) were not present in the other language {otherLanguage.name} but it's in this language {name}:\n");
                foreach (string key in removedKeys) {
                    message.Append(key);
                    message.Append("\n");
                }
                message.Append("Do you want to remove from this file?");
                if (EditorUtility.DisplayDialog("Synchronization - Removing", message.ToString(), "Yes", "No")) {
                    foreach (string key in removedKeys) {
                        dataDic.Remove(key);
                    }
                }
            }
            if (hasAddedKeys) {
                StringBuilder message = new StringBuilder($"The following {addedKeys.Count} key(s) were present in the other language {otherLanguage.name} but  it's not in this language {name}:\n");
                foreach (string key in addedKeys) {
                    message.Append(key);
                    message.Append("\n");
                }
                message.Append("Do you want to add from this file?");
                if (EditorUtility.DisplayDialog("Synchronization - Adding", message.ToString(), "Yes", "No")) {
                    foreach (string key in addedKeys) {
                        dataDic.Add(key, otherLanguage.dataDic[key]);
                    }
                }
            }
            AssetDatabase.SaveAssets();
        }

        private HashSet<string> GetKeys() {
            HashSet<string> result = new HashSet<string>();
            for (int i = 0; i < _data.Length; i += 2) {
                result.Add(_data[i]);
            }
            return result;
        }

#endif
        public void OnAfterDeserialize() {
#if UNITY_EDITOR
            int numberOfElements = _data.Length / 2;
            dataDic = new SortedDictionary<string, string>();
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
                dataDic = new SortedDictionary<string, string>();
                return;
            }
            _data = new string[dataDic.Count * 2];
            int i = 0;
            foreach (var datum in dataDic) {
                _data[i++] = Regex.Replace(datum.Key.ToUpperInvariant(), @"s", "");
                _data[i++] = datum.Value;
            }
#endif
        }

        
    }
}