using UnityEngine;

namespace Funbites.Localization {
    public class TMP_Text_RandomMultiLanguage : TMP_Text_Base {
        [SerializeField]
        private string m_randomMessageKeyPrefix = "PREFIX_";
        [SerializeField]
        private int randomMessageMaxIndex = 1;

        public override void UpdateText() {
            text.SetMultilanguageText(m_randomMessageKeyPrefix + Random.Range(0, randomMessageMaxIndex + 1));
        }
    }
}
