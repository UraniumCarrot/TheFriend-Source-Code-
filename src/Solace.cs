using System;
using System.Linq;
using BepInEx;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Fisobs.Core;
using System.Security;
using BepInEx.Logging;
using Solace.Creatures.LizardThings;
using Solace.Creatures.PebblesLLCreature;
using Solace.Creatures.SnowSpiderCreature;
using Solace.FriendThings;
using Solace.HudThings;
using Solace.NoirThings;
using Solace.Objects.BoomMineObject;
using Solace.Objects.BoulderObject;
using Solace.Objects.FakePlayerEdible;
using Solace.Objects.LittleCrackerObject;
using Solace.PoacherThings;
using Solace.SaveThings;
using Solace.SlugcatThings;
using Solace.WorldChanges;
using ind = Player.AnimationIndex;
using bod = Player.BodyModeIndex;
using Solace.Objects;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Solace
{
    [BepInPlugin(MOD_ID, "The Friend", "0.3.0")]
    class Solace : BaseUnityPlugin
    {
        public const string MOD_ID = "thefriend";

        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("friend/super_jump");
        public static readonly PlayerFeature<float> SuperCrawl = PlayerFloat("friend/super_crawl");
        public static readonly PlayerFeature<bool> SuperSlide = PlayerBool("friend/super_slide");
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
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.RainWorld.PostModsInit += RainWorldOnPostModsInit;

            // Hooks
            AbstractObjectType.Apply();
            UpdateDeleteCWT.Apply();
            MotherKillTracker.Apply();
            SolaceSaveData.Apply();
            YoungLizardAI.Apply();
            FirecrackerFix.Apply();
            FriendWorldState.Apply();
            FriendCrawl.Apply();
            FriendCrawlTurn.Apply();
            DragonCrafts.Apply();
            SLOracleHandler.Apply();
            FamineWorld.Apply();
            SnowSpiderGraphics.Apply();
            PebblesLL.Apply();
            DragonClassFeatures.Apply();
            DragonRepInterface.Apply();
            SlugcatGameplay.Apply();
            SlugcatGraphics.Apply();
            HudHooks.Apply();
            NoirCatto.Apply();

            // Misc IL
            IL.Player.ThrowObject += Player_ThrowObject;
            IL.Player.UpdateAnimation += ilcontext =>
            {
                var cursor = new ILCursor(ilcontext);
                if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(18.1f)))
                {
                    return;
                }

                cursor.MoveAfterLabels();
                cursor.RemoveRange(2);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate((Player player) =>
                {
                    if (player != null)
                    {
                        return 14f;
                    }
                    else
                    {
                        return 18.1f;
                    }
                });
                cursor.Emit(OpCodes.Stloc, 24);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate((Player player) =>
                {
                    if (SuperSlide.TryGet(player, out bool slideup) && slideup)
                    {
                        return 22f;
                    }
                    else
                    {
                        return 28f;
                    }
                });
                cursor.Emit(OpCodes.Stloc, 25);
            };

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
            Content.Register(new PebblesLLCritob());
            Content.Register(new MotherLizardCritob());
            Content.Register(new YoungLizardCritob());
            Content.Register(new SnowSpiderCritob());
            Content.Register(new BoulderFisob());
            Content.Register(new LittleCrackerFisob());
            Content.Register(new BoomMineFisob());
        }
        public void LoadResources(RainWorld rainWorld)
        {
            MachineConnector.SetRegisteredOI("thefriend", new SolaceOptions());
            Futile.atlasManager.LoadAtlas("atlases/friendsprites");
            Futile.atlasManager.LoadAtlas("atlases/friendlegs");
            Futile.atlasManager.LoadAtlas("atlases/dragonskull2");
            Futile.atlasManager.LoadAtlas("atlases/dragonskull3");
            NoirCatto.LoadSounds();
            NoirCatto.LoadAtlases();
        }
        private void RainWorldOnPostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
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
        public static void Player_ThrowObject(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdsfld<Player.AnimationIndex>("Flip")))
            {
                return;
            }

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(1)))
            {
                return;
            }

            cursor.MoveAfterLabels();

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<Player, bool>>(player =>
            {
                return (IsFriendChar.TryGet(player, out var isFriend) && 
                        isFriend && 
                        player.bodyChunks[1].ContactPoint.y != -1 && 
                        !(player.bodyMode == Player.BodyModeIndex.Crawl || 
                          player.standing)) 
                       || 
                       (player.grasps[0]?.grabbed is LittleCracker ||
                        (player.grasps[0]?.grabbed == null && player.grasps[1]?.grabbed is LittleCracker));
            });
            cursor.Emit(OpCodes.Or);
        }
        #endregion

        public static readonly SlugcatStats.Name FriendName = new SlugcatStats.Name("Friend", false); // Makes Friend's campaign more accessible to me
        public static readonly SlugcatStats.Name DragonName = new SlugcatStats.Name("FriendDragonslayer", false); // Makes Poacher's campaign more accessible to me
        public static readonly SlugcatStats.Name NoirName = new SlugcatStats.Name("NoirCatto", false);

        #region options
        // Friend Settings
        public static bool FriendAutoCrouch()
        {
            return SolaceOptions.FriendAutoCrouch.Value;
        }
        public static bool PoleCrawl()
        {
            return SolaceOptions.PoleCrawl.Value;
        }
        public static bool FriendUnNerf()
        {
            return SolaceOptions.FriendUnNerf.Value;
        }
        public static bool FriendBackspear()
        {
            return SolaceOptions.FriendBackspear.Value;
        }
        public static bool FriendRepLock()
        {
            return SolaceOptions.FriendRepLock.Value;
        }
        // Poacher Settings
        public static bool PoacherBackspear()
        {
            return SolaceOptions.PoacherBackspear.Value;
        }
        public static bool PoacherPupActs()
        {
            return SolaceOptions.PoacherPupActs.Value;
        }
        public static bool PoacherFreezeFaster()
        {
            return SolaceOptions.PoacherFreezeFaster.Value;
        }
        public static bool PoacherFoodParkour()
        {
            return SolaceOptions.PoacherFoodParkour.Value;
        }
        // Famine Settings
        public static bool NoFamine()
        {
            return SolaceOptions.NoFamine.Value;
        }
        public static bool ExpeditionFamine()
        {
            return SolaceOptions.ExpeditionFamine.Value;
        }
        public static bool FaminesForAll()
        {
            return SolaceOptions.FaminesForAll.Value;
        }
        // Lizard Reputation Settings
        public static bool LizRep()
        {
            return SolaceOptions.LizRepMeter.Value;
        }
        public static bool LizRepAll()
        {
            return SolaceOptions.LizRepMeterForAll.Value;
        }
        public static bool LizRide()
        {
            return SolaceOptions.LizRide.Value;
        }
        public static bool LizRideAll()
        {
            return SolaceOptions.LizRideAll.Value;
        }
        public static bool LocalLizRep()
        {
            return SolaceOptions.LocalizedLizRep.Value;
        }
        public static bool LocalLizRepAll()
        {
            return SolaceOptions.LocalizedLizRepForAll.Value;
        }
        // Misc Hud Settings
        public static bool ShowCycleTimer()
        {
            return SolaceOptions.SolaceBlizzTimer.Value;
        }
        #endregion
    }
}