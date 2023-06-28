using System;
using BepInEx;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Fisobs.Core;
using System.Security;
using TheFriend.WorldChanges;
using TheFriend.Objects.BoulderObject;
using TheFriend.Objects.LittleCrackerObject;
using TheFriend.Objects.BoomMineObject;
using TheFriend.PoacherThings;
using BepInEx.Logging;
using ind = Player.AnimationIndex;
using bod = Player.BodyModeIndex;
using TheFriend.Creatures.PebblesLLCreature;
using TheFriend.Creatures.LizardThings;
using TheFriend.Creatures.SnowSpiderCreature;
using TheFriend.FriendThings;
using TheFriend.SlugcatThings;
using TheFriend.HudThings;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace TheFriend
{
    [BepInPlugin(MOD_ID, "The Friend", "0.2.0.3")]
    class Plugin : BaseUnityPlugin
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
        public void OnEnable()
        {
            LogSource = Logger;
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // Hooks
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
            MachineConnector.SetRegisteredOI("thefriend", new Options());
            Futile.atlasManager.LoadAtlas("atlases/friendsprites");
            Futile.atlasManager.LoadAtlas("atlases/friendlegs");
            Futile.atlasManager.LoadAtlas("atlases/dragonskull2");
            Futile.atlasManager.LoadAtlas("atlases/dragonskull3");
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
                return (IsFriendChar.TryGet(player, out var isFriend) && isFriend && player.bodyChunks[1].ContactPoint.y != -1 && !(player.bodyMode == Player.BodyModeIndex.Crawl || player.standing));
            });
            cursor.Emit(OpCodes.Or);
        }
        #endregion

        public static readonly SlugcatStats.Name FriendName = new SlugcatStats.Name("Friend", false); // Makes Friend's campaign more accessible to me
        public static readonly SlugcatStats.Name DragonName = new SlugcatStats.Name("FriendDragonslayer", false); // Makes Poacher's campaign more accessible to me

        #region options
        // Friend Settings
        public static bool FriendAutoCrouch()
        {
            return Options.FriendAutoCrouch.Value;
        }
        public static bool PoleCrawl()
        {
            return Options.PoleCrawl.Value;
        }
        public static bool FriendUnNerf()
        {
            return Options.FriendUnNerf.Value;
        }
        public static bool FriendBackspear()
        {
            return Options.FriendBackspear.Value;
        }
        public static bool FriendRepLock()
        {
            return Options.FriendRepLock.Value;
        }
        // Poacher Settings
        public static bool PoacherBackspear()
        {
            return Options.PoacherBackspear.Value;
        }
        public static bool PoacherPupActs()
        {
            return Options.PoacherPupActs.Value;
        }
        public static bool PoacherFreezeFaster()
        {
            return Options.PoacherFreezeFaster.Value;
        }
        public static bool PoacherFoodParkour()
        {
            return Options.PoacherFoodParkour.Value;
        }
        // Famine Settings
        public static bool NoFamine()
        {
            return Options.NoFamine.Value;
        }
        public static bool ExpeditionFamine()
        {
            return Options.ExpeditionFamine.Value;
        }
        public static bool FaminesForAll()
        {
            return Options.FaminesForAll.Value;
        }
        // Lizard Reputation Settings
        public static bool LizRep()
        {
            return Options.LizRepMeter.Value;
        }
        public static bool LizRepAll()
        {
            return Options.LizRepMeterForAll.Value;
        }
        public static bool LizRide()
        {
            return Options.LizRide.Value;
        }
        public static bool LizRideAll()
        {
            return Options.LizRideAll.Value;
        }
        public static bool LocalLizRep()
        {
            return Options.LocalizedLizRep.Value;
        }
        public static bool LocalLizRepAll()
        {
            return Options.LocalizedLizRepForAll.Value;
        }
        // Misc Hud Settings
        public static bool ShowCycleTimer()
        {
            return Options.SolaceBlizzTimer.Value;
        }
        #endregion
    }
}