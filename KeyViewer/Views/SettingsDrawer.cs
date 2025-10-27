using KeyViewer.Core;
using KeyViewer.Core.Translation;
using KeyViewer.Models;
using KeyViewer.Utils;
using SFB;
using System.IO;
using UnityEngine;
using TKP = KeyViewer.Core.Translation.TranslationKeys.Profile;
using TKS = KeyViewer.Core.Translation.TranslationKeys.Settings;

namespace KeyViewer.Views
{
    public class SettingsDrawer : ModelDrawable<Settings>
    {
        public SettingsDrawer(Settings settings) : base(settings, L(TKS.Prefix)) { }
        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            {
                Drawer.ButtonLabel(L(TKS.SelectLanguage));
                if (Drawer.DrawEnum(L(TKS.Language), ref model.Language))
                {
                    Main.GUI.Skip(() =>
                    {
                        //KeyViewerUtils.OpenDiscordUrl();
                        Main.Lang = Language.GetLanguage(model.Language);
                        Main.OnLanguageInitialize();
                    });
                }
                //if (Drawer.Button(L(TKM.ShowUpdateNote)))
                //    Main.GUI.Push(new MethodDrawable(() =>
                //    {
                //        GUILayout.Label(L(TK.UpdateNote));
                //    }, L(TKM.ShowUpdateNote)));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (Drawer.Button(L(TKS.ImportProfile)))
                {
                    var profiles = StandaloneFileBrowser.OpenFilePanel(L(TKS.SelectProfile), Main.Mod.Path, new[] { new ExtensionFilter("V4", "json", "encryptedProfile"), new ExtensionFilter("V3", "xml"), }, true);
                    foreach (var profile in profiles)
                    {
                        FileInfo file = new FileInfo(profile);
                        if (file.Extension == ".json")
                        {
                            if (!File.Exists(Path.Combine(Main.Mod.Path, file.Name)))
                                file.CopyTo(Path.Combine(Main.Mod.Path, file.Name));
                            var activeProfile = new ActiveProfile(Path.GetFileNameWithoutExtension(file.FullName), true);
                            model.ActiveProfiles.Add(activeProfile);
                            Main.AddManager(activeProfile, true);
                        }
                        else if (file.Extension == ".xml")
                            Main.MigrateFromV3Xml(file.FullName);
                        else if (file.Extension == ".encryptedProfile")
                            Main.GUI.Push(new EncryptedProfileDrawer(File.ReadAllBytes(file.FullName)));
                    }
                }
                if (Drawer.Button(L(TKS.CreateProfile)))
                {
                    var profile = new ActiveProfile(GetNewProfileName(), true);
                    model.ActiveProfiles.Add(profile);
                    Profile newProfile = new Profile();
                    File.WriteAllText(Path.Combine(Main.Mod.Path, $"{profile.Name}.json"), newProfile.Serialize().ToString(4));
                    Main.AddManager(profile, true);
                }
                if (Drawer.Button(L(TKS.OpenModDir)))
                    Application.OpenURL(Path.GetFullPath(Main.Mod.Path));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            for (int i = 0; i < model.ActiveProfiles.Count; i++)
            {
                GUILayout.BeginHorizontal();
                {
                    var profile = model.ActiveProfiles[i];
                    var encrypted = profile.Key != null;
                    Drawer.ButtonLabel(profile.Name);
                    var newActive = GUILayout.Toggle(profile.Active, string.Empty);
                    if (profile.Active != newActive)
                    {
                        profile.Active = newActive;
                        if (newActive && !Main.Managers.TryGetValue(profile.Name, out _))
                            Main.AddManager(profile, true);
                        if (!newActive && Main.Managers.TryGetValue(profile.Name, out var m))
                            Main.RemoveManager(profile);
                        model.ActiveProfiles[i] = profile;
                    }
                    if (!encrypted && profile.Active)
                    {
                        var manager = Main.Managers[profile.Name];
                        if (Drawer.Button(L(TKP.ConfigurateProfile, profile.Name)))
                            Main.GUI.Push(new ProfileDrawer(manager, manager.profile, profile.Name));
                    }
                    if (!encrypted && Drawer.Button(L(TKS.ExportProfile)))
                    {
                        string target = StandaloneFileBrowser.SaveFilePanel(L(TKS.SelectProfile), Persistence.GetLastUsedFolder(), $"{profile.Name}.json", "json");
                        if (!string.IsNullOrWhiteSpace(target))
                        {
                            Profile p = Main.Managers[profile.Name].profile;
                            var node = p.Serialize();
                            node["References"] = ProfileImporter.GetReferencesAsJson(p);
                            File.WriteAllText(target, node.ToString(4));
                        }
                    }
                    if (!encrypted && Drawer.Button(L(TKS.ExportCryptedProfile)))
                        Main.GUI.Push(new EncryptedProfileSaveDrawer(profile));
                    if (Drawer.Button(L(TKP.DeleteProfile)))
                    {
                        Main.RemoveManager(profile);
                        string path;
                        if (encrypted)
                            path = Path.Combine(Main.Mod.Path, $"{profile.Name}.encryptedProfile");
                        else path = Path.Combine(Main.Mod.Path, $"{profile.Name}.json");
                        //File.Delete(path);
                        Main.ToDeleteFiles.Add(path);
                        model.ActiveProfiles.RemoveAll(p => p.Name == profile.Name);
                        break;
                    }
                }
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
