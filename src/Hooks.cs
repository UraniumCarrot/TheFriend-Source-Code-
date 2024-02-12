using System;
using System.Diagnostics;
using System.Reflection;
using Fisobs.Core;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TheFriend.SlugcatThings;
using TheFriend.CharacterThings.NoirThings;
using TheFriend.CharacterThings.NoirThings.HuntThings;
using TheFriend.Creatures.FamineCreatures;
using TheFriend.PoacherThings;
using TheFriend.FriendThings;
using TheFriend.Creatures.LizardThings;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics;
using TheFriend.Creatures.LizardThings.MotherLizard;
using TheFriend.Creatures.LizardThings.PilgrimLizard;
using TheFriend.Creatures.LizardThings.YoungLizard;
using TheFriend.Creatures.PebblesLLCreature;
using TheFriend.Creatures.SnowSpiderCreature;
using TheFriend.Expedition;
using TheFriend.HudThings;
using TheFriend.Objects.BoomMineObject;
using TheFriend.Objects.BoulderObject;
using TheFriend.Objects.DelugePearlObject;
using TheFriend.Objects.LittleCrackerObject;
using TheFriend.Objects.SolaceScarfObject;
using TheFriend.WorldChanges;
using ColdRoom = On.MoreSlugcats.ColdRoom;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace TheFriend
{
    public class Hooks
    {
        private static bool _modsInit;
        public static void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (_modsInit) return;
            
            DragonCrafts.InitRecipes();
            
            try
            {
                _modsInit = true;
                Plugin.LoadResources();
                
                #region ON Hooks (~15ms load time each)
                #region Creature Hooks
                On.AbstractCreature.setCustomFlags += FriendWorldState.AbstractCreature_setCustomFlags;
                On.ScavengerAbstractAI.InitGearUp += FriendWorldState.ScavengerAbstractAI_InitGearUp;
                On.AbstractCreature.ctor += PebblesLL.AbstractCreature_ctor;
                On.AbstractCreature.DrainWorldDenFlooded += DangerTypes.AbstractCreatureOnDrainWorldDenFlooded;
                On.BigSpiderGraphics.ScaleAttachPos += SnowSpiderGraphics.BigSpiderGraphics_ScaleAttachPos;
                On.BigSpider.Violence += SnowSpiderGraphics.BigSpider_Violence;
                On.Centipede.ShortCutColor += FamineCentipede.Centipede_ShortCutColor;
                On.Centipede.Violence += FamineCentipede.Centipede_Violence;
                On.CentipedeGraphics.ctor += FamineCentipede.CentipedeGraphics_ctor;
                On.CentipedeGraphics.DrawSprites += FamineCentipede.CentipedeGraphics_DrawSprites;
                On.CentipedeGraphics.InitiateSprites += FamineCentipede.CentipedeGraphics_InitiateSprites;
                On.Creature.Die += On_Creature_Die;
                On.Creature.Grab += LizardRideFixes.Creature_Grab;
                On.Creature.LoseAllGrasps += PoacherSkullFeatures.Creature_LoseAllGrasps;
                On.Creature.NewRoom += LizardRideFixes.Creature_NewRoom;
                On.Creature.SpitOutOfShortCut += LizardRideFixes.CreatureOnSpitOutOfShortCut;
                On.Creature.SuckedIntoShortCut += YoungLizardAI.CreatureOnSuckedIntoShortCut;
                On.Creature.Violence += PoacherSkullFeatures.CreatureOnViolence;
                On.CreatureCommunities.LoadDefaultCommunityAlignments += FriendWorldState.CreatureCommunitiesOnLoadDefaultCommunityAlignments;
                On.CreatureCommunities.InfluenceCell += FriendWorldState.CreatureCommunitiesOnInfluenceCell;
                On.DaddyGraphics.DaddyTubeGraphic.ApplyPalette += PebblesLLGraphics.DaddyTubeGraphic_ApplyPalette;
                On.DaddyGraphics.DaddyDeadLeg.ApplyPalette += PebblesLLGraphics.DaddyDeadLeg_ApplyPalette;
                On.DaddyGraphics.DaddyDeadLeg.DrawSprite += PebblesLLGraphics.DaddyDeadLeg_DrawSprite;
                On.DaddyGraphics.DaddyDangleTube.ApplyPalette += PebblesLLGraphics.DaddyDangleTube_ApplyPalette;
                On.EggBug.ctor += FamineEggBug.EggBugOnctor;
                On.EggBugGraphics.ApplyPalette += FamineEggBug.EggBugGraphicsOnApplyPalette;
                On.EggBugGraphics.ctor += FamineEggBug.EggBugGraphicsOnctor;
                On.EggBugGraphics.DrawSprites += FamineEggBug.EggBugGraphicsOnDrawSprites;
                On.EggBugGraphics.EggColors += FamineEggBug.EggBugGraphicsOnEggColors;
                On.EggBugGraphics.InitiateSprites += FamineEggBug.EggBugGraphicsOnInitiateSprites;
                On.EggBugEgg.ctor += FamineEggBug.EggBugEggOnctor;
                On.FireFly.ctor += FriendWorldState.FireFly_ctor;
                On.Fly.ctor += FamineBatFly.Fly_ctor;
                On.FlyAI.RoomNotACycleHazard += DelugeWorldState.FlyAIOnRoomNotACycleHazard;
                On.LanternMouse.Update += PoacherGameplay.LanternMouse_Update;
                #region Lizards
                On.Lizard.Bite += LizardHooks.Lizard_Bite;
                On.Lizard.ctor += LizardHooks.Lizard_ctor;
                On.Lizard.Update += On_Lizard_Update;
                On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += LizardHooks.LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship;
                On.LizardAI.SocialEvent += LizardRideFixes.LizardAI_SocialEvent;
                On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += 
                    LizardHooks.LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
                On.LizardCosmetics.BodyScales.GeneratePatchPattern += LizardCosmeticHooks.BodyScales_GeneratePatchPattern;
                On.LizardCosmetics.BodyScales.GenerateSegments += LizardCosmeticHooks.BodyScales_GenerateSegments;
                On.LizardCosmetics.LongBodyScales.DrawSprites += LizardCosmeticHooks.LongBodyScales_DrawSprites;
                On.LizardGraphics.ctor += LizardHooks.LizardGraphics_ctor;
                On.LizardGraphics.ApplyPalette += LizardHooks.LizardGraphics_ApplyPalette;
                On.LizardGraphics.BodyColor += LizardHooks.LizardGraphics_BodyColor;
                On.LizardGraphics.DynamicBodyColor += LizardHooks.LizardGraphics_DynamicBodyColor;
                On.LizardGraphics.InitiateSprites += LizardHooks.LizardGraphics_InitiateSprites;
                On.LizardGraphics.AddToContainer += LizardHooks.LizardGraphics_AddToContainer;
                On.LizardGraphics.DrawSprites += LizardHooks.LizardGraphics_DrawSprites;
                On.LizardLimb.ctor += LizardHooks.LizardLimb_ctor;
                On.LizardVoice.GetMyVoiceTrigger += LizardHooks.LizardVoice_GetMyVoiceTrigger;
                new Hook(typeof(LizardGraphics)
                    .GetProperty(nameof(LizardGraphics.HeadColor1), BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetGetMethod(true), FancyHeadColors.FancyHeadColor1);
                #endregion
                On.PoleMimic.ctor += FaminePolePlant.PoleMimicOnctor;
                On.PoleMimicGraphics.DrawSprites += FaminePolePlant.PoleMimicGraphicsOnDrawSprites;
                On.WormGrass.WormGrassPatch.Update += WormGrassImmunizer.WormGrassPatch_Update;
                #endregion
                #region Expedition
                On.Expedition.ExpeditionProgression.BurdenMenuColor += ExpeditionBurdens.ExpeditionProgressionOnBurdenMenuColor;
                On.Expedition.ExpeditionProgression.SetupBurdenGroups += ExpeditionBurdens.ExpeditionProgressionOnSetupBurdenGroups;
                On.Expedition.ExpeditionProgression.BurdenName += ExpeditionBurdens.ExpeditionProgressionOnBurdenName;
                On.Expedition.ExpeditionProgression.BurdenManualDescription += ExpeditionBurdens.ExpeditionProgressionOnBurdenManualDescription;
                On.Expedition.ExpeditionProgression.BurdenScoreMultiplier += ExpeditionBurdens.ExpeditionProgressionOnBurdenScoreMultiplier;
                On.Expedition.ExpeditionProgression.CountUnlockables += ExpeditionBurdens.ExpeditionProgressionOnCountUnlockables;
                #endregion
                #region HUD Hooks
                On.HUD.RainMeter.Draw += HudHooks.RainMeter_Draw;
                On.HUD.RainMeter.ctor += HudHooks.RainMeter_ctor;
                On.HUD.RainMeter.Update += HudHooks.RainMeter_Update;
                On.HUD.HUD.InitSinglePlayerHud += On_HUD_HUD_InitSinglePlayerHud;
                On.HUD.TextPrompt.UpdateGameOverString += PoacherSkullFeatures.TextPrompt_UpdateGameOverString;
                #endregion
                #region Iterator Hooks
                On.SLOrcacleState.ForceResetState += SLOracleHandler.SLOrcacleState_ForceResetState;
                On.SLOracleBehavior.Update += SLOracleHandler.SLOracleBehavior_Update;
                On.SLOracleBehavior.Move += SLOracleHandler.SLOracleBehavior_Move;
                On.SLOracleBehaviorNoMark.Update += SLOracleHandler.SLOracleBehaviorNoMark_Update;
                On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += SLOracleHandler.MoonConversationOnAddEvents;
                #endregion
                #region Menu Hooks
                On.Menu.IntroRoll.ctor += MainMenu.IntroRollOnctor;
                On.Menu.KarmaLadder.ctor += HuntQuestThings.KarmaLadderOnctor;
                On.Menu.KarmaLadderScreen.Singal += HuntQuestThings.KarmaLadderScreenOnSingal;
                On.Menu.Menu.CommunicateWithUpcomingProcess += NoirCatto.MenuOnCommunicateWithUpcomingProcess;
                On.Menu.Menu.Update += On_Menu_Menu_Update;
                On.Menu.RainEffect.ctor += MainMenu.RainEffectOnctor;
                On.Menu.RainEffect.GrafUpdate += MainMenu.RainEffectOnGrafUpdate; 
                On.Menu.SleepAndDeathScreen.GetDataFromGame += HudHooks.SleepAndDeathScreen_GetDataFromGame;
                On.Menu.UnlockDialog.TogglePerk += ExpeditionGeneral.UnlockDialogOnTogglePerk;
                On.Menu.UnlockDialog.UpdateBurdens += ExpeditionBurdens.UnlockDialogOnUpdateBurdens;
                On.Menu.UnlockDialog.SetUpBurdenDescriptions += ExpeditionBurdens.UnlockDialogOnSetUpBurdenDescriptions;
                On.Menu.UnlockDialog.Update += ExpeditionBurdens.UnlockDialogOnUpdate;
                On.Menu.UnlockDialog.ctor += ExpeditionBurdens.UnlockDialogOnctor;
                On.MenuMicrophone.PlaySound_SoundID_float_float_float += MainMenu.MenuMicrophoneOnPlaySound_SoundID_float_float_float;
                #endregion
                #region Object Hooks
                On.AbstractPhysicalObject.Abstractize += NoirCatto.AbstractPhysicalObjectOnAbstractize;
                On.AbstractPhysicalObject.Realize += AbstractObjectType.AbstractPhysicalObjectOnRealize;
                On.DangleFruit.ApplyPalette += FamineWorld.DangleFruit_ApplyPalette;
                On.DangleFruit.DrawSprites += FamineWorld.DangleFruit_DrawSprites;
                On.DangleFruit.Update += PoacherGameplay.DangleFruit_Update;
                On.DataPearl.DrawSprites += DelugePearlGraphics.DataPearlOnDrawSprites;
                On.DataPearl.ctor += DelugePearlMechanics.DataPearlOnctor;
                On.DataPearl.Update += DelugePearlMechanics.DataPearlOnUpdate;
                On.FirecrackerPlant.Update += FirecrackerFix.FirecrackerPlant_Update;
                On.MoreSlugcats.DandelionPeach.ApplyPalette += FamineWorld.DandelionPeach_ApplyPalette;
                On.MoreSlugcats.DandelionPeach.ctor += FamineWorld.DandelionPeach_ctor;
                On.MoreSlugcats.DandelionPeach.Update += PoacherGameplay.DandelionPeach_Update;
                On.MoreSlugcats.GooieDuck.ApplyPalette += FamineWorld.GooieDuck_ApplyPalette;
                On.MoreSlugcats.LillyPuck.ApplyPalette += FamineWorld.LillyPuck_ApplyPalette;
                On.MoreSlugcats.LillyPuck.Update += FamineWorld.LillyPuck_Update;
                On.PhysicalObject.Grabbed += DelugePearlMechanics.PhysicalObjectOnGrabbed;
                On.SeedCob.PlaceInRoom += NoirCatto.SeedCobOnPlaceInRoom;
                On.Spear.Update += NoirCatto.SpearOnUpdate;
                On.Weapon.HitThisObject += LizardRideFixes.Weapon_HitThisObject;
                On.Weapon.NewRoom += DragonCrafts.Weapon_NewRoom;
                #endregion
                #region "Player" Class Hooks
                On.Player.AllowGrabbingBatflys += NoirCatto.PlayerOnAllowGrabbingBatflys;
                On.Player.checkInput += On_Player_checkInput;
                On.Player.ctor += SlugcatGameplay.Player_ctor;
                On.Player.Grabability += On_Player_Grabability;
                On.Player.Grabbed += PoacherSkullFeatures.Player_Grabbed;
                On.Player.GraphicsModuleUpdated += On_Player_GraphicsModuleUpdated;
                On.Player.GraspsCanBeCrafted += DragonCrafts.Player_GraspsCanBeCrafted;
                On.Player.GrabUpdate += On_Player_GrabUpdate;
                On.Player.HeavyCarry += PoacherGameplay.Player_HeavyCarry;
                On.Player.IsCreatureLegalToHoldWithoutStun += LizardRideFixes.Player_IsCreatureLegalToHoldWithoutStun;
                On.Player.Jump += SlugcatGameplay.Player_Jump;
                On.Player.Jump += NoirCatto.PlayerOnJump;
                On.Player.MovementUpdate += FriendCrawlTurn.PlayerOnMovementUpdate;
                On.Player.MovementUpdate += NoirCatto.PlayerOnMovementUpdate;
                On.Player.NewRoom += DelugePearlMechanics.PlayerOnNewRoom;
                On.Player.ObjectEaten += PoacherGameplay.PlayerOnObjectEaten;
                On.Player.PickupCandidate += NoirCatto.PlayerOnPickupCandidate;
                On.Player.ReleaseGrasp += LizardRideFixes.PlayerOnReleaseGrasp;
                On.Player.SpearStick += PoacherSkullFeatures.Player_SpearStick;
                On.Player.SpitUpCraftedObject += DragonCrafts.Player_SpitUpCraftedObject;
                On.Player.ThrowObject += SlugcatGameplay.PlayerOnThrowObject;
                On.Player.ThrowObject += NoirCatto.PlayerOnThrowObject;
                On.Player.ThrownSpear += On_Player_ThrownSpear;
                On.Player.UpdateAnimation += On_Player_UpdateAnimation;
                On.Player.Update += On_Player_Update;
                On.Player.UpdateBodyMode += On_Player_UpdateBodyMode;
                On.Player.WallJump += SlugcatGameplay.Player_WallJump;
                #endregion
                #region Player Graphics Hooks
                On.GraphicsModule.HypothermiaColorBlend += SlugcatGraphics.GraphicsModule_HypothermiaColorBlend;
                On.PlayerGraphics.AddToContainer += NoirCatto.PlayerGraphicsOnAddToContainer;
                On.PlayerGraphics.AddToContainer += SlugcatGraphics.PlayerGraphics_AddToContainer;
                On.PlayerGraphics.ApplyPalette += On_PlayerGraphics_ApplyPalette;
                On.PlayerGraphics.ctor += On_PlayerGraphics_Ctor;
                On.PlayerGraphics.DrawSprites += On_PlayerGaphics_DrawSprites;
                On.PlayerGraphics.InitiateSprites += NoirCatto.PlayerGraphicsOnInitiateSprites;
                On.PlayerGraphics.InitiateSprites += SlugcatGraphics.PlayerGraphics_InitiateSprites;
                On.PlayerGraphics.Reset += NoirCatto.PlayerGraphicsOnReset;
                On.PlayerGraphics.Update += On_PlayerGraphics_Update;
                #endregion
                #region RainWorld Global Hooks
                On.ProcessManager.RequestMainProcessSwitch_ProcessID += HuntQuestThings.ProcessManagerOnRequestMainProcessSwitch_ProcessID;
                On.RainWorld.Update += NoirCatto.RainWorldOnUpdate;
                On.RainWorldGame.ctor += NoirCatto.RainWorldGameOnctor;
                On.RainWorldGame.IsMoonActive += SLOracleHandler.RainWorldGame_IsMoonActive;
                On.RainWorldGame.IsMoonHeartActive += SLOracleHandler.RainWorldGame_IsMoonHeartActive;
                On.RainWorldGame.MoonHasRobe += SLOracleHandler.RainWorldGame_MoonHasRobe;
                On.RainWorldGame.Win += HuntQuestThings.RainWorldGameOnWin;
                On.StoryGameSession.ctor += On_StoryGameSession_Ctor;
                #endregion
                #region Save related Hooks
                On.SaveState.AbstractPhysicalObjectFromString += FirecrackerFix.SaveState_AbstractPhysicalObjectFromString;
                On.SaveState.SessionEnded += DelugePearlMechanics.SaveStateOnSessionEnded;
                On.SaveState.SetCustomData_AbstractPhysicalObject_string += FirecrackerFix.SaveState_SetCustomData_AbstractPhysicalObject_string;
                On.SaveState.setDenPosition += NoirCatto.SaveStateOnsetDenPosition;
                On.SaveUtils.PopulateUnrecognizedStringAttrs += FirecrackerFix.SaveUtils_PopulateUnrecognizedStringAttrs;
                On.PlayerProgression.ClearOutSaveStateFromMemory += SaveThings.SolaceCustom.PlayerProgressionOnClearOutSaveStateFromMemory;
                On.PlayerProgression.WipeSaveState += SaveThings.SolaceCustom.PlayerProgressionOnWipeSaveState;
                On.PlayerSessionRecord.AddEat += HuntQuestThings.PlayerSessionRecordOnAddEat;
                #endregion
                #region Slugcat Hooks
                On.SlugcatHand.EngageInMovement += On_SlugcatHand_Engage_In_Movement;
                On.SlugcatStats.ctor += On_SlugcatStats_Ctor;
                On.SlugcatStats.NourishmentOfObjectEaten += FamineWorld.SlugcatStats_NourishmentOfObjectEaten;
                On.SlugcatStats.SlugcatCanMaul += SlugcatGameplay.SlugcatStats_SlugcatCanMaul;
                #endregion
                #region World Hooks
                ColdRoom.ColdBreath.Update += DelugeWorldState.ColdBreathOnUpdate;
                On.Region.ctor += FriendWorldState.Region_ctor;
                On.RoofTopView.DustpuffSpawner.DustPuff.Update += DelugeWorldState.DustPuffOnUpdate;
                On.Room.AddObject += NoirCatto.RoomOnAddObject;
                On.Room.ctor += DelugeWorldState.RoomOnctor;
                // Shaded Citadel hooks
                On.Room.Loaded += FriendWorldState.Room_Loaded;
                On.Room.SlugcatGamemodeUniqueRoomSettings += FriendWorldState.Room_SlugcatGamemodeUniqueRoomSettings;
                On.Room.Update += DelugeWorldState.RoomOnUpdate;
                On.RoomRain.ctor += DangerTypes.RoomRainOnctor;
                On.RoomRain.Update += DangerTypes.RoomRainOnUpdate;
                new Hook(typeof(RoomRain)
                    .GetProperty(nameof(RoomRain.OutsidePushAround))!
                    .GetGetMethod(), DangerTypes.ChillyOutsidePushAround);
                On.RoomSpecificScript.AddRoomSpecificScript += WorldChanges.ScarfScripts.RoomScript.RoomSpecificScriptOnAddRoomSpecificScript;
                On.RoomSpecificScript.SU_A43SuperJumpOnly.Update += FriendWorldState.SU_A43SuperJumpOnlyOnUpdate;
                On.ShelterDoor.Update += DangerTypes.ShelterDoorOnUpdate;
                On.Water.ctor += DangerTypes.WaterOnctor;
                On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += FriendWorldState.WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues;
                #endregion
                #region Misc
                On.DeafLoopHolder.Update += CharacterThings.DelugeThings.DelugeGameplay.DeafLoopHolderOnUpdate;
                #endregion
                #endregion
                #region IL Hooks (100ms+ load time each)
                #region Creature Hooks
                IL.Creature.HypothermiaUpdate += DelugeWorldState.CreatureOnHypothermiaUpdate;
                IL.DaddyLongLegs.ctor += PebblesLL.DaddyLongLegs_ctor;
                IL.EggBugGraphics.ApplyPalette += FamineEggBug.EggBugGraphicsILApplyPalette;
                IL.PoleMimicGraphics.InitiateSprites += FaminePolePlant.PoleMimicGraphicsILInitiateSprites;
                #endregion
                #region Menu Hooks
                IL.Menu.UnlockDialog.ctor += ExpeditionBurdens.UnlockDialogOnctor;
                #endregion
                #region Object Hooks
                IL.FirecrackerPlant.ctor += FirecrackerFix.FirecrackerPlant_ctor;
                IL.SeedCob.Update += NoirCatto.SeedCobILUpdate;
                IL.Spear.Update += NoirCatto.SpearILUpdate;
                IL.Spear.Update += PebblesLL.Spear_Update;
                IL.Weapon.Update += NoirCatto.WeaponILUpdate;
                #endregion
                #region "Player" Class Hooks
                IL.Player.EatMeatUpdate += FamineCentipede.Player_EatMeatUpdate;
                IL.Player.MovementUpdate += SlugcatGameplay.PlayerOnMovementUpdate;
                IL.Player.ThrowObject += CharacterThings.FriendThings.FriendGameplay.Player_ThrowObject;
                IL.Player.UpdateAnimation += IL_Player_UpdateAnimation;
                #endregion
                #region Save related Hooks
                IL.SaveState.SessionEnded += LizardHooks.HandleYoungLizardMotherPassage;
                #endregion
                #region World Hooks
                IL.RoomCamera.Update += DangerTypes.RoomCameraOnUpdate;
                new ILHook(typeof(RoomRain)
                    .GetProperty(nameof(RoomRain.FloodLevel))!
                    .GetGetMethod(), DangerTypes.ChillyFloodLevel);
                #endregion
                #region Misc
                IL.GhostWorldPresence.SpawnGhost += NoirCatto.GhostWorldPresenceILSpawnGhost;
                IL.Menu.KarmaLadderScreen.GetDataFromGame += NoirCatto.KarmaLadderScreenILGetDataFromGame;
                IL.SharedPhysics.TraceProjectileAgainstBodyChunks += NoirCatto.SharedPhysicsILTraceProjectileAgainstBodyChunks;
                #endregion
                #endregion
                
                #region Fisobs
                Content.Register(new PebblesLLCritob());
                Content.Register(new MotherLizardCritob());
                Content.Register(new PilgrimLizardCritob());
                Content.Register(new YoungLizardCritob());
                Content.Register(new SnowSpiderCritob());
                Content.Register(new BoulderFisob());
                Content.Register(new LittleCrackerFisob());
                Content.Register(new BoomMineFisob());
                Content.Register(new SolaceScarfFisob());
                #endregion
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Plugin.LogSource.LogError(ex);
            }
        }

        private static void On_Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
        {
            orig(self, eu);
            LizardRideControl.LizardOnUpdate(self, eu);
            LizardHooks.Lizard_Update(self, eu);
        }

        private static void On_HUD_HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig(self, cam);
            HuntQuestThings.HUDOnInitSinglePlayerHud(self, cam);
            HudHooks.HUDOnInitSinglePlayerHud(self, cam);
        }

        private static void On_StoryGameSession_Ctor(On.StoryGameSession.orig_ctor orig, StoryGameSession self, SlugcatStats.Name savestatenumber, RainWorldGame game)
        {
            orig(self, savestatenumber, game);
            HuntQuestThings.StoryGameSessionOnctor(self, savestatenumber, game);
            WorldChanges.ScarfScripts.RoomScript.StoryGameSessionOnctor(self, savestatenumber, game);
        }

        private static void On_PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, RoomPalette palette)
        {
            orig(self, sleaser, rcam, palette);
            SlugcatGraphics.PlayerGraphics_ApplyPalette(self, sleaser, rcam, palette);
            NoirCatto.PlayerGraphicsOnApplyPalette(self, sleaser, rcam, palette);
        }

        private static void On_PlayerGaphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
        {
            orig(self, sleaser, rcam, timestacker, campos);
            SlugcatGraphics.PlayerGraphics_DrawSprites(self, sleaser, rcam, timestacker, campos);
            NoirCatto.PlayerGraphicsOnDrawSprites(self, sleaser, rcam, timestacker, campos);
        }

        private static void On_PlayerGraphics_Ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            SlugcatGraphics.PlayerGraphics_ctor(self, ow);
            NoirCatto.PlayerGraphicsOnctor(self, ow);
        }

        private static void On_Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyviewed, bool eu)
        {
            orig(self, actuallyviewed, eu);
            NoirCatto.PlayerOnGraphicsModuleUpdated(self, actuallyviewed, eu);
            LizardRideFixes.Player_GraphicsModuleUpdated(self, actuallyviewed, eu);
        }

        private static void On_Menu_Menu_Update(On.Menu.Menu.orig_Update orig, Menu.Menu self)
        {
            orig(self);
            MainMenu.MenuOnUpdate(self);
            NoirCatto.MenuOnUpdate(self);
        }

        private static void On_Creature_Die(On.Creature.orig_Die orig, Creature self)
        {
            HuntQuestThings.CreatureOnDie(self);
            orig(self);
            SaveThings.SolaceSaveData.CreatureOnDie(self);
        }

        private static void On_Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            SlugcatGameplay.Player_Update(self, eu);
            NoirCatto.PlayerOnUpdate(self, eu);
            DelugePearlMechanics.PlayerOnUpdate(self, eu);
            SaveThings.SolaceSaveData.PlayerOnUpdate(self, eu);
        }

        private static void On_Player_UpdateAnimation(On.Player.orig_UpdateAnimation orig, Player self)
        {
            orig(self);
            SlugcatGameplay.Player_UpdateAnimation(self);
            NoirCatto.PlayerOnUpdateAnimation(self);
        }

        private static void On_Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);
            SlugcatGameplay.Player_GrabUpdate(self, eu);
            NoirCatto.PlayerOnGrabUpdate(self, eu);
        }

        private static Player.ObjectGrabability On_Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            Player.ObjectGrabability? result = SlugcatGameplay.Player_Grabability(self, obj);
            result ??= DelugePearlMechanics.PlayerOnGrabability(self, obj);
            return result ?? orig(self, obj);
        }

        private static void On_Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            var timer = self.GetFriend().poleSuperJumpTimer;
            orig(self);
            SlugcatGameplay.Player_checkInput(timer, self);
            NoirCatto.PlayerOncheckInput(self);
        }

        private static void On_SlugcatStats_Ctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
        {
            orig(self, slugcat, malnourished);
            SlugcatGameplay.SlugcatStats_ctor(self, slugcat, malnourished);
            NoirCatto.SlugcatStatsOnctor(self, slugcat, malnourished);
        }

        private static void On_Player_UpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self)
        {
            orig(self);
            FriendCrawlTurn.PlayerOnUpdateBodyMode(self);
            SlugcatGameplay.Player_UpdateBodyMode(self);
            NoirCatto.PlayerOnUpdateBodyMode(self);
        }

        // Crawl turn IL Hook for Friend & Noir
        private static void IL_Player_UpdateAnimation(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);
                ILLabel label = null;
                c.GotoNext(
                    i => i.MatchLdsfld<Player.AnimationIndex>("CrawlTurn"),
                    i => i.MatchCall(out _),
                    i => i.MatchBrfalse(out label)
                );
                c.GotoPrev(MoveType.Before, i => i.MatchLdarg(0));
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(FriendCrawlTurn.CustomCrawlTurn);
                c.Emit(OpCodes.Brtrue, label);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(NoirCatto.CustomCrawlTurn);
                c.Emit(OpCodes.Brtrue, label);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"ILHook failed for CrawlTurn (Friend & Noir) due to {ex}");
            }
        }

        private static bool On_SlugcatHand_Engage_In_Movement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
        {
            bool? result = FriendCrawl.SlugcatHand_EngageInMovement(self);
            result ??= NoirCatto.SlugcatHandOnEngageInMovement(self);
            return result ?? orig(self);
        }

        private static void On_PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            SlugcatGraphics.PlayerGraphics_Update(self);
            FriendCrawl.PlayerGraphics_Update(self);
            NoirCatto.PlayerGraphicsOnUpdate(self);
        }

        private static void On_Player_ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
        {
            orig(self, spear);
            DragonCrafts.Player_ThrownSpear(self, spear);
            NoirCatto.PlayerOnThrownSpear(self, spear);
        }
    }
}