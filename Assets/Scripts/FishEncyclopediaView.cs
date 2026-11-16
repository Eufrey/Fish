using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace Fish
{
    // Presentation within the collection panel; no second input or cursor handler.
    public sealed class FishEncyclopediaView
    {
        private readonly Text contents, title, note;
        private readonly Image portrait;
        private readonly Button previous, next;
        private readonly ScrollRect scroll;
        private readonly FishCollection collection;
        private readonly FishCatchCatalog catalog;
        private int index;
        public FishEncyclopediaView(GameObject panel, Text body, Button close, FishCollection owned, FishCatchCatalog data)
        {
            contents = body; collection = owned; catalog = data;
            title = panel.transform.Find("Title").GetComponent<Text>();
            note = panel.transform.Find("Session Note").GetComponent<Text>();
            Position(title.rectTransform, new Vector2(0,1), new Vector2(1,1), new Vector2(28,-78), new Vector2(-160,-28));
            Position(note.rectTransform, new Vector2(0,1), new Vector2(1,1), new Vector2(28,-140), new Vector2(-160,-88));
            note.fontSize = 20; note.horizontalOverflow = HorizontalWrapMode.Wrap;
            var imageRect = Rect("Species Image", panel.transform);
            Position(imageRect, Vector2.one, Vector2.one, new Vector2(-140,-140), new Vector2(-28,-28));
            portrait = imageRect.gameObject.AddComponent<Image>(); portrait.preserveAspect = true; portrait.raycastTarget = false;
            var viewport = Rect("Entry Scroll", panel.transform);
            Position(viewport, Vector2.zero, Vector2.one, new Vector2(28,110), new Vector2(-28,-154));
            viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color = new Color(0,0,0,.02f);
            scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.viewport = viewport;
            contents.transform.SetParent(viewport,false);
            contents.rectTransform.anchorMin = new Vector2(0,1); contents.rectTransform.anchorMax = Vector2.one;
            contents.rectTransform.pivot = new Vector2(.5f,1); contents.rectTransform.anchoredPosition = Vector2.zero;
            contents.rectTransform.sizeDelta = Vector2.zero;
            contents.alignment = TextAnchor.UpperLeft; contents.fontSize = 24;
            contents.horizontalOverflow = HorizontalWrapMode.Wrap; contents.verticalOverflow = VerticalWrapMode.Overflow;
            contents.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = contents.rectTransform;
            Position(close.GetComponent<RectTransform>(), new Vector2(.5f,0), new Vector2(.5f,0), new Vector2(-130,28),new Vector2(130,84));
            previous = Navigation("Previous Entry",panel.transform,contents.font,-310);
            next = Navigation("Next Entry",panel.transform,contents.font,310);
            previous.onClick.AddListener(() => { index--; Show(true); ResetScroll(); });
            next.onClick.AddListener(() => { index++; Show(true); ResetScroll(); });
            FishUITheme.Apply(panel);
        }
        public void ResetScroll()
        {
            Canvas.ForceUpdateCanvases();
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1;
        }
        public void Show(bool encyclopedia)
        {
            previous.gameObject.SetActive(encyclopedia); next.gameObject.SetActive(encyclopedia);
            portrait.gameObject.SetActive(false);
            title.text = encyclopedia ? "Fish Encyclopedia" : "Fish Collection";
            note.text = encyclopedia ? "Rarity means gameplay frequency, not abundance or conservation status. N / Escape: close." : "Progress autosaves at safe points. C / Escape: close.";
            scroll.verticalNormalizedPosition = 1;
            if (!encyclopedia) return;
            if (catalog == null || !catalog.IsValid()) { contents.text = "Encyclopedia needs a valid catch catalog."; return; }
            index = Mathf.Clamp(index,0,catalog.entries.Length-1);
            previous.interactable = index > 0; next.interactable = index + 1 < catalog.entries.Length;
            FishSpecies species = catalog.entries[index].species;
            bool discovered = false;
            foreach (FishOwnedCatch fish in collection.Catches)
                if (fish.Species != null && fish.Species.speciesId == species.speciesId) { discovered = true; break; }
            string heading = "Entry " + (index+1) + " / " + catalog.entries.Length + "\n\n";
            if (!discovered) { contents.text = heading + "Undiscovered fish\nCatch this species to reveal its entry."; return; }
            portrait.sprite = species.icon; portrait.gameObject.SetActive(species.icon != null);
            var text = new StringBuilder(heading);
            text.AppendLine(string.IsNullOrWhiteSpace(species.displayName) ? "Unnamed species" : species.displayName);
            text.AppendLine("Rarity: " + species.rarity + " (gameplay)");
            if (species.icon == null) text.AppendLine("Image not available.");
            Field(text,"Local name",species.localName); Field(text,"Common name",species.englishCommonName);
            Field(text,"Scientific name",species.scientificName); Field(text,"Name region/language",species.localNameLanguageOrRegion);
            text.AppendLine("\nBattle stats: HP " + species.health + " | ATK " + species.attack + " | DEF " + species.defense + " | SPD " + species.speed);
            Field(text,"Habitat",species.habitat); Field(text,"Description",species.description);
            Field(text,"Educational fact",species.educationalFact);
            Field(text,"References",species.referenceUrls); Field(text,"Reference notes",species.referenceNotes);
            contents.text = text.ToString();
        }
        private static void Field(StringBuilder text,string label,string value) => text.AppendLine("\n" + label + ": " + (string.IsNullOrWhiteSpace(value) ? "Not researched yet." : value));
        private static RectTransform Rect(string name,Transform parent)
        { var r = new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); return r; }
        private static void Position(RectTransform r,Vector2 min,Vector2 max,Vector2 low,Vector2 high)
        { r.anchorMin=min; r.anchorMax=max; r.offsetMin=low; r.offsetMax=high; }
        private static Button Navigation(string name,Transform parent,Font font,float x)
        {
            var r=Rect(name,parent); Position(r,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(x-130,28),new Vector2(x+130,84));
            var image=r.gameObject.AddComponent<Image>(); image.color=new Color32(71,123,86,255);
            var button=r.gameObject.AddComponent<Button>(); button.targetGraphic=image;
            var label=Rect("Label",r); Position(label,Vector2.zero,Vector2.one,new Vector2(12,8),new Vector2(-12,-8));
            var text=label.gameObject.AddComponent<Text>(); text.font=font; text.fontSize=22; text.text=name; text.alignment=TextAnchor.MiddleCenter; text.raycastTarget=false;
            return button;
        }
    }
}

