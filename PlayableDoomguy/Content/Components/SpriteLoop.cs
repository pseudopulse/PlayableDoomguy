using System;

namespace PlayableDoomguy.Components {
    public class SpriteLoop : MonoBehaviour {
        public SpriteRenderer renderer;
        public Sprite[] sprites;
        public float delay = 0.3f;
        //
        private int index = 0;
        private float stopwatch = 0f;

        public void FixedUpdate() {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= delay) {
                stopwatch = 0f;

                index++;
                if (index > sprites.Length - 1) {
                    index = 0;
                }

                renderer.sprite = sprites[index];
            }
        }
    }
}