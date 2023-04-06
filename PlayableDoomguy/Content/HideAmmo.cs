using System;
using RoR2.UI;
using MonoMod.RuntimeDetour;
using System.Reflection;
using BepInEx;

namespace PlayableDoomguy {
    public static class HideAmmo {
        public delegate void orig_Rebuild(LoadoutPanelController self);
        private static BindingFlags RebuildFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        private static BindingFlags RebuildHookFlags = BindingFlags.NonPublic | BindingFlags.Static;
        public static void Hook() {
            if (true) {
                Hook RebuildHook = new Hook(
                    typeof(LoadoutPanelController).GetMethod(nameof(LoadoutPanelController.Rebuild), RebuildFlags),
                    typeof(HideAmmo).GetMethod(nameof(HopooGamesTheSequel), RebuildHookFlags)
                );
            }
        }

        private static void HopooGamesTheSequel(orig_Rebuild orig, LoadoutPanelController self) {
            try {
                self.DestroyRows();
                CharacterBody prefab = BodyCatalog.GetBodyPrefabBodyComponent(self.currentDisplayData.bodyIndex);
                if (prefab) {
                    GenericSkill[] skills = prefab.GetComponents<GenericSkill>();

                    for (int i = 0; i < skills.Length; i++) {
                        if (skills[i].skillName != null && prefab.GetComponent<WeaponController>() && skills[i].hideInCharacterSelect) {
                            continue;
                        }
                        else {
                            self.rows.Add(LoadoutPanelController.Row.FromSkillSlot(self, self.currentDisplayData.bodyIndex, i, skills[i]));
                        }
                    }

                    self.rows.Add(LoadoutPanelController.Row.FromSkin(self, self.currentDisplayData.bodyIndex));
                }
            } catch {
                orig(self);
            }
        }
    }
}