using KeyViewer.Core;
using KeyViewer.Models;
using KeyViewer.Unity;
using KeyViewer.Utils;
using System.Linq;
using UnityEngine;
using TKKC = KeyViewer.Core.Translation.TranslationKeys.KeyConfig;
using TKM = KeyViewer.Core.Translation.TranslationKeys.Misc;

namespace KeyViewer.Views
{
    public class KeyConfigDrawer : ModelDrawable<KeyConfig>
    {
        public KeyManager manager;
        public KeyConfigDrawer(KeyManager manager, KeyConfig config) : base(config, L(TKKC.KeyConfiguration, config.DummyName != null ? config.DummyName : config.Code))
        {
            this.manager = manager;
        }
        public override void Draw()
        {
            if (model.DummyName != null)
            {
                if (Drawer.DrawString(L(TKKC.DummyKeyName), ref model.DummyName))
                    Name = model.DummyName;
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    Drawer.ButtonLabel(L(TKKC.KeyCode) + (model.Code == KeyCode.Menu ? " (Fake)" : ""));
                    Drawer.DrawEnum(L(TKKC.KeyCode), ref model.Code, model.GetHashCode());
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            bool prevBgBlurEnabled = model.BackgroundBlurEnabled;
            bool changed = false;
            changed |= Drawer.DrawString(L(TKKC.TextFont), ref model.Font, true);
            if (model.DummyName == null)
            {
                if (Drawer.DrawBool(L(TKKC.EnableKPSMeter), ref model.EnableKPSMeter))
                {
                    changed = true;
                    if (model.EnableKPSMeter)
                        KPSCalculator.Sync(manager.keys.Select(k => k.Config.EnableKPSMeter ? k.KpsCalc : null).Where(c => c != null));
                    else manager[model.Code.ToString()].KpsCalc.Stop();
                }
            }
            changed |= Drawer.DrawBool(L(TKKC.UpdateTextAlways), ref model.UpdateTextAlways);
            changed |= Drawer.DrawBool(L(TKKC.EnableCountText), ref model.EnableCountText);
            changed |= Drawer.DrawBool(L(TKKC.EnableOutlineImage), ref model.EnableOutlineImage);
            changed |= Drawer.DrawBool(L(TKKC.DisableSorting), ref model.DisableSorting);
            changed |= Drawer.DrawBool(L(TKKC.DoNotScaleText), ref model.DoNotScaleText);
            changed |= Drawer.DrawBool(L(TKKC.EnableBackgroundBlur), ref model.BackgroundBlurEnabled);
            changed |= Drawer.DrawSingleWithSlider(L(TKKC.TextFontSize), ref model.TextFontSize, 0, 300, 300);
            changed |= Drawer.DrawSingleWithSlider(L(TKKC.CountTextFontSize), ref model.CountTextFontSize, 0, 300, 300);

            changed |= Drawer.DrawPressReleaseH(L(TKKC.Text), model.Text, Drawer.CD_H_STR);
            if (model.EnableCountText)
                changed |= Drawer.DrawPressReleaseH(L(TKKC.CountText), model.CountText, Drawer.CD_H_STR);
            changed |= Drawer.DrawPressReleaseH(L(TKKC.BackgroundImage), model.Background, Drawer.CD_H_STR_TRIMQUOTE);
            if (model.EnableOutlineImage)
                changed |= Drawer.DrawPressReleaseH(L(TKKC.OutlineImage), model.Outline, Drawer.CD_H_STR_TRIMQUOTE);
            if (model.BackgroundBlurEnabled)
                changed |= Drawer.DrawBlurConfig(L(TKKC.KeyBackground, KeyViewerUtils.KeyName(model)), model.BackgroundBlurConfig);

            changed |= Drawer.DrawVectorConfig(model.VectorConfig);

            Drawer.DrawObjectConfig(L(TKKC.EditTextConfig), L(TKKC.KeyText, model.DummyName != null ? model.DummyName : model.Code), model.TextConfig, () => manager.UpdateLayout());
            if (model.EnableCountText)
                Drawer.DrawObjectConfig(L(TKKC.EditCountTextConfig), L(TKKC.KeyCountText, model.DummyName != null ? model.DummyName : model.Code), model.CountTextConfig, () => manager.UpdateLayout());
            Drawer.DrawObjectConfig(L(TKKC.EditBackgroundConfig), L(TKKC.KeyBackground, model.DummyName != null ? model.DummyName : model.Code), model.BackgroundConfig, () => manager.UpdateLayout());
            if (model.EnableOutlineImage)
                Drawer.DrawObjectConfig(L(TKKC.EditOutlineConfig), L(TKKC.KeyOutline, model.DummyName != null ? model.DummyName : model.Code), model.OutlineConfig, () => manager.UpdateLayout());

            changed |= Drawer.DrawSingleWithSlider(L(TKKC.BackgroundImageRoundness), ref model.BackgroundRoundness, 0, Constants.Rad2Deg100, 300);
            changed |= Drawer.DrawSingleWithSlider(L(TKKC.OutlineImageRoundness), ref model.OutlineRoundness, 0, Constants.Rad2Deg100, 300);

            changed |= Drawer.DrawBool(L(TKKC.EnableRain), ref model.RainEnabled);
            if (model.RainEnabled)
                Drawer.TitleButton(L(TKKC.EditRainConfig), L(TKM.EditThis), () => Main.GUI.Push(new RainConfigDrawer(manager, model)));

            if (changed)
            {
                if (!prevBgBlurEnabled && model.BackgroundBlurEnabled)
                    KeyViewerUtils.ApplyBlurColorConfig(model);
                manager.UpdateLayout();
            }
        }
    }
}
