using System;

namespace PlayableDoomguy.Components {
    public class StatusBarFace : MonoBehaviour {
        public CharacterBody body;
        public Image image;

        //
        public Sprite IDLE;
        public Sprite BL;
        public Sprite BR;
        public Sprite LR;
        public Sprite LL;
        public Sprite EVIL;
        public Sprite OUCH;
        public Sprite KILL;
        public Sprite GOD;
        public Sprite[] blSprs;
        //
        public float stopwatch = 0f;
        public float delay = 1f;
        public STFAction action = STFAction.None;

        public void Start() {
            BL = Plugin.bundle.LoadAsset<Sprite>("STFBL.png");
            BR = Plugin.bundle.LoadAsset<Sprite>("STFBR.png");
            LL = Plugin.bundle.LoadAsset<Sprite>("STFLL.png");
            LR = Plugin.bundle.LoadAsset<Sprite>("STFLR.png");
            OUCH = Plugin.bundle.LoadAsset<Sprite>("STFOuch.png");
            EVIL = Plugin.bundle.LoadAsset<Sprite>("STFEvil.png");
            GOD = Plugin.bundle.LoadAsset<Sprite>("STFGod.png");
            KILL = Plugin.bundle.LoadAsset<Sprite>("STFKill.png");
            IDLE = Plugin.bundle.LoadAsset<Sprite>("STFIdle.png");
            blSprs = new Sprite[3] { BL, IDLE, BR };
            body = GetComponent<CharacterBody>();

            RectTransform rect = image.GetComponent<RectTransform>();
            if (Plugin.StatusBar) {
                rect.anchoredPosition = new(-668, 51); // move face over to the status bar position
            }
        }

        public void FixedUpdate() {
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= delay) {
                stopwatch = 0f;
                // god
                if (body.HasBuff(RoR2Content.Buffs.Immune) || body.HasBuff(RoR2Content.Buffs.HiddenInvincibility)) {
                    image.sprite = GOD;
                    delay = 3f;
                    return;
                }

                // evil
                if (action == STFAction.Pickup) {
                    image.sprite = EVIL;
                    action = STFAction.None;
                    delay = 5f;
                    return;
                }

                // ouch
                if (action == STFAction.Hurt) {
                    image.sprite = OUCH;
                    action = STFAction.None;
                    delay = 1f;
                    return;
                }

                // kill
                if (action == STFAction.Kill) {
                    image.sprite = KILL;
                    action = STFAction.None;
                    delay = 2f;
                    return;
                }

                // blinking
                image.sprite = blSprs[UnityEngine.Random.Range(0, 3)];
                delay = 1f;
            }
        }
    }

    public enum STFAction {
        Kill,
        Hurt,
        Pickup,
        None,
    }
}