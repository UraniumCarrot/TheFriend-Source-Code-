using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using IL.MoreSlugcats;
using RWCustom;
using MoreSlugcats;
using System.Linq;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using HUD;
using JollyCoop;
using System.Globalization;
using CoralBrain;
using System.Collections.Generic;
using System.IO;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Fisobs.Core;
using On;
using System.Security;
using System.Runtime.InteropServices.WindowsRuntime;
using MonoMod;
using IL;
using TheFriend.WorldChanges;
using TheFriend.Creatures;
using TheFriend.Objects.BoulderObject;
using TheFriend.Objects.LittleCrackerObject;
using TheFriend.Objects.BoomMineObject;
using TheFriend.PoacherThings;
using BepInEx.Logging;
using System.Net;
using SlugBase;
using SlugBase.DataTypes;
using ind = Player.AnimationIndex;
using bod = Player.BodyModeIndex;
using JollyColorMode = Options.JollyColorMode;
using Vector2 = UnityEngine.Vector2;
using System.Numerics;
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

        public static PlayerFeature<float> SuperJump = PlayerFloat("friend/super_jump");
        public static PlayerFeature<float> SuperCrawl = PlayerFloat("friend/super_crawl");
        public static PlayerFeature<bool> SuperSlide = PlayerBool("friend/super_slide");
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
                    if (player is Player)
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
                return (IsFriendChar.TryGet(player, out var isFriend) && isFriend && !(player.bodyChunks[1].ContactPoint.y == -1) && !(player.bodyMode == Player.BodyModeIndex.Crawl || player.standing));
            });
            cursor.Emit(OpCodes.Or);
        }
        #endregion

        public static readonly SlugcatStats.Name FriendName = new SlugcatStats.Name("Friend", false); // Makes Friend's campaign more accessible to me
        public static readonly SlugcatStats.Name DragonName = new SlugcatStats.Name("FriendDragonslayer", false); // Makes Poacher's campaign more accessible to me

        #region options
        // Options code
        public static bool FriendAutoCrouch()
        {
            if (Options.FriendAutoCrouch.Value == true) return true;
            else return false;
        }
        public static bool PoleCrawl()
        {
            if (Options.PoleCrawl.Value == true) return true;
            else return false;
        }
        public static bool FriendUnNerf()
        {
            if (Options.FriendUnNerf.Value == true) return true;
            else return false;
        }
        public static bool FriendBackspear()
        {
            if (Options.FriendBackspear.Value == true) return true;
            else return false;
        }
        public static bool FriendRepLock()
        {
            if (Options.FriendRepLock.Value == true) return true;
            else return false;
        }

        public static bool PoacherBackspear()
        {
            if (Options.PoacherBackspear.Value == true) return true;
            else return false;
        }
        public static bool PoacherPupActs()
        {
            if (Options.PoacherPupActs.Value == true) return true;
            else return false;
        }
        public static bool PoacherFreezeFaster()
        {
            if (Options.PoacherFreezeFaster.Value == true) return true;
            else return false;
        }
        public static bool PoacherFoodParkour()
        {
            if (Options.PoacherFoodParkour.Value == true) return true;
            else return false;
        }

        public static bool NoFamine()
        {
            if (Options.NoFamine.Value == true) return true;
            else return false;
        }
        public static bool ExpeditionFamine()
        {
            if (Options.ExpeditionFamine.Value == true) return true;
            else return false;
        }
        public static bool FaminesForAll()
        {
            if (Options.FaminesForAll.Value == true) return true;
            else return false;
        }

        public static bool LizRep()
        {
            if (Options.LizRepMeter.Value == true) return true;
            else return false;
        }
        public static bool LizRepAll()
        {
            if (Options.LizRepMeterForAll.Value == true) return true;
            else return false;
        }
        public static bool LizRide()
        {
            if (Options.LizRide.Value == true) return true;
            else return false;
        }
        public static bool LizRideAll()
        {
            if (Options.LizRideAll.Value == true) return true;
            else return false;
        }
        public static bool LocalLizRep()
        {
            if (Options.LocalizedLizRep.Value == true) return true;
            else return false;
        }
        public static bool LocalLizRepAll()
        {
            if (Options.LocalizedLizRepForAll.Value == true) return true;
            else return false;
        }
        public static bool ShowCycleTimer()
        {
            if (Options.SolaceBlizzTimer.Value == true) return true;
            else return false;
        }
        #endregion
    }
}