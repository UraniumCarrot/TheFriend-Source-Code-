using System;
using System.Linq;
using BepInEx;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;
using Fisobs.Core;
using System.Security;
using TheFriend.WorldChanges;
using TheFriend.Objects.BoulderObject;
using TheFriend.Objects.LittleCrackerObject;
using TheFriend.Objects.BoomMineObject;
using BepInEx.Logging;
using TheFriend.CharacterThings;
using TheFriend.CharacterThings.DelugeThings;
using ind = Player.AnimationIndex;
using bod = Player.BodyModeIndex;
using TheFriend.Creatures.PebblesLLCreature;
using TheFriend.Creatures.LizardThings;
using TheFriend.Creatures.SnowSpiderCreature;
using TheFriend.SlugcatThings;
using TheFriend.HudThings;
using TheFriend.Expedition;
using TheFriend.RemixMenus;
using TheFriend.Creatures.FamineCreatures;
using TheFriend.CharacterThings.NoirThings;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using TheFriend.Creatures.LizardThings.MotherLizard;
using TheFriend.Creatures.LizardThings.PilgrimLizard;
using TheFriend.Creatures.LizardThings.YoungLizard;
using TheFriend.Objects.DelugePearlObject;
using TheFriend.Objects.FakePlayerEdible;
using TheFriend.Objects.SolaceScarfObject;
using TheFriend.SaveThings;
using UnityEngine;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace TheFriend
{
    [BepInPlugin(MOD_ID, "The Friend", MOD_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string MOD_ID = "thefriend";
        public const string MOD_VERSION = "0.3.1.0";

        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("friend/super_jump");
        public static readonly PlayerFeature<float> SuperCrawl = PlayerFloat("friend/super_crawl");
        public static readonly PlayerFeature<bool> FriendHead = PlayerBool("friend/friendhead");
        public static readonly PlayerFeature<bool> IsFriendChar = PlayerBool("friend/is_the_friend");
        public static readonly PlayerFeature<bool> CustomTail = PlayerBool("friend/fancytail");
        public static readonly PlayerFeature<bool> MaulEnabled = PlayerBool("friend/maul");

        #region hooks
        public static ManualLogSource LogSource { get; private set; }
        public static bool RotundWorld;
        public void OnEnable()
        {
            LogSource = Logger;
            On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
            On.RainWorld.PostModsInit += RainWorldOnPostModsInit;

            On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
            {
                orig(self, newlyDisabledMods);
                for (var i = 0; i < newlyDisabledMods.Length; i++)
                {
                    if (newlyDisabledMods[i].id == "The Friend")
                    {
                        //if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.PebblesLL))
                        //    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.PebblesLL);
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.MotherLizard))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.MotherLizard);
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.YoungLizard))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.YoungLizard);
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.SnowSpider))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.SnowSpider);
                        if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.UnlockLittleCracker))
                            MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.UnlockLittleCracker);
                        //if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.UnlockBoulder))
                        //    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.UnlockBoulder);
                        CreatureTemplateType.UnregisterValues();
                        SandboxUnlockID.UnregisterValues();
                        break;
                    }
                }
            };
        }

        private bool _modsInit;
        private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (_modsInit) return;

            try
            {
                _modsInit = true;
                LoadResources();

                // Hooks
                CharacterHooks.Apply();
                DelugePearlHooks.Apply();

                AbstractObjectType.Apply();
                UpdateDeleteCWT.Apply();
                SolaceSaveData.Apply();
                SolaceCustom.Apply();

                LizardRideControl.Apply();
                YoungLizardAI.Apply();
                Hooks.Apply();
                SnowSpiderGraphics.Apply();
                PebblesLL.Apply();

                HudHooks.Apply();
                MotherKillTracker.Apply();
                MainMenu.Apply();
                ExpeditionHooks.Apply();

                FriendWorldState.Apply();
                DelugeWorldState.Apply();
                SLOracleHandler.Apply();
                FamineWorld.Apply();
                FamineCreatures.Apply();
                DangerTypes.Apply();

                // Fisobs
                Content.Register(new PebblesLLCritob());
                Content.Register(new MotherLizardCritob());
                Content.Register(new PilgrimLizardCritob());
                Content.Register(new YoungLizardCritob());
                Content.Register(new SnowSpiderCritob());
                Content.Register(new BoulderFisob());
                Content.Register(new LittleCrackerFisob());
                Content.Register(new BoomMineFisob());
                Content.Register(new SolaceScarfFisob());
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Logger.LogError(ex);
            }
        }

        private bool _postModsInit;
        private void RainWorldOnPostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            if (_postModsInit) return;

            try
            {
                _postModsInit = true;
                SlugcatNameFix.Apply();

                if (ModManager.ActiveMods.Any(x => x.id == "willowwisp.bellyplus"))
                {
                    RotundWorld = true;
                    Logger.LogInfo("Rotund World detected! Cats gonna be chonky...");
                }
                else
                {
                    RotundWorld = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private void LoadResources()
        {
            MachineConnector.SetRegisteredOI("thefriend", new RemixMain());
            Futile.atlasManager.LoadAtlas("atlases/friendsprites");
            Futile.atlasManager.LoadAtlas("atlases/friendlegs");
            Futile.atlasManager.LoadAtlas("atlases/dragonskull2");
            Futile.atlasManager.LoadAtlas("atlases/dragonskull3");
            Futile.atlasManager.LoadAtlas("atlases/solacesymbols");
            Futile.atlasManager.LoadAtlas("atlases/ForeheadSpots");
            Futile.atlasManager.LoadAtlas("atlases/CentipedeLegB_Fade");
            DelugeSounds.LoadSounds();
            NoirCatto.LoadSounds();
            NoirCatto.LoadAtlases();
        }
        #endregion

        public static readonly SlugcatStats.Name FriendName = new SlugcatStats.Name("Friend", false); // Makes Friend's campaign more accessible to me
        public static readonly SlugcatStats.Name DragonName = new SlugcatStats.Name("FriendDragonslayer", false); // Makes Poacher's campaign more accessible to me
        public static readonly SlugcatStats.Name NoirName = new SlugcatStats.Name("NoirCatto", false);
        public static readonly SlugcatStats.Name DelugeName = new SlugcatStats.Name("FriendDeluge", false);
        public static readonly SlugcatStats.Name BelieverName = new SlugcatStats.Name("FriendBeliever", false);
        
        public const string MothersKilled = "MothersKilledInRegionStr";
        public const string MotherKillNum = "MotherKillCount";
        
    }
}