using KeyViewer.Core;
using KeyViewer.Core.TextReplacing;
using KeyViewer.Models;
using KeyViewer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KeyViewer.Unity
{
    public class KeyManager : MonoBehaviour
    {
        public Tag CurKPSTag { get; private set; }
        public Tag MaxKPSTag { get; private set; }
        public Tag AvgKPSTag { get; private set; }
        public Tag CountTag { get; private set; }
        public List<Tag> AllTags { get; private set; }

        public Profile profile;
        public Canvas keysCanvas;
        public Vector2 defaultSize;

        internal bool encrypted;
        internal KPSCalculator kpsCalc;
        internal List<Key> keys;
        internal Vector2 centerOffset;
        internal RectTransform keysRt;
        internal bool prevPressed;
        internal bool initialized;
        public void Init()
        {
            if (initialized) return;
            kpsCalc = new KPSCalculator(profile);
            kpsCalc.Start();
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1280, 720);
            keys = new List<Key>();
            CurKPSTag = new Tag("CurKPS").SetGetter(new Func<string, int>(name =>
            {
                if (string.IsNullOrEmpty(name)) return kpsCalc.Kps;
                Key key = keys.Find(k => KeyViewerUtils.KeyName(k.Config) == name);
                if (key == null) return -1;
                if (!key.KpsCalc.Running) return 0;
                return key.KpsCalc.Kps;
            }));
            MaxKPSTag = new Tag("MaxKPS").SetGetter(new Func<string, int>(name =>
            {
                if (string.IsNullOrEmpty(name)) return kpsCalc.Max;
                Key key = keys.Find(k => KeyViewerUtils.KeyName(k.Config) == name);
                if (key == null) return -1;
                if (!key.KpsCalc.Running) return 0;
                return key.KpsCalc.Max;
            }));
            AvgKPSTag = new Tag("AvgKPS").SetGetter(new Func<string, double>(name =>
            {
                if (string.IsNullOrEmpty(name)) return kpsCalc.Average;
                Key key = keys.Find(k => KeyViewerUtils.KeyName(k.Config) == name);
                if (key == null) return -1;
                if (!key.KpsCalc.Running) return 0;
                return key.KpsCalc.Average;
            }));
            CountTag = new Tag("Count").SetGetter(new Func<string, int>(name =>
            {
                if (string.IsNullOrEmpty(name))
                {
                    int total = 0;
                    foreach (var k in keys)
                        total += k.Config.Count;
                    return total;
                }
                Key key = keys.Find(k => KeyViewerUtils.KeyName(k.Config) == name);
                if (key == null) return -1;
                return key.Config.Count;
            }));
            AllTags = new List<Tag> { CurKPSTag, MaxKPSTag, AvgKPSTag, CountTag };
            initialized = true;
        }
        public Key this[string keyName]
        {
            get => keys.Find(k => KeyViewerUtils.KeyName(k.Config) == keyName);
            set
            {
                int index = keys.FindIndex(k => KeyViewerUtils.KeyName(k.Config) == keyName);
                if (index < 0) return;
                keys[index] = value;
            }
        }
        private void Update()
        {
            if (!initialized) return;
            var pressed = keys.Any(k => k.Pressed);
            if (prevPressed == pressed) return;
            prevPressed = pressed;
            KeyViewerUtils.ApplyVectorConfig(keysRt, profile.VectorConfig, pressed, 0, false, defaultSize);
        }
        public void UpdateKeys()
        {
            if (keysCanvas)
                Destroy(keysCanvas.gameObject);
            GameObject keysObject = new GameObject("Keys Canvas");
            keysObject.transform.SetParent(transform);
            keysCanvas = keysObject.AddComponent<Canvas>();
            keysRt = keysCanvas.GetComponent<RectTransform>();
            keys = new List<Key>();
            foreach (KeyConfig config in profile.Keys)
            {
                string name = KeyViewerUtils.KeyName(config);
                GameObject keyObject = new GameObject($"Key {name}");
                Key key = keyObject.AddComponent<Key>();
                key.Init(this, config);
                keys.Add(key);
            }
            UpdateLayout();
        }
        public void UpdateLayout()
        {
            int count = keys.Count;
            float keyHeight = profile.Keys.Any(k => k.EnableCountText) ? 150 : 100;
            float width = count * 100 + (count - 1) * profile.KeySpacing;

            var vecConfig = profile.VectorConfig;
            keysRt.SetAnchor(profile.VectorConfig.Anchor);
            keysRt.sizeDelta = defaultSize = new Vector2(width, keyHeight);
            keysRt.pivot = new Vector2(0.5f, 0.5f);
            keysRt.anchoredPosition = vecConfig.Offset.Released;
            keysRt.localRotation = Quaternion.Euler(vecConfig.Rotation.Released);
            keysRt.localScale = vecConfig.Scale.Released;

            bool first = true;
            float totalX = 0;
            foreach (Key k in keys)
                if (!k.Config.DisableSorting)
                {
                    var releasedScale = k.Config.VectorConfig.Scale.Released;
                    if (first)
                    {
                        totalX += releasedScale.x * 100;
                        first = false;
                    }
                    totalX += releasedScale.x * 100 + profile.KeySpacing;
                }
            Vector2 size = new Vector2(totalX - profile.KeySpacing, keyHeight);
            centerOffset = KeyViewerUtils.GetPivot(profile.VectorConfig.Pivot) * size;

            float x = 0;
            keys.ForEach(k => k.UpdateLayout(ref x));
        }
        public static KeyManager CreateManager(string name, Profile profile)
        {
            if (profile == null) return null;
            GameObject manager = new GameObject($"KeyViewer {name} Profile");
            DontDestroyOnLoad(manager);
            var km = manager.AddComponent<KeyManager>();
            km.profile = profile;
            return km;
        }
    }
}
