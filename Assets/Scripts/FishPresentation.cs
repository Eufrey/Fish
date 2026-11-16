using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Fish
{
    public enum FishSound { Click, FishingSuccess, FishingFailure, Attack, Victory, Defeat }
    // Presentation/preferences only. Never owns gameplay progression or fish state.
    [DefaultExecutionOrder(-150)]
    [DisallowMultipleComponent]
    public sealed class FishPresentation : MonoBehaviour
    {
        private static FishPresentation instance;
        private FishAudioSettings clips;
        private AudioSource effects, ambience;
        private GameObject pausePanel;
        private Text hint, volumeLabel;
        private bool paused;
        private FishCollection savedCollection;
        private FishBattleTeam savedTeam;
        private Text saveFeedback;
        private Button continueButton;
        private bool confirmNewGame;
        private int newGameFrame;
        private Vector3 shoreSpawn;
        private Quaternion shoreRotation;
        private int releaseFrame=-1;
        private float oldTime;
        private CursorLockMode oldLock;
        private bool oldVisible, oldMovement, oldCamera;
        private FishExplorationPlayer player;
        private FishOrbitCamera orbit;
        private const string VolumeKey="FISH.MasterVolume";
        private readonly int[] soundFrames = { -1, -1, -1, -1, -1, -1 };
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance=null;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if(instance!=null)return;
            var root=new GameObject("FISH Presentation and Audio");root.AddComponent<FishPresentation>();
        }
        private void Awake()
        {
            if(instance!=null && instance!=this){enabled=false;Destroy(gameObject);return;}
            instance=this;DontDestroyOnLoad(gameObject);
            clips=Resources.Load<FishAudioSettings>("FishAudioSettings");
            effects=gameObject.AddComponent<AudioSource>();effects.playOnAwake=false;effects.spatialBlend=0;effects.ignoreListenerPause=true;
            ambience=gameObject.AddComponent<AudioSource>();ambience.playOnAwake=false;ambience.spatialBlend=0;ambience.loop=true;
            AudioListener.volume=Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey,1));
            BuildPause(); SceneManager.sceneLoaded+=Loaded;SceneManager.sceneUnloaded+=Unloaded;
            StartCoroutine(StyleScene(SceneManager.GetActiveScene()));
        }
        private void Loaded(Scene scene,LoadSceneMode mode)
        {
            UpdateAmbience();
            StartCoroutine(StyleScene(scene));
        }
        private void Unloaded(Scene scene){UpdateAmbience();}
        private IEnumerator StyleScene(Scene scene)
        {
            yield return null;
            if(scene.isLoaded) foreach(var root in scene.GetRootGameObjects()) FishUITheme.Apply(root);
            UpdateAmbience();
        }
        private void UpdateAmbience()
        {
            bool shore=SceneManager.GetSceneByPath("Assets/Scenes/CoastalPrototype.unity").isLoaded && !SceneManager.GetSceneByPath(FishBattleEncounter.BattleScenePath).isLoaded;
            if(shore && clips!=null && clips.shorelineAmbience!=null)
            { if(!ambience.isPlaying) {ambience.clip=clips.shorelineAmbience;ambience.Play();} }
            else ambience.Stop();
        }
        public static void Play(FishSound sound)
        {
            if(instance==null || instance.clips==null)return;
            var settings = instance.clips;
            AudioClip clip;
            switch (sound)
            {
                case FishSound.Click: clip = settings.uiClick; break;
                case FishSound.FishingSuccess: clip = settings.fishingSuccess; break;
                case FishSound.FishingFailure: clip = settings.fishingFailure; break;
                case FishSound.Attack: clip = settings.battleAttack; break;
                case FishSound.Victory: clip = settings.victory; break;
                default: clip = settings.defeat; break;
            }
            // Duplicate callbacks in one frame must not layer the same sound.
            if (clip == null || instance.soundFrames[(int)sound] == Time.frameCount) return;
            instance.soundFrames[(int)sound] = Time.frameCount;
            instance.effects.PlayOneShot(clip);
        }
        private bool CanPause => player != null && player.isActiveAndEnabled && orbit != null && orbit.isActiveAndEnabled &&
            player.ExplorationInputEnabled && orbit.ExplorationInputEnabled && releaseFrame < 0;
        private void LateUpdate()
        {
            // Other panels claim input later in Update. Never draw this hint over them.
            hint.gameObject.SetActive(CanPause && !paused);
            if (savedCollection != null && savedTeam != null) FishSave.Poll(savedCollection, savedTeam);
            if (saveFeedback != null) saveFeedback.text = confirmNewGame ? "Replace saved and current progress? Choose Confirm New Game or Cancel." : FishSave.Feedback;
        }
        private void Update()
        {
            if(releaseFrame>=0 && Time.frameCount>releaseFrame){releaseFrame=-1;Restore();}
            if(player==null)player=FindAnyObjectByType<FishExplorationPlayer>();
            if(orbit==null)orbit=FindAnyObjectByType<FishOrbitCamera>();
            if (savedCollection == null && player != null)
            {
                savedCollection = player.GetComponent<FishCollection>();
                savedTeam = player.GetComponent<FishBattleTeam>();
                shoreSpawn = player.transform.position;
                shoreRotation = player.transform.rotation;
            }
            if(!Application.isFocused || Keyboard.current==null)return;
            if(paused){if(Keyboard.current.pKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)Close();return;}
            if(CanPause && Keyboard.current.pKey.wasPressedThisFrame && !Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                oldTime=Time.timeScale;oldLock=Cursor.lockState;oldVisible=Cursor.visible;
                oldMovement=player.ExplorationInputEnabled;oldCamera=orbit.ExplorationInputEnabled;
                player.SetExplorationInputEnabled(false);orbit.SetExplorationInputEnabled(false);
                Time.timeScale=0;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;paused=true;pausePanel.SetActive(true); continueButton.interactable = FishSave.CanContinue;
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            }
        }
        private void Close()
        {
            if (!paused) return;
            confirmNewGame = false;
            paused = false;
            pausePanel.SetActive(false);
            // Let the closing UI input finish before exploration can receive it.
            releaseFrame = Time.frameCount + 1;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            PlayerPrefs.Save();
        }
        private void Restore()
        {
            Time.timeScale=oldTime;
            if(player!=null)player.SetExplorationInputEnabled(oldMovement);
            if(orbit!=null)orbit.SetExplorationInputEnabled(oldCamera);
            Cursor.lockState=Application.isFocused?oldLock:CursorLockMode.None;Cursor.visible=!Application.isFocused||oldVisible;
        }
        private void OnDestroy()
        {
            if(instance!=this)return;
            SceneManager.sceneLoaded-=Loaded;SceneManager.sceneUnloaded-=Unloaded;
            if(paused || releaseFrame>=0)Restore();instance=null;
        }
        private static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max,Vector2 low,Vector2 high)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.offsetMin=low;r.offsetMax=high;return r;
        }
        private Text Text(string name,Transform parent,string content,Vector2 position,Vector2 size)
        {
            var r=Rect(name,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f),position-size/2,position+size/2);
            var t=r.gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=26;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;t.text=content;t.raycastTarget=false;return t;
        }
        private Button SaveButton(Transform parent, string name, float x, UnityEngine.Events.UnityAction action)
        {
            var rect = Rect(name, parent, new Vector2(.5f,.5f), new Vector2(.5f,.5f), new Vector2(x-120,-240), new Vector2(x+120,-180));
            var image = rect.gameObject.AddComponent<Image>();
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            Text("Label", rect, name, Vector2.zero, new Vector2(220,50)).fontSize = 20;
            button.onClick.AddListener(action);
            return button;
        }
        private void RestoreShore()
        {
            player.TeleportTo(shoreSpawn, shoreRotation);
            orbit.SnapToTarget();
            var story = FindAnyObjectByType<FishStoryHUD>();
            if (story != null) story.RefreshSavedShore();
            Close();
        }
        private void BuildPause()
        {
            var canvas=Rect("Pause UI",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            canvas.gameObject.AddComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<Canvas>().sortingOrder=100;
            canvas.gameObject.AddComponent<CanvasScaler>();canvas.gameObject.AddComponent<GraphicRaycaster>();
            hint=Text("Pause hint",canvas,"P: Pause / Volume",Vector2.zero,new Vector2(350,34));
            hint.rectTransform.anchorMin=hint.rectTransform.anchorMax=new Vector2(1,1);hint.rectTransform.pivot=new Vector2(1,1);hint.rectTransform.anchoredPosition=new Vector2(-28,-124);hint.fontSize=20;hint.alignment=TextAnchor.MiddleRight;
            var panel=Rect("Pause Panel",canvas,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);pausePanel=panel.gameObject;
            panel.gameObject.AddComponent<Image>().color=FishUITheme.Background;
            Text("Title",panel,"Paused",new Vector2(0,170),new Vector2(900,60)).fontSize = 32;
            volumeLabel=Text("Volume",panel,"Master volume",new Vector2(0,65),new Vector2(700,50));
            var sliderRect=Rect("Master Volume",panel,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-280,-20),new Vector2(280,20));
            sliderRect.gameObject.AddComponent<Image>().color=new Color(.15f,.3f,.3f);
            var handle=Rect("Handle",sliderRect,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);handle.sizeDelta=new Vector2(24,0);handle.gameObject.AddComponent<Image>().color=new Color(.47f,.8f,.73f);
            var slider=sliderRect.gameObject.AddComponent<Slider>();slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();slider.minValue=0;slider.maxValue=1;slider.value=AudioListener.volume;
            slider.onValueChanged.AddListener(value=>{AudioListener.volume=value;PlayerPrefs.SetFloat(VolumeKey,value);volumeLabel.text="Master volume: "+Mathf.RoundToInt(value*100)+"%";});
            volumeLabel.text="Master volume: "+Mathf.RoundToInt(slider.value*100)+"%";
            var buttonRect=Rect("Resume",panel,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-180,-150),new Vector2(180,-90));
            var image=buttonRect.gameObject.AddComponent<Image>();var button=buttonRect.gameObject.AddComponent<Button>();button.targetGraphic=image;
            Text("Label",buttonRect,"Resume",Vector2.zero,new Vector2(330,50));button.onClick.AddListener(Close);
            continueButton = SaveButton(panel, "Continue", -390, () => {
                if (FishSave.Load(savedCollection, savedTeam)) RestoreShore();
            });
            SaveButton(panel, "Save", -130, () => FishSave.Manual(savedCollection, savedTeam));
            var newGame = SaveButton(panel, "New Game", 130, () => {
                if (!confirmNewGame) { confirmNewGame = true; newGameFrame = Time.frameCount; return; }
                if (Time.frameCount <= newGameFrame) return;
                if (FishSave.NewGame(savedCollection, savedTeam)) { confirmNewGame = false; RestoreShore(); }
            });
            SaveButton(panel, "Cancel", 390, () => confirmNewGame = false);
            saveFeedback = Text("Save Feedback", panel, "", new Vector2(0,-295), new Vector2(1150,72));
            saveFeedback.fontSize = 20;
            newGame.GetComponentInChildren<Text>().text = "Confirm New Game";
            FishUITheme.Apply(canvas.gameObject);pausePanel.SetActive(false);
        }
    }
}


