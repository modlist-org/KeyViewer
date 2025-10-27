using KeyViewer.Core;
using KeyViewer.Core.Input;
using KeyViewer.Core.Interfaces;
using KeyViewer.Models;
using KeyViewer.Unity;
using KeyViewer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeyViewer.Views
{
    public class MultipleRainConfigDrawer : ModelDrawable<RainConfig>
    {
        public KeyManager manager;
        public RainConfig modelCopy;
        public List<RainConfig> targets;
        public List<RainConfig> targetsCopy;
        private string name;
        private bool imageListExpanded = false;
        public MultipleRainConfigDrawer(KeyManager manager, List<string> targets, RainConfig criterion) : base(criterion ?? new RainConfig(), string.Format(Main.Lang.Get("RAINCONFIG_RAIN_CONFIGURATION", "Key {0} Rain Configuration"), KeyViewerUtils.AggregateComma(targets)))
        {
            this.manager = manager;
            name = KeyViewerUtils.AggregateComma(targets);
            modelCopy = model.Copy();
            this.targets = manager.keys.Where(k => targets.Contains(KeyViewerUtils.KeyName(k.Config))).Select(k => k.Config.Rain).ToList();
            targetsCopy = this.targets.Select(k => k.Copy()).ToList();
        }
        public override void Draw()
        {
            bool changed = false;
            GUILayout.Label(Name);
            changed |= Drawer.DrawInt32(FormatText(Main.Lang.Get("RAINCONFIG_RAIN_POOL_SIZE", "Rain Pool Size"), "PoolSize"), ref model.PoolSize).IfTrue(() => Set("PoolSize"));
            changed |= Drawer.DrawSingleWithSlider(FormatText(Main.Lang.Get("MISC_ROUNDNESS", "Roundness"), "Roundness"), ref model.Roundness, 0, Constants.Rad2Deg100, 300).IfTrue(() => Set("Roundness"));
            //changed |= Drawer.DrawBool(LD(TKRC.BlurEnabled, "BlurEnabled"), ref model.BlurEnabled).IfTrue(() => Set("BlurEnabled"));
            changed |= Drawer.DrawPressReleaseH(Main.Lang.Get("RAINCONFIG_RAIN_SPEED", "Rain Speed"), model.Speed, Drawer.CD_H_FLT_SPEEDONLY).IfTrue(() => SetPR<float>("Speed"));
            changed |= Drawer.DrawPressReleaseH(Main.Lang.Get("RAINCONFIG_RAIN_LENGTH", "Rain Length"), model.Length, Drawer.CD_H_FLT_LENGTHONLY).IfTrue(() => SetPR<float>("Length"));
            changed |= Drawer.DrawPressReleaseH(Main.Lang.Get("RAINCONFIG_RAIN_SOFTNESS", "Rain Softness"), model.Softness, Drawer.CD_H_INT32_SOFTNESSONLY).IfTrue(() => SetPR<int>("Softness"));

            GUILayoutEx.ExpandableGUI(() =>
            {
                changed |= Drawer.DrawList(model.RainImages, (ref RainImage i) =>
                {
                    bool result = false;
                    result |= Drawer.DrawString(Main.Lang.Get("RAINCONFIG_RAIN_IMAGE_PATH", "Image Path"), ref i.Image, true);
                    result |= Drawer.DrawInt32(Main.Lang.Get("RAINCONFIG_RAIN_IMAGE_COUNT", "Image Count"), ref i.Count);
                    result |= Drawer.DrawSingleWithSlider(Main.Lang.Get("MISC_ROUNDNESS", "Roundness"), ref i.Roundness, 0, Constants.Rad2Deg100, 300);
                    //result |= Drawer.DrawBlurConfig(L(TKM.BlurConfig, i), i.BlurConfig);
                    return result;
                }).IfTrue(() => SetList<RainImage>("RainImages"));
            }, Main.Lang.Get("RAINCONFIG_RAIN_IMAGES", "Rain Images"), ref imageListExpanded);

            Drawer.DrawObjectConfig(Main.Lang.Get("RAINCONFIG_EDIT_RAIN_CONFIG", "Edit Rain Object Config"), string.Format(Main.Lang.Get("RAINCONFIG_KEY_RAIN", "Key {0} Rain"), name), model.ObjectConfig, () => OnChangeOC("ObjectConfig"));

            //if (model.BlurEnabled) changed |= Drawer.DrawBlurConfig(L(TKM.BlurConfig, Name), model.BlurConfig).IfTrue(() => SetBlurConfig("PoolSize"));

            GUILayout.BeginHorizontal();
            {
                Drawer.Button(FormatText(string.Format(Main.Lang.Get("RAINCONFIG_DIRECTION", "Key {0} Rain Direction"), name), "Direction"));
                changed |= Drawer.DrawEnum(FormatText(string.Format(Main.Lang.Get("RAINCONFIG_DIRECTION", "Key {0} Rain Direction"), name), "Direction"), ref model.Direction).IfTrue(() => Set("Direction"));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                Drawer.Button(FormatText(string.Format(Main.Lang.Get("RAINCONFIG_IMAGE_DISPLAY_MODE", "Key {0} Rain Image Display Mode"), name), "ImageDisplayMode"));
                changed |= Drawer.DrawEnum(FormatText(string.Format(Main.Lang.Get("RAINCONFIG_IMAGE_DISPLAY_MODE", "Key {0} Rain Image Display Mode"), name), "ImageDisplayMode"), ref model.ImageDisplayMode).IfTrue(() => Set("ImageDisplayMode"));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (changed) manager.UpdateLayout();
        }
        void Set(string fieldName)
        {
            KeyViewerUtils.SetMultiple(model, modelCopy, targets, targetsCopy, fieldName, (o, t) => KeyInput.Shift);
        }
        void SetList<T>(string fieldName) where T : IModel, ICopyable<T>
        {
            var field = typeof(RainConfig).GetField(fieldName);
            var originalList = field.GetValue(model) as List<T>;
            foreach (var t in targets)
                field.SetValue(t, originalList.Select(t => t.Copy()).ToList());
        }
        void OnChangeOC(string objectConfigFieldName)
        {
            SetObjectConfig(objectConfigFieldName);
            manager.UpdateLayout();
        }
        void SetObjectConfig(string targetObjectConfigFieldName)
        {
            var field = typeof(RainConfig).GetField(targetObjectConfigFieldName);
            var original = field.GetValue(model) as ObjectConfig;
            var originalCopy = field.GetValue(modelCopy) as ObjectConfig;
            var targets = this.targets.Select(k => field.GetValue(k) as ObjectConfig);
            var targetsCopy = this.targetsCopy.Select(k => field.GetValue(k) as ObjectConfig);

            KeyViewerUtils.SetMultiple(original, originalCopy, targets, targetsCopy, "ChangeColorWithJudge", (o, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiple(original, originalCopy, targets, targetsCopy, "Color", (o, t) => KeyInput.Shift);
            if (original.JudgeColors != null)
            {
                KeyViewerUtils.SetMultiple(original, originalCopy, targets, targetsCopy, "JudgeColors", (o, t) => KeyInput.Shift);
                KeyViewerUtils.SetMultiple(original, originalCopy, targets, targetsCopy, "JudgeColorEase", (o, t) => KeyInput.Shift);
            }

            var vOriginal = original.VectorConfig;
            var vOriginalCopy = originalCopy.VectorConfig;
            var vTargets = targets.Select(t => t.VectorConfig);
            var vTargetsCopy = targetsCopy.Select(t => t.VectorConfig);
            var relativeRefs = this.targets.Cast<object>().ToList();

            KeyViewerUtils.SetMultiple(vOriginal, vOriginalCopy, vTargets, vTargetsCopy, "Pivot", (o, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiple(vOriginal, vOriginalCopy, vTargets, vTargetsCopy, "Anchor", (o, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiplePR(vOriginal.Rotation, vOriginalCopy.Rotation, vTargets.Select(t => t.Rotation), vTargetsCopy.Select(t => t.Rotation), (i, o, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiplePR(vOriginal.Offset, vOriginalCopy.Offset, vTargets.Select(t => t.Offset), vTargetsCopy.Select(t => t.Offset), (i, o, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiplePR(vOriginal.Scale, vOriginalCopy.Scale, vTargets.Select(t => t.Scale), vTargetsCopy.Select(t => t.Scale), (i, o, t) => KeyInput.Shift);
        }
        void SetBlurConfig(string targetBlurConfigFieldName)
        {
            var field = typeof(RainConfig).GetField(targetBlurConfigFieldName);
            var original = field.GetValue(model) as BlurConfig;
            var originalCopy = field.GetValue(modelCopy) as BlurConfig;
            var targets = this.targets.Select(k => field.GetValue(k) as BlurConfig);
            var targetsCopy = this.targetsCopy.Select(k => field.GetValue(k) as BlurConfig);
            KeyViewerUtils.SetMultiple(original, originalCopy, targets, targetsCopy, "Spacing", (i, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiple(original, originalCopy, targets, targetsCopy, "Vibrancy", (i, t) => KeyInput.Shift);
        }
        void SetPR<T>(string fieldName)
        {
            var field = typeof(RainConfig).GetField(fieldName);
            var original = field.GetValue(model) as PressRelease<T>;
            var originalCopy = field.GetValue(modelCopy) as PressRelease<T>;
            var targets = this.targets.Select(k => field.GetValue(k) as PressRelease<T>);
            var targetsCopy = this.targetsCopy.Select(k => field.GetValue(k) as PressRelease<T>);
            KeyViewerUtils.SetMultiplePR(original, originalCopy, targets, targetsCopy, (i, o, t) => KeyInput.Shift);
        }
        string FormatText(string text, string fieldName, params object[] args) {
            if(!KeyViewerUtils.IsEquals(targets, fieldName))
                text += " <color=cyan>(Diff!)</color>";
            return text;
        }
    }
}
