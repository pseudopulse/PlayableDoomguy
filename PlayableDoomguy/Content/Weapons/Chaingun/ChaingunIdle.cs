using System;

namespace PlayableDoomguy.Weapons.Chaingun {
    public class ChaingunIdle : BaseWeaponState {
        public Sprite idleSprite;

        public override void OnEnter()
        {
            base.OnEnter();
            controller.FlashSprite.enabled = false;
            idleSprite = Plugin.bundle.LoadAsset<Sprite>("ChaingunIdle.png");
            weaponSprite.sprite = idleSprite;
        }
    }
}