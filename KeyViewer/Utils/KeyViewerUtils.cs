using DG.Tweening;
using KeyViewer.Core;
using KeyViewer.Core.Interfaces;
using KeyViewer.Models;
using KeyViewer.Unity;
using KeyViewer.Unity.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CameraFilterPack_NightVisionFX;

namespace KeyViewer.Utils
{
    public static class KeyViewerUtils
    {
        /// <summary>
        /// Color (1, 1, 1, 0.2)
        /// </summary>
        public static readonly int Blur_TintColor = Shader.PropertyToID("_TintColor");
        /// <summary>
        /// float 5.0 (0.0 ~ 40.0)
        /// </summary>
        public static readonly int Blur_Size = Shader.PropertyToID("_Size");
        /// <summary>
        /// float 0.2 (0.0 ~ 2.0)
        /// </summary>
        public static readonly int Blur_Vibrancy = Shader.PropertyToID("_Vibrancy");
        /// <summary>
        /// Texture2D "white"
        /// </summary>
        public static readonly int Blur_MainTex = Shader.PropertyToID("_MainTex");
        public static string KeyName(KeyConfig config)
        {
            return config.DummyName ?? config.Code.ToString();
        }
        public static bool IsDummyKey(string keyName) => !EnumHelper<KeyCode>.GetNames().Contains(keyName);
        public static Vector2 InjectPivot(Key key, Vector2 pivot)
        {
            var k = key.transform;
            var origin = k.position;
            Vector3 offset = key.Size * pivot;
            foreach (Transform child in k)
                child.position += offset;
            k.position = origin;
            return offset;
        }
        public static void ApplyColorLayout(Image image, GColor color, bool blurEnabled)
        {
            UICornersGradient grad = image.GetComponent<UICornersGradient>();
            if (blurEnabled)
            {
                if (grad) Object.Destroy(grad);
                image.material.SetColor(Blur_TintColor, color);
                return;
            }
            if (color.gradientEnabled)
            {
                image.color = Color.white;
                if (!grad) grad = image.gameObject.AddComponent<UICornersGradient>();
                grad.topLeft = color.topLeft;
                grad.topRight = color.topRight;
                grad.bottomLeft = color.bottomLeft;
                grad.bottomRight = color.bottomRight;
                image.SetVerticesDirty();
            }
            else
            {
                if (grad) Object.Destroy(grad);
                else image.color = color;
            }
        }
        public static void ApplyColorLayout(TextMeshProUGUI text, GColor color)
        {
            if (color.gradientEnabled) text.colorGradient = color;
            else text.colorGradient = new VertexGradient(color);
        }
        public static void ApplyConfigLayout(Image image, ObjectConfig config, Vector2 sizeDelta, bool blurEnabled)
        {
            ApplyColorLayout(image, config.Color.Released, blurEnabled);
            var vConfig = config.VectorConfig;
            var rt = image.rectTransform;
            rt.localRotation = Quaternion.Euler(vConfig.Rotation.Released);
            rt.localPosition = vConfig.Offset.Released;
            rt.sizeDelta = sizeDelta * vConfig.Scale.Released;
        }
        public static void ApplyConfigLayout(Key k, VectorConfig vConfig)
        {
            var t = k.transform;
            t.localRotation = Quaternion.Euler(vConfig.Rotation.Released);
            t.localScale = vConfig.Scale.Released;
            t.localPosition = vConfig.Offset.Released + (Vector3)k.Position;
        }
        public static void ApplyConfigLayout(Rain r, VectorConfig vConfig, Vector2 sizeDelta, bool scaleSizeDelta)
        {
            var rt = r.rt;
            rt.localRotation = Quaternion.Euler(vConfig.Rotation.Released);
            rt.localPosition = vConfig.Offset.Released + (Vector3)r.Position;
            if (scaleSizeDelta)
                rt.sizeDelta = sizeDelta * vConfig.Scale.Released;
            else rt.localScale = vConfig.Scale.Released;
        }
        public static void ApplyConfigLayout(TextMeshProUGUI text, ObjectConfig config, float heightOffset, bool fixScale)
        {
            ApplyColorLayout(text, config.Color.Released);
            var vConfig = config.VectorConfig;
            var rt = text.rectTransform;
            rt.localRotation = Quaternion.Euler(vConfig.Rotation.Released);
            rt.localPosition = vConfig.Offset.Released.WithRelativeY(heightOffset);
            Vector3 scale = vConfig.Scale.Released;
            if (fixScale)
                scale = FixedScale(rt.parent.localScale, scale);
            rt.localScale = scale;
        }
        public static void ApplyColor(Image image, GColor from, GColor to, EaseConfig easeConfig, bool blurEnabled)
        {
            DOTween.Kill(image, true);
            if (!easeConfig.IsValid)
            {
                ApplyColorLayout(image, to, blurEnabled);
                return;
            }
            UICornersGradient grad = image.GetComponent<UICornersGradient>();
            if (blurEnabled)
            {
                if (grad) Object.Destroy(grad);
                Material mat = image.material;
                DOVirtual.Float(0, 1, easeConfig.Duration, f =>
                {
                    mat.SetColor(Blur_TintColor, EasedColor(from, to, f));
                }).SetEase(easeConfig.Ease).SetAutoKill(false).SetTarget(image);
            }
            var gradEnabled = from.gradientEnabled || to.gradientEnabled;
            if (gradEnabled)
            {
                image.color = Color.white;
                if (!grad) grad = image.gameObject.AddComponent<UICornersGradient>();
                var fromGrad = from.gradientEnabled ? from._color : new VertexGradient(from);
                var toGrad = to.gradientEnabled ? to._color : new VertexGradient(to);
                DOVirtual.Float(0, 1, easeConfig.Duration, f =>
                {
                    grad.topLeft = EasedColor(fromGrad.topLeft, toGrad.topLeft, f);
                    grad.topRight = EasedColor(fromGrad.topRight, toGrad.topRight, f);
                    grad.bottomLeft = EasedColor(fromGrad.bottomLeft, toGrad.bottomLeft, f);
                    grad.bottomRight = EasedColor(fromGrad.bottomRight, toGrad.bottomRight, f);
                    image.SetVerticesDirty();
                }).SetEase(easeConfig.Ease).SetAutoKill(false).SetTarget(image);
            }
            else
            {
                if (grad) Object.Destroy(grad);
                else image.DOColor(to, easeConfig.Duration).SetEase(easeConfig.Ease).SetAutoKill(false);
            }
        }
        public static void ApplyColor(TextMeshProUGUI text, GColor from, GColor to, EaseConfig easeConfig)
        {
            DOTween.Kill(text, true);
            if (!easeConfig.IsValid)
            {
                text.color = Color.white;
                ApplyColorLayout(text, to);
                return;
            }
            var gradEnabled = from.gradientEnabled || to.gradientEnabled;
            if (gradEnabled)
            {
                var fromGrad = from.gradientEnabled ? from._color : new VertexGradient(from);
                var toGrad = to.gradientEnabled ? to._color : new VertexGradient(to);
                DOVirtual.Float(0, 1, easeConfig.Duration, f =>
                {
                    var tl = EasedColor(fromGrad.topLeft, toGrad.topLeft, f);
                    var tr = EasedColor(fromGrad.topRight, toGrad.topRight, f);
                    var bl = EasedColor(fromGrad.bottomLeft, toGrad.bottomLeft, f);
                    var br = EasedColor(fromGrad.bottomRight, toGrad.bottomRight, f);
                    text.colorGradient = new VertexGradient(tl, tr, bl, br);
                }).SetEase(easeConfig.Ease).SetAutoKill(false).SetTarget(text);
            }
            else
            {
                text.DOColor(to, easeConfig.Duration).SetEase(easeConfig.Ease).SetAutoKill(false);
            }
        }
        public static Color EasedColor(Color from, Color to, float lifetime)
        {
            var r = to.r - from.r;
            var g = to.g - from.g;
            var b = to.b - from.b;
            var a = to.a - from.a;
            return new Color(from.r + r * lifetime, from.g + g * lifetime, from.b + b * lifetime, from.a + a * lifetime);
        }
        public static void ApplyVectorConfig(Key k, VectorConfig vConfig, bool pressed)
        {
            Transform t = k.transform;

            DOTween.Kill(t, true);

            var rEase = vConfig.Rotation.GetEase(pressed);
            if (rEase.IsValid)
                t.DOLocalRotate(vConfig.Rotation.Get(pressed), rEase.Duration)
                .SetEase(rEase.Ease)
                .SetAutoKill(false);
            else t.localRotation = Quaternion.Euler(vConfig.Rotation.Get(pressed));

            var oEase = vConfig.Offset.GetEase(pressed);
            if (oEase.IsValid)
                t.DOLocalMove(vConfig.Offset.Get(pressed) + (Vector3)k.Position, oEase.Duration)
                .SetEase(oEase.Ease)
                .SetAutoKill(false);
            else t.localPosition = vConfig.Offset.Get(pressed) + (Vector3)k.Position;

            var sEase = vConfig.Scale.GetEase(pressed);
            if (sEase.IsValid)
                t.DOScale(vConfig.Scale.Get(pressed), sEase.Duration)
                .SetEase(sEase.Ease)
                .SetAutoKill(false);
            else t.localScale = vConfig.Scale.Get(pressed);
        }
        public static void ApplyVectorConfig(RectTransform rt, VectorConfig vConfig, bool pressed, float heightOffset, bool fixScale, Vector2 sizeDelta, bool scaleSizeDelta = true)
        {
            ApplyVectorConfig(rt, vConfig, pressed, new Vector2(0, heightOffset), fixScale, sizeDelta, scaleSizeDelta);
        }
        public static void ApplyVectorConfig(RectTransform rt, VectorConfig vConfig, bool pressed, Vector2 offset, bool fixScale, Vector2 sizeDelta, bool scaleSizeDelta = true)
        {
            DOTween.Kill(rt, true);

            var rEase = vConfig.Rotation.GetEase(pressed);
            if (rEase.IsValid)
                rt.DOLocalRotate(vConfig.Rotation.Get(pressed), rEase.Duration)
                .SetEase(rEase.Ease)
                .SetAutoKill(false);
            else rt.localRotation = Quaternion.Euler(vConfig.Rotation.Get(pressed));

            var oEase = vConfig.Offset.GetEase(pressed);
            if (oEase.IsValid)
                rt.DOLocalMove(vConfig.Offset.Get(pressed) + (Vector3)offset, oEase.Duration)
                .SetEase(oEase.Ease)
                .SetAutoKill(false);
            else rt.localPosition = vConfig.Offset.Get(pressed) + (Vector3)offset;

            Vector3 scale = vConfig.Scale.Get(pressed);
            if (fixScale)
                scale = FixedScale(rt.parent.localScale, scale);
            var sEase = vConfig.Scale.GetEase(pressed);
            if (scaleSizeDelta)
            {
                if (sEase.IsValid)
                    rt.DOSizeDelta(sizeDelta * scale, sEase.Duration)
                    .SetEase(sEase.Ease)
                    .SetAutoKill(false);
                else rt.sizeDelta = sizeDelta * scale;
            }
            else
            {
                if (sEase.IsValid)
                    rt.DOScale(scale, sEase.Duration)
                    .SetEase(sEase.Ease)
                    .SetAutoKill(false);
                else rt.localScale = scale;
            }
        }
        public static void ApplyVectorConfig(RectTransform rt, VectorConfig vConfig, bool pressed, bool fixScale, Vector2 sizeDelta, bool scaleSizeDelta = true)
        {
            DOTween.Kill(rt, true);

            var rEase = vConfig.Rotation.GetEase(pressed);
            if (rEase.IsValid)
                rt.DOLocalRotate(vConfig.Rotation.Get(pressed), rEase.Duration)
                .SetEase(rEase.Ease)
                .SetAutoKill(false);
            else rt.localRotation = Quaternion.Euler(vConfig.Rotation.Get(pressed));

            Vector3 scale = vConfig.Scale.Get(pressed);
            if (fixScale)
                scale = FixedScale(rt.parent.localScale, scale);
            var sEase = vConfig.Scale.GetEase(pressed);
            if (scaleSizeDelta)
            {
                if (sEase.IsValid)
                    rt.DOSizeDelta(sizeDelta * scale, sEase.Duration)
                    .SetEase(sEase.Ease)
                    .SetAutoKill(false);
                else rt.sizeDelta = sizeDelta * scale;
            }
            else
            {
                if (sEase.IsValid)
                    rt.DOScale(scale, sEase.Duration)
                    .SetEase(sEase.Ease)
                    .SetAutoKill(false);
                else rt.localScale = scale;
            }
        }
        public static void ApplyVectorConfigForRainMaskOnly(RectTransform rt, VectorConfig vConfig, bool pressed, Vector2 offset)
        {
            DOTween.Kill(rt, true);

            var oEase = vConfig.Offset.GetEase(pressed);
            if (oEase.IsValid)
                rt.DOMove(vConfig.Offset.Get(pressed) + (Vector3)offset, oEase.Duration)
                .SetEase(oEase.Ease)
                .SetAutoKill(false);
            else rt.position = vConfig.Offset.Get(pressed) + (Vector3)offset;
        }
        public static void SetMaskAnchor(RectTransform rt, Direction dir, Pivot pivot = Pivot.MiddleCenter, Anchor anchor = Anchor.MiddleCenter)
        {
            switch (dir)
            {
                case Direction.Up:
                    rt.pivot = new Vector2(0.5f, 0);
                    rt.anchorMin = new Vector2(0.5f, 0);
                    rt.anchorMax = new Vector2(0.5f, 0);
                    break;
                case Direction.Down:
                    rt.pivot = new Vector2(0.5f, 1);
                    rt.anchorMin = new Vector2(0.5f, 1);
                    rt.anchorMax = new Vector2(0.5f, 1);
                    break;
                case Direction.Right:
                    rt.pivot = new Vector2(0, 0.5f);
                    rt.anchorMin = new Vector2(0, 0.5f);
                    rt.anchorMax = new Vector2(0, 0.5f);
                    break;
                case Direction.Left:
                    rt.pivot = new Vector2(1, 0.5f);
                    rt.anchorMin = new Vector2(1, 0.5f);
                    rt.anchorMax = new Vector2(1, 0.5f);
                    break;
            }
            if (pivot != Pivot.MiddleCenter)
                rt.pivot = GetPivot(pivot);
            if (anchor != Anchor.MiddleCenter)
                rt.SetAnchor(anchor);
        }
        public static Vector3 FixedScale(Vector3 parentScale, Vector3 fixedScale)
        {
            return new Vector3(fixedScale.x / parentScale.x, fixedScale.y / parentScale.y, fixedScale.z == 0 ? 0 : fixedScale.z / parentScale.z);
        }
        public static Vector2 GetPivot(Pivot pivot)
        {
            switch (pivot)
            {
                case Pivot.TopLeft: return new Vector2(0, 1);
                case Pivot.TopCenter: return new Vector2(0.5f, 1);
                case Pivot.TopRight: return new Vector2(1, 1);

                case Pivot.MiddleLeft: return new Vector2(0, 0.5f);
                case Pivot.MiddleCenter: return new Vector2(0.5f, 0.5f);
                case Pivot.MiddleRight: return new Vector2(1, 0.5f);

                case Pivot.BottomLeft: return new Vector2(0, 0);
                case Pivot.BottomCenter: return new Vector2(0.5f, 0);
                case Pivot.BottomRight: return new Vector2(1, 0);
            }
            return Vector2.zero;
        }
        public static Vector2 AdjustRainPosition(Direction dir, Vector2 position, Vector2 offset)
        {
            switch (dir)
            {
                case Direction.Up:
                case Direction.Left:
                    return position.WithRelativeX(offset.x).WithRelativeY(-offset.y);
                case Direction.Down:
                case Direction.Right:
                    return position.WithRelativeX(-offset.x).WithRelativeY(-offset.y);
                default: return position;
            }
        }
        public static Vector2 GetSize(Profile profile)
        {
            float keyHeight = profile.Keys.Any(k => k.EnableCountText) ? 150 : 100;
            float totalX = 0;
            foreach (var k in profile.Keys)
                if (!k.DisableSorting)
                {
                    var releasedScale = k.VectorConfig.Scale.Released;
                    totalX += releasedScale.x * 100 + profile.KeySpacing;
                }
            return new Vector2(totalX - profile.KeySpacing - 5, keyHeight);
        }
        public static void MakeBar(Profile profile, List<KeyConfig> keys)
        {
            keys.ForEach(k => k.DisableSorting = k.EnableCountText = true);
            Vector2 size = GetSize(profile);
            int keysCount = keys.Count;
            int totalCount = profile.Keys.Count - keysCount;
            float spaceCount = (totalCount / (float)keysCount) - 1;
            float spaceScale = spaceCount * 0.1f;
            float spacing = profile.KeySpacing;
            for (int i = 0; i < keys.Count; i++)
            {
                var config = keys[i];
                config.TextFontSize -= 20;
                config.CountTextFontSize -= 10;
                var newScale = new Vector2(totalCount / (float)keysCount + spaceScale, .5f);
                config.BackgroundConfig.VectorConfig.Scale.Set(newScale * 0.9f, newScale);
                config.OutlineConfig.VectorConfig.Scale.Set(newScale * 0.9f, newScale);
                float x = 100 * newScale.x;
                var newOffset = new Vector3(x / 2 + i * x + (i * spacing), -size.y - spacing);
                config.VectorConfig.Offset.Set(newOffset);
                var newBgOlOffset = new Vector3(0, size.y / 4);
                config.BackgroundConfig.VectorConfig.Offset.Set(newBgOlOffset);
                config.OutlineConfig.VectorConfig.Offset.Set(newBgOlOffset);
                float heightOffset = size.y / 8f;
                config.TextConfig.VectorConfig.Offset.Set(newBgOlOffset.WithRelativeY(-heightOffset));
                config.CountTextConfig.VectorConfig.Offset.Set(newBgOlOffset.WithRelativeY(heightOffset));
                if (config.Rain.Direction == Direction.Up || config.Rain.Direction == Direction.Down)
                    config.Rain.ObjectConfig.VectorConfig.Scale.Set(new Vector2(newScale.x, 1));
                else config.Rain.ObjectConfig.VectorConfig.Scale.Set(new Vector2(1, newScale.y));
            }
        }
        public static void ApplyRoundnessBlurLayout(Image image, ref float roundness, BlurConfig config, bool blurEnabled)
        {
            image.material = null;
            RoundedCorners rounder = image.GetComponent<RoundedCorners>();
            if (blurEnabled)
            {
                roundness = 0;
                if (rounder) Object.Destroy(rounder);
                Material mat = image.material = new Material(AssetManager.Blur);
                mat.SetFloat(Blur_Size, config.Spacing);
                mat.SetFloat(Blur_Vibrancy, config.Vibrancy);
                mat.SetColor(Blur_TintColor, image.color);
            }
            else
            {
                if (roundness > 0)
                {
                    if (!rounder)
                        rounder = image.gameObject.AddComponent<RoundedCorners>();
                    rounder.radius = roundness * 90;
                    rounder.Validate();
                    rounder.Refresh();
                }
                else if (rounder != null) Object.Destroy(rounder);
            }
        }
        public static void ApplyBlurColorConfig(KeyConfig config)
        {
            if (!config.BackgroundBlurEnabled) return;
            Color pressed = config.BackgroundConfig.Color.Pressed;
            Color released = config.BackgroundConfig.Color.Released;
            config.BackgroundConfig.Color.Set(pressed.WithAlpha(0f), released.WithAlpha(0.2f));
        }
        public static string AggregateComma(IEnumerable<string> input)
        {
            var result = input.Aggregate("", (c, n) => $"{c}{n},");
            return result.Remove(result.Length - 1, 1);
        }
        public static void SetMultiple<T>(T original, T originalCopy, IEnumerable<T> instances, IEnumerable<T> instanceCopys, string fieldName, System.Func<object, System.Type, bool> relative) where T : IModel, ICopyable<T>
        {
            SetMultiple(original, originalCopy, instances, instanceCopys, fieldName, (instance, rRef, t) => relative(instance, t), null);
        }
        public static void SetMultiple<T>(T original, T originalCopy, IEnumerable<T> instances, IEnumerable<T> instanceCopys, string fieldName, System.Func<object, object, System.Type, bool> relative, List<object> relativeIterationRef) where T : IModel, ICopyable<T>
        {
            var field = typeof(T).GetField(fieldName);
            var originalVal = field.GetValue(original);
            var insts = instances is List<T> list ? list : instances.ToList();
            var instCopys = instanceCopys is List<T> list2 ? list2 : instanceCopys.ToList();
            if (originalVal == null)
            {
                for (int i = 0; i < insts.Count; i++)
                    field.SetValue(insts[i], originalVal);
                return;
            }
            var fromCriterion = field.GetValue(originalCopy);
            var valType = originalVal.GetType();
            var setRelative = CanRelativeOperation(valType);
            for (int i = 0; i < insts.Count; i++)
            {
                var toCriterion = field.GetValue(instCopys[i]);
                if (setRelative && relative(insts[i], relativeIterationRef != null ? relativeIterationRef[i] : null, valType))
                    field.SetValue(insts[i], RelativeOperation(fromCriterion, originalVal, toCriterion));
                else field.SetValue(insts[i], originalVal);
            }
        }
        public static bool IsEquals<T>(IEnumerable<T> instances, string fieldName)
        {
            var field = typeof(T).GetField(fieldName);
            return IsEquals(instances.Select(k => field.GetValue(k)));
        }
        public static bool IsFieldsEquals<T>(System.Type t, IEnumerable<T> instances)
        {
            bool equals = true;
            var fields = t.GetFields();
            foreach (var field in fields)
            {
                var ft = field.FieldType;
                if (!ft.IsClass || ft.IsPrimitive || ft == typeof(string))
                {
                    equals &= IsEquals(instances.Select(t => t == null ? null : field.GetValue(t)));
                    continue;
                }
                equals &= IsFieldsEquals(ft, instances.Select(t => t == null ? null : field.GetValue(t)));
            }
            return equals;
        }
        public static bool IsEquals(IEnumerable<object> objects)
        {
            if (!objects?.Any() ?? true) return false;
            object first = objects.First();
            return objects.All(o => Equals(o, first));
        }
        public static bool CanRelativeOperation(System.Type t)
        {
            return
                t == typeof(float) ||
                t == typeof(int) ||
                t == typeof(Vector2) ||
                t == typeof(Vector3) ||
                t == typeof(Vector4) ||
                t == typeof(Color) ||
                t == typeof(GColor) ||
                t == typeof(sbyte) ||
                t == typeof(byte) ||
                t == typeof(short) ||
                t == typeof(ushort) ||
                t == typeof(uint) ||
                t == typeof(long) ||
                t == typeof(ulong) ||
                t == typeof(double) ||
                t == typeof(decimal);
        }
        public static object RelativeOperation(object fromCriterion, object obj, object toCriterion)
        {
            if (obj is float)
                return (float)toCriterion + ((float)obj - (float)fromCriterion);
            if (obj is int)
                return (int)toCriterion + ((int)obj - (int)fromCriterion);
            if (obj is Vector2)
                return (Vector2)toCriterion + ((Vector2)obj - (Vector2)fromCriterion);
            if (obj is Vector3)
                return (Vector3)toCriterion + ((Vector3)obj - (Vector3)fromCriterion);
            if (obj is Vector4)
                return (Vector4)toCriterion + ((Vector4)obj - (Vector4)fromCriterion);
            if (obj is Color)
                return (Color)toCriterion + ((Color)obj - (Color)fromCriterion);
            if (obj is GColor)
                return (GColor)toCriterion + ((GColor)obj - (GColor)fromCriterion);
            if (obj is sbyte)
                return (sbyte)toCriterion + ((sbyte)obj - (sbyte)fromCriterion);
            if (obj is byte)
                return (byte)toCriterion + ((byte)obj - (byte)fromCriterion);
            if (obj is short)
                return (short)toCriterion + ((short)obj - (short)fromCriterion);
            if (obj is ushort)
                return (ushort)toCriterion + ((ushort)obj - (ushort)fromCriterion);
            if (obj is uint)
                return (uint)toCriterion + ((uint)obj - (uint)fromCriterion);
            if (obj is long)
                return (long)toCriterion + ((long)obj - (long)fromCriterion);
            if (obj is ulong)
                return (ulong)toCriterion + ((ulong)obj - (ulong)fromCriterion);
            if (obj is double)
                return (double)toCriterion + ((double)obj - (double)fromCriterion);
            if (obj is decimal)
                return (decimal)toCriterion + ((decimal)obj - (decimal)fromCriterion);
            return obj;
        }
        public static bool IsVectorType(System.Type t) => t.Name.StartsWith("Vector");
        public static void SetMultiplePR<T>(PressRelease<T> original, PressRelease<T> originalCopy, IEnumerable<PressRelease<T>> targets, IEnumerable<PressRelease<T>> targetsCopy, System.Func<object, object, System.Type, bool> relative = null, List<object> relativeIteraionRef = null)
        {
            if (relative != null && relativeIteraionRef != null)
            {
                SetMultiple(original, originalCopy, targets, targetsCopy, "Pressed", relative, relativeIteraionRef);
                SetMultiple(original.PressedEase, originalCopy.PressedEase, targets.Select(t => t.PressedEase), targetsCopy.Select(t => t.PressedEase), "Ease", (instance, t) => false);
                SetMultiple(original.PressedEase, originalCopy.PressedEase, targets.Select(t => t.PressedEase), targetsCopy.Select(t => t.PressedEase), "Duration", (instance, t) => false);
                SetMultiple(original, originalCopy, targets, targetsCopy, "Released", relative, relativeIteraionRef);
                SetMultiple(original.ReleasedEase, originalCopy.ReleasedEase, targets.Select(t => t.ReleasedEase), targetsCopy.Select(t => t.ReleasedEase), "Ease", (instance, t) => false);
                SetMultiple(original.ReleasedEase, originalCopy.ReleasedEase, targets.Select(t => t.ReleasedEase), targetsCopy.Select(t => t.ReleasedEase), "Duration", (instance, t) => false);
            }
            else
            {
                SetMultiple(original, originalCopy, targets, targetsCopy, "Pressed", (instance, t) => false);
                SetMultiple(original.PressedEase, originalCopy.PressedEase, targets.Select(t => t.PressedEase), targetsCopy.Select(t => t.PressedEase), "Ease", (instance, t) => false);
                SetMultiple(original.PressedEase, originalCopy.PressedEase, targets.Select(t => t.PressedEase), targetsCopy.Select(t => t.PressedEase), "Duration", (instance, t) => false);
                SetMultiple(original, originalCopy, targets, targetsCopy, "Released", (instance, t) => false);
                SetMultiple(original.ReleasedEase, originalCopy.ReleasedEase, targets.Select(t => t.ReleasedEase), targetsCopy.Select(t => t.ReleasedEase), "Ease", (instance, t) => false);
                SetMultiple(original.ReleasedEase, originalCopy.ReleasedEase, targets.Select(t => t.ReleasedEase), targetsCopy.Select(t => t.ReleasedEase), "Duration", (instance, t) => false);
            }
        }
    }
}
