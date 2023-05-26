using System;

namespace PlayableDoomguy {
    public class STBar : MonoBehaviour {
        internal CharacterBody source;
        internal WeaponController controller;
        public STPercentage Health;
        public STPercentage Armor;
        public STPercentage Ammo;

        public void Initialize(CharacterBody cb, WeaponController wc) {
            source = cb;
            controller = wc;

            if (Plugin.StatusBar) {
                base.gameObject.SetActive(true);
            }
        }

        public void FixedUpdate() {
            if (!controller || !source || !Plugin.StatusBar) {
                return;
            }

            HealthComponent hc = source.healthComponent;

            Health.UpdatePercentage(hc.combinedHealth, hc.fullCombinedHealth);
            Armor.UpdatePercentage(hc.barrier, hc.fullBarrier);

            GenericSkill slot = controller.GetAmmoSlot();
            Ammo.UpdatePercentage(slot.stock);
        }
    }
}