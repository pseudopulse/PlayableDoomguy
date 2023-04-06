using System;

namespace PlayableDoomguy.Weapons.BFG {
    public class BFGReload : BaseWeaponState {

        public override void OnEnter()
        {
            base.OnEnter();
            controller.SetToIdle();
        }
    }
}