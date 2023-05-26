using RoR2;
using System;
using UnityEngine.UI;

namespace PlayableDoomguy.Weapons {
    public class BaseWeaponState : BaseState {
        public WeaponController controller;
        public Image weaponSprite;
        public AudioSource AudioSource;
        public AudioCollection AudioCollection;

        public override void OnEnter()
        {
            base.OnEnter();
            controller = base.characterBody.GetComponent<WeaponController>();
            weaponSprite = controller.WeaponSprite;
            AudioSource = GetComponent<AudioSource>();
            AudioCollection = Plugin.AudioCollection;
        }
    }
}