using UnityEngine;
using UnityEngine.UI;
namespace Fish
{
    public static class FishUITheme
    {
        public static readonly Color Background = new Color(.03f,.08f,.1f,.98f);
        public static readonly Color Success = new Color32(168, 226, 181, 255);
        public static readonly Color Failure = new Color32(255, 178, 155, 255);
        public static readonly Color Selected = new Color32(45, 105, 95, 255);
        public static void Apply(GameObject root)
        {
            Font font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach(var text in root.GetComponentsInChildren<Text>(true))
            {
                text.font=font; text.resizeTextForBestFit=false; text.horizontalOverflow=HorizontalWrapMode.Wrap;
                // Leave result colors to their controllers. Titles share the sand accent.
                if (text.name == "Title") text.color = new Color32(244, 232, 208, 255);
            }
            foreach(var scaler in root.GetComponentsInChildren<CanvasScaler>(true))
            {
                if(scaler.GetComponent<Canvas>().renderMode==RenderMode.WorldSpace) continue;
                scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution=new Vector2(1280,720); scaler.matchWidthOrHeight=.5f;
            }
            foreach(var button in root.GetComponentsInChildren<Button>(true))
            {
                var colors=button.colors;
                colors.normalColor=new Color32(71,123,86,255);
                colors.highlightedColor=new Color32(78,137,100,255);
                colors.selectedColor=Selected;
                colors.pressedColor=new Color32(45,88,66,255);
                colors.disabledColor=new Color32(62,76,79,255);
                colors.colorMultiplier=1; colors.fadeDuration=.08f;
                button.colors=colors; button.transition=Selectable.Transition.ColorTint;
                if(button.targetGraphic!=null) button.targetGraphic.color=Color.white;
                foreach(var label in button.GetComponentsInChildren<Text>(true))
                {
                    label.color=Color.white;
                    label.rectTransform.anchorMin=Vector2.zero;
                    label.rectTransform.anchorMax=Vector2.one;
                    label.rectTransform.offsetMin=new Vector2(16,8);
                    label.rectTransform.offsetMax=new Vector2(-16,-8);
                }
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
