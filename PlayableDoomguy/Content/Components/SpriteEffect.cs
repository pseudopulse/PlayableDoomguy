using System;
using UnityEngine.Events;

namespace PlayableDoomguy.Components {
    public class SpriteEffect : MonoBehaviour {
        public SpriteRenderer renderer;
        public Sprite[] sprites;
        public float delay = 0.3f;
        //
        private int index = 0;
        private float stopwatch = 0f;
        public UnityEvent OnFinish;

        public void FixedUpdate() {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= delay) {
                stopwatch = 0f;

                index++;
                if (index > sprites.Length - 1) {
                    renderer.enabled = false;
                    OnFinish?.Invoke();
                    return;
                }

                renderer.sprite = sprites[index];
            }
        }
    }
}