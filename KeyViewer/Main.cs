using System;
using HarmonyLib;
using JSON;
using KeyViewer.Controllers;
using KeyViewer.Core;
using KeyViewer.Core.Input;
using KeyViewer.Core.TextReplacing;
using KeyViewer.Core.Translation;
using KeyViewer.Migration.V3;
using KeyViewer.Models;
using KeyViewer.Patches;
using KeyViewer.Unity;
using KeyViewer.Utils;
using KeyViewer.Views;
using SkyHook;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using UnityEngine;
using static UnityModManagerNet.UnityModManager;
using static UnityModManagerNet.UnityModManager.ModEntry;
using Object = UnityEngine.Object;

namespace KeyViewer
{
    public static class Main
    {
        public static bool IsEnabled { get; private set; }
        public static bool IsPlaying { get; private set; }
        public static Language Lang { get; internal set; }
        public static ModEntry Mod { get; private set; }
        public static ModLogger Logger { get; private set; }
        public static Settings Settings { get; private set; }
        public static Dictionary<string, KeyManager> Managers { get; private set; }
        public static bool BlockInput { get; internal set; }
        public static ModelDrawable<Profile> ListeningDrawer { get; internal set; }
        public static Harmony Harmony { get; private set; }
        public static GUIController GUI { get; private set; }
        public static HttpClient HttpClient { get; private set; }
        public static System.Version LastestVersion { get; private set; }
        public static System.Version ModVersion { get; private set; }
        public static HashSet<string> ToDeleteFiles { get; private set; }
        public static event System.Action OnManagersInitialized = delegate { };
        public static bool IsWindows { get; private set; }
        public static void Load(ModEntry modEntry)
        {
            Mod = modEntry;
            Logger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnUpdate = OnUpdate;
            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnShowGUI = OnShowGUI;
            modEntry.OnHideGUI = OnHideGUI;
            modEntry.OnLateUpdate += OnLateUpdate;
            HttpClient = new HttpClient();
            modEntry.Info.Version = Constants.Version;
            typeof(ModEntry).GetField(nameof(ModEntry.Version)).SetValue(modEntry, ModVersion = System.Version.Parse(Constants.Version));
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
        public static bool OnToggle(ModEntry modEntry, bool toggle)
        {
            if (toggle)
            {
                WinInput.Initialize();
                Tag.InitializeWrapperAssembly();
                FontManager.Initialize();
                AssetManager.Initialize();
                JudgementColorPatch.Initialize();
                GUI = new GUIController();
                Settings = new Settings();
                if (File.Exists(Constants.SettingsPath))
                    Settings.Deserialize(JsonNode.Parse(File.ReadAllText(Constants.SettingsPath)));
                Managers = new Dictionary<string, KeyManager>();
                ToDeleteFiles = new HashSet<string>();
                List<string> notExistProfiles = new List<string>();
                var jsonProfiles = Settings.ActiveProfiles.Where(ap => ap.Key == null);
                var encryptedProfiles = Settings.ActiveProfiles.Where(ap => ap.Key != null);
                foreach (var profile in jsonProfiles.Concat(encryptedProfiles))
                {
                    if (!AddManager(profile))
                        notExistProfiles.Add(profile.Name);
                }
                Settings.ActiveProfiles.RemoveAll(p => notExistProfiles.Contains(p.Name));
                if (!Settings.ActiveProfiles.Any())
                {
                    File.WriteAllText(Path.Combine(Mod.Path, "Default.json"),
                        new Profile().Serialize().ToString(4));
                    var def = new ActiveProfile("Default", true);
                    Settings.ActiveProfiles.Add(def);
                    AddManager(def);
                }
                Lang = Language.GetLanguage(Settings.Language);
                Language.OnInitialize += OnLanguageInitialize;
                Harmony = new Harmony(modEntry.Info.Id);
                Harmony.PatchAll(Assembly.GetExecutingAssembly());
                StaticCoroutine.Run(InitializeManagersCo());
                IsEnabled = true;
            }
            else
            {
                IsEnabled = false;
                ReleaseManagers();
                ToDeleteFiles = null;
                Harmony.UnpatchAll(Harmony.Id);
                Harmony = null;
                Language.OnInitialize -= OnLanguageInitialize;
                JudgementColorPatch.Release();
                //AssetManager.Release();
                FontManager.Release();
                WinInput.Release();
                Tag.ReleaseWrapperAssembly();
                Resources.UnloadUnusedAssets();
                System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, true);
            }
            return true;
        }
        public static void OnUpdate(ModEntry modEntry, float deltaTime)
        {
            if (scrController.instance && scrConductor.instance)
                IsPlaying = !scrController.instance.paused && scrConductor.instance.isGameWorld;
            if (ListeningDrawer != null)
                foreach (var code in EnumHelper<KeyCode>.GetValues())
                    if (Input.GetKeyDown(code))
                        ListeningDrawer.OnKeyDown(code);
            foreach (var manager in Managers.Values)
            {
                bool showViewer = true;
                if (manager.profile.ViewOnlyGamePlay)
                    showViewer = IsPlaying;
                if (showViewer != manager.gameObject.activeSelf)
                    manager.gameObject.SetActive(showViewer);
            }
        }
        public static void OnGUI(ModEntry modEntry)
        {
            if (!Lang.Initialized)
                Drawer.ButtonLabel("Preparing...");
            else GUI.Draw();
        }
        public static void OnSaveGUI(ModEntry modEntry)
        {
            File.WriteAllText(Constants.SettingsPath, Settings.Serialize().ToString(4));
            foreach (var (name, manager) in Managers)
            {
                if (manager.encrypted) continue;
                File.WriteAllText(Path.Combine(Mod.Path, $"{name}.json"), manager.profile.Serialize().ToString(4));
            }
            foreach (var path in ToDeleteFiles)
                File.Delete(path);
        }
        public static void OnShowGUI(ModEntry modEntry)
        {
            BlockInput = true;
            GUI.Flush();
            ListeningDrawer = null;
        }
        public static void OnHideGUI(ModEntry modEntry)
        {
            GUI.Flush();
            ListeningDrawer = null;
            BlockInput = false;
        }
        public static void OnLateUpdate(ModEntry modEntry, float deltaTime)
        {
            WinInput.UpdatePrevStates();
        }
        public static void OnLanguageInitialize()
        {
            GUI.Flush();
            ListeningDrawer = null;
            GUI.Init(new SettingsDrawer(Settings));
        }
        public static bool AddManager(ActiveProfile profile, bool forceInit = false)
        {
            var hasKey = !string.IsNullOrWhiteSpace(profile.Key);
            var profilePath = !hasKey ?
                Path.Combine(Mod.Path, $"{profile.Name}.json") :
                Path.Combine(Mod.Path, $"{profile.Name}.encryptedProfile");
            if (File.Exists(profilePath))
            {
                if (profile.Active)
                {
                    if (hasKey)
                    {
                        if (Managers.TryGetValue(profile.Name, out _)) return true;
                        KeyViewerUtils.LoadEncryptedProfile(File.ReadAllBytes(profilePath), profile.Key);
                        return true;
                    }
                    var profileNode = JsonNode.Parse(File.ReadAllText(profilePath));
                    var p = ProfileImporter.Import(profileNode);
                    if (Managers.TryGetValue(profile.Name, out var manager))
                        Object.Destroy(manager);
                    Managers[profile.Name] = KeyManager.CreateManager(profile.Name, p);
                    if (forceInit)
                    {
                        Managers[profile.Name].Init();
                        Managers[profile.Name].UpdateKeys();
                        Logger.Log($"Initialized Key Manager {profile.Name}.");
                    }
                }
                return true;
            }
            return false;
        }
        public static void RemoveManager(ActiveProfile profile)
        {
            if (Managers.TryGetValue(profile.Name, out var manager))
            {
                Object.Destroy(manager.gameObject);
                Managers.Remove(profile.Name);
                Logger.Log($"Released Key Manager {profile.Name}.");
            }
        }
        public static (KeyManager manager, ActiveProfile activeProfile) CreateManagerImmediate(string name, Profile p, string key = null)
        {
            var profile = new ActiveProfile(name, true, key);
            var manager = KeyManager.CreateManager(profile.Name, p);
            manager.Init();
            manager.UpdateKeys();
            manager.encrypted = true;
            Managers[name] = manager;
            Logger.Log($"Initialized Key Manager {profile.Name}.");
            return (manager, profile);
        }
        public static IEnumerator InitializeManagersCo()
        {
            if (!AssetManager.Initialized)
                yield return new WaitUntil(() => !AssetManager.Initialized);
            foreach (var (name, manager) in Managers)
            {
                var elapsed = MiscUtils.MeasureTime(() =>
                {
                    manager.Init();
                    manager.UpdateKeys();
                });
                Logger.Log($"Initialized Key Manager {name}. ({elapsed.TotalMilliseconds}ms)");
                yield return null;
            }
            OnManagersInitialized();
            yield break;
        }
        public static void ReleaseManagers()
        {
            foreach (var (name, manager) in Managers)
            {
                Object.Destroy(manager.gameObject);
                Logger.Log($"Released Key Manager {name}.");
            }
            Managers = null;
        }
        public static void ResetKeys()
        {
            foreach (var manager in Managers.Values)
            {
                foreach (var key in manager.keys)
                {
                    if (!key) continue;
                    key.Pressed = false;
                    key.ResetRains();
                }
            }
        }
        public static void MigrateFromV3Xml(string path)
        {
            XmlSerializer serializer;
            try
            {
                serializer = new XmlSerializer(typeof(V3Settings), GetXAO(true));
                var v3s = serializer.Deserialize(File.OpenRead(path)) as V3Settings;
                var newSettings = V3Migrator.Migrate(v3s, out var profilesNode);
                foreach (var (name, manager) in Managers)
                {
                    Object.Destroy(manager.gameObject);
                    Logger.Log($"Released Key Manager {name}.");
                }
                Managers.Clear();
                for (int i = 0; i < newSettings.ActiveProfiles.Count; i++)
                {
                    var profile = newSettings.ActiveProfiles[i];
                    File.WriteAllText(Path.Combine(Mod.Path, $"{profile.Name}.json"), profilesNode[i].ToString(4));
                    AddManager(profile, true);
                }
                GUI.Flush();
                GUI.Init(new SettingsDrawer(Settings = newSettings));
                Logger.Log($"Successfully Migrated Settings Xml '{path}'");
            }
            catch (System.Exception e)
            {
                try
                {
                    serializer = new XmlSerializer(typeof(V3Profile), GetXAO(false));
                    var v3p = serializer.Deserialize(File.OpenRead(path)) as V3Profile;
                    var profile = V3Migrator.MigrateProfile(v3p);
                    File.WriteAllText(Path.Combine(Mod.Path, $"{v3p.Name}.json"), profile.Serialize().ToString(4));
                    var activeProfile = new ActiveProfile(v3p.Name, true);
                    Settings.ActiveProfiles.Add(activeProfile);
                    AddManager(activeProfile, true);
                    Logger.Log($"Successfully Migrated Profile Xml '{path}'");
                }
                catch (System.Exception ee) { Logger.Log($"Failed To Migrate Xml..\n{e}\n\n{ee}"); }
            }
        }
        public static V3Settings ReadV3Settings(string path)
        {
            var serializer = new XmlSerializer(typeof(V3Settings), GetXAO(true));
            return serializer.Deserialize(File.OpenRead(path)) as V3Settings;
        }
        public static V3Profile ReadV3Profile(string path)
        {
            var serializer = new XmlSerializer(typeof(V3Profile), GetXAO(true));
            return serializer.Deserialize(File.OpenRead(path)) as V3Profile;
        }
        private static XmlAttributeOverrides GetXAO(bool settings)
        {
            XmlAttributeOverrides xao = new XmlAttributeOverrides();

            if (settings)
            {
                XmlAttributes settingsAttr = new XmlAttributes();
                settingsAttr.XmlRoot = new XmlRootAttribute("Settings");
                xao.Add(typeof(V3Settings), settingsAttr);
            }
            else
            {
                XmlAttributes profileAttr = new XmlAttributes();
                profileAttr.XmlRoot = new XmlRootAttribute("Profile");
                xao.Add(typeof(V3Profile), profileAttr);
            }

            return xao;
        }
    }
}
