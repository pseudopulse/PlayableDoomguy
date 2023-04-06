using System;

namespace PlayableDoomguy.Weapons.BFG {
    public class BFGIdle : BaseWeaponState {
        public Sprite idleSprite;

        public override void OnEnter()
        {
            base.OnEnter();
            idleSprite = Plugin.bundle.LoadAsset<Sprite>("BFGIdle.png");
            weaponSprite.sprite = idleSprite;
        }
    }
}