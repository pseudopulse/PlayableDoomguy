using System;

namespace PlayableDoomguy.Components {
    public class BFGController : MonoBehaviour {
        public ProjectileController controller;
        public GameObject bfgEffect;
        private CharacterBody owner;
        public AudioSource source;

        public void FixedUpdate() {
            if (controller && !owner) {
                GameObject owner = controller.owner;
                this.owner = owner.GetComponent<CharacterBody>();
            }
        }

        public void PlayBoom() {
            source.clip = Plugin.AudioCollection.FetchClipByName("BSPLOD");
            source.Play();
        }

        public void FireBFGSpray() {
            // Debug.Log("impact");
            int totalSprays = 140;
            float shotAngle = 1f;
            int index = (totalSprays / 2) * -1;

            List<HealthComponent> bfgHits = new();
            List<Vector3> bfgHitsVec = new();

            for (int i = 0; i < totalSprays; i++) {
                Vector3 aimDirection = owner.inputBank.aimDirection;
                float bonusYaw = index * shotAngle;
                index++;
                aimDirection = Util.ApplySpread(aimDirection, 0, 0, 1, 1, bonusYaw);
                
                BulletAttack attack = new();
                attack.damage = owner.damage * 2f;
                attack.falloffModel = BulletAttack.FalloffModel.None;
                // attack.tracerEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab;
                attack.aimVector = aimDirection;
                attack.isCrit = Util.CheckRoll(owner.crit, owner.master);
                attack.owner = owner.gameObject;
                attack.origin = owner.corePosition;
                attack.procCoefficient = 1f;
                attack.radius = 5f;
                attack.smartCollision = true;
                // attack.hitEffectPrefab = bfgEffect;
                attack.hitCallback = (BulletAttack attack, ref BulletAttack.BulletHit hit) => {
                    if (hit.hitHurtBox && !bfgHits.Contains(hit.hitHurtBox.healthComponent)) {
                        bfgHits.Add(hit.hitHurtBox.healthComponent);
                        bfgHitsVec.Add(hit.point);
                        // Debug.Log("adding hit");
                    }

                    return BulletAttack.DefaultHitCallbackImplementation(attack, ref hit);
                };
                attack.Fire();
            }

            foreach (HealthComponent com in bfgHits) {
                EffectManager.SpawnEffect(bfgEffect, new EffectData {
                    origin = com?.body?.mainHurtBox?.transform.position ?? com.transform.position
                }, true);
                // Debug.Log("spawning effect");
            }

            Destroy(base.gameObject);
        }
    }
}