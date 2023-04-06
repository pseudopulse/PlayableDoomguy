using RoR2;

namespace PlayableDoomguy.Weapons.SSG {
    public class SSGIdle : BaseWeaponState {
        public Sprite ssgIdleSprite;
        public override void OnEnter()
        {
            ssgIdleSprite = Plugin.bundle.LoadAsset<Sprite>("SSGIdle.png");
            base.OnEnter();
            weaponSprite.sprite = ssgIdleSprite;
        }
    }
}