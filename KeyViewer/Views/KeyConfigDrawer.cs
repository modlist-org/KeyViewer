using KeyViewer.Core;
using KeyViewer.Models;
using KeyViewer.Unity;
using KeyViewer.Utils;
using System.Linq;
using UnityEngine;

namespace KeyViewer.Views
{
    public class KeyConfigDrawer : ModelDrawable<KeyConfig>
    {
        public KeyManager manager;
        public KeyConfigDrawer(KeyManager manager, KeyConfig config) : base(config, string.Format(Main.Lang.Get("KEYCONFIG", "{0} Key Config"), config.DummyName == null ? config.Code : config.DummyName))
        {
            this.manager = manager;
        }
        public override void Draw()
        {
            if (model.DummyName != null)
            {
                if (Drawer.DrawString(Main.Lang.Get("KEYCONFIG_DUMMY_KEY_NAME", "Dummy Key Name"), ref model.DummyName))
                    Name = model.DummyName;
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(Main.Lang.Get("KEYCONFIG_KEY_CODE", "Key Code") + (model.Code == KeyCode.Menu ? " (Fake)" : ""));
                    Drawer.DrawEnum(Main.Lang.Get("KEYCONFIG_KEY_CODE", "Key Code"), ref model.Code, model.GetHashCode());
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            bool prevBgBlurEnabled = model.BackgroundBlurEnabled;
            bool changed = false;
            changed |= Drawer.DrawString(Main.Lang.Get("KEYCONFIG_TEXT_FONT", "Text Font"), ref model.Font, true);
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
            changed |= Drawer.DrawBool(Main.Lang.Get("KEYCONFIG_UPDATE_TEXT_ALWAYS", "Update Text Always"), ref model.UpdateTextAlways);
            changed |= Drawer.DrawBool(Main.Lang.Get("KEYCONFIG_ENABLE_COUNT_TEXT", "Enable Count Text"), ref model.EnableCountText);
            changed |= Drawer.DrawBool(Main.Lang.Get("KEYCONFIG_ENABLE_OUTLINE_IMAGE", "Enable Outline Image"), ref model.EnableOutlineImage);
            changed |= Drawer.DrawBool(Main.Lang.Get("KEYCONFIG_DISABLE_SORTING", "Disable Sorting"), ref model.DisableSorting);
            changed |= Drawer.DrawBool(Main.Lang.Get("KEYCONFIG_DO_NOT_SCALE_TEXT", "Do Not Scale Text"), ref model.DoNotScaleText);
            changed |= Drawer.DrawBool(Main.Lang.Get("KEYCONFIG_ENABLE_BACKGROUND_BLUR", "Enable Backgruond Blur"), ref model.BackgroundBlurEnabled);
            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("KEYCONFIG_TEXT_FONT_SIZE", "Text Font Size"), ref model.TextFontSize, 0, 300, 300);
            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("KEYCONFIG_COUNT_TEXT_FONT_SIZE", "Count Text Font Size"), ref model.CountTextFontSize, 0, 300, 300);

            changed |= Drawer.DrawPressReleaseH(Main.Lang.Get("KEYCONFIG_TEXT", "Text "), model.Text, Drawer.CD_H_STR);
            if (model.EnableCountText)
                changed |= Drawer.DrawPressReleaseH(Main.Lang.Get("KEYCONFIG_COUNT_TEXT", "Count Text"), model.CountText, Drawer.CD_H_STR);
            changed |= Drawer.DrawPressReleaseH(Main.Lang.Get("KEYCONFIG_BACKGROUND_IMAGE", "Background Image"), model.Background, Drawer.CD_H_STR_TRIMQUOTE);
            if (model.EnableOutlineImage)
                changed |= Drawer.DrawPressReleaseH(Main.Lang.Get("KEYCONFIG_OUTLINE_IMAGE", "Outline Image"), model.Outline, Drawer.CD_H_STR_TRIMQUOTE);
            if (model.BackgroundBlurEnabled)
                changed |= Drawer.DrawBlurConfig(string.Format(Main.Lang.Get("KEYCONFIG_KEY_BACKGROUND", "Key {0} Background"), KeyViewerUtils.KeyName(model)), model.BackgroundBlurConfig);

            changed |= Drawer.DrawVectorConfig(model.VectorConfig);

            Drawer.DrawObjectConfig(Main.Lang.Get("KEYCONFIG_EDIT_TEXT_CONFIG", "Edit Text Config"), string.Format(Main.Lang.Get("KEYCONFIG_KEY_TEXT", "Key {0} Text"), (model.DummyName != null ? model.DummyName : model.Code)), model.TextConfig, () => manager.UpdateLayout());
            if(model.EnableCountText)
                Drawer.DrawObjectConfig(Main.Lang.Get("KEYCONFIG_EDIT_COUNT_TEXT_CONFIG", "Edit Count Text Config"), string.Format(Main.Lang.Get("KEYCONFIG_EDIT_COUNT_TEXT_CONFIG", "Edit Count Text Config"), model.DummyName != null ? model.DummyName : model.Code), model.CountTextConfig, () => manager.UpdateLayout());
            Drawer.DrawObjectConfig(Main.Lang.Get("KEYCONFIG_EDIT_BACKGROUND_CONFIG", "Edit Background Config"), string.Format(Main.Lang.Get("KEYCONFIG_KEY_BACKGROUND", "Key {0} Background"), model.DummyName != null ? model.DummyName : model.Code), model.BackgroundConfig, () => manager.UpdateLayout());
            if (model.EnableOutlineImage)
                Drawer.DrawObjectConfig(Main.Lang.Get("KEYCONFIG_EDIT_OUTLINE_CONFIG", "Edit Outline Config"), string.Format(Main.Lang.Get("KEYCONFIG_KEY_OUTLINE", "Key {0} Outline"), model.DummyName != null ? model.DummyName : model.Code), model.OutlineConfig, () => manager.UpdateLayout());

            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("KEYCONFIG_BACKGROUND_IMAGE_ROUNDNESS", "Background Image Roundness"), ref model.BackgroundRoundness, 0, Constants.Rad2Deg100, 300);
            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("KEYCONFIG_OUTLINE_IMAGE_ROUNDNESS", "Outline Image Roundness"), ref model.OutlineRoundness, 0, Constants.Rad2Deg100, 300);

            changed |= Drawer.DrawBool(Main.Lang.Get("KEYCONFIG_ENABLE_RAIN", "Enable Rain"), ref model.RainEnabled);
            if (model.RainEnabled)
                Drawer.TitleButton(Main.Lang.Get("KEYCONFIG_EDIT_RAIN_CONFIG", "Edit Rain Config"), Main.Lang.Get("MISC_EDIT", "Edit"), () => Main.GUI.Push(new RainConfigDrawer(manager, model)));

            if (changed)
            {
                if (!prevBgBlurEnabled && model.BackgroundBlurEnabled)
                    KeyViewerUtils.ApplyBlurColorConfig(model);
                manager.UpdateLayout();
            }
        }
    }
}
