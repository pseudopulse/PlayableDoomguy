using System.Diagnostics;
using System;

namespace PlayableDoomguy.Weapons.Rocket {
    public class RocketFire : BaseWeaponState {
        public override void OnEnter()
        {
            base.OnEnter();
            GenericSkill ammo = controller.GetAmmoSlot();
            ammo.DeductStock(1);

            FireProjectileInfo info = new();
            info.position = base.transform.position;
            info.crit = base.RollCrit();
            info.rotation = Util.QuaternionSafeLookRotation(base.GetAimRay().direction);
            info.damage = base.damageStat * 12f;
            info.owner = base.gameObject;
            info.projectilePrefab = Utils.Paths.GameObject.ToolbotGrenadeLauncherProjectile.Load<GameObject>();

            ProjectileManager.instance.FireProjectile(info);
            AkSoundEngine.PostEvent(Events.Play_MULT_m1_grenade_launcher_shoot, base.gameObject);
            controller.SetToReload();
        }
    }
}