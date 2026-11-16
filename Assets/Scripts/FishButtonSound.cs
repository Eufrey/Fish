using UnityEngine;
using UnityEngine.UI;

namespace Fish
{
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public sealed class FishButtonSound : MonoBehaviour
    {
        private Button button;
        private int playedFrame = -1;

        private void OnEnable()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Click);
        }

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(Click);
        }

        private void Click()
        {
            if (playedFrame == Time.frameCount) return;
            playedFrame = Time.frameCount;
            FishPresentation.Play(FishSound.Click);
        }
    }
}
