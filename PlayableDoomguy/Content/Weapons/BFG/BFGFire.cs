using System;

namespace PlayableDoomguy.Weapons.BFG {
    public class BFGFire : BaseWeaponState {
        public int index = 1;
        public Sprite fire1;
        public Sprite fire2;
        public Sprite flash1;
        public Sprite flash2;
        public float delay = 0.3f;

        public override void OnEnter()
        {
            base.OnEnter();
            fire1 = Plugin.bundle.LoadAsset<Sprite>("BFGFire1.png");
            fire2 = Plugin.bundle.LoadAsset<Sprite>("BFGFire2.png");
            flash1 = Plugin.bundle.LoadAsset<Sprite>("BFGFlash1.png");
            flash2 = Plugin.bundle.LoadAsset<Sprite>("BFGFlash2.png");
            controller.FlashSprite.enabled = true;
        }

        public override void FixedUpdate()
        {
            base.fixedAge -= Time.fixedDeltaTime;
            if (base.fixedAge <= 0) {
                base.fixedAge = delay;

                if (index > 2) {
                    FireProjectileInfo info = new();
                    info.damage = base.damageStat * 20f;
                    info.position = base.transform.position;
                    info.rotation = Util.QuaternionSafeLookRotation(base.GetAimRay().direction);
                    info.crit = base.RollCrit();
                    info.owner = base.gameObject;
                    info.projectilePrefab = Plugin.BFGProjectile;

                    ProjectileManager.instance.FireProjectile(info);
                    controller.GetAmmoSlot().DeductStock(40);
                    controller.FlashSprite.enabled = false;
                    controller.SetToIdle();
                }

                weaponSprite.sprite = index == 1 ? fire1 : fire2;
                controller.FlashSprite.sprite = index == 1 ? flash1 : flash2;

                index++;
            }
        }
    }
}