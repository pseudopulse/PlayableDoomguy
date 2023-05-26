using System.Diagnostics;
using System;
using UnityEngine.UI;
using RoR2.UI;

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
        private CameraRigController cam;
        private CharacterBody body;
        private AudioSource source;

        public void Start() {
            body = GetComponent<CharacterBody>();
            input = GetComponent<InputBankTest>();
            esm = EntityStateMachine.FindByCustomName(base.gameObject, "Gun");
            locator = GetComponent<SkillLocator>();
            source = GetComponent<AudioSource>();


            Invoke(nameof(SetupHUD), 0.2f); // unity technologies !!!
            // the authority check will always fail at Start()
        }

        public void SetupHUD() {
            if (!body.hasAuthority) {
                return;
            }
            WeaponDisplayInstance = GameObject.Instantiate(WeaponDisplayPrefab, cam?.hud?.transform ?? base.transform);
            WeaponSprite = WeaponDisplayInstance.transform.Find("Weapon").GetComponent<Image>();
            FlashSprite = WeaponDisplayInstance.transform.Find("Flash").GetComponent<Image>();
            SwitchWeapon(Weapon.SSG, true);

            StatusBarFace stf = base.gameObject.AddComponent<StatusBarFace>();
            stf.image = WeaponDisplayInstance.transform.Find("STFace").GetComponent<Image>();

            /*STBar bar = WeaponDisplayInstance.transform.Find("STBar").GetComponent<STBar>();
            bar.Initialize(body, this);
            */
            if (cam?.hud) {
                cam.hud.canvas.sortingOrder = 2; // put the hud above the weapon display
            }
        }

        public void Fire() {
            if (!esm.state.GetType().IsEquivalentTo(CurrentWeaponDef.IdleState)) {
                if (esm.state.GetType().IsEquivalentTo(CurrentWeaponDef.ReloadState) && CurrentWeapon == Weapon.Plasma) {
                    // plasma rifle has a "fake" reload and can be interrupted at any point
                }
                else {
                    return; // weapon currently firing or reloading, reject input
                }
            }

            if (!CanAfford()) {
                return;
            }
            
            SetToFire();

            body.OnSkillActivated(locator.primary);
        }

        public void SwitchWeapon(Weapon newWeapon, bool force = false) {
            if (!force && !esm.state.GetType().IsEquivalentTo(CurrentWeaponDef.IdleState)) {
                if (!Plugin.QuickSwitch && CurrentWeapon != Weapon.Plasma) { // plasma rifle recoil frames dont count as reloads
                    return;
                }
            }

            if (!CanAfford(newWeapon)) {
                return;
            }

            source.loop = false;

            CurrentWeapon = newWeapon;
            CurrentWeaponDef = WeaponInfo.WeaponToDef[CurrentWeapon];
            FlashSprite.enabled = false;
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

        public void Update() {
            if (!body.hasAuthority || !WeaponDisplayInstance) {
                return;
            }
            // check inputs
            if (input.skill1.down) {
                Fire();
            }

            if (Plugin.CheatCodes) {
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
                    Chat.AddMessage("<style=cIsDamage>... doesn't suck - GM</style>");
                }

                currentCheat += Input.inputString;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) {
                SwitchWeapon(Weapon.Saw);
                return;
            }

            // casting like this is awful but i couldn't thing of a better way
            // (cast shifts inputs over by 1 to match the classic doom keybinds, which are 1 off here due to lack of pistol)

            if (Input.GetKeyDown((KeyCode)((int)(KeyCode.Alpha2) + (Plugin.UseClassicKeybinds ? 1 : 0)))) {
                SwitchWeapon(Weapon.SSG);
                return;
            }

            if (Input.GetKeyDown((KeyCode)((int)(KeyCode.Alpha3) + (Plugin.UseClassicKeybinds ? 1 : 0)))) {
                SwitchWeapon(Weapon.Chaingun);
                return;
            }

            if (Input.GetKeyDown((KeyCode)((int)(KeyCode.Alpha4) + (Plugin.UseClassicKeybinds ? 1 : 0)))) {
                SwitchWeapon(Weapon.Rocket);
                return;
            }

            if (Input.GetKeyDown((KeyCode)((int)(KeyCode.Alpha5) + (Plugin.UseClassicKeybinds ? 1 : 0)))) {
                SwitchWeapon(Weapon.Plasma);
                return;
            }

            if (Input.GetKeyDown((KeyCode)((int)(KeyCode.Alpha6) + (Plugin.UseClassicKeybinds ? 1 : 0)))) {
                SwitchWeapon(Weapon.BFG);
                return;
            }

            // hud
            if (!cam) {
                cam = LocalUserManager.GetFirstLocalUser()?._cameraRigController ?? null;
                return;
            }

            if (cam && !cam.isHudAllowed) {
                WeaponDisplayInstance.SetActive(false);
            }
            else {
                WeaponDisplayInstance.SetActive(true);
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
            if (!body.hasAuthority) {
                return;
            }
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
            if (!body.hasAuthority) {
                return;
            }
            RectTransform rect = WeaponSprite.GetComponent<RectTransform>();

            switch (CurrentWeapon) {
                case Weapon.SSG:
                    rect.anchoredPosition = new(4f, 109.48f);
                    rect.sizeDelta = new(308.1886f, 218.9649f);
                    break;
                case Weapon.BFG:
                    rect.anchoredPosition = new(4f, 74f);
                    rect.sizeDelta = new(308.1886f, 218.9649f);
                    break;
                case Weapon.Chaingun:
                    rect.anchoredPosition = new(4f, 109.48f);
                    rect.sizeDelta = new(308.1886f, 218.9649f);
                    break;
                case Weapon.Plasma:
                    rect.anchoredPosition = new(-3.2452f, 109f);
                    rect.sizeDelta = new(289.7734f, 229.3235f);
                    break;
                case Weapon.Rocket:
                    rect.anchoredPosition = new(-3.2452f, 109f);
                    rect.sizeDelta = new(289.7734f, 229.3235f);
                    break;
                case Weapon.Saw:
                    rect.anchoredPosition = new(-1.4305f, 75f);
                    rect.sizeDelta = new(417.5289f, 301.8334f);
                    break;
            }

            if (Plugin.StatusBar) {
                rect.anchoredPosition = new(rect.anchoredPosition.x, rect.anchoredPosition.y + 90); // shift up 90 to account for stbar
            }
        }

        public void UpdateFlash() {
            if (!body.hasAuthority) {
                return;
            }
            RectTransform rect = FlashSprite.GetComponent<RectTransform>();

            switch (CurrentWeapon) {
                case Weapon.SSG:
                    rect.anchoredPosition = new(4, 240f);
                    rect.sizeDelta = new(212.3218f, 151.8875f);
                    break;
                case Weapon.Chaingun:
                    rect.anchoredPosition = new(9.2f, 213f);
                    rect.sizeDelta = new(328.6252f, 114.0049f);
                    break;
                case Weapon.BFG:
                    rect.anchoredPosition = new(4f, 143f);
                    rect.sizeDelta = new(128.2542f, 105.9482f);
                    break;
                default:
                    break;
            }

            if (Plugin.StatusBar) {
                rect.anchoredPosition = new(rect.anchoredPosition.x, rect.anchoredPosition.y + 90); // shift up 90 to account for stbar
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