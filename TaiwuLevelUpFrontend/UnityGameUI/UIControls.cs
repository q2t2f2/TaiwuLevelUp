using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityGameUI
{
    // Token: 0x02000006 RID: 6
    internal class UIControls : MonoBehaviour
    {
        // Token: 0x0600000F RID: 15 RVA: 0x000021F9 File Offset: 0x000003F9
        private static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.AddComponent<RectTransform>().sizeDelta = size;
            return gameObject;
        }

        // Token: 0x06000010 RID: 16 RVA: 0x0000220D File Offset: 0x0000040D
        private static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.AddComponent<RectTransform>();
            UIControls.SetParentAndAlign(gameObject, parent);
            return gameObject;
        }

        // Token: 0x06000011 RID: 17 RVA: 0x00002223 File Offset: 0x00000423
        private static void SetDefaultTextValues(Text lbl)
        {
            lbl.color = UIControls.s_TextColor;
            lbl.font = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        // Token: 0x06000012 RID: 18 RVA: 0x00002240 File Offset: 0x00000440
        private static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }

        // Token: 0x06000013 RID: 19 RVA: 0x000022A5 File Offset: 0x000004A5
        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (!(parent == null))
            {
                child.transform.SetParent(parent.transform, false);
                UIControls.SetLayerRecursively(child, parent.layer);
            }
        }

        // Token: 0x06000014 RID: 20 RVA: 0x000022D0 File Offset: 0x000004D0
        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform transform = go.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                UIControls.SetLayerRecursively(transform.GetChild(i).gameObject, layer);
            }
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002310 File Offset: 0x00000510
        public static GameObject CreatePanel(UIControls.Resources resources)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("Panel", UIControls.s_ThickElementSize);
            RectTransform component = gameObject.GetComponent<RectTransform>();
            component.anchorMin = Vector2.zero;
            component.anchorMax = Vector2.one;
            component.anchoredPosition = Vector2.zero;
            component.sizeDelta = Vector2.zero;
            Image image = gameObject.AddComponent<Image>();
            image.sprite = resources.background;
            image.type = Image.Type.Sliced;
            image.color = UIControls.s_PanelColor;
            return gameObject;
        }

        // Token: 0x06000016 RID: 22 RVA: 0x00002380 File Offset: 0x00000580
        public static GameObject CreateButton(UIControls.Resources resources, string Text)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("Button", UIControls.s_ThickElementSize);
            GameObject gameObject2 = new GameObject("Text");
            gameObject2.AddComponent<RectTransform>();
            UIControls.SetParentAndAlign(gameObject2, gameObject);
            Image image = gameObject.AddComponent<Image>();
            image.sprite = resources.standard;
            image.type = Image.Type.Sliced;
            image.color = UIControls.s_DefaultSelectableColor;
            UIControls.SetDefaultColorTransitionValues(gameObject.AddComponent<Button>());
            Text text = gameObject2.AddComponent<Text>();
            text.text = Text;
            text.alignment = TextAnchor.MiddleCenter;
            UIControls.SetDefaultTextValues(text);
            RectTransform component = gameObject2.GetComponent<RectTransform>();
            component.anchorMin = Vector2.zero;
            component.anchorMax = Vector2.one;
            component.sizeDelta = Vector2.zero;
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(80f, 40f);
            return gameObject;
        }

        // Token: 0x06000017 RID: 23 RVA: 0x0000243C File Offset: 0x0000063C
        public static GameObject CreateText(UIControls.Resources resources)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("Text", UIControls.s_ThickElementSize);
            Text text = gameObject.AddComponent<Text>();
            text.text = "New Text";
            UIControls.SetDefaultTextValues(text);
            return gameObject;
        }

        // Token: 0x06000018 RID: 24 RVA: 0x00002463 File Offset: 0x00000663
        public static GameObject CreateImage(UIControls.Resources resources)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("Image", UIControls.s_ImageElementSize);
            gameObject.AddComponent<Image>().sprite = resources.background;
            return gameObject;
        }

        // Token: 0x06000019 RID: 25 RVA: 0x00002485 File Offset: 0x00000685
        public static GameObject CreateRawImage(UIControls.Resources resources)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("RawImage", UIControls.s_ImageElementSize);
            gameObject.AddComponent<RawImage>().texture = resources.background.texture;
            return gameObject;
        }

        // Token: 0x0600001A RID: 26 RVA: 0x000024AC File Offset: 0x000006AC
        public static GameObject CreateSlider(UIControls.Resources resources)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("Slider", UIControls.s_ThinElementSize);
            GameObject gameObject6 = UIControls.CreateUIObject("Background", gameObject);
            GameObject gameObject2 = UIControls.CreateUIObject("Fill Area", gameObject);
            GameObject gameObject3 = UIControls.CreateUIObject("Fill", gameObject2);
            GameObject gameObject4 = UIControls.CreateUIObject("Handle Slide Area", gameObject);
            GameObject gameObject5 = UIControls.CreateUIObject("Handle", gameObject4);
            Image image4 = gameObject6.AddComponent<Image>();
            image4.sprite = resources.background;
            image4.type = Image.Type.Sliced;
            image4.color = UIControls.s_DefaultSelectableColor;
            RectTransform component = gameObject6.GetComponent<RectTransform>();
            component.anchorMin = new Vector2(0f, 0.25f);
            component.anchorMax = new Vector2(1f, 0.75f);
            component.sizeDelta = new Vector2(0f, 0f);
            RectTransform component2 = gameObject2.GetComponent<RectTransform>();
            component2.anchorMin = new Vector2(0f, 0.25f);
            component2.anchorMax = new Vector2(1f, 0.75f);
            component2.anchoredPosition = new Vector2(-5f, 0f);
            component2.sizeDelta = new Vector2(-20f, 0f);
            Image image5 = gameObject3.AddComponent<Image>();
            image5.sprite = resources.standard;
            image5.type = Image.Type.Sliced;
            image5.color = UIControls.s_DefaultSelectableColor;
            gameObject3.GetComponent<RectTransform>().sizeDelta = new Vector2(10f, 0f);
            RectTransform component3 = gameObject4.GetComponent<RectTransform>();
            component3.sizeDelta = new Vector2(-20f, 0f);
            component3.anchorMin = new Vector2(0f, 0f);
            component3.anchorMax = new Vector2(1f, 1f);
            Image image3 = gameObject5.AddComponent<Image>();
            image3.sprite = resources.knob;
            image3.color = UIControls.s_DefaultSelectableColor;
            gameObject5.GetComponent<RectTransform>().sizeDelta = new Vector2(20f, 0f);
            Slider slider = gameObject.AddComponent<Slider>();
            slider.fillRect = gameObject3.GetComponent<RectTransform>();
            slider.handleRect = gameObject5.GetComponent<RectTransform>();
            slider.targetGraphic = image3;
            slider.direction = Slider.Direction.LeftToRight;
            UIControls.SetDefaultColorTransitionValues(slider);
            return gameObject;
        }

        // Token: 0x0600001B RID: 27 RVA: 0x000026B8 File Offset: 0x000008B8
        public static GameObject CreateScrollbar(UIControls.Resources resources)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("Scrollbar", UIControls.s_ThinElementSize);
            GameObject gameObject2 = UIControls.CreateUIObject("Sliding Area", gameObject);
            GameObject gameObject3 = UIControls.CreateUIObject("Handle", gameObject2);
            Image image3 = gameObject.AddComponent<Image>();
            image3.sprite = resources.background;
            image3.type = Image.Type.Sliced;
            image3.color = UIControls.s_DefaultSelectableColor;
            Image image2 = gameObject3.AddComponent<Image>();
            image2.sprite = resources.standard;
            image2.type = Image.Type.Sliced;
            image2.color = UIControls.s_DefaultSelectableColor;
            RectTransform component3 = gameObject2.GetComponent<RectTransform>();
            component3.sizeDelta = new Vector2(-20f, -20f);
            component3.anchorMin = Vector2.zero;
            component3.anchorMax = Vector2.one;
            RectTransform component2 = gameObject3.GetComponent<RectTransform>();
            component2.sizeDelta = new Vector2(20f, 20f);
            Scrollbar scrollbar = gameObject.AddComponent<Scrollbar>();
            scrollbar.handleRect = component2;
            scrollbar.targetGraphic = image2;
            UIControls.SetDefaultColorTransitionValues(scrollbar);
            return gameObject;
        }

        // Token: 0x0600001C RID: 28 RVA: 0x0000279C File Offset: 0x0000099C
        public static GameObject CreateToggle(UIControls.Resources resources)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("Toggle", UIControls.s_ThinElementSize);
            GameObject gameObject2 = UIControls.CreateUIObject("Background", gameObject);
            GameObject gameObject4 = UIControls.CreateUIObject("Checkmark", gameObject2);
            GameObject gameObject3 = UIControls.CreateUIObject("Label", gameObject);
            Toggle toggle = gameObject.AddComponent<Toggle>();
            toggle.isOn = true;
            Image image = gameObject2.AddComponent<Image>();
            image.sprite = resources.standard;
            image.type = Image.Type.Sliced;
            image.color = UIControls.s_DefaultSelectableColor;
            Image image2 = gameObject4.AddComponent<Image>();
            image2.sprite = resources.checkmark;
            Text text = gameObject3.AddComponent<Text>();
            text.text = "Toggle";
            UIControls.SetDefaultTextValues(text);
            toggle.graphic = image2;
            toggle.targetGraphic = image;
            UIControls.SetDefaultColorTransitionValues(toggle);
            RectTransform component = gameObject2.GetComponent<RectTransform>();
            component.anchorMin = new Vector2(0f, 1f);
            component.anchorMax = new Vector2(0f, 1f);
            component.anchoredPosition = new Vector2(10f, -10f);
            component.sizeDelta = new Vector2(20f, 20f);
            RectTransform component2 = gameObject4.GetComponent<RectTransform>();
            component2.anchorMin = new Vector2(0.5f, 0.5f);
            component2.anchorMax = new Vector2(0.5f, 0.5f);
            component2.anchoredPosition = Vector2.zero;
            component2.sizeDelta = new Vector2(20f, 20f);
            RectTransform component3 = gameObject3.GetComponent<RectTransform>();
            component3.anchorMin = new Vector2(0f, 0f);
            component3.anchorMax = new Vector2(1f, 1f);
            component3.offsetMin = new Vector2(23f, 1f);
            component3.offsetMax = new Vector2(-5f, -2f);
            return gameObject;
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00002958 File Offset: 0x00000B58
        public static GameObject CreateInputField(UIControls.Resources resources)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("InputField", UIControls.s_ThickElementSize);
            GameObject gameObject2 = UIControls.CreateUIObject("Placeholder", gameObject);
            GameObject gameObject3 = UIControls.CreateUIObject("Text", gameObject);
            Image image = gameObject.AddComponent<Image>();
            image.sprite = resources.inputField;
            image.type = Image.Type.Sliced;
            image.color = UIControls.s_DefaultSelectableColor;
            InputField inputField = gameObject.AddComponent<InputField>();
            UIControls.SetDefaultColorTransitionValues(inputField);
            Text text = gameObject3.AddComponent<Text>();
            text.text = "";
            text.supportRichText = false;
            UIControls.SetDefaultTextValues(text);
            Text text2 = gameObject2.AddComponent<Text>();
            text2.text = "Enter text...";
            text2.fontStyle = FontStyle.Italic;
            Color color = text.color;
            color.a *= 0.5f;
            text2.color = color;
            RectTransform component = gameObject3.GetComponent<RectTransform>();
            component.anchorMin = Vector2.zero;
            component.anchorMax = Vector2.one;
            component.sizeDelta = Vector2.zero;
            component.offsetMin = new Vector2(10f, 6f);
            component.offsetMax = new Vector2(-10f, -7f);
            RectTransform component2 = gameObject2.GetComponent<RectTransform>();
            component2.anchorMin = Vector2.zero;
            component2.anchorMax = Vector2.one;
            component2.sizeDelta = Vector2.zero;
            component2.offsetMin = new Vector2(10f, 6f);
            component2.offsetMax = new Vector2(-10f, -7f);
            inputField.textComponent = text;
            inputField.placeholder = text2;
            return gameObject;
        }

        // Token: 0x0600001E RID: 30 RVA: 0x00002AC8 File Offset: 0x00000CC8
        public static GameObject CreateDropdown(UIControls.Resources resources, List<string> options, Color LabelColor)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("Dropdown", UIControls.s_ThickElementSize);
            GameObject gameObject2 = UIControls.CreateUIObject("Label", gameObject);
            GameObject gameObject3 = UIControls.CreateUIObject("Arrow", gameObject);
            GameObject gameObject4 = UIControls.CreateUIObject("Template", gameObject);
            GameObject gameObject5 = UIControls.CreateUIObject("Viewport", gameObject4);
            GameObject gameObject6 = UIControls.CreateUIObject("Content", gameObject5);
            GameObject gameObject7 = UIControls.CreateUIObject("Item", gameObject6);
            GameObject gameObject8 = UIControls.CreateUIObject("Item Background", gameObject7);
            GameObject gameObject9 = UIControls.CreateUIObject("Item Checkmark", gameObject7);
            GameObject gameObject10 = UIControls.CreateUIObject("Item Label", gameObject7);
            GameObject gameObject11 = UIControls.CreateScrollbar(resources);
            gameObject11.name = "Scrollbar";
            UIControls.SetParentAndAlign(gameObject11, gameObject4);
            Scrollbar component = gameObject11.GetComponent<Scrollbar>();
            component.SetDirection(Scrollbar.Direction.BottomToTop, true);
            RectTransform component2 = gameObject11.GetComponent<RectTransform>();
            component2.anchorMin = Vector2.right;
            component2.anchorMax = Vector2.one;
            component2.pivot = Vector2.one;
            component2.sizeDelta = new Vector2(component2.sizeDelta.x, 0f);
            Text text = gameObject10.AddComponent<Text>();
            UIControls.SetDefaultTextValues(text);
            text.alignment = TextAnchor.MiddleLeft;
            Image image = gameObject8.AddComponent<Image>();
            image.color = new Color32(245, 245, 245, byte.MaxValue);
            Image image2 = gameObject9.AddComponent<Image>();
            image2.sprite = resources.checkmark;
            Toggle toggle = gameObject7.AddComponent<Toggle>();
            toggle.targetGraphic = image;
            toggle.graphic = image2;
            toggle.isOn = true;
            Image image4 = gameObject4.AddComponent<Image>();
            image4.sprite = resources.standard;
            image4.type = Image.Type.Sliced;
            ScrollRect scrollRect = gameObject4.AddComponent<ScrollRect>();
            scrollRect.content = gameObject6.GetComponent<RectTransform>();
            scrollRect.viewport = gameObject5.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.verticalScrollbar = component;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarSpacing = -3f;
            gameObject5.AddComponent<Mask>().showMaskGraphic = false;
            Image image5 = gameObject5.AddComponent<Image>();
            image5.sprite = resources.mask;
            image5.type = Image.Type.Sliced;
            Text text2 = gameObject2.AddComponent<Text>();
            UIControls.SetDefaultTextValues(text2);
            text2.alignment = TextAnchor.MiddleLeft;
            text2.color = LabelColor;
            gameObject3.AddComponent<Image>().sprite = resources.dropdown;
            Image image3 = gameObject.AddComponent<Image>();
            image3.sprite = resources.standard;
            image3.color = UIControls.s_DefaultSelectableColor;
            image3.type = Image.Type.Sliced;
            Dropdown dropdown = gameObject.AddComponent<Dropdown>();
            dropdown.targetGraphic = image3;
            UIControls.SetDefaultColorTransitionValues(dropdown);
            dropdown.template = gameObject4.GetComponent<RectTransform>();
            dropdown.captionText = text2;
            dropdown.itemText = text;
            text.text = options[0];
            foreach (string item in options)
            {
                dropdown.options.Add(new Dropdown.OptionData
                {
                    text = item
                });
            }
            dropdown.RefreshShownValue();
            RectTransform component3 = gameObject2.GetComponent<RectTransform>();
            component3.anchorMin = Vector2.zero;
            component3.anchorMax = Vector2.one;
            component3.offsetMin = new Vector2(10f, 6f);
            component3.offsetMax = new Vector2(-25f, -7f);
            RectTransform component4 = gameObject3.GetComponent<RectTransform>();
            component4.anchorMin = new Vector2(1f, 0.5f);
            component4.anchorMax = new Vector2(1f, 0.5f);
            component4.sizeDelta = new Vector2(20f, 20f);
            component4.anchoredPosition = new Vector2(-15f, 0f);
            RectTransform component5 = gameObject4.GetComponent<RectTransform>();
            component5.anchorMin = new Vector2(0f, 0f);
            component5.anchorMax = new Vector2(1f, 0f);
            component5.pivot = new Vector2(0.5f, 1f);
            component5.anchoredPosition = new Vector2(0f, 2f);
            component5.sizeDelta = new Vector2(0f, 150f);
            RectTransform component6 = gameObject5.GetComponent<RectTransform>();
            component6.anchorMin = new Vector2(0f, 0f);
            component6.anchorMax = new Vector2(1f, 1f);
            component6.sizeDelta = new Vector2(-18f, 0f);
            component6.pivot = new Vector2(0f, 1f);
            RectTransform component7 = gameObject6.GetComponent<RectTransform>();
            component7.anchorMin = new Vector2(0f, 1f);
            component7.anchorMax = new Vector2(1f, 1f);
            component7.pivot = new Vector2(0.5f, 1f);
            component7.anchoredPosition = new Vector2(0f, 0f);
            component7.sizeDelta = new Vector2(0f, 28f);
            RectTransform component8 = gameObject7.GetComponent<RectTransform>();
            component8.anchorMin = new Vector2(0f, 0.5f);
            component8.anchorMax = new Vector2(1f, 0.5f);
            component8.sizeDelta = new Vector2(0f, 20f);
            RectTransform component9 = gameObject8.GetComponent<RectTransform>();
            component9.anchorMin = Vector2.zero;
            component9.anchorMax = Vector2.one;
            component9.sizeDelta = Vector2.zero;
            RectTransform component10 = gameObject9.GetComponent<RectTransform>();
            component10.anchorMin = new Vector2(0f, 0.5f);
            component10.anchorMax = new Vector2(0f, 0.5f);
            component10.sizeDelta = new Vector2(20f, 20f);
            component10.anchoredPosition = new Vector2(10f, 0f);
            RectTransform component11 = gameObject10.GetComponent<RectTransform>();
            component11.anchorMin = Vector2.zero;
            component11.anchorMax = Vector2.one;
            component11.offsetMin = new Vector2(20f, 1f);
            component11.offsetMax = new Vector2(-10f, -2f);
            gameObject4.SetActive(false);
            return gameObject;
        }

        // Token: 0x0600001F RID: 31 RVA: 0x000030A8 File Offset: 0x000012A8
        public static GameObject CreateScrollView(UIControls.Resources resources)
        {
            GameObject gameObject = UIControls.CreateUIElementRoot("Scroll View", new Vector2(200f, 200f));
            GameObject gameObject2 = UIControls.CreateUIObject("Viewport", gameObject);
            GameObject gameObject5 = UIControls.CreateUIObject("Content", gameObject2);
            GameObject gameObject3 = UIControls.CreateScrollbar(resources);
            gameObject3.name = "Scrollbar Horizontal";
            UIControls.SetParentAndAlign(gameObject3, gameObject);
            RectTransform component = gameObject3.GetComponent<RectTransform>();
            component.anchorMin = Vector2.zero;
            component.anchorMax = Vector2.right;
            component.pivot = Vector2.zero;
            component.sizeDelta = new Vector2(0f, component.sizeDelta.y);
            GameObject gameObject4 = UIControls.CreateScrollbar(resources);
            gameObject4.name = "Scrollbar Vertical";
            UIControls.SetParentAndAlign(gameObject4, gameObject);
            gameObject4.GetComponent<Scrollbar>().SetDirection(Scrollbar.Direction.BottomToTop, true);
            RectTransform component4 = gameObject4.GetComponent<RectTransform>();
            component4.anchorMin = Vector2.right;
            component4.anchorMax = Vector2.one;
            component4.pivot = Vector2.one;
            component4.sizeDelta = new Vector2(component4.sizeDelta.x, 0f);
            RectTransform component2 = gameObject2.GetComponent<RectTransform>();
            component2.anchorMin = Vector2.zero;
            component2.anchorMax = Vector2.one;
            component2.sizeDelta = Vector2.zero;
            component2.pivot = Vector2.up;
            RectTransform component3 = gameObject5.GetComponent<RectTransform>();
            component3.anchorMin = Vector2.up;
            component3.anchorMax = Vector2.one;
            component3.sizeDelta = new Vector2(0f, 300f);
            component3.pivot = Vector2.up;
            ScrollRect scrollRect = gameObject.AddComponent<ScrollRect>();
            scrollRect.content = component3;
            scrollRect.viewport = component2;
            scrollRect.horizontalScrollbar = gameObject3.GetComponent<Scrollbar>();
            scrollRect.verticalScrollbar = gameObject4.GetComponent<Scrollbar>();
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarSpacing = -3f;
            scrollRect.verticalScrollbarSpacing = -3f;
            Image image = gameObject.AddComponent<Image>();
            image.sprite = resources.background;
            image.type = Image.Type.Sliced;
            image.color = UIControls.s_PanelColor;
            gameObject2.AddComponent<Mask>().showMaskGraphic = false;
            Image image2 = gameObject2.AddComponent<Image>();
            image2.sprite = resources.mask;
            image2.type = Image.Type.Sliced;
            return gameObject;
        }

        // Token: 0x06000020 RID: 32 RVA: 0x000032C1 File Offset: 0x000014C1
        public static Color32 HTMLString2Color(string htmlcolorstring)
        {
            return htmlcolorstring.HexToColor();
        }

        // Token: 0x06000021 RID: 33 RVA: 0x000032CC File Offset: 0x000014CC
        public static Texture2D createDefaultTexture(string htmlcolorstring)
        {
            Color32 color = UIControls.HTMLString2Color(htmlcolorstring);
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.SetPixel(0, 0, color);
            texture2D.Apply();
            return texture2D;
        }

        // Token: 0x06000022 RID: 34 RVA: 0x000032FC File Offset: 0x000014FC
        public static Texture2D createTextureFromFile(string FilePath)
        {
            if (File.Exists(FilePath))
            {
                byte[] FileData = File.ReadAllBytes(FilePath);
                Texture2D texture2D = new Texture2D(265, 198);
                texture2D.LoadRawTextureData(FileData);
                texture2D.Apply();
                return texture2D;
            }
            return null;
        }

        // Token: 0x06000023 RID: 35 RVA: 0x00003336 File Offset: 0x00001536
        public static Sprite createSpriteFrmTexture(Texture2D SpriteTexture)
        {
            return Sprite.Create(SpriteTexture, new Rect(0f, 0f, (float)SpriteTexture.width, (float)SpriteTexture.height), new Vector2(0f, 0f), 100f, 0U, SpriteMeshType.Tight);
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00003374 File Offset: 0x00001574
        public static GameObject createUICanvas()
        {
            Debug.Log("创建画布");
            GameObject gameObject = new GameObject("CanvasGO");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasScaler.referencePixelsPerUnit = 100f;
            canvasScaler.referenceResolution = new Vector2(1024f, 788f);
            gameObject.AddComponent<GraphicRaycaster>();
            return gameObject;
        }

        // Token: 0x06000025 RID: 37 RVA: 0x000033DC File Offset: 0x000015DC
        public static GameObject createUIPanel(GameObject canvas, string width, string height, Sprite BgSprite = null)
        {
            UIControls.Resources uiResources = default(UIControls.Resources);
            uiResources.background = BgSprite;
            Debug.Log("创建UI面板");
            GameObject gameObject = UIControls.CreatePanel(uiResources);
            gameObject.transform.SetParent(canvas.transform, false);
            RectTransform component = gameObject.GetComponent<RectTransform>();
            float size = float.Parse(height);
            component.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            size = float.Parse(width);
            component.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            return gameObject;
        }

        // Token: 0x06000026 RID: 38 RVA: 0x00003440 File Offset: 0x00001640
        public static GameObject createUIButton(GameObject parent, string backgroundColor, string Text, UnityAction action, Vector3 localPosition = default(Vector3))
        {
            Debug.Log("创建UI按钮");
            Sprite btnSprite = UIControls.createSpriteFrmTexture(UIControls.createDefaultTexture(backgroundColor));
            GameObject gameObject = UIControls.CreateButton(new UIControls.Resources
            {
                standard = btnSprite
            }, Text);
            gameObject.transform.SetParent(parent.transform, false);
            Button btnComp = gameObject.GetComponent<Button>();
            btnComp.onClick.AddListener(action);
            gameObject.GetComponent<RectTransform>().localPosition = localPosition;
            ColorBlock colors = btnComp.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.5f);
            btnComp.colors = colors;
            return gameObject;
        }

        // Token: 0x06000027 RID: 39 RVA: 0x000034DC File Offset: 0x000016DC
        public static GameObject createUIToggle(GameObject parent, Sprite BgSprite, Sprite customCheckmarkSprite)
        {
            UIControls.Resources uiResources = default(UIControls.Resources);
            uiResources.standard = BgSprite;
            uiResources.checkmark = customCheckmarkSprite;
            Debug.Log("创建UI切换");
            GameObject gameObject = UIControls.CreateToggle(uiResources);
            gameObject.transform.SetParent(parent.transform, false);
            return gameObject;
        }

        // Token: 0x06000028 RID: 40 RVA: 0x00003524 File Offset: 0x00001724
        public static GameObject createUISlider(GameObject parent, Sprite BgSprite, Sprite FillSprite, Sprite KnobSprite)
        {
            UIControls.Resources uiResources = default(UIControls.Resources);
            uiResources.background = BgSprite;
            uiResources.standard = FillSprite;
            uiResources.knob = KnobSprite;
            Debug.Log("创建滑块");
            GameObject gameObject = UIControls.CreateSlider(uiResources);
            gameObject.transform.SetParent(parent.transform, false);
            return gameObject;
        }

        // Token: 0x06000029 RID: 41 RVA: 0x00003574 File Offset: 0x00001774
        public static GameObject createUIInputField(GameObject parent, Sprite BgSprite, string textColor)
        {
            UIControls.Resources uiResources = default(UIControls.Resources);
            uiResources.inputField = BgSprite;
            Debug.Log("创建UI输入框");
            GameObject uiInputField = UIControls.CreateInputField(uiResources);
            uiInputField.transform.SetParent(parent.transform, false);
            Text[] componentsInChildren = uiInputField.GetComponentsInChildren<Text>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].color = UIControls.HTMLString2Color(textColor);
            }
            return uiInputField;
        }

        // Token: 0x0600002A RID: 42 RVA: 0x000035E0 File Offset: 0x000017E0
        public static GameObject createUIDropDown(GameObject parent, Sprite BgSprite, Sprite ScrollbarSprite, Sprite DropDownSprite, Sprite CheckmarkSprite, Sprite customMaskSprite, List<string> options, Color LabelColor)
        {
            UIControls.Resources uiResources = default(UIControls.Resources);
            uiResources.standard = BgSprite;
            uiResources.background = ScrollbarSprite;
            uiResources.dropdown = DropDownSprite;
            uiResources.checkmark = CheckmarkSprite;
            uiResources.mask = customMaskSprite;
            Debug.Log("创建 UI 下拉菜单");
            GameObject gameObject = UIControls.CreateDropdown(uiResources, options, LabelColor);
            gameObject.transform.SetParent(parent.transform, false);
            return gameObject;
        }

        // Token: 0x0600002B RID: 43 RVA: 0x00003648 File Offset: 0x00001848
        public static GameObject createUIImage(GameObject parent, Sprite BgSprite)
        {
            UIControls.Resources uiResources = default(UIControls.Resources);
            uiResources.background = BgSprite;
            Debug.Log("创建图片");
            GameObject gameObject = UIControls.CreateImage(uiResources);
            gameObject.transform.SetParent(parent.transform, false);
            return gameObject;
        }

        // Token: 0x0600002C RID: 44 RVA: 0x00003688 File Offset: 0x00001888
        public static GameObject createUIRawImage(GameObject parent, Sprite BgSprite)
        {
            UIControls.Resources uiResources = default(UIControls.Resources);
            uiResources.background = BgSprite;
            Debug.Log("创建原始图片");
            GameObject gameObject = UIControls.CreateRawImage(uiResources);
            gameObject.transform.SetParent(parent.transform, false);
            return gameObject;
        }

        // Token: 0x0600002D RID: 45 RVA: 0x000036C8 File Offset: 0x000018C8
        public static GameObject createUIScrollbar(GameObject parent, Sprite ScrollbarSprite)
        {
            UIControls.Resources uiResources = default(UIControls.Resources);
            uiResources.background = ScrollbarSprite;
            Debug.Log("创建滚动条");
            GameObject gameObject = UIControls.CreateScrollbar(uiResources);
            gameObject.transform.SetParent(parent.transform, false);
            return gameObject;
        }

        // Token: 0x0600002E RID: 46 RVA: 0x00003708 File Offset: 0x00001908
        public static GameObject createUIScrollView(GameObject parent, Sprite BgSprite, Sprite customMaskSprite, Sprite customScrollbarSprite)
        {
            UIControls.Resources uiResources = default(UIControls.Resources);
            uiResources.background = BgSprite;
            uiResources.knob = BgSprite;
            uiResources.standard = customScrollbarSprite;
            uiResources.mask = customMaskSprite;
            Debug.Log("创建滚动视图");
            GameObject gameObject = UIControls.CreateScrollView(uiResources);
            gameObject.transform.SetParent(parent.transform, false);
            return gameObject;
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00003760 File Offset: 0x00001960
        public static GameObject createUIText(GameObject parent, Sprite BgSprite, string textColor = null)
        {
            UIControls.Resources uiResources = default(UIControls.Resources);
            uiResources.background = BgSprite;
            Debug.Log("创建文本");
            GameObject uiText = UIControls.CreateText(uiResources);
            uiText.transform.SetParent(parent.transform, false);
            if (textColor != null)
            {
                uiText.GetComponent<Text>().color = UIControls.HTMLString2Color(textColor);
            }
            return uiText;
        }

        // Token: 0x04000006 RID: 6
        private const float kWidth = 160f;

        // Token: 0x04000007 RID: 7
        private const float kThickHeight = 30f;

        // Token: 0x04000008 RID: 8
        private const float kThinHeight = 20f;

        // Token: 0x04000009 RID: 9
        private static Vector2 s_ThickElementSize = new Vector2(160f, 30f);

        // Token: 0x0400000A RID: 10
        private static Vector2 s_ThinElementSize = new Vector2(160f, 20f);

        // Token: 0x0400000B RID: 11
        private static Vector2 s_ImageElementSize = new Vector2(100f, 100f);

        // Token: 0x0400000C RID: 12
        private static Color s_DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);

        // Token: 0x0400000D RID: 13
        private static Color s_PanelColor = new Color(1f, 1f, 1f, 0.392f);

        // Token: 0x0400000E RID: 14
        private static Color s_TextColor = new Color(0.19607843f, 0.19607843f, 0.19607843f, 1f);

        // Token: 0x02000025 RID: 37
        public struct Resources
        {
            // Token: 0x040002F6 RID: 758
            public Sprite standard;

            // Token: 0x040002F7 RID: 759
            public Sprite background;

            // Token: 0x040002F8 RID: 760
            public Sprite inputField;

            // Token: 0x040002F9 RID: 761
            public Sprite knob;

            // Token: 0x040002FA RID: 762
            public Sprite checkmark;

            // Token: 0x040002FB RID: 763
            public Sprite dropdown;

            // Token: 0x040002FC RID: 764
            public Sprite mask;
        }
    }
}
