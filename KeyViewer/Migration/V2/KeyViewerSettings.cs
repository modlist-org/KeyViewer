using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace KeyViewer.Migration.V2
{
    public class KeyViewerSettings
    {
        public Color P;
        public Color EP;
        public Color LP;
        public Color VE;
        public Color VL;
        public Color TE;
        public Color TL;
        public Color MP;
        public bool ViewerOnlyGameplay { get; set; }
        public bool AnimateKeys { get; set; } = true;
        public float KeyViewerSize { get; set; } = 55f;
        public float KeyViewerXPos { get; set; } = 0.01f;
        public float KeyViewerYPos { get; set; } = 0.04f;
        public Color PressedOutlineColor;
        public Color ReleasedOutlineColor;
        public Color PressedBackgroundColor;
        public Color ReleasedBackgroundColor;
        public Color PressedTextColor;
        public Color ReleasedTextColor;
        public KeyViewerSettings()
        {
            this.PressedOutlineColor = Color.black;
            this.ReleasedOutlineColor = Color.black;
            this.PressedBackgroundColor = Color.black;
            this.ReleasedBackgroundColor = Color.black;
            this.PressedTextColor = Color.black;
            this.ReleasedTextColor = Color.black;
            this.Profiles = new List<KeyViewerProfile>();
            this.ProfileIndex = 0;
        }
        public List<KeyViewerProfile> Profiles { get; set; }
        public int ProfileIndex { get; set; }
        public bool CustomColor;
        public bool ColorAsJudge;
        public Ease ease = Ease.InElastic;
        public float ed = 0.1f;
        public float sf = 0.9f;
        public bool IgnoreSkippedKeys;
        public float danwi;
        public int UpdateRate = 20;
        public bool SaveKeyCounts;
    }
}
