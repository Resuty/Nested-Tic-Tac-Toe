using System;
using UnityEngine;
using UnityEngine.UI;

namespace WellWithinWell.Presentation
{
    [Serializable]
    public struct LocalizedLabel
    {
        [SerializeField] private Text label;
        [SerializeField] private TextKey key;
        [SerializeField] private int defaultSize;
        [SerializeField] private int englishSize;

        public LocalizedLabel(Text label, TextKey key, int englishSize)
        {
            this.label = label;
            this.key = key;
            defaultSize = label.fontSize;
            this.englishSize = englishSize;
        }

        public void Apply(GameLanguage language)
        {
            label.fontSize = language == GameLanguage.English && englishSize > 0 ? englishSize : defaultSize;
            label.text = TextCatalog.Get(key, language);
        }
    }
}
