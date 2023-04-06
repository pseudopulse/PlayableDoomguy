using RoR2;
using System;
using UnityEngine.UI;

namespace PlayableDoomguy.Weapons {
    public class BaseWeaponState : BaseState {
        public WeaponController controller;
        public Image weaponSprite;

        public override void OnEnter()
        {
            base.OnEnter();
            controller = base.characterBody.GetComponent<WeaponController>();
            weaponSprite = controller.WeaponSprite;
        }
    }
}