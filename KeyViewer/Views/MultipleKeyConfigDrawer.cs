﻿using KeyViewer.Core;
using KeyViewer.Core.Input;
using KeyViewer.Models;
using KeyViewer.Unity;
using KeyViewer.Utils;
using System.Collections.Generic;
using System.Linq;
using TKKC = KeyViewer.Core.Translation.TranslationKeys.KeyConfig;
using TKM = KeyViewer.Core.Translation.TranslationKeys.Misc;

namespace KeyViewer.Views
{
    public class MultipleKeyConfigDrawer : ModelDrawable<KeyConfig>
    {
        public KeyManager manager;
        public KeyConfig modelCopy;
        public List<KeyConfig> targets;
        public List<KeyConfig> targetsCopy;
        public List<string> relativeKeyNames;
        public MultipleKeyConfigDrawer(KeyManager manager, List<string> targets, KeyConfig criterion) : base(criterion ?? new KeyConfig(), L(TKKC.KeyConfiguration, KeyViewerUtils.AggregateComma(targets)))
        {
            this.manager = manager;
            modelCopy = model.Copy();
            this.targets = manager.keys.Where(k => targets.Contains(KeyViewerUtils.KeyName(k.Config))).Select(k => k.Config).ToList();
            targetsCopy = this.targets.Select(k => k.Copy()).ToList();
            relativeKeyNames = this.targets.Where(k => k.DisableSorting).Select(k => KeyViewerUtils.KeyName(k)).ToList();
        }
        public override void Draw()
        {
            Drawer.ButtonLabel(Name, KeyViewerUtils.OpenDiscordUrl);
            if (relativeKeyNames.Any())
                Drawer.ButtonLabel($"<color=#45FFCA>{L(TKKC.RelativeCoordinateMode)}: {KeyViewerUtils.AggregateComma(relativeKeyNames)}</color>", KeyViewerUtils.OpenDiscordUrl);

            bool changed = false;
            bool prevBgBlurEnabled = model.BackgroundBlurEnabled;
            changed |= Drawer.DrawString(LD(TKKC.TextFont, "Font"), ref model.Font).IfTrue(() => Set("Font"));
            if (model.DummyName == null)
            {
                if (Drawer.DrawBool(LD(TKKC.EnableKPSMeter, "EnableKPSMeter"), ref model.EnableKPSMeter))
                {
                    changed = true;
                    if (model.EnableKPSMeter)
                        KPSCalculator.Sync(manager.keys.Select(k => k.Config.EnableKPSMeter ? k.KpsCalc : null).Where(c => c != null));
                    else manager[model.Code.ToString()].KpsCalc.Stop();
                }
            }
            changed |= Drawer.DrawBool(LD(TKKC.UpdateTextAlways, "UpdateTextAlways"), ref model.UpdateTextAlways).IfTrue(() => Set("UpdateTextAlways"));
            changed |= Drawer.DrawBool(LD(TKKC.EnableCountText, "EnableCountText"), ref model.EnableCountText).IfTrue(() => Set("EnableCountText"));
            changed |= Drawer.DrawBool(LD(TKKC.EnableOutlineImage, "EnableOutlineImage"), ref model.EnableOutlineImage).IfTrue(() => Set("EnableOutlineImage"));
            //changed |= Drawer.DrawBool(LD(TKKC.DisableSorting, "DisableSorting"), ref model.DisableSorting).IfTrue(() => Set("DisableSorting"));
            changed |= Drawer.DrawBool(LD(TKKC.DoNotScaleText, "DoNotScaleText"), ref model.DoNotScaleText).IfTrue(() => Set("DoNotScaleText"));
            changed |= Drawer.DrawBool(LD(TKKC.EnableBackgroundBlur, "BackgroundBlurEnabled"), ref model.BackgroundBlurEnabled).IfTrue(() => Set("BackgroundBlurEnabled"));
            changed |= Drawer.DrawSingleWithSlider(LD(TKKC.TextFontSize, "TextFontSize"), ref model.TextFontSize, 0, 300, 300).IfTrue(() => Set("TextFontSize"));
            changed |= Drawer.DrawSingleWithSlider(LD(TKKC.CountTextFontSize, "CountTextFontSize"), ref model.CountTextFontSize, 0, 300, 300).IfTrue(() => Set("CountTextFontSize"));

            changed |= Drawer.DrawPressReleaseH(LD<PressRelease<string>>(TKKC.Text, "Text"), model.Text, Drawer.CD_H_STR).IfTrue(() => SetPR<string>("Text"));
            if (model.EnableCountText)
                changed |= Drawer.DrawPressReleaseH(LD<PressRelease<string>>(TKKC.CountText, "CountText"), model.CountText, Drawer.CD_H_STR).IfTrue(() => SetPR<string>("CountText"));
            changed |= Drawer.DrawPressReleaseH(LD<PressRelease<string>>(TKKC.BackgroundImage, "Background"), model.Background, Drawer.CD_H_STR).IfTrue(() => SetPR<string>("Background"));
            if (model.EnableOutlineImage)
                changed |= Drawer.DrawPressReleaseH(LD<PressRelease<string>>(TKKC.OutlineImage, "Outline"), model.Outline, Drawer.CD_H_STR).IfTrue(() => SetPR<string>("Outline"));
            if (model.BackgroundBlurEnabled)
                changed |= Drawer.DrawBlurConfig(LD(TKKC.KeyBackground, "BackgroundBlurConfig", KeyViewerUtils.KeyName(model)), model.BackgroundBlurConfig).IfTrue(() => SetBlurConfig("BackgroundBlurConfig"));

            changed |= Drawer.DrawVectorConfig(model.VectorConfig).IfTrue(() => SetVectorConfig("VectorConfig"));

            Drawer.DrawObjectConfig(LD<ObjectConfig>(TKKC.EditTextConfig, "TextConfig"), L(TKKC.KeyText, model.DummyName != null ? model.DummyName : model.Code), model.TextConfig, () => OnChangeOC("TextConfig"));
            if (model.EnableCountText)
                Drawer.DrawObjectConfig(LD<ObjectConfig>(TKKC.EditCountTextConfig, "CountTextConfig"), L(TKKC.KeyCountText, model.DummyName != null ? model.DummyName : model.Code), model.CountTextConfig, () => OnChangeOC("CountTextConfig"));
            Drawer.DrawObjectConfig(LD<ObjectConfig>(TKKC.EditBackgroundConfig, "BackgroundConfig"), L(TKKC.KeyBackground, model.DummyName != null ? model.DummyName : model.Code), model.BackgroundConfig, () => OnChangeOC("BackgroundConfig"));
            if (model.EnableOutlineImage)
                Drawer.DrawObjectConfig(LD<ObjectConfig>(TKKC.EditOutlineConfig, "OutlineConfig"), L(TKKC.KeyOutline, model.DummyName != null ? model.DummyName : model.Code), model.OutlineConfig, () => OnChangeOC("OutlineConfig"));

            changed |= Drawer.DrawSingleWithSlider(LD(TKKC.BackgroundImageRoundness, "BackgroundRoundness"), ref model.BackgroundRoundness, 0, Constants.Rad2Deg100, 300).IfTrue(() => Set("BackgroundRoundness"));
            changed |= Drawer.DrawSingleWithSlider(LD(TKKC.OutlineImageRoundness, "OutlineRoundness"), ref model.OutlineRoundness, 0, Constants.Rad2Deg100, 300).IfTrue(() => Set("OutlineRoundness"));

            changed |= Drawer.DrawBool(LD(TKKC.EnableRain, "RainEnabled"), ref model.RainEnabled).IfTrue(() => Set("RainEnabled"));
            if (model.RainEnabled)
                Drawer.TitleButton(L(TKKC.EditRainConfig), L(TKM.EditThis), () => Main.GUI.Push(new MultipleRainConfigDrawer(manager, targets.Select(t => KeyViewerUtils.KeyName(t)).ToList())));

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
        string LD(string tk, string fieldName, params object[] args)
        {
            string l = L(tk, args);
            if (!KeyViewerUtils.IsEquals(targets, fieldName))
                l += " <color=cyan>(Diff!)</color>";
            return l;
        }
        string LD<T>(string tk, string fieldName, params object[] args)
        {
            // FUCKING PERFORMANCE.....
            //var field = typeof(KeyConfig).GetField(fieldName);
            string l = L(tk, args);
            //if (!KeyViewerUtils.IsFieldsEquals(typeof(T), targets.Select(t => field.GetValue(t))))
            //    l += " <color=cyan>(Diff!)</color>";
            return l;
        }
    }
}
