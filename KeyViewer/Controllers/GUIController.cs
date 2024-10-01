﻿using KeyViewer.Core;
using KeyViewer.Core.Interfaces;
using KeyViewer.Core.Translation;
using KeyViewer.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KeyViewer.Controllers
{
    public class GUIController
    {
        private List<IDrawable> drawables = new List<IDrawable>();
        private int depth;
        private bool isUndoAvailable => depth > 0;
        private bool isRedoAvailable => depth < drawables.Count;
        private IDrawable current;
        private IDrawable first;
        private int skipFrames = 0;
        private Stack<Action> onSkipCallbacks = new Stack<Action>();
        public void Init(IDrawable drawable)
        {
            first = current = drawable;
        }
        public void Push(IDrawable drawable)
        {
            if (drawables.Count == depth)
            {
                drawables.Add(current);
                depth++;
            }
            else
            {
                if (drawable.Name != drawables[depth].Name)
                {
                    drawables.RemoveRange(depth, drawables.Count - depth);
                    drawables.Add(current);
                    depth++;
                }
                else drawables[depth++] = current;
            }
            current = drawable;
        }
        public void Pop()
        {
            if (!isUndoAvailable) return;
            var cache = current;
            current = drawables[--depth];
            drawables[depth] = cache;
        }
        public void Draw()
        {
            if (skipFrames > 0)
            {
                skipFrames--;
                if (onSkipCallbacks.Count > 0)
                    onSkipCallbacks.Pop()?.Invoke();
                return;
            }
            GUILayout.BeginHorizontal();
            {
                if (isUndoAvailable)
                {
                    if (Drawer.Button("◀ " + drawables[depth - 1].Name))
                        Pop();
                }
                if (isRedoAvailable)
                {
                    var draw = drawables[depth];
                    if (Drawer.Button(draw.Name + " ▶"))
                        Push(draw);
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            current.Draw();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Main.Lang[TranslationKeys.KO], GUI.skin.label)) KeyViewerUtils.OpenMysteryUrl();
            if (GUILayout.Button(Main.Lang[TranslationKeys.LINK], GUI.skin.label)) KeyViewerUtils.OpenDiscord2Url();
            GUILayout.Space(1);
            if (GUILayout.Button(Main.Lang[TranslationKeys.KW], GUI.skin.label)) KeyViewerUtils.OpenMysteryUrl();
            if (GUILayout.Button(Main.Lang[TranslationKeys.LINK], GUI.skin.label)) KeyViewerUtils.OpenWikiUrl();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            string[] ver = Main.Lang[TranslationKeys.Misc.RealLatestVersion].Split('.');
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Square Mod Server"))
                Application.OpenURL(Main.DiscordLink);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (GUILayout.Button(Main.Lang[TranslationKeys.BOATK], GUI.skin.label))
                Application.OpenURL("https://github.com/PizzaLovers007/AdofaiTweaks/tree/master/AdofaiTweaks/Tweaks/KeyViewer");
        }
        public void Skip(Action onSkip = null, int frames = 1)
        {
            skipFrames += frames;
            onSkipCallbacks.Push(onSkip);
        }
        public void Flush()
        {
            current = first;
            drawables = new List<IDrawable>();
            depth = 0;
            onSkipCallbacks = new Stack<Action>();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false);
        }
    }
}
