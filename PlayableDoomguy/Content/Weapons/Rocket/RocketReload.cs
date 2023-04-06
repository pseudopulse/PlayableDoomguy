using System;

namespace PlayableDoomguy.Weapons.Rocket {
    public class RocketReload : BaseWeaponState {
        public Sprite reloadSprite;
        public float duration = 1.3f;

        public override void OnEnter()
        {
            base.OnEnter();
            reloadSprite = Plugin.bundle.LoadAsset<Sprite>("RocketReload.png");
            weaponSprite.sprite = reloadSprite;
            duration /= base.attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration) {
                controller.SetToIdle();
            }
        }
    }
}