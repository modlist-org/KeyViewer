using KeyViewer.Core;
using KeyViewer.Models;
using KeyViewer.Utils;
using Overlayer.Core.Translation;
using RapidGUI;
using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KeyViewer.Views
{
    public class SettingsDrawer : ModelDrawable<Settings>
    {
        public SettingsDrawer(Settings settings) : base(settings, Main.Lang.Get("SETTINGS", "Settings")) { }

        private bool isOpenedExtraMenu = false;

        public override void Draw()
        {
            int translatorsCount = Main.Lang.GetArrCount("0TRANSLATORS");
            string translatorsText = "[UNKNOWN]";

            if(translatorsCount > 0) {
                var names = new List<string>();
                for(int i = 0; i < translatorsCount; i++) {
                    names.Add(Main.Lang.GetArr("0TRANSLATORS", i, "[UNKNOWN]"));
                }
                translatorsText = string.Join(" & ", names);
            }
            GUILayout.Label($"{Main.Lang.Get("SETTINGS_SELECT_LANGUAGE", "Select Language")} | {Main.Lang.CurrentLanguage} by {translatorsText}");
            GUILayout.BeginHorizontal();
            string[] languageNames = Main.Lang.GetLanguages();
            int selectedIndex = Array.IndexOf(languageNames, Main.Lang.CurrentLanguage);

            if(Drawer.Button("◀", GUILayout.Width(40))) {
                selectedIndex = (selectedIndex - 1 + languageNames.Length) % languageNames.Length;
                UpdateLanguageSetting(selectedIndex);
            }

            if(Drawer.SelectionPopup(ref selectedIndex, languageNames, "", GUILayout.Width(400))) {
                UpdateLanguageSetting(selectedIndex);
            }
            if(Drawer.Button("▶", GUILayout.Width(40))) {
                selectedIndex = (selectedIndex + 1) % languageNames.Length;
                UpdateLanguageSetting(selectedIndex);
            }

            void UpdateLanguageSetting(int index) {
                Main.Lang.CurrentLanguage = languageNames[index];
                model.Lang = Main.Lang.CurrentLanguage;
            }
            if(Drawer.Button(Main.Lang.GetFail() ? TranslatorHelper.FailString(Main.Lang) : Main.Lang.Get("RELOADLANG", "Reload Language Pack"), GUILayout.Width(320))) {
                _ = Main.Lang.LoadTranslationsAsync(Path.Combine(Main.Mod.Path, "lang"));
                Main.Lang.CurrentLanguage = model.Lang;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(Drawer.Button(Main.Lang.Get("EXTRA_MENU", "Extra Menu") + " " + (isOpenedExtraMenu ? "▼" : "▲"))) {
                isOpenedExtraMenu = !isOpenedExtraMenu;
            }
            /*
            if(Drawer.Button(Main.Lang.Get("OPEN_WIKI_MENU","Open Wiki Menu"))) {
                if(Main.Wiki == null) {
                    Main.Wiki = new GameObject().AddComponent<Wiki.Wiki>();
                    UnityEngine.Object.DontDestroyOnLoad(Main.Wiki);
                } else {
                    Main.Wiki.BringToFrontOnce();
                }
            }
            */
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if(isOpenedExtraMenu) {
                if(Drawer.DrawBool(
                        string.Format(Main.Lang.Get("USE_THIS", "Use {0}"), Main.Lang.Get("LEGACY_THEME", "Legacy Theme")),
                        ref model.useLegacyTheme)) {
                    Drawer.SetStyle(model.useLegacyTheme);
                    RGUIStyle.CreateStyles();
                }
            }
            GUILayout.BeginHorizontal();
            if(Drawer.Button(Main.Lang.Get("SETTINGS_IMPORT_PROFILE", "Import Profile"))) {
                var profiles = StandaloneFileBrowser.OpenFilePanel(Main.Lang.Get("SETTINGS_SELECT_PROFILE", "Select Profile"), Main.Mod.Path, new[] { new ExtensionFilter("V4", "json"), new ExtensionFilter("V3", "xml"), }, true);
                foreach(var profile in profiles) {
                    FileInfo file = new FileInfo(profile);
                    if(file.Extension == ".json") {
                        if(!File.Exists(Path.Combine(Main.Mod.Path, file.Name)))
                            file.CopyTo(Path.Combine(Main.Mod.Path, file.Name));
                        var activeProfile = new ActiveProfile(Path.GetFileNameWithoutExtension(file.FullName), true);
                        model.ActiveProfiles.Add(activeProfile);
                        Main.AddManager(activeProfile, true);
                    } else if(file.Extension == ".xml")
                        Main.MigrateFromV3Xml(file.FullName);
                }
            }
            if(Drawer.Button(Main.Lang.Get("SETTINGS_CREATE_PROFILE", "Create New Profile"))) {
                var profile = new ActiveProfile(GetNewProfileName(), true);
                model.ActiveProfiles.Add(profile);
                Profile newProfile = new Profile();
                File.WriteAllText(Path.Combine(Main.Mod.Path, $"{profile.Name}.json"), newProfile.Serialize().ToString(4));
                Main.AddManager(profile, true);
            }
            if(Drawer.Button(Main.Lang.Get("SETTINGS_OPEN_MOD_DIR", "Open Mod Directory")))
                Application.OpenURL(Path.GetFullPath(Main.Mod.Path));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            for (int i = 0; i < model.ActiveProfiles.Count; i++)
            {
                GUILayout.BeginHorizontal();
                var profile = model.ActiveProfiles[i];
                bool profileActiveDiff = Drawer.DrawOnlyBool(ref profile.Active);
                if(profileActiveDiff) {
                    if(profile.Active && !Main.Managers.TryGetValue(profile.Name, out _))
                        Main.AddManager(profile, true);
                    if(!profile.Active && Main.Managers.TryGetValue(profile.Name, out var m))
                        Main.RemoveManager(profile);
                    model.ActiveProfiles[i] = profile;
                }
                GUI.color = profile.Active ? new Color(0.8f, 0.8f, 1f) : Color.gray;
                if(Drawer.Button(Main.Lang.Get("EDIT", "Edit")) && profile.Active) {
                    var manager = Main.Managers[profile.Name];
                    Main.GUI.Push(new ProfileDrawer(manager, manager.profile, profile.Name));
                }
                GUI.color = new Color(1f, 0.8f, 0.8f);
                if(Drawer.Button(Main.Lang.Get("DESTROY", "Destroy"))) {
                    Main.RemoveManager(profile);
                    string path = Path.Combine(Main.Mod.Path, $"{profile.Name}.json");
                    //File.Delete(path);
                    Main.ToDeleteFiles.Add(path);
                    model.ActiveProfiles.RemoveAll(p => p.Name == profile.Name);
                    break;
                }
                GUI.color = new Color(1f, 0.8f, 1f);
                if(Drawer.Button(Main.Lang.Get("EXPORT", "Export"))) {
                    string target = StandaloneFileBrowser.SaveFilePanel(Main.Lang.Get("SETTINGS_SELECT_PROFILE", "Select Profile"), Persistence.GetLastUsedFolder(), $"{profile.Name}.json", "json");
                    if(!string.IsNullOrWhiteSpace(target)) {
                        Profile p = Main.Managers[profile.Name].profile;
                        var node = p.Serialize();
                        node["References"] = ProfileImporter.GetReferencesAsJson(p);
                        File.WriteAllText(target, node.ToString(4));
                    }
                }
                GUI.color = Color.white;
                GUILayout.Label(profile.Name);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }
        private static int newProfileNum = 1;
        private static string GetNewProfileName()
        {
            string result = "Profile " + newProfileNum + ".json";
            while (File.Exists(Path.Combine(Main.Mod.Path, result)))
                result = "Profile " + ++newProfileNum + ".json";
            return $"Profile {newProfileNum++}";
        }
    }
}
