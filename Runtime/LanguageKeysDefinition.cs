using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Funbites.Localization {
    [CreateAssetMenu(menuName ="Localization/Language Keys")]
    public class LanguageKeysDefinition : ScriptableObject, ISerializationCallbackReceiver {
        [ShowInInspector, ListDrawerSettings(HideAddButton =true, DraggableItems =false)]
        private SortedSet<string> keys;
        [SerializeField, HideInInspector]
        private string[] m_keys = null;

        public void OnAfterDeserialize() {
            keys = new SortedSet<string>();
            foreach (var key in m_keys) {
                keys.Add(key);
            }
        }

        public void OnBeforeSerialize() {
            m_keys = new string[keys.Count];
            int i = 0;
            foreach (var key in keys) {
                m_keys[i] = key;
                i++;
            }
        }
        [Button(Style=ButtonStyle.CompactBox, Expanded = true)]
        public void AddKey(string key) {
            keys.Add(Regex.Replace(key.ToUpperInvariant(), @"\s", ""));
        }

        [Button(Style = ButtonStyle.CompactBox, Expanded = true)]
        public void Syncronize(LanguageData language) {

        }
    }
}