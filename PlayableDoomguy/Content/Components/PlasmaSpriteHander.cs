using System;

namespace PlayableDoomguy.Components {
    public class PlasmaSpriteHandler : MonoBehaviour {
        public Sprite spr1;
        public Sprite spr2;
        public float delay = 0.1f;
        public float stopwatch = 0f;
        public int index = 1;
        public SpriteRenderer renderer;

        public void Start() {
            renderer = GetComponent<SpriteRenderer>();
            spr1 = Plugin.bundle.LoadAsset<Sprite>("PLSSA0.png");
            spr2 = Plugin.bundle.LoadAsset<Sprite>("PLSSB0.png");
            index = UnityEngine.Random.Range(1, 3);
        }

        public void FixedUpdate() {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= delay) {
                stopwatch = 0f;

                renderer.sprite = index % 2 == 0 ? spr1 : spr2;
                index++;
            }
        }
    }
}