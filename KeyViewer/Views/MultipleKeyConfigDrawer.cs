using KeyViewer.Core;
using KeyViewer.Core.Input;
using KeyViewer.Models;
using KeyViewer.Unity;
using KeyViewer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeyViewer.Views
{
    public class MultipleKeyConfigDrawer : ModelDrawable<KeyConfig>
    {
        public KeyManager manager;
        public KeyConfig modelCopy;
        public List<KeyConfig> targets;
        public List<KeyConfig> targetsCopy;
        public List<string> relativeKeyNames;
        public MultipleKeyConfigDrawer(KeyManager manager, List<string> targets, KeyConfig criterion) : base(criterion ?? new KeyConfig(), string.Format(Main.Lang.Get("KEYCONFIG_KEY_CONFIGURATION", "Key {0} Configuration"), KeyViewerUtils.AggregateComma(targets)))
        {
            this.manager = manager;
            modelCopy = model.Copy();
            this.targets = manager.keys.Where(k => targets.Contains(KeyViewerUtils.KeyName(k.Config))).Select(k => k.Config).ToList();
            targetsCopy = this.targets.Select(k => k.Copy()).ToList();
            relativeKeyNames = this.targets.Where(k => k.DisableSorting).Select(k => KeyViewerUtils.KeyName(k)).ToList();
        }
        public override void Draw()
        {
            GUILayout.Label(Name);
            if (relativeKeyNames.Any())
                GUILayout.Label($"<color=#45FFCA>{Main.Lang.Get("KEYCONFIG_RELATIVE_COORDINATE_MODE", "Relative Coordinate Mode Due To Disable Sorting")}: {KeyViewerUtils.AggregateComma(relativeKeyNames)}</color>");

            bool changed = false;
            bool prevBgBlurEnabled = model.BackgroundBlurEnabled;
            changed |= Drawer.DrawString(FormatText(Main.Lang.Get("KEYCONFIG_TEXT_FONT", "Text Font"), "Font"), ref model.Font, true).IfTrue(() => Set("Font"));
            if (model.DummyName == null)
            {
                if (Drawer.DrawBool(Main.Lang.Get("KEYCONFIG_ENABLE_KPS_METER", "Enable KPS Meter"), ref model.EnableKPSMeter))
                {
                    changed = true;
                    if (model.EnableKPSMeter)
                        KPSCalculator.Sync(manager.keys.Select(k => k.Config.EnableKPSMeter ? k.KpsCalc : null).Where(c => c != null));
                    else manager[model.Code.ToString()].KpsCalc.Stop();
                }
            }
            changed |= Drawer.DrawBool(FormatText(Main.Lang.Get("KEYCONFIG_UPDATE_TEXT_ALWAYS", "Update Text Always"), "UpdateTextAlways"), ref model.UpdateTextAlways).IfTrue(() => Set("UpdateTextAlways"));
            changed |= Drawer.DrawBool(FormatText(Main.Lang.Get("KEYCONFIG_ENABLE_COUNT_TEXT", "Enable Count Text"), "EnableCountText"), ref model.EnableCountText).IfTrue(() => Set("EnableCountText"));
            changed |= Drawer.DrawBool(FormatText(Main.Lang.Get("KEYCONFIG_ENABLE_OUTLINE_IMAGE", "Enable Outline Image"), "EnableOutlineImage"), ref model.EnableOutlineImage).IfTrue(() => Set("EnableOutlineImage"));
            //changed |= Drawer.DrawBool(LD(TKKC.DisableSorting, "DisableSorting"), ref model.DisableSorting).IfTrue(() => Set("DisableSorting"));
            changed |= Drawer.DrawBool(FormatText(Main.Lang.Get("KEYCONFIG_DO_NOT_SCALE_TEXT", "Do Not Scale Text"), "DoNotScaleText"), ref model.DoNotScaleText).IfTrue(() => Set("DoNotScaleText"));
            changed |= Drawer.DrawBool(FormatText(Main.Lang.Get("KEYCONFIG_ENABLE_BACKGROUND_BLUR", "Enable Backgruond Blur"), "BackgroundBlurEnabled"), ref model.BackgroundBlurEnabled).IfTrue(() => Set("BackgroundBlurEnabled"));
            changed |= Drawer.DrawSingleWithSlider(FormatText(Main.Lang.Get("KEYCONFIG_TEXT_FONT_SIZE", "Text Font Size"), "TextFontSize"), ref model.TextFontSize, 0, 300, 300).IfTrue(() => Set("TextFontSize"));
            changed |= Drawer.DrawSingleWithSlider(FormatText(Main.Lang.Get("KEYCONFIG_COUNT_TEXT_FONT_SIZE", "Count Text Font Size"), "CountTextFontSize"), ref model.CountTextFontSize, 0, 300, 300).IfTrue(() => Set("CountTextFontSize"));

            changed |= Drawer.DrawPressReleaseH(FormatText(Main.Lang.Get("KEYCONFIG_TEXT", "Text "), "Text"), model.Text, Drawer.CD_H_STR).IfTrue(() => SetPR<string>("Text"));
            if (model.EnableCountText)
                changed |= Drawer.DrawPressReleaseH(FormatText(Main.Lang.Get("KEYCONFIG_COUNT_TEXT", "Count Text"), "CountText"), model.CountText, Drawer.CD_H_STR).IfTrue(() => SetPR<string>("CountText"));
            changed |= Drawer.DrawPressReleaseH(FormatText(Main.Lang.Get("KEYCONFIG_BACKGROUND_IMAGE", "Background Image"), "Background"), model.Background, Drawer.CD_H_STR_TRIMQUOTE).IfTrue(() => SetPR<string>("Background"));
            if (model.EnableOutlineImage)
                changed |= Drawer.DrawPressReleaseH(FormatText(Main.Lang.Get("KEYCONFIG_OUTLINE_IMAGE", "Outline Image"), "Outline"), model.Outline, Drawer.CD_H_STR_TRIMQUOTE).IfTrue(() => SetPR<string>("Outline"));
            if (model.BackgroundBlurEnabled)
                changed |= Drawer.DrawBlurConfig(FormatText(Main.Lang.Get("KEYCONFIG_KEY_BACKGROUND", "Key {0} Background"), "BackgroundBlurConfig", KeyViewerUtils.KeyName(model)), model.BackgroundBlurConfig).IfTrue(() => SetBlurConfig("BackgroundBlurConfig"));

            changed |= Drawer.DrawVectorConfig(model.VectorConfig).IfTrue(() => SetVectorConfig("VectorConfig"));

            Drawer.DrawObjectConfig(FormatText(Main.Lang.Get("KEYCONFIG_EDIT_TEXT_CONFIG", "Edit Text Config"), "TextConfig"), string.Format(Main.Lang.Get("KEYCONFIG_KEY_TEXT", "Key {0} Text"), model.DummyName != null ? model.DummyName : model.Code), model.TextConfig, () => OnChangeOC("TextConfig"));
            if (model.EnableCountText)
                Drawer.DrawObjectConfig(FormatText(Main.Lang.Get("KEYCONFIG_EDIT_COUNT_TEXT_CONFIG", "Edit Count Text Config"), "CountTextConfig"), string.Format(Main.Lang.Get("KEYCONFIG_EDIT_COUNT_TEXT_CONFIG", "Edit Count Text Config"), model.DummyName != null ? model.DummyName : model.Code), model.CountTextConfig, () => OnChangeOC("CountTextConfig"));
            Drawer.DrawObjectConfig(FormatText(Main.Lang.Get("KEYCONFIG_EDIT_BACKGROUND_CONFIG", "Edit Background Config"), "BackgroundConfig"), string.Format(Main.Lang.Get("KEYCONFIG_KEY_BACKGROUND", "Key {0} Background"), model.DummyName != null ? model.DummyName : model.Code), model.BackgroundConfig, () => OnChangeOC("BackgroundConfig"));
            if (model.EnableOutlineImage)
                Drawer.DrawObjectConfig(FormatText(Main.Lang.Get("KEYCONFIG_EDIT_OUTLINE_CONFIG", "Edit Outline Config"), "OutlineConfig"), string.Format(Main.Lang.Get("KEYCONFIG_KEY_OUTLINE", "Key {0} Outline"), model.DummyName != null ? model.DummyName : model.Code), model.OutlineConfig, () => OnChangeOC("OutlineConfig"));

            changed |= Drawer.DrawSingleWithSlider(FormatText(Main.Lang.Get("KEYCONFIG_BACKGROUND_IMAGE_ROUNDNESS", "Background Image Roundness"), "BackgroundRoundness"), ref model.BackgroundRoundness, 0, Constants.Rad2Deg100, 300).IfTrue(() => Set("BackgroundRoundness"));
            changed |= Drawer.DrawSingleWithSlider(FormatText(Main.Lang.Get("KEYCONFIG_OUTLINE_IMAGE_ROUNDNESS", "Outline Image Roundness"), "OutlineRoundness"), ref model.OutlineRoundness, 0, Constants.Rad2Deg100, 300).IfTrue(() => Set("OutlineRoundness"));

            changed |= Drawer.DrawBool(FormatText(Main.Lang.Get("KEYCONFIG_ENABLE_RAIN", "Enable Rain"), "RainEnabled"), ref model.RainEnabled).IfTrue(() => Set("RainEnabled"));
            if (model.RainEnabled)
                Drawer.TitleButton(Main.Lang.Get("KEYCONFIG_EDIT_RAIN_CONFIG", "Edit Rain Config"), Main.Lang.Get("MISC_EDIT", "Edit"), () => Main.GUI.Push(new MultipleRainConfigDrawer(manager, targets.Select(t => KeyViewerUtils.KeyName(t)).ToList(), model.Rain)));

            if (changed)
            {
                if (!prevBgBlurEnabled && model.BackgroundBlurEnabled)
                    KeyViewerUtils.ApplyBlurColorConfig(model);
                manager.UpdateLayout();
            }
        }
        void Set(string fieldName)
        {
            KeyViewerUtils.SetMultiple(model, modelCopy, targets, targetsCopy, fieldName, (o, t) => KeyInput.Shift || (((KeyConfig)o).DisableSorting && KeyViewerUtils.IsVectorType(t)));
        }
        void OnChangeOC(string objectConfigFieldName)
        {
            SetObjectConfig(objectConfigFieldName);
            manager.UpdateLayout();
        }
        void SetObjectConfig(string targetObjectConfigFieldName)
        {
            var field = typeof(KeyConfig).GetField(targetObjectConfigFieldName);
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

            bool IsRelative(object instance, object o, System.Type t) => KeyInput.Shift || (((KeyConfig)o).DisableSorting && KeyViewerUtils.IsVectorType(t));
            var vOriginal = original.VectorConfig;
            var vOriginalCopy = originalCopy.VectorConfig;
            var vTargets = targets.Select(t => t.VectorConfig);
            var vTargetsCopy = targetsCopy.Select(t => t.VectorConfig);
            var relativeRefs = this.targets.Cast<object>().ToList();

            KeyViewerUtils.SetMultiple(vOriginal, vOriginalCopy, vTargets, vTargetsCopy, "Pivot", (o, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiple(vOriginal, vOriginalCopy, vTargets, vTargetsCopy, "Anchor", (o, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiplePR(vOriginal.Rotation, vOriginalCopy.Rotation, vTargets.Select(t => t.Rotation), vTargetsCopy.Select(t => t.Rotation), IsRelative, relativeRefs);
            KeyViewerUtils.SetMultiplePR(vOriginal.Offset, vOriginalCopy.Offset, vTargets.Select(t => t.Offset), vTargetsCopy.Select(t => t.Offset), IsRelative, relativeRefs);
            KeyViewerUtils.SetMultiplePR(vOriginal.Scale, vOriginalCopy.Scale, vTargets.Select(t => t.Scale), vTargetsCopy.Select(t => t.Scale), IsRelative, relativeRefs);
        }
        void SetVectorConfig(string targetVectorConfigFieldName)
        {
            bool IsRelative(object instance, object o, System.Type t) => KeyInput.Shift || (((KeyConfig)o).DisableSorting && KeyViewerUtils.IsVectorType(t));
            var field = typeof(KeyConfig).GetField(targetVectorConfigFieldName);
            var vOriginal = field.GetValue(model) as VectorConfig;
            var vOriginalCopy = field.GetValue(modelCopy) as VectorConfig;
            var vTargets = targets.Select(k => field.GetValue(k) as VectorConfig);
            var vTargetsCopy = targetsCopy.Select(k => field.GetValue(k) as VectorConfig);
            var relativeRefs = targets.Cast<object>().ToList();

            KeyViewerUtils.SetMultiple(vOriginal, vOriginalCopy, vTargets, vTargetsCopy, "Pivot", (o, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiple(vOriginal, vOriginalCopy, vTargets, vTargetsCopy, "Anchor", (o, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiplePR(vOriginal.Rotation, vOriginalCopy.Rotation, vTargets.Select(t => t.Rotation), vTargetsCopy.Select(t => t.Rotation), IsRelative, relativeRefs);
            KeyViewerUtils.SetMultiplePR(vOriginal.Offset, vOriginalCopy.Offset, vTargets.Select(t => t.Offset), vTargetsCopy.Select(t => t.Offset), IsRelative, relativeRefs);
            KeyViewerUtils.SetMultiplePR(vOriginal.Scale, vOriginalCopy.Scale, vTargets.Select(t => t.Scale), vTargetsCopy.Select(t => t.Scale), IsRelative, relativeRefs);
        }
        void SetBlurConfig(string targetBlurConfigFieldName)
        {
            var field = typeof(KeyConfig).GetField(targetBlurConfigFieldName);
            var original = field.GetValue(model) as BlurConfig;
            var originalCopy = field.GetValue(modelCopy) as BlurConfig;
            var targets = this.targets.Select(k => field.GetValue(k) as BlurConfig);
            var targetsCopy = this.targetsCopy.Select(k => field.GetValue(k) as BlurConfig);
            KeyViewerUtils.SetMultiple(original, originalCopy, targets, targetsCopy, "Spacing", (i, t) => KeyInput.Shift);
            KeyViewerUtils.SetMultiple(original, originalCopy, targets, targetsCopy, "Vibrancy", (i, t) => KeyInput.Shift);
        }
        void SetPR<T>(string fieldName)
        {
            var field = typeof(KeyConfig).GetField(fieldName);
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
