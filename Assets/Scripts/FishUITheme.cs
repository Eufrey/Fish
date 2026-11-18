using UnityEngine;
using UnityEngine.UI;
namespace Fish
{
    public static class FishUITheme
    {
        public static readonly Color Background = new Color(25/255f,62/255f,75/255f,.96f);
        public static readonly Color Ink = new Color32(23, 49, 59, 255);
        public static readonly Color Gold = new Color32(242, 189, 87, 255);
        private static Sprite raisedSurface;

        // Cached, sliced artwork gives every button rounded corners and a soft bevel.
        private static Sprite RaisedSurface()
        {
            if (raisedSurface != null) return raisedSurface;
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(12 - x, x - 51);
                    float dy = Mathf.Max(12 - y, y - 51);
                    float distance = new Vector2(Mathf.Max(0, dx), Mathf.Max(0, dy)).magnitude;
                    float shade = Mathf.Lerp(.72f, 1f, y / 63f);
                    texture.SetPixel(x, y, new Color(shade, shade, shade, Mathf.Clamp01(12 - distance)));
                }
            texture.Apply();
            raisedSurface = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * .5f,
                100, 0, SpriteMeshType.FullRect, new Vector4(14, 14, 14, 14));
            raisedSurface.hideFlags = HideFlags.HideAndDontSave;
            return raisedSurface;
        }

        public static void Surface(Image image, Color tint)
        {
            image.sprite = RaisedSurface();
            image.type = Image.Type.Sliced;
            image.color = tint;
            var shadow = image.GetComponent<Shadow>() ?? image.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, .07f, .05f, .42f);
            shadow.effectDistance = new Vector2(0, -5);
        }
        public static readonly Color Success = new Color32(168, 226, 181, 255);
        public static readonly Color Failure = new Color32(255, 178, 155, 255);
        public static readonly Color Selected = new Color32(45, 105, 95, 255);
        public static void Apply(GameObject root)
        {
            Font font=Resources.Load<Font>("UI/Fonts/Nunito-ExtraBold") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach(var text in root.GetComponentsInChildren<Text>(true))
            {
                // Imported labels retain the export's type and single-line layout.
                if (text.name.StartsWith("Figma ")) continue;
                text.font=text.fontSize >= 30 ? (Resources.Load<Font>("UI/Fonts/Nunito-Black") ?? font) : font; text.fontStyle=FontStyle.Normal; text.resizeTextForBestFit=false; text.horizontalOverflow=HorizontalWrapMode.Wrap;
                if (text.name == "Collection Hint" || text.name == "Team Hint" || text.name == "Pause Hint") text.enabled = false;
                // Leave result colors to their controllers. Titles share the sand accent.
                if (text.name == "Title") text.color = new Color32(244, 232, 208, 255);
                if (text.name == "Nearby Prompt")
                {
                    text.supportRichText = true;
                    text.text = "<size=20>E</size>  <size=36>FISH</size>";
                    text.color = Gold;
                    var shadow = text.GetComponent<Shadow>() ?? text.gameObject.AddComponent<Shadow>();
                    shadow.effectDistance = new Vector2(0, -2);
                }
            }
            foreach(var scaler in root.GetComponentsInChildren<CanvasScaler>(true))
            {
                if(scaler.GetComponent<Canvas>().renderMode==RenderMode.WorldSpace) continue;
                scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
            }
            foreach(var button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.name.StartsWith("Figma ")) continue;
                var colors=button.colors;
                colors.normalColor=Gold;
                if (button.name == "Defend") colors.normalColor = new Color32(168, 226, 213, 255);
                if (button.name == "Special Ability") colors.normalColor = new Color32(190, 240, 120, 255);
                colors.highlightedColor=new Color32(255,235,166,255);
                colors.selectedColor=new Color32(190,240,120,255);
                colors.pressedColor=new Color32(218,166,74,255);
                colors.disabledColor=new Color32(115,132,122,255);
                colors.colorMultiplier=1; colors.fadeDuration=.08f;
                button.colors=colors; button.transition=Selectable.Transition.ColorTint;
                if(button.targetGraphic is Image buttonImage && buttonImage.name != "Figma Button Surface") Surface(buttonImage, Color.white);
                foreach(var label in button.GetComponentsInChildren<Text>(true))
                {
                    label.color=Ink;
                    label.rectTransform.anchorMin=Vector2.zero;
                    label.rectTransform.anchorMax=Vector2.one;
                    label.rectTransform.offsetMin=new Vector2(12,6);
                    label.rectTransform.offsetMax=new Vector2(-12,-6);
                    if (button.transform.Find("Figma attack icon") != null || button.transform.Find("Figma shield icon") != null || button.transform.Find("Figma star icon") != null)
                    {
                        label.rectTransform.offsetMin = new Vector2(64, 6); label.alignment = TextAnchor.MiddleLeft;
                    }
                }
                if (button.targetGraphic != null) FishFigmaUI.ButtonArt(button, "2:26");
                if(Application.isPlaying && button.GetComponent<FishButtonSound>()==null) button.gameObject.AddComponent<FishButtonSound>();
            }
            foreach (var image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.name == "Battle UI")
                {
                    image.color = Color.clear;
                    image.raycastTarget = false;
                    continue;
                }
                if (image.name == "Fishing Panel" || image.name == "Collection Panel" || image.name == "Team Panel" ||
                    image.name == "Story Presentation" || image.name == "Pause Panel")
                    image.color = Background;
            }
        }
    }
}
