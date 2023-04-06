using System;

namespace PlayableDoomguy.Weapons.Rocket {
    public class RocketIdle : BaseWeaponState {
        public Sprite idleSprite;

        public override void OnEnter()
        {
            base.OnEnter();
            idleSprite = Plugin.bundle.LoadAsset<Sprite>("RocketIdle.png");
            weaponSprite.sprite = idleSprite;
        }
    }
}