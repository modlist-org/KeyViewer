using KeyViewer.Core;
using KeyViewer.Models;
using KeyViewer.Unity;
using KeyViewer.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using TEP = KeyViewer.Core.Translation.TranslationKeys.EncryptedProfile;

namespace KeyViewer.Views
{
    public class EncryptedProfileDrawer : ModelDrawable<EncryptedProfile>
    {
        private byte[] encProfile;
        private Metadata meta;
        private string key;
        private bool tryDecrypting;
        private string resultMessage;
        private bool success;
        public EncryptedProfileDrawer(byte[] encProfile) : base(null, L(TEP.Prefix))
        {
            this.encProfile = encProfile;
            Open().Await();
        }
        public override void Draw()
        {
            if (encProfile == null)
            {
                Drawer.ButtonLabel(L(TEP.Damaged));
                return;
            }
            if (meta == null)
            {
                Drawer.ButtonLabel(L(TEP.Opening));
                return;
            }
            GUILayout.BeginHorizontal();
            Drawer.ButtonLabel(L(TEP.NameFormat, meta.Name));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Drawer.ButtonLabel(L(TEP.AuthorFormat, meta.Author));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            Drawer.ButtonLabel(L(TEP.DescriptionFormat, meta.Description));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            var d = DateTime.FromBinary(meta.CreationTick);
            Drawer.ButtonLabel(L(TEP.CreationTimeFormat, $"{d.Year}/{d.Month}/{d.Day} {d.Hour}:{d.Minute}:{d.Second}"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (tryDecrypting)
                    Drawer.ButtonLabel(L(TEP.Decrypting));
                else
                {
                    Drawer.ButtonLabel(L(TEP.Key));
                    key = GUILayout.TextField(key);
                    if (Drawer.Button(L(TEP.Import)))
                    {
                        tryDecrypting = true;
                        Decrypt().ContinueWith(t =>
                        {
                            if (success) Main.GUI.Pop();
                        }).Await();
                    }
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(resultMessage))
                Drawer.ButtonLabel(resultMessage);
        }
        private async Task Open()
        {
            model = await KeyViewerWebAPI.OpenEncryptedProfile(encProfile);
            meta = model?.Metadata;
            if (meta == null) encProfile = null;
        }
        private async Task Decrypt()
        {
            success = false;
            try
            {
                var profile = await KeyViewerWebAPI.DecryptRawProfile(model.RawProfile, key);
                if (profile != null)
                {
                    StaticCoroutine.QAct(() => Main.Settings.ActiveProfiles.Add(Main.CreateManagerImmediate(meta.Name, profile, key).activeProfile));
                    resultMessage = L(TEP.Success);
                    success = true;
                }
                else resultMessage = L(TEP.KeyMismatch);
            }
            catch { resultMessage = L(TEP.KeyMismatch); }
            finally { tryDecrypting = false; }
        }
    }
}
