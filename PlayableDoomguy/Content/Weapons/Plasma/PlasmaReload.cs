using System;

namespace PlayableDoomguy.Weapons.Plasma {
    public class PlasmaReload : BaseWeaponState {
        public Sprite reloadSprite;
        public float duration = 0.3f;

        public override void OnEnter()
        {
            base.OnEnter();
            reloadSprite = Plugin.bundle.LoadAsset<Sprite>("PlasmaReload.png");
            weaponSprite.sprite = reloadSprite;
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