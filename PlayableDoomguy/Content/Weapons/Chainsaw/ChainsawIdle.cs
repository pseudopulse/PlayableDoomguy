using System;

namespace PlayableDoomguy.Weapons.Chainsaw {
    public class ChainsawIdle : BaseWeaponState {
        public Sprite idle1;
        public Sprite idle2;
        public int index = 1;
        public float delay = 0.2f;

        public override void OnEnter()
        {
            base.OnEnter();
            idle1 = Plugin.bundle.LoadAsset<Sprite>("SawIdle.png");
            idle2 = Plugin.bundle.LoadAsset<Sprite>("SawIdle2.png");
            AudioSource.clip = AudioCollection.FetchClipByName("CSIdle");
            AudioSource.loop = true;
            AudioSource.Play();
        }

        public override void FixedUpdate()
        {
            base.fixedAge -= Time.fixedDeltaTime;
            if (base.fixedAge <= 0f) {
                base.fixedAge = delay;
                weaponSprite.sprite = index % 2 == 0 ? idle1 : idle2;
                index++;
            }
        }
    }
}