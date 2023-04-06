using System;

namespace PlayableDoomguy.Weapons.Chainsaw {
    public class ChainsawReload : BaseWeaponState {
        public override void OnEnter()
        {
            base.OnEnter();
            controller.SetToIdle();
        }
    }
}