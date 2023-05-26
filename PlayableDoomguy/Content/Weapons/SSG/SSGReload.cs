using System;

namespace PlayableDoomguy.Weapons.SSG {
    public class SSGReload : BaseWeaponState {
        public int index = 0;
        public Sprite[] sprites;
        public float delay = 0.2f;
        public bool hasPlayedRl = false;
        public override void OnEnter()
        {
            base.OnEnter();
            weaponSprite.preserveAspect = false;
            sprites = new Sprite[7];
            for (int i = 0; i < 7; i++) {
                sprites[i] = Plugin.bundle.LoadAsset<Sprite>("SSGReload" + (i + 1) + ".png");
            }

            AudioSource.clip = AudioCollection.FetchClipByName("SSGOP");
            AudioSource.Play();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            weaponSprite.sprite = sprites[index];
            if (base.fixedAge > delay) {
                if (!hasPlayedRl) {
                    AudioSource.clip = AudioCollection.FetchClipByName("SSGRL");
                    AudioSource.Play();
                    hasPlayedRl = true;
                }
                base.fixedAge = 0;
                index++;
            }

            if (index > 6) {
                weaponSprite.preserveAspect = true;
                AudioSource.clip = AudioCollection.FetchClipByName("SSGCL");
                AudioSource.Play();
                controller.SetToIdle();
            }
        }  
    }
}