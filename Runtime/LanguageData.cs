namespace Funbites.Localization {
    using Sirenix.OdinInspector;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    
    [CreateAssetMenu(menuName = "Funbites/Localization/Language Data")]
    public class LanguageData : ScriptableObject, ISerializationCallbackReceiver
    {


        internal Dictionary<string, string> GetData() {
            //int numberOfElements = _data.Length / 2;
            int numberOfElements = _data.Count / 2;
            var result = new Dictionary<string, string>(numberOfElements);
            for (int i = 0; i < numberOfElements; i++) {
                result.Add(_data[i * 2], _data[i * 2 + 1]);
            }
            return result;
        }
#if UNITY_EDITOR
        /*[ShowInInspector]
        [InfoBox("Don't edit while the game is playing", 
            InfoMessageType = InfoMessageType.Error, VisibleIf = "IsApplicationPlaying")]
        private System.Collections.Generic.SortedDictionary<string, string> dataDic;*/

        [OnValueChanged("UpdateFilter"), ShowInInspector]
        private string m_filter;

        [ShowInInspector,DictionaryDrawerSettings(IsReadOnly =true)]
        private SortedDictionary<string, LocalizedStringValue> filteredDataDic = new System.Collections.Generic.SortedDictionary<string, LocalizedStringValue>();
        


        [System.Serializable]
        public struct LocalizedStringValue
        {
            public string Value;
            public LocalizedStringValueOperation Operation;
            public LocalizedStringValue(string value)
            {
                Value = value;
                Operation = LocalizedStringValueOperation.None;
            }
        }

        public enum LocalizedStringValueOperation
        {
            None,
            Update,
            Remove
        }

        //TODO: Improve performance of Dictionary view...
        public class LocalizedStringValueDrawer : Sirenix.OdinInspector.Editor.OdinValueDrawer<LocalizedStringValue>
        {
            LocalizedStringValue value;

            protected override void DrawPropertyLayout(GUIContent label)
            {
                
                value = ValueEntry.SmartValue;
                
                if (value.Operation == LocalizedStringValueOperation.Remove)
                {
                    GUILayout.Label("REMOVED");
                    return;
                }
                GUILayout.BeginVertical();
                Color restoreColor = GUI.color;
                if (value.Operation == LocalizedStringValueOperation.Update)
                {
                    GUI.color = Color.yellow;
                }
                string newValue = GUILayout.TextArea(value.Value, EditorStyles.textArea);
                GUI.color = restoreColor;
                if (newValue != value.Value)
                {
                    value.Value = newValue;
                    value.Operation = LocalizedStringValueOperation.Update;
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove"))
                {
                    value.Operation = LocalizedStringValueOperation.Remove;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                ValueEntry.SmartValue = value;
            }

        }

        private void UpdateDataFromFilteredDictionary()
        {
            //TODO: This update should happen also when user does not change filter, like when unselect the editor or save the scene
            bool didChange = false;
            foreach (var keyValue in filteredDataDic)
            {
                switch (keyValue.Value.Operation)
                {
                    case LocalizedStringValueOperation.Remove:
                        Remove(keyValue.Key);
                        didChange = true;
                        break;
                    case LocalizedStringValueOperation.Update:
                        UpdateValue(keyValue.Key, keyValue.Value.Value);
                        didChange = true;
                        break;
                }
            }
            if (didChange)
            {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        private void UpdateFilter(string newValue)
        {
            UpdateDataFromFilteredDictionary();
            filteredDataDic.Clear();
            if (newValue == null) newValue = "";

            newValue = newValue.ToUpperInvariant();
            string[] possibleValues = newValue.Split(spaceSeparetor);
            int end = _data.Count / 2;
            string key, value;
            int k = 0;
            for (int i = 0; i < end; i++)
            {
                key = _data[k];
                k++;
                value = _data[k];
                k++;
                bool allPassed = true;
                foreach (var possibleValue in possibleValues)
                {
                    if (!key.Contains(possibleValue) && !value.ToUpperInvariant().Contains(possibleValue))
                    {
                        allPassed = false;
                        break;
                    }
                }
                if (allPassed)
                {
                    filteredDataDic.Add(key, new LocalizedStringValue(value));
                }
            }
        }

        

        private bool IsApplicationPlaying => Application.isPlaying;
        [FoldoutGroup("Operations")]
        [Button(Style = ButtonStyle.CompactBox, Expanded = true)]
        private void Synchronize(LanguageData otherLanguage)
        {
            if (otherLanguage == null)
            {
                EditorUtility.DisplayDialog("Synchronization - Error", "Please, select a file", "Ok");
                return;
            }
            if (otherLanguage == this)
            {
                EditorUtility.DisplayDialog("Synchronization - Error", "You are trying to synchronize with the same file.", "Ok");
                return;
            }
            AssetDatabase.SaveAssets();
            var removedKeys = GetKeys();
            var addedKeys = otherLanguage.GetKeys();
            var otherKeys = otherLanguage.GetKeys();
            addedKeys.ExceptWith(removedKeys);
            removedKeys.ExceptWith(otherKeys);
            bool hasRemovedKeys = removedKeys.Count > 0;
            bool hasAddedKeys = addedKeys.Count > 0;
            if (!hasRemovedKeys && !hasAddedKeys)
            {
                EditorUtility.DisplayDialog("Synchronization", "File is syncronized", "Ok");
                return;
            }

            if (hasRemovedKeys)
            {


                if (EditorUtility.DisplayDialog("Synchronization - Removing",
                    BuildSynchonizationMessage($"The following {removedKeys.Count.ToString()} key(s) were not present in the other language {otherLanguage.name} but it's in this language {name}:",
                    removedKeys, "Do you want to remove from this file?"), "Yes", "No"))
                {
                    foreach (string key in removedKeys)
                    {
                        Remove(key);
                    }
                }
            }
            if (hasAddedKeys)
            {
                if (EditorUtility.DisplayDialog("Synchronization - Adding",
                    BuildSynchonizationMessage($"The following {addedKeys.Count.ToString()} key(s) were present in the other language {otherLanguage.name} but  it's not in this language {name}:",
                    addedKeys, "Do you want to add from this file?"), "Yes", "No"))
                {
                    foreach (string key in addedKeys)
                    {
                        Add(key, otherLanguage.GetValue(key));
                    }
                }
            }
            AssetDatabase.SaveAssets();
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
        private static readonly char[] spaceSeparetor = new char[] { ' ' };
        [FoldoutGroup("Operations")]
        [Button]
        private void AddKeysInBatchWithCommaSeparetor(string keys, string prefixes = "", string suffixes = "")
        {
            AddKeysInBatchWithSeparetor(keys, prefixes, suffixes, commaSeparetor);
        }
        [FoldoutGroup("Operations")]
        [Button]
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
                        if (ContainsKey(newKey))
                        {
                            Debug.LogError($"Key {newKey} is already on language data");
                        } else
                        {
                            Add(newKey, newKey);
                        }
                        sb.Remove(sb.Length - suffix.Length, suffix.Length);
                    }
                    sb.Clear();
                }
            }
        }
        [FoldoutGroup("Operations")]
        [Button]
        public void Add(string key, string value)
        {
            key = key.Trim().ToUpperInvariant();
            if (_data.Count == 0 || _data[_data.Count - 2].CompareTo(key) < 0)
            {
                _data.AddRange(new string[] { key, value });
            } else if (_data[0].CompareTo(key) > 0)
            {
                _data.InsertRange(0, new string[] { key, value });
            } else
            {
                int insertIndex = FindKeyIndex(key);
                if (insertIndex >= 0) throw new InvalidOperationException("Key already exists in Language Data: " + key);
                insertIndex = (~insertIndex) * 2;
                if (_data[insertIndex].CompareTo(key) < 0) insertIndex += 2;
                _data.InsertRange(insertIndex, new string[] { key, value });
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void UpdateValue(string key, string value)
        {
            int keyIndex = FindKeyIndex(key);
            if (keyIndex < 0) throw new KeyNotFoundException("Key not foudn in Language Data: " + key);
            _data[keyIndex * 2 + 1] = value;
        }

        public bool ContainsKey(string key)
        {
            return FindKeyIndex(key) >= 0;
        }

        public int FindKeyIndex(string key)
        {
            int min = 0;
            int max = _data.Count - 2;
            int comparison;
            int mid = 0;
            while (min <= max)
            {
                mid = (min + max) / 2;
                mid -= (mid % 2);
                comparison = key.CompareTo(_data[mid]);
                if (comparison == 0)
                {
                    return mid/2;
                } else if (comparison < 0)
                {
                    max = mid - 2;
                } else
                {
                    min = mid + 2;
                }
            }
            return ~(mid / 2);
        }

        

        private bool Remove(string key)
        {
            int keyIndex = FindKeyIndex(key);
            if (keyIndex < 0) return false;
            _data.RemoveRange(keyIndex * 2, 2);
            return true;
        }

        private string GetValue(string key)
        {
            int keyIndex = FindKeyIndex(key);
            if (keyIndex < 0) throw new KeyNotFoundException("Key not foudn in Language Data: " + key);
            return _data[keyIndex * 2 + 1];
        }

        private HashSet<string> GetKeys() {
            var result = new HashSet<string>();
            for (int i = 0; i < _data.Count; i += 2)
            {
                result.Add(_data[i]);
            }
            return result;
        }

#endif
        public void OnAfterDeserialize() {
#if UNITY_EDITOR
            UpdateFilter(m_filter);
            /*int numberOfElements = _data.Length / 2;
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
            }*/
#endif
        }
        private const string allSpacesPattern = @"\s+";
        
        public void OnBeforeSerialize() {
            //UpdateFilter("");
            /*
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
            */
        }


#pragma warning disable 414
        [SerializeField, FoldoutGroup("Debug")]
        private bool m_drawSerializedData = false;
#pragma warning restore 414
        [SerializeField, FoldoutGroup("Debug"), ReadOnly, ShowIf("m_drawSerializedData")]
        private List<string> _data = null;


    }
}