using System;

namespace PlayableDoomguy.Weapons.Chaingun {
    public class ChaingunFire : BaseWeaponState {
        public Sprite flash1;
        public Sprite flash2;
        public Sprite fire1;
        public Sprite fire2;
        public int shots = 1;
        public float delay = 0.1f;
        public GenericSkill ammo;

        public override void OnEnter()
        {
            base.OnEnter();
            fire1 = Plugin.bundle.LoadAsset<Sprite>("ChaingunIdle.png");
            fire2 = Plugin.bundle.LoadAsset<Sprite>("ChaingunFire.png");
            flash1 = Plugin.bundle.LoadAsset<Sprite>("ChaingunFlash1.png");
            flash2 = Plugin.bundle.LoadAsset<Sprite>("ChaingunFlash2.png");
            controller.FlashSprite.sprite = flash1;
            weaponSprite.sprite = fire1;
            controller.FlashSprite.enabled = true;
            ammo = controller.GetAmmoSlot();
            delay /= base.attackSpeedStat;
        }

        public override void FixedUpdate()
        {
            base.fixedAge -= Time.fixedDeltaTime;

            if (!inputBank.skill1.down) {
                controller.SetToIdle();
                return;
            }

            if (!controller.CanAfford()) {
                controller.SetToIdle();
                return;
            }

            if (base.fixedAge <= 0f) {
                base.fixedAge = delay;

                for (int i = 0; i < 1; i++) {
                    BulletAttack attack = new();
                    attack.damage = base.damageStat * 1f;
                    attack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
                    attack.minSpread = 0;
                    attack.maxSpread = 1;
                    attack.tracerEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab;
                    attack.aimVector = base.GetAimRay().direction;
                    attack.isCrit = base.RollCrit();
                    attack.owner = base.gameObject;
                    attack.origin = base.transform.position;
                    attack.procCoefficient = 1f;
                    attack.Fire();
                    AkSoundEngine.PostEvent(Events.Play_wPistol, base.gameObject);
                }

                ammo.DeductStock(1);

                weaponSprite.sprite = shots % 2 == 0 ? fire1 : fire2;
                controller.FlashSprite.sprite = shots % 2 == 0 ? flash1 : flash2;

                shots++;
            }
        }
    }
}