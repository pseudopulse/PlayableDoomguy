using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;

namespace PlayableDoomguy {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class Plugin : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulse";
        public const string PluginName = "PlayableDoomguy";
        public const string PluginVersion = "1.2.0";

        public static AssetBundle bundle;
        public static BepInEx.Logging.ManualLogSource ModLogger;
        // assets
        public static GameObject DoomguyBody;
        public static SurvivorDef sdDoomguy;
        //
        public static GameObject BFGProjectile;
        public static GameObject PlasmaRifleProjectile;
        public static DamageAPI.ModdedDamageType ChainsawType = DamageAPI.ReserveDamageType();
        public static AudioCollection AudioCollection;
        // configs

        public static bool CheatCodes;
        public static bool QuickSwitch;
        public static bool StatusBar;
        public static bool UseClassicKeybinds;
        public static bool BypassChainsawCheck;

        public void Awake() {
            // assetbundle loading 
            bundle = AssetBundle.LoadFromFile(Assembly.GetExecutingAssembly().Location.Replace("PlayableDoomguy.dll", "doombundle"));

            AudioCollection = Plugin.bundle.LoadAsset<AudioCollection>("WeaponSounds.asset");

            // set logger
            ModLogger = Logger;

            LoadAssets();
            ModifyAssets();
            SetupLanguage();

            foreach (SkillFamily family in bundle.LoadAllAssets<SkillFamily>()) {
                ContentAddition.AddSkillFamily(family);
            }

            ContentAddition.AddBody(DoomguyBody);
            ContentAddition.AddSurvivorDef(sdDoomguy);

            WeaponInfo.Populate();
            HideAmmo.Hook();

            QuickSwitch = Config.Bind<bool>("Configuration", "Quick Switching", false, "Allows for DOOM Eternal style quickswitching.").Value;
            CheatCodes = Config.Bind<bool>("Configuration", "Cheat Codes", true, "Enables the DOOM 2 cheat codes when playing as Doomguy.").Value;
            // StatusBar = Config.Bind<bool>("Configuration", "Status Bar", true, "Replace the standard HUD with the DOOM status bar.").Value;
            StatusBar = false; // disabled for now
            BypassChainsawCheck = Config.Bind<bool>("Configuration", "Chainsaw Ammo Restore", false, "Allows the Chainsaw to restore ammo on any hit instead of just bosses. For use with Simulacrum of Artifact of Sacrifice.").Value;
            UseClassicKeybinds = Config.Bind<bool>("Configuration", "Classic Keybinds", false, "Shifts keyinds over by +1 to match that of doom").Value;
            // STBarTextManager.Init();

            // ammo drops
            On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, interactor) => {
                orig(self, interactor);
                if (interactor.GetComponent<WeaponController>()) {
                    interactor.GetComponent<WeaponController>().RestoreAmmoFraction(0.5f);
                }
            };

            On.RoR2.SkillLocator.ApplyAmmoPack += (orig, self) => {
                orig(self);
                if (self.GetComponent<WeaponController>()) {
                    self.GetComponent<WeaponController>().RestoreAmmoFraction(0.1f);
                }
            };  


            On.RoR2.GlobalEventManager.ServerDamageDealt += (orig, report) => {
                orig(report);
                if (report.attacker) {
                    WeaponController controller = report.attacker.GetComponent<WeaponController>();
                    bool isValid = BypassChainsawCheck || report.victimIsChampion;
                    if (isValid && report.damageInfo.HasModdedDamageType(ChainsawType)) {
                        controller.RestoreAmmoFraction(0.01f);
                    }
                }
            };

            // status face

            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, report) => {
                orig(self, report);
                if (report.attacker)  {
                    StatusBarFace face = report.attacker.GetComponent<StatusBarFace>();

                    if (face) {
                        face.action = STFAction.Kill;
                        face.stopwatch = face.delay;
                    }
                }

                if (report.victim) {
                    WeaponController controller = report.victim.GetComponent<WeaponController>();
                    if (controller) {
                        controller.HandleDeath();
                    }
                }
            };

            On.RoR2.HealthComponent.TakeDamage += (orig, self, info) => {
                orig(self, info);
                StatusBarFace face = self.GetComponent<StatusBarFace>();
                if (face) {
                    face.action = STFAction.Hurt;
                } 
            };

            On.RoR2.GenericPickupController.AttemptGrant += (orig, self, interactor) => {
                orig(self, interactor);
                StatusBarFace face = interactor.GetComponent<StatusBarFace>();
                if (face) {
                    face.action = STFAction.Pickup;
                } 
            };

            On.RoR2.CharacterBody.RecalculateStats += (orig, self) => {
                orig(self);
                StatusBarFace face = self.GetComponent<StatusBarFace>();
                if (face) {
                    face.stopwatch = face.delay;
                } 
            };

            On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += (orig, self) => {
                if (self.baseNameToken == "DG_BODY_NAME") {
                    return; // dont display vfx on doomguy
                }

                orig(self);
            };

            /*On.RoR2.Projectile.ProjectileManager.InitializeProjectile += (orig, projectile, info) => {
                orig(projectile, info);
                if (projectile.name.Contains("GrenadeLauncher") && info.owner && info.owner.name.Contains("Doomguy")) {
                    TeamFilter filter = projectile.GetComponent<TeamFilter>();
                    if (filter) {
                        filter.teamIndex = TeamIndex.Neutral;
                        filter.teamIndexInternal = (int)TeamIndex.Neutral;
                        filter.defaultTeam = TeamIndex.Neutral;
                    }

                    ProjectileImpactExplosion explosion = projectile.GetComponent<ProjectileImpactExplosion>();
                    if (explosion) {
                        explosion.bonusBlastForce = Vector3.up * 4000f;
                    }

                    
                }
            };*/

            BFGProjectile = bundle.LoadAsset<GameObject>("BFGProjectile.prefab");
            ContentAddition.AddProjectile(BFGProjectile);
            GameObject effect = bundle.LoadAsset<GameObject>("BFGEffect.prefab");
            ContentAddition.AddEffect(effect);
            PlasmaRifleProjectile = Plugin.bundle.LoadAsset<GameObject>("PlasmaProjectile.prefab");
            PlasmaRifleProjectile.AddComponent<PlasmaSpriteHandler>();
        }

        public void LoadAssets() {
            DoomguyBody = bundle.LoadAsset<GameObject>("Assets/Doomguy/DoomguyBody.prefab");
            sdDoomguy = bundle.LoadAsset<SurvivorDef>("Assets/Doomguy/sdDoomguy.asset");
        }

        public void ModifyAssets() {
            DoomguyBody.AddComponent<WeaponController>();

            DoomguyBody.GetComponent<CharacterBody>().bodyFlags |= CharacterBody.BodyFlags.SprintAnyDirection;
            DoomguyBody.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Utils.Paths.GameObject.StandardCrosshair.Load<GameObject>();

            if (Config.Bind<bool>("Configuration", "Faster Movespeed", false, "Increases Doomguy's movespeed to be on par with that of DOOM2").Value) {
                DoomguyBody.GetComponent<CharacterBody>().baseMoveSpeed = 10;
                DoomguyBody.GetComponent<CharacterBody>().sprintingSpeedMultiplier = 2;
            }
        }

        public void SetupLanguage() {
            // body
            "DG_BODY_NAME".Add("Doomguy");
            "DG_BODY_DESC".Add(
                """
                The Doomguy is a space marine wielding an arsenal of weapons to deal with almost any threat.

                <!> The Super Shotgun and Chaingun are reliable damage dealers with abundant ammunition.
                
                <!> The Plasma Rifle can effectively lock down targets, but drains cells quickly.

                <!> Make good use of Interactables to keep your ammo topped off!

                <!> The BFG-9000 deals colossal damage, but has very limited ammunition. Think carefully when deciding to use it.
                """
            );
            // ammo
            "DG_AMMO_BULLETS_NAME".Add("Bullets");
            "DG_AMMO_BULLETS_DESC".Add("Used by the Chaingun.");
            "DG_AMMO_SHELLS_NAME".Add("Shells");
            "DG_AMMO_SHELLS_DESC".Add("Used by the Super Shotgun");
            "DG_AMMO_CELLS_NAME".Add("Cells");
            "DG_AMMO_CELLS_DESC".Add("Used by the Plasma Rifle and BFG-9000");
            "DG_AMMO_ROCKETS_NAME".Add("Rockets");
            "DG_AMMO_ROCKETS_DESC".Add("Used by the Rocket Launcher");
            // weapons
            "DG_SAW_NAME".Add("Chainsaw");
            "DG_SAW_DESC".Add("Saw enemies for <style=cIsDamage>1000% damage per second</style>. <style=cIsUtility>Restores ammo when used against bosses</style>.");
            "DG_SHOTGUN_NAME".Add("Super Shotgun");
            "DG_SHOTGUN_DESC".Add("Fire 20 pellets for <style=cIsDamage>80% damage</style> each. <style=cIsUtility>Consumes 2 Shells</style>.");
            "DG_CHAINGUN_NAME".Add("Chaingun");
            "DG_CHAINGUN_DESC".Add("Fire bullets for <style=cIsDamage>80% damage</style>. <style=cIsUtility>Consumes 1 Bullet</style>.");
            "DG_ROCKET_NAME".Add("Rocket Launcher");
            "DG_ROCKET_DESC".Add("Launch a rocket for <style=cIsDamage>900% damage</style>. <style=cIsUtility>Consumes 1 Rocket</style>.");
            "DG_PLASMA_NAME".Add("Plasma Rifle");
            "DG_PLASMA_DESC".Add("Rapidly fire plasma for <style=cIsDamage>120% damage</style>. Plasma can <style=cIsDamage>stun</style> targets. <style=cIsUtility>Consumes 1 Cell");
            "DG_BFG_NAME".Add("BFG-9000");
            "DG_BFG_DESC".Add("Launch an orb of destruction for <style=cIsDamage>2000% damage</style> that <style=cIsDamage>arcs 400% damage</style> to nearby targets. <style=cIsUtility>Consumes 40 Cells</style>.");
        }
    }
}