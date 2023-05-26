using System;
using R2API;

namespace PlayableDoomguy.Weapons.Chainsaw {
    public class ChainsawFire : BaseWeaponState {
        public HitBoxGroup hitbox;
        public Sprite fireSprite;
        public float delay = 0.1f;

        public override void OnEnter()
        {
            base.OnEnter();
            fireSprite = Plugin.bundle.LoadAsset<Sprite>("SawFire.png");
            weaponSprite.sprite = fireSprite;
            hitbox = FindHitBoxGroup("Chainsaw");
            AkSoundEngine.PostEvent(Events.Play_MULT_m1_sawblade_start, base.gameObject);
            delay /= base.attackSpeedStat;

            AudioSource.clip = AudioCollection.FetchClipByName("CSRev");
            AudioSource.loop = true;
            AudioSource.Play();
        }

        public override void FixedUpdate()
        {
            base.fixedAge -= Time.fixedDeltaTime;

            if (!inputBank.skill1.down) {
                AkSoundEngine.PostEvent(Events.Play_MULT_m1_sawblade_stop, base.gameObject);
                AudioSource.loop = false;
                controller.SetToIdle();
                return;
            }

            if (base.fixedAge <= 0) {
                base.fixedAge = delay;

                BulletAttack attack = new();
                attack.damage = base.damageStat * 1f;
                attack.falloffModel = BulletAttack.FalloffModel.None;
                attack.minSpread = 0;
                attack.maxSpread = 0;
                attack.aimVector = base.GetAimRay().direction;
                attack.isCrit = base.RollCrit();
                attack.owner = base.gameObject;
                attack.origin = base.transform.position;
                attack.maxDistance = 4f;
                attack.stopperMask = LayerIndex.world.collisionMask;
                attack.radius = 3f;
                attack.procCoefficient = 1f;
                attack.AddModdedDamageType(Plugin.ChainsawType);
                attack.Fire();
            }
        }
    }
}