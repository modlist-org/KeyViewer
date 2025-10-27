using KeyViewer.Core;
using KeyViewer.Models;
using KeyViewer.Unity;
using KeyViewer.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KeyViewer.Views
{
    public class ProfileDrawer : ModelDrawable<Profile>
    {
        public KeyManager manager;
        private bool listening = false;
        private bool configMode = true;
        private int dummyNumber = 1;
        private HashSet<KeyConfig> selectedKeys = new HashSet<KeyConfig>();
        private KeyConfig criterion;
        public ProfileDrawer(KeyManager manager, Profile profile, string name) : base(profile, string.Format(Main.Lang.Get("PROFILE_CONFIGURATE_PROFILE", "Configurate {0} Profile"), name))
        {
            this.manager = manager;
        }
        public override void Draw()
        {
            bool changed = false;
            Drawer.DrawBool(Main.Lang.Get("PROFILE_VIEW_ONLY_GAME_PLAY", "View Only Game Play"), ref model.ViewOnlyGamePlay);
            changed |= Drawer.DrawBool(Main.Lang.Get("PROFILE_LIMIT_NOT_REGISTERED_KEYS", "Limit Input Not Registered Keys"), ref model.LimitNotRegisteredKeys);
            changed |= Drawer.DrawBool(Main.Lang.Get("PROFILE_RESET_ON_START", "Reset On Start"), ref model.ResetOnStart);
            //if (model.DoNotAssAss) Drawer.DrawBool(L(TKP.DoNotAssAss), ref model.DoNotAssAss);
            changed |= Drawer.DrawInt32(Main.Lang.Get("PROFILE_KPS_UPDATE_RATE", "KPS Update Rate"), ref model.KPSUpdateRate);
            changed |= Drawer.DrawSingleWithSlider(Main.Lang.Get("PROFILE_KEY_SPACING", "Key Spacing"), ref model.KeySpacing, 0, 100, 300f);
            changed |= Drawer.DrawVectorConfig(model.VectorConfig);
            GUILayoutEx.HorizontalLine(1);
            DrawKeyConfigGUI();
            if (changed) manager.UpdateLayout();
        }
        public override void OnKeyDown(KeyCode code)
        {
            if (code == KeyCode.Mouse0) return;
            if (model.Keys.Any(kc => kc.Code == code))
                model.Keys.RemoveAll(kc => kc.Code == code);
            else model.Keys.Add(new KeyConfig() { Code = code });
            manager.UpdateKeys();
        }
        private void DrawKeyConfigGUI()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(Main.Lang.Get("PROFILE_REGISTERED_KEYS", "Registered Keys"));
                if (model.Keys.Any(k => !selectedKeys.Contains(k)))
                {
                    if (Drawer.Button(Main.Lang.Get("PROFILE_SELECT_ALL_KEYS", "Select All Keys")))
                        model.Keys.ForEach(k => selectedKeys.Add(k));
                }
                else
                {
                    if (Drawer.Button(Main.Lang.Get("PROFILE_DESELECT_ALL_KEYS", "Deselect All Keys")))
                    {
                        selectedKeys.Clear();
                        criterion = null;
                    }
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    for (int i = 0; i < model.Keys.Count; i++)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            var key = model.Keys[i];
                            var str = key.DummyName != null ? key.DummyName : key.Code.ToString();

                            // ㅄ같은 비동기 때문에 예외조건 추가
                            if (key.Code == KeyCode.Menu) str = "RightAlt";

                            var selected = selectedKeys.Contains(key);
                            if (criterion == key) str = $"<color=yellow>{str}</color>";
                            else if (selected) str = $"<color=cyan>{str}</color>";
                            if (Drawer.Button(str))
                            {
                                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                                {
                                    if (!selectedKeys.Add(key))
                                    {
                                        if (criterion != key)
                                            criterion = key;
                                        else
                                        {
                                            selectedKeys.Remove(key);
                                            criterion = null;
                                        }
                                    }
                                }
                                else if (configMode)
                                    Main.GUI.Push(new KeyConfigDrawer(manager, key));
                                else
                                {
                                    model.Keys.RemoveAt(i);
                                    manager.UpdateKeys();
                                }
                                break;
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (Drawer.Button(!listening ? Main.Lang.Get("PROFILE_START_KEY_REGISTERING", "Start Key Register") : Main.Lang.Get("PROFILE_STOP_KEY_REGISTERING", "Stop Key Register")))
                    {
                        if (Main.ListeningDrawer != null)
                            Main.ListeningDrawer = null;
                        else Main.ListeningDrawer = this;
                        listening = Main.ListeningDrawer != null;
                    }
                    GUILayout.Space(10);
                    if (Drawer.Button(!configMode ? string.Format(Main.Lang.Get("MISC_ENABLE", "Enable {0}"), Main.Lang.Get("PROFILE_CONFIGURATION_MODE", "Configuration Mode")) : string.Format(Main.Lang.Get("MISC_DISABLE", "Disable {0}"), Main.Lang.Get("PROFILE_CONFIGURATION_MODE", "Configuration Mode"))))
                        configMode = !configMode;
                    GUILayout.Space(10);
                    if (Drawer.Button(Main.Lang.Get("PROFILE_CREATE_DUMMY_KEY", "Create Dummy Key")))
                    {
                        var dummy = new KeyConfig() { DummyName = string.Format(Main.Lang.Get("PROFILE_DUMMY_NAME", "Dummy {0}"), dummyNumber++) };
                        model.Keys.Add(dummy);
                        manager.UpdateKeys();
                    }
                    if (!model.Keys.Any(k => k.Code == KeyCode.Mouse0))
                    {
                        GUILayout.Space(10);
                        if (Drawer.Button(Main.Lang.Get("PROFILE_REGISTER_MOUSE0_KEY", "Register Left Click Key")))
                        {
                            model.Keys.Add(new KeyConfig() { Code = KeyCode.Mouse0 });
                            manager.UpdateKeys();
                        }
                    }
                    if (!model.Keys.Any(k => k.Code == KeyCode.RightAlt))
                    {
                        GUILayout.Space(10);
                        if (Drawer.Button(Main.Lang.Get("PROFILE_REGISTER_RALT_KEY", "Register RAlt Key")))
                        {
                            model.Keys.Add(new KeyConfig() { Code = KeyCode.RightAlt });
                            manager.UpdateKeys();
                        }
                    }
                    if (!model.Keys.Any(k => k.Code == KeyCode.RightControl))
                    {
                        GUILayout.Space(10);
                        if (Drawer.Button(Main.Lang.Get("PROFILE_REGISTER_RCTRL_KEY", "Register RCtrl Key")))
                        {
                            model.Keys.Add(new KeyConfig() { Code = KeyCode.RightControl });
                            manager.UpdateKeys();
                        }
                    }
                    if (selectedKeys.Count == 2)
                    {
                        GUILayout.Space(10);
                        if (Drawer.Button(Main.Lang.Get("PROFILE_SWAP_KEYS", "Swap Keys Order")))
                        {
                            var list = selectedKeys.ToList();
                            int a = model.Keys.IndexOf(list[0]);
                            int b = model.Keys.IndexOf(list[1]);
                            var temp = model.Keys[a];
                            model.Keys[a] = model.Keys[b];
                            model.Keys[b] = temp;
                            manager.UpdateKeys();
                            selectedKeys.Clear();
                            criterion = null;
                        }
                    }
                    if (selectedKeys.Count > 0)
                    {
                        GUILayout.Space(10);
                        if (Drawer.Button(Main.Lang.Get("PROFILE_MAKE_BAR", "Make Bar Key")))
                        {
                            KeyViewerUtils.MakeBar(manager.profile, manager.keys.FindAll(k => selectedKeys.Contains(k.Config)).Select(k => k.Config).ToList());
                            manager.UpdateLayout();
                            selectedKeys.Clear();
                            criterion = null;
                        }
                    }
                    if (selectedKeys.Count > 1)
                    {
                        GUILayout.Space(10);
                        if (Drawer.Button(Main.Lang.Get("PROFILE_EDIT_MULTIPLE_KEY", "Edit Multiple Keys")))
                        {
                            Main.GUI.Push(new MultipleKeyConfigDrawer(manager, selectedKeys.Select(k => KeyViewerUtils.KeyName(k)).ToList(), criterion?.Copy()));
                            selectedKeys.Clear();
                            criterion = null;
                        }
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
}
