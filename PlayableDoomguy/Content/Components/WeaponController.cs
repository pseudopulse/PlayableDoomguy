using System.Diagnostics;
using System;
using UnityEngine.UI;

namespace PlayableDoomguy.Components {
    public class WeaponController : MonoBehaviour {
        public Weapon CurrentWeapon;
        public WeaponDef CurrentWeaponDef;
        public EntityStateMachine esm;
        public Image WeaponSprite;
        public Image FlashSprite;
        public GameObject WeaponDisplayInstance;
        private GameObject WeaponDisplayPrefab => Plugin.bundle.LoadAsset<GameObject>("WeaponDisplay.prefab");
        private InputBankTest input;
        private SkillLocator locator;
        private string currentCheat = "";


        public void Start() {
            WeaponDisplayInstance = GameObject.Instantiate(WeaponDisplayPrefab, base.transform);
            WeaponSprite = WeaponDisplayInstance.transform.Find("Weapon").GetComponent<Image>();
            FlashSprite = WeaponDisplayInstance.transform.Find("Flash").GetComponent<Image>();
            input = GetComponent<InputBankTest>();
            esm = EntityStateMachine.FindByCustomName(base.gameObject, "Gun");
            locator = GetComponent<SkillLocator>();
            SwitchWeapon(Weapon.SSG, true);

            StatusBarFace stf = base.gameObject.AddComponent<StatusBarFace>();
            stf.image = WeaponDisplayInstance.transform.Find("STFace").GetComponent<Image>();
        }

        public void Fire() {
            if (!esm.state.GetType().IsEquivalentTo(CurrentWeaponDef.IdleState)) {
                return;
            }

            if (!CanAfford()) {
                return;
            }
            
            SetToFire();
        }

        public void SwitchWeapon(Weapon newWeapon, bool force = false) {
            if (!force && !esm.state.GetType().IsEquivalentTo(CurrentWeaponDef.IdleState)) {
                return;
            }

            if (!CanAfford(newWeapon)) {
                return;
            }

            CurrentWeapon = newWeapon;
            CurrentWeaponDef = WeaponInfo.WeaponToDef[CurrentWeapon];
            UpdateFlash();
            UpdateSpritePos();
            SetToIdle();
        }

        public void SetToIdle() {
            EntityState state = (EntityState)System.Activator.CreateInstance(CurrentWeaponDef.IdleState);
            esm.SetNextState(state);

            if (!CanAfford()) {
                SwitchWeapon(HandleAutoswitch(), true);
            }
        }

        public void SetToReload() {
            EntityState state = (EntityState)System.Activator.CreateInstance(CurrentWeaponDef.ReloadState);
            esm.SetNextState(state);
        }

        public void SetToFire() {
            EntityState state = (EntityState)System.Activator.CreateInstance(CurrentWeaponDef.FiringState);
            esm.SetNextState(state);
        }

        public void FixedUpdate() {
            // check inputs
            if (input.skill1.down) {
                Fire();
            }

            // idkfa
            if (currentCheat.Contains("idkfa")) {
                currentCheat = "";
                HandleIDKFA();
                Chat.AddMessage("<style=cIsDamage>Very happy ammo added.</style>");
            }

            // iddqd
            if (currentCheat.Contains("iddqd")) {
                currentCheat = "";
                CharacterBody body = GetComponent<CharacterBody>();
                if (body.HasBuff(RoR2Content.Buffs.Immune)) {
                    body.RemoveBuff(RoR2Content.Buffs.Immune);
                    Chat.AddMessage("<style=cIsDamage>Degreelessness Mode Off</style>.");
                }
                else {
                    body.AddBuff(RoR2Content.Buffs.Immune);
                    Chat.AddMessage("<style=cIsDamage>Degreelessness Mode On</style>.");
                }
            }

            // iddt
            if (currentCheat.Contains("iddt")) {
                currentCheat = "";
                EquipmentSlot slot = GetComponent<EquipmentSlot>();
                slot.FireScanner();
            }

            // idchoppers
            if (currentCheat.Contains("idchoppers")) {
                currentCheat = "";
                CharacterBody body = GetComponent<CharacterBody>();
                SwitchWeapon(Weapon.Saw, true);
                body.AddTimedBuff(RoR2Content.Buffs.Immune, 0.0016f);
                Chat.AddMessage("... doesn't suck - GM");
            }

            currentCheat += Input.inputString;

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                SwitchWeapon(Weapon.Saw);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2)) {
                SwitchWeapon(Weapon.SSG);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3)) {
                SwitchWeapon(Weapon.Chaingun);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha4)) {
                SwitchWeapon(Weapon.Rocket);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha5)) {
                SwitchWeapon(Weapon.Plasma);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha6)) {
                SwitchWeapon(Weapon.BFG);
                return;
            }
        }

        public void HandleIDKFA() {
            GenericSkill bullets = locator.FindSkill(WeaponInfo.AmmoToSkillName[Ammo.Bullets]);
            GenericSkill shells = locator.FindSkill(WeaponInfo.AmmoToSkillName[Ammo.Shells]);
            GenericSkill cells = locator.FindSkill(WeaponInfo.AmmoToSkillName[Ammo.Cells]);
            GenericSkill rockets = locator.FindSkill(WeaponInfo.AmmoToSkillName[Ammo.Rockets]);

            bullets.stock = bullets.maxStock;
            shells.stock = shells.maxStock;
            cells.stock = cells.maxStock;
            rockets.stock = rockets.maxStock;
        }

        public void RestoreAmmoFraction(float fraction) {
            GenericSkill bullets = locator.FindSkill(WeaponInfo.AmmoToSkillName[Ammo.Bullets]);
            GenericSkill shells = locator.FindSkill(WeaponInfo.AmmoToSkillName[Ammo.Shells]);
            GenericSkill cells = locator.FindSkill(WeaponInfo.AmmoToSkillName[Ammo.Cells]);
            GenericSkill rockets = locator.FindSkill(WeaponInfo.AmmoToSkillName[Ammo.Rockets]);

            bullets.stock += (int)(bullets.maxStock * fraction);
            shells.stock += (int)(shells.maxStock * fraction);
            cells.stock += (int)(cells.maxStock * fraction);
            rockets.stock += (int)(rockets.maxStock * fraction);

            bullets.stock = Mathf.Min(bullets.stock, bullets.maxStock);
            shells.stock = Mathf.Min(shells.stock, shells.maxStock);
            cells.stock = Mathf.Min(cells.stock, cells.maxStock);
            rockets.stock = Mathf.Min(rockets.stock, rockets.maxStock);
        }

        public void HandleDeath() {
            WeaponDisplayInstance.SetActive(false);
        }

        public GenericSkill GetAmmoSlot() {
            return locator.FindSkill(WeaponInfo.AmmoToSkillName[CurrentWeaponDef.AmmoType]);
        }

        public GenericSkill GetAmmoSlot(Ammo ammo) {
            return locator.FindSkill(WeaponInfo.AmmoToSkillName[ammo]);
        }

        public bool CanAfford() {
            if (CurrentWeapon == Weapon.Saw) {
                return true; // chainsaw costs no ammo
            }
            GenericSkill slot = GetAmmoSlot();
            if (slot.stock >= CurrentWeaponDef.AmmoRequired) {
                return true;
            }
            return false;
        }

        public bool CanAfford(Weapon weapon) {
            if (weapon == Weapon.Saw) {
                return true;
            }

            GenericSkill slot = locator.FindSkill(WeaponInfo.AmmoToSkillName[WeaponInfo.WeaponToDef[weapon].AmmoType]);
            if (slot.stock >= WeaponInfo.WeaponToDef[weapon].AmmoRequired) {
                return true;
            }
            return false;
        }

        public Weapon HandleAutoswitch() {
            if (CanAfford(Weapon.SSG)) {
                return Weapon.SSG; // abundant ammo
            }

            if (CanAfford(Weapon.Chaingun)) {
                return Weapon.Chaingun;
            }

            if (CanAfford(Weapon.Plasma)) {
                return Weapon.Plasma;
            }

            if (CanAfford(Weapon.Rocket)) {
                return Weapon.Rocket;
            }

            if (CanAfford(Weapon.BFG)) {
                return Weapon.BFG; 
            }

            return Weapon.Saw; // chainsaw is last priority
        }

        public void UpdateSpritePos() {
            RectTransform rect = WeaponSprite.GetComponent<RectTransform>();

            switch (CurrentWeapon) {
                case Weapon.SSG:
                    rect.anchoredPosition = new(6f, 109.48f);
                    rect.sizeDelta = new(308.1886f, 218.9649f);
                    break;
                case Weapon.BFG:
                    rect.anchoredPosition = new(6f, 74f);
                    rect.sizeDelta = new(308.1886f, 218.9649f);
                    break;
                case Weapon.Chaingun:
                    rect.anchoredPosition = new(6f, 109.48f);
                    rect.sizeDelta = new(308.1886f, 218.9649f);
                    break;
                case Weapon.Plasma:
                    rect.anchoredPosition = new(-5.2452f, 109f);
                    rect.sizeDelta = new(289.7734f, 229.3235f);
                    break;
                case Weapon.Rocket:
                    rect.anchoredPosition = new(-5.2452f, 109f);
                    rect.sizeDelta = new(289.7734f, 229.3235f);
                    break;
                case Weapon.Saw:
                    rect.anchoredPosition = new(1.4305f, 75f);
                    rect.sizeDelta = new(417.5289f, 301.8334f);
                    break;
            }
        }

        public void UpdateFlash() {
            RectTransform rect = FlashSprite.GetComponent<RectTransform>();

            switch (CurrentWeapon) {
                case Weapon.SSG:
                    rect.anchoredPosition = new(6, 240f);
                    rect.sizeDelta = new(212.3218f, 151.8875f);
                    break;
                case Weapon.Chaingun:
                    rect.anchoredPosition = new(11.2f, 213f);
                    rect.sizeDelta = new(328.6252f, 114.0049f);
                    break;
                case Weapon.BFG:
                    rect.anchoredPosition = new(6f, 143f);
                    rect.sizeDelta = new(128.2542f, 105.9482f);
                    break;
                default:
                    break;
            }
        }
    }

    public static class WeaponInfo {
        public static Dictionary<Weapon, WeaponDef> WeaponToDef;
        public static Dictionary<Ammo, string> AmmoToSkillName;
        public static WeaponDef SSG;
        public static WeaponDef PLR;
        public static WeaponDef CHG;
        public static WeaponDef RCK;
        public static WeaponDef BFG;
        public static WeaponDef SAW;

        public static void Populate() {
            MakeWeaponDefs();

            WeaponToDef = new();
            AmmoToSkillName = new();

            WeaponToDef.Add(Weapon.SSG, SSG);
            WeaponToDef.Add(Weapon.Plasma, PLR);
            WeaponToDef.Add(Weapon.Rocket, RCK);
            WeaponToDef.Add(Weapon.Chaingun, CHG);
            WeaponToDef.Add(Weapon.BFG, BFG);
            WeaponToDef.Add(Weapon.Saw, SAW);

            AmmoToSkillName.Add(Ammo.Cells, "Cells");
            AmmoToSkillName.Add(Ammo.Rockets, "Rockets");
            AmmoToSkillName.Add(Ammo.Shells, "Shells");
            AmmoToSkillName.Add(Ammo.Bullets, "Bullets");
        }

        private static void MakeWeaponDefs() {
            SSG = new() {
                AmmoType = Ammo.Shells,
                AmmoRequired = 2,
                IdleState = typeof(Weapons.SSG.SSGIdle),
                ReloadState = typeof(Weapons.SSG.SSGReload),
                FiringState = typeof(Weapons.SSG.SSGFire),
            };

            PLR = new() {
                AmmoType = Ammo.Cells,
                AmmoRequired = 1,
                ReloadState = typeof(Weapons.Plasma.PlasmaReload),
                IdleState = typeof(Weapons.Plasma.PlasmaIdle),
                FiringState = typeof(Weapons.Plasma.PlasmaFire),
            };

            CHG = new() {
                AmmoType = Ammo.Bullets,
                AmmoRequired = 1,
                ReloadState = typeof(Weapons.Chaingun.ChaingunReload),
                IdleState = typeof(Weapons.Chaingun.ChaingunIdle),
                FiringState = typeof(Weapons.Chaingun.ChaingunFire)
            };

            SAW = new() {
                ReloadState = typeof(Weapons.Chainsaw.ChainsawReload),
                IdleState = typeof(Weapons.Chainsaw.ChainsawIdle),
                FiringState = typeof(Weapons.Chainsaw.ChainsawFire),
            };

            RCK = new() {
                AmmoType = Ammo.Rockets,
                AmmoRequired = 1,
                ReloadState = typeof(Weapons.Rocket.RocketReload),
                IdleState = typeof(Weapons.Rocket.RocketIdle),
                FiringState = typeof(Weapons.Rocket.RocketFire),
            };

            BFG = new() {
                AmmoType = Ammo.Cells,
                AmmoRequired = 40,
                ReloadState = typeof(Weapons.BFG.BFGReload),
                IdleState = typeof(Weapons.BFG.BFGIdle),
                FiringState = typeof(Weapons.BFG.BFGFire),
            };
        }
    }

    public enum Weapon {
        SSG,
        Plasma,
        Rocket,
        Chaingun,
        BFG,
        Saw
    }

    public enum Ammo {
        Shells,
        Bullets,
        Cells,
        Rockets
    }

    public struct WeaponDef {
        public Ammo AmmoType;
        public int AmmoRequired;
        public Type FiringState;
        public Type IdleState;
        public Type ReloadState;
    }
}