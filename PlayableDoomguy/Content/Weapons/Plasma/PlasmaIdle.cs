using System;

namespace PlayableDoomguy.Weapons.Plasma {
    public class PlasmaIdle : BaseWeaponState {
        public Sprite idleSprite;

        public override void OnEnter()
        {
            base.OnEnter();
            idleSprite = Plugin.bundle.LoadAsset<Sprite>("PlasmaIdle.png");
            weaponSprite.sprite = idleSprite;
        }
    }
}