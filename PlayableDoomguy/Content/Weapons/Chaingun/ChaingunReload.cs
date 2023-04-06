using System;

namespace PlayableDoomguy.Weapons.Chaingun {
    public class ChaingunReload : BaseWeaponState {
        public override void OnEnter()
        {
            base.OnEnter();
            controller.SetToIdle();
        }
    }
}