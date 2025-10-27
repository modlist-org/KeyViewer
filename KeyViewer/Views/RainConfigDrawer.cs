using KeyViewer.Core;
using KeyViewer.Models;
using KeyViewer.Unity;
using KeyViewer.Utils;
using UnityEngine;

namespace KeyViewer.Views
{
    public class RainConfigDrawer : ModelDrawable<RainConfig>
    {
        public KeyManager manager;
        public KeyConfig config;
        private bool imageListExpanded = false;
        public RainConfigDrawer(KeyManager manager, KeyConfig config) : base(config.Rain, string.Format(Main.Lang.Get("RAINCONFIG_RAIN_CONFIGURATION", "Key {0} Rain Configuration"), config.DummyName != null ? config.DummyName : config.Code))
        {
            this.manager = manager;
            this.config = config;
        }
        public override void Draw()
        {
            bool changed = false;
            string name = config.DummyName ?? config.Code.ToString();
            GUILayout.Label(Name);
            changed |= Drawer.DrawInt32(Main.Lang.Get("RAINCONFIG_RAIN_POOL_SIZE", "Rain Pool Size"), ref model.PoolSize);
            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("MISC_ROUNDNESS", "Roundness"), ref model.Roundness, 0, Constants.Rad2Deg100, 300);
            //changed |= Drawer.DrawBool(L(TKRC.BlurEnabled), ref model.BlurEnabled);
            changed |= Drawer.DrawPressReleaseH(Main.Lang.Get("RAINCONFIG_RAIN_SPEED", "Rain Speed"), model.Speed, Drawer.CD_H_FLT_SPEEDONLY);
            changed |= Drawer.DrawPressReleaseH(Main.Lang.Get("RAINCONFIG_RAIN_LENGTH", "Rain Length"), model.Length, Drawer.CD_H_FLT_LENGTHONLY);
            changed |= Drawer.DrawPressReleaseH(Main.Lang.Get("RAINCONFIG_RAIN_SOFTNESS", "Rain Softness"), model.Softness, Drawer.CD_H_INT32_SOFTNESSONLY);

            GUILayoutEx.ExpandableGUI(() =>
            {
                changed |= Drawer.DrawList(model.RainImages, (ref RainImage i) =>
                {
                    bool result = false;
                    result |= Drawer.DrawString(Main.Lang.Get("RAINCONFIG_RAIN_IMAGE_PATH", "Image Path"), ref i.Image);
                    result |= Drawer.DrawInt32(Main.Lang.Get("RAINCONFIG_RAIN_IMAGE_COUNT", "Image Count"), ref i.Count);
                    result |= Drawer.DrawSingleWithSlider(Main.Lang.Get("MISC_ROUNDNESS", "Roundness"), ref i.Roundness, 0, Constants.Rad2Deg100, 300);
                    //result |= Drawer.DrawBlurConfig(L(TKM.BlurConfig, i), i.BlurConfig);
                    return result;
                });
            }, Main.Lang.Get("RAINCONFIG_RAIN_IMAGES", "Rain Images"), ref imageListExpanded);

            Drawer.DrawObjectConfig(Main.Lang.Get("RAINCONFIG_EDIT_RAIN_CONFIG", "Edit Rain Object Config"), string.Format(Main.Lang.Get("RAINCONFIG_KEY_RAIN", "Key {0} Rain"), name), model.ObjectConfig, () => manager.UpdateLayout());

            //if (model.BlurEnabled) changed |= Drawer.DrawBlurConfig(L(TKM.BlurConfig, Name), model.BlurConfig);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(string.Format(Main.Lang.Get("RAINCONFIG_DIRECTION", "Key {0} Rain Direction"), name));
                changed |= Drawer.DrawEnum(string.Format(Main.Lang.Get("RAINCONFIG_DIRECTION", "Key {0} Rain Direction"), name), ref model.Direction);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(string.Format(Main.Lang.Get("RAINCONFIG_IMAGE_DISPLAY_MODE", "Key {0} Rain Image Display Mode"), name));
                changed |= Drawer.DrawEnum(string.Format(Main.Lang.Get("RAINCONFIG_IMAGE_DISPLAY_MODE", "Key {0} Rain Image Display Mode"), name), ref model.ImageDisplayMode);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (changed) manager.UpdateLayout();
        }
    }
}
