using System;
using UnityEngine.AddressableAssets;

namespace Funbites.Localization {
    [Serializable]
    public class AssetReferenceLanguageData : AssetReferenceT<LanguageData> {
        public AssetReferenceLanguageData(string guid) : base(guid) {
        }
    }
}