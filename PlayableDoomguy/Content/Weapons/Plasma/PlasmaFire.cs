using System;

namespace PlayableDoomguy.Weapons.Plasma {
    public class PlasmaFire : BaseWeaponState {
        public int shots = 1;
        public Sprite fire1;
        public Sprite fire2;
        public GameObject prefab;
        public float delay = 0.16f;
        public GenericSkill ammo;

        public override void OnEnter()
        {
            base.OnEnter();
            prefab = Plugin.PlasmaRifleProjectile;
            fire1 = Plugin.bundle.LoadAsset<Sprite>("PlasmaFire1.png");
            fire2 = Plugin.bundle.LoadAsset<Sprite>("PlasmaFire2.png");
            ammo = controller.GetAmmoSlot();
            delay /= base.attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.fixedAge -= Time.fixedDeltaTime;
            if (!inputBank.skill1.down) {
                controller.SetToReload();
                return;
            }
            if (base.fixedAge <= 0) {
                if (!controller.CanAfford()) {
                    controller.SetToReload();
                    return;
                }

                base.fixedAge = delay;

                weaponSprite.sprite = shots % 2 == 0 ? fire1 : fire2;

                FireProjectileInfo info = new();
                info.position = base.transform.position;
                info.rotation = Util.QuaternionSafeLookRotation(base.inputBank.GetAimRay().direction);
                info.damage = base.damageStat * 1.2f;
                info.crit = base.RollCrit();
                info.owner = base.gameObject;
                info.projectilePrefab = prefab;

                if (Util.CheckRoll(20f)) {
                    info.damageTypeOverride = DamageType.Stun1s;
                }

                ProjectileManager.instance.FireProjectile(info);
                AkSoundEngine.PostEvent(Events.Play_mage_m2_charge, base.gameObject);
                ammo.DeductStock(1);
                shots++;
            }
        }
    }
}