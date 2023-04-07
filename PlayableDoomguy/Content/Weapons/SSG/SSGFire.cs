using System;

namespace PlayableDoomguy.Weapons.SSG {
    public class SSGFire : BaseWeaponState {
        public Sprite flash;
        public float duration = 0.3f;
        public GenericSkill ammo;
        public override void OnEnter()
        {
            base.OnEnter();
            flash = Plugin.bundle.LoadAsset<Sprite>("SSGFlash1.png");
            controller.FlashSprite.sprite = flash;
            controller.FlashSprite.enabled = true;
            for (int i = 0; i < 20; i++) {
                BulletAttack attack = new();
                attack.damage = base.damageStat * 0.8f;
                attack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
                attack.minSpread = 0;
                attack.maxSpread = 5;
                attack.tracerEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab;
                attack.aimVector = base.GetAimRay().direction;
                attack.isCrit = base.RollCrit();
                attack.owner = base.gameObject;
                attack.origin = base.transform.position;
                attack.procCoefficient = 0.6f;
                attack.stopperMask = LayerIndex.world.collisionMask;
                attack.Fire();
            }

            ammo = controller.GetAmmoSlot();
            ammo.DeductStock(2);

            AkSoundEngine.PostEvent(Events.Play_captain_m1_shotgun_shootTight, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration) {
                controller.FlashSprite.enabled = false;
                controller.SetToReload();
            }
        }
    }
}