using System;
using System.Reflection;
using Fisobs.Core;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using TheFriend.SlugcatThings;
using TheFriend.CharacterThings.NoirThings;
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
using TheFriend.Objects.LittleCrackerObject;
using TheFriend.Objects.SolaceScarfObject;
using TheFriend.WorldChanges;
using TheFriend.WorldChanges.WorldStates.General;
using ColdRoom = On.MoreSlugcats.ColdRoom;
using Debug = UnityEngine.Debug;

namespace TheFriend
{
    public class Hooks
    {
        private static bool _modsInit;
        public static void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit initOrig, RainWorld initSelf)
        {
            initOrig(initSelf);
            if (_modsInit) return;
            
            try
            {
                _modsInit = true;
                Plugin.LoadResources();
                
                #region Creatures
                #region PebblesLL
                On.AbstractCreature.ctor += PebblesLL.AbstractCreature_ctor;
                On.DaddyGraphics.DaddyTubeGraphic.ApplyPalette += PebblesLLGraphics.DaddyTubeGraphic_ApplyPalette;
                On.DaddyGraphics.DaddyDeadLeg.ApplyPalette += PebblesLLGraphics.DaddyDeadLeg_ApplyPalette;
                On.DaddyGraphics.DaddyDeadLeg.DrawSprite += PebblesLLGraphics.DaddyDeadLeg_DrawSprite;
                On.DaddyGraphics.DaddyDangleTube.ApplyPalette += PebblesLLGraphics.DaddyDangleTube_ApplyPalette;
                IL.DaddyLongLegs.ctor += PebblesLL.DaddyLongLegs_ctor;
                IL.Spear.Update += PebblesLL.Spear_Update;
                Content.Register(new PebblesLLCritob());
                #endregion
                #region SnowSpider
                On.BigSpiderGraphics.ScaleAttachPos += SnowSpiderGraphics.BigSpiderGraphics_ScaleAttachPos;
                On.BigSpider.Violence += SnowSpiderGraphics.BigSpider_Violence;
                Content.Register(new SnowSpiderCritob());
                #endregion
                #region Lizards
                On.Lizard.Update += (update, lizard, eu) =>
                {
                    update(lizard, eu);
                    LizardRideControl.LizardOnUpdate(lizard, eu);
                    LizardHooks.Lizard_Update(lizard, eu);
                };
                
                On.Creature.Grab += LizardRideFixes.Creature_Grab;
                On.Creature.NewRoom += LizardRideFixes.Creature_NewRoom;
                On.Creature.SpitOutOfShortCut += LizardRideFixes.CreatureOnSpitOutOfShortCut;
                On.LizardAI.SocialEvent += LizardRideFixes.LizardAI_SocialEvent;
                On.Player.IsCreatureLegalToHoldWithoutStun += LizardRideFixes.Player_IsCreatureLegalToHoldWithoutStun;
                On.Player.ReleaseGrasp += LizardRideFixes.PlayerOnReleaseGrasp;
                On.Weapon.HitThisObject += LizardRideFixes.Weapon_HitThisObject;
                
                On.Lizard.Bite += LizardHooks.Lizard_Bite;
                On.Lizard.ctor += LizardHooks.Lizard_ctor;
                On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += LizardHooks.LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship;
                On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += 
                    LizardHooks.LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
                On.LizardGraphics.ctor += LizardHooks.LizardGraphics_ctor;
                On.LizardGraphics.AddToContainer += LizardHooks.LizardGraphics_AddToContainer;
                On.LizardGraphics.ApplyPalette += LizardHooks.LizardGraphics_ApplyPalette;
                On.LizardGraphics.BodyColor += LizardHooks.LizardGraphics_BodyColor;
                On.LizardGraphics.DrawSprites += LizardHooks.LizardGraphics_DrawSprites;
                On.LizardGraphics.DynamicBodyColor += LizardHooks.LizardGraphics_DynamicBodyColor;
                On.LizardGraphics.InitiateSprites += LizardHooks.LizardGraphics_InitiateSprites;
                On.LizardLimb.ctor += LizardHooks.LizardLimb_ctor;
                On.LizardVoice.GetMyVoiceTrigger += LizardHooks.LizardVoice_GetMyVoiceTrigger;
                IL.SaveState.SessionEnded += LizardHooks.HandleYoungLizardMotherPassage;
                
                On.LizardCosmetics.BodyScales.GeneratePatchPattern += LizardCosmeticHooks.BodyScales_GeneratePatchPattern;
                On.LizardCosmetics.BodyScales.GenerateSegments += LizardCosmeticHooks.BodyScales_GenerateSegments;
                On.LizardCosmetics.LongBodyScales.DrawSprites += LizardCosmeticHooks.LongBodyScales_DrawSprites;
                
                new Hook(typeof(LizardGraphics)
                    .GetProperty(nameof(LizardGraphics.HeadColor1), BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetGetMethod(true), FancyHeadColors.FancyHeadColor1);
                
                On.Creature.SuckedIntoShortCut += YoungLizardAI.CreatureOnSuckedIntoShortCut;
                On.WormGrass.WormGrassPatch.Update += WormGrassImmunizer.WormGrassPatch_Update;
                
                Content.Register(new MotherLizardCritob());
                Content.Register(new PilgrimLizardCritob());
                Content.Register(new YoungLizardCritob());
                #endregion
                #endregion
                
                #region DangerTypes
                On.AbstractCreature.DrainWorldDenFlooded += DangerTypes.AbstractCreatureOnDrainWorldDenFlooded;
                On.RoomRain.ctor += DangerTypes.RoomRainOnctor;
                On.RoomRain.Update += DangerTypes.RoomRainOnUpdate;
                new Hook(typeof(RoomRain)
                    .GetProperty(nameof(RoomRain.OutsidePushAround))!
                    .GetGetMethod(), DangerTypes.ChillyOutsidePushAround);
                On.ShelterDoor.Update += DangerTypes.ShelterDoorOnUpdate;
                On.Water.ctor += DangerTypes.WaterOnctor;
                IL.RoomCamera.Update += DangerTypes.RoomCameraOnUpdate;
                new ILHook(typeof(RoomRain)
                    .GetProperty(nameof(RoomRain.FloodLevel))!
                    .GetGetMethod(), DangerTypes.ChillyFloodLevel);
                #endregion
                
                #region Expedition
                //On.Expedition.ExpeditionProgression.BurdenMenuColor += ExpeditionBurdens.ExpeditionProgressionOnBurdenMenuColor;
                //On.Expedition.ExpeditionProgression.SetupBurdenGroups += ExpeditionBurdens.ExpeditionProgressionOnSetupBurdenGroups;
                //On.Expedition.ExpeditionProgression.BurdenName += ExpeditionBurdens.ExpeditionProgressionOnBurdenName;
                //On.Expedition.ExpeditionProgression.BurdenManualDescription += ExpeditionBurdens.ExpeditionProgressionOnBurdenManualDescription;
                //On.Expedition.ExpeditionProgression.BurdenScoreMultiplier += ExpeditionBurdens.ExpeditionProgressionOnBurdenScoreMultiplier;
                //On.Expedition.ExpeditionProgression.CountUnlockables += ExpeditionBurdens.ExpeditionProgressionOnCountUnlockables;
                On.Menu.UnlockDialog.TogglePerk += ExpeditionGeneral.UnlockDialogOnTogglePerk;
                //On.Menu.UnlockDialog.UpdateBurdens += ExpeditionBurdens.UnlockDialogOnUpdateBurdens;
                //On.Menu.UnlockDialog.SetUpBurdenDescriptions += ExpeditionBurdens.UnlockDialogOnSetUpBurdenDescriptions;
                //On.Menu.UnlockDialog.Update += ExpeditionBurdens.UnlockDialogOnUpdate;
                //On.Menu.UnlockDialog.ctor += ExpeditionBurdens.UnlockDialogOnctor;
                //IL.Menu.UnlockDialog.ctor += ExpeditionBurdens.UnlockDialogOnctor;
                #endregion
                
                #region FriendWorldState
                On.AbstractCreature.setCustomFlags                                    += FriendWorldState.AbstractCreature_setCustomFlags;
                On.ScavengerAbstractAI.InitGearUp                                     += FriendWorldState.ScavengerAbstractAI_InitGearUp;
                On.CreatureCommunities.LoadDefaultCommunityAlignments                 += FriendWorldState.CreatureCommunitiesOnLoadDefaultCommunityAlignments;
                On.CreatureCommunities.InfluenceCell                                  += FriendWorldState.CreatureCommunitiesOnInfluenceCell;
                On.Region.ctor                                                        += FriendWorldState.Region_ctor;
                On.Room.Loaded                                                        += FriendWorldState.Room_Loaded;
                On.Room.SlugcatGamemodeUniqueRoomSettings                             += FriendWorldState.Room_SlugcatGamemodeUniqueRoomSettings;
                On.FireFly.ctor                                                       += FriendWorldState.FireFly_ctor;
                On.RoomSpecificScript.SU_A43SuperJumpOnly.Update                      += FriendWorldState.SU_A43SuperJumpOnlyOnUpdate;
                //On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += FriendWorldState.WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues;
                #endregion
                
                #region FamineWorld
                On.DangleFruit.ApplyPalette += FamineWorld.DangleFruit_ApplyPalette;
                On.DangleFruit.DrawSprites += FamineWorld.DangleFruit_DrawSprites;
                On.MoreSlugcats.DandelionPeach.ApplyPalette += FamineWorld.DandelionPeach_ApplyPalette;
                On.MoreSlugcats.DandelionPeach.ctor += FamineWorld.DandelionPeach_ctor;
                On.MoreSlugcats.GooieDuck.ApplyPalette += FamineWorld.GooieDuck_ApplyPalette;
                On.MoreSlugcats.LillyPuck.ApplyPalette += FamineWorld.LillyPuck_ApplyPalette;
                On.MoreSlugcats.LillyPuck.Update += FamineWorld.LillyPuck_Update;
                On.SlugcatStats.NourishmentOfObjectEaten += FamineWorld.SlugcatStats_NourishmentOfObjectEaten;
                On.Fly.ctor += FamineBatFly.Fly_ctor;
                #region FamineCentipede
                On.Centipede.ShortCutColor += FamineCentipede.Centipede_ShortCutColor;
                On.Centipede.Violence += FamineCentipede.Centipede_Violence;
                On.CentipedeGraphics.ctor += FamineCentipede.CentipedeGraphics_ctor;
                On.CentipedeGraphics.DrawSprites += FamineCentipede.CentipedeGraphics_DrawSprites;
                On.CentipedeGraphics.InitiateSprites += FamineCentipede.CentipedeGraphics_InitiateSprites;
                IL.Player.EatMeatUpdate += FamineCentipede.Player_EatMeatUpdate;
                #endregion
                #region FamineEggBug
                /*On.EggBug.ctor += FamineEggBug.EggBugOnctor;
                On.EggBugGraphics.ApplyPalette += FamineEggBug.EggBugGraphicsOnApplyPalette;
                On.EggBugGraphics.ctor += FamineEggBug.EggBugGraphicsOnctor;
                On.EggBugGraphics.DrawSprites += FamineEggBug.EggBugGraphicsOnDrawSprites;
                On.EggBugGraphics.EggColors += FamineEggBug.EggBugGraphicsOnEggColors;
                On.EggBugGraphics.InitiateSprites += FamineEggBug.EggBugGraphicsOnInitiateSprites;
                On.EggBugEgg.ctor += FamineEggBug.EggBugEggOnctor;
                IL.EggBugGraphics.ApplyPalette += FamineEggBug.EggBugGraphicsILApplyPalette;*/
                #endregion
                #region FaminePolePlant
                /*On.PoleMimic.ctor += FaminePolePlant.PoleMimicOnctor;
                On.PoleMimicGraphics.DrawSprites += FaminePolePlant.PoleMimicGraphicsOnDrawSprites;
                IL.PoleMimicGraphics.InitiateSprites += FaminePolePlant.PoleMimicGraphicsILInitiateSprites;*/
                #endregion
                #endregion
                
                #region HudHooks
                On.HUD.RainMeter.Draw += HudHooks.RainMeter_Draw;
                On.HUD.RainMeter.ctor += HudHooks.RainMeter_ctor;
                On.HUD.RainMeter.Update += HudHooks.RainMeter_Update;
                On.Menu.SleepAndDeathScreen.GetDataFromGame += HudHooks.SleepAndDeathScreen_GetDataFromGame;
                #endregion
                
                #region SLOracleHandler
                On.SLOrcacleState.ForceResetState += SLOracleHandler.SLOrcacleState_ForceResetState;
                On.SLOracleBehavior.Update += SLOracleHandler.SLOracleBehavior_Update;
                On.SLOracleBehavior.Move += SLOracleHandler.SLOracleBehavior_Move;
                On.SLOracleBehaviorNoMark.Update += SLOracleHandler.SLOracleBehaviorNoMark_Update;
                On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += SLOracleHandler.MoonConversationOnAddEvents;
                On.RainWorldGame.IsMoonActive += SLOracleHandler.RainWorldGame_IsMoonActive;
                On.RainWorldGame.IsMoonHeartActive += SLOracleHandler.RainWorldGame_IsMoonHeartActive;
                On.RainWorldGame.MoonHasRobe += SLOracleHandler.RainWorldGame_MoonHasRobe;
                #endregion
                
                #region Slugcat Hooks
                #region SlugcatGameplay
                On.Player.ctor += SlugcatGameplay.Player_ctor;
                On.Player.Jump += SlugcatGameplay.Player_Jump;
                On.Player.ThrowObject += SlugcatGameplay.PlayerOnThrowObject;
                On.Player.WallJump += SlugcatGameplay.Player_WallJump;
                On.SlugcatStats.SlugcatCanMaul += SlugcatGameplay.SlugcatStats_SlugcatCanMaul;
                IL.Player.MovementUpdate += SlugcatGameplay.PlayerOnMovementUpdate;
                #endregion
                #region SlugcatGraphics
                On.GraphicsModule.HypothermiaColorBlend += SlugcatGraphics.GraphicsModule_HypothermiaColorBlend;
                On.PlayerGraphics.AddToContainer += SlugcatGraphics.PlayerGraphics_AddToContainer;
                On.PlayerGraphics.InitiateSprites += SlugcatGraphics.PlayerGraphics_InitiateSprites;
                On.PlayerGraphics.JollyFaceColorMenu += SlugcatGraphics.JollyFaceColorMenu;
                On.PlayerGraphics.JollyUniqueColorMenu += SlugcatGraphics.JollyUniqueColorMenu;
                On.JollyCoop.JollyMenu.SymbolButtonTogglePupButton.HasUniqueSprite += SlugcatGraphics.HasUniqueSprite;
                On.JollyCoop.JollyMenu.JollyPlayerSelector.GetPupButtonOffName += SlugcatGraphics.GetPupButtonOffName;
                #endregion
                #region Poacher
                #region FirecrackerFix
                On.FirecrackerPlant.Update += FirecrackerFix.FirecrackerPlant_Update;
                On.SaveState.AbstractPhysicalObjectFromString += FirecrackerFix.SaveState_AbstractPhysicalObjectFromString;
                On.SaveState.SetCustomData_AbstractPhysicalObject_string += FirecrackerFix.SaveState_SetCustomData_AbstractPhysicalObject_string;
                On.SaveUtils.PopulateUnrecognizedStringAttrs += FirecrackerFix.SaveUtils_PopulateUnrecognizedStringAttrs;
                IL.FirecrackerPlant.ctor += FirecrackerFix.FirecrackerPlant_ctor;
                #endregion
                #region PoacherSkullFeatures
                On.Creature.LoseAllGrasps += PoacherSkullFeatures.Creature_LoseAllGrasps;
                On.Creature.Violence += PoacherSkullFeatures.CreatureOnViolence;
                On.HUD.TextPrompt.UpdateGameOverString += PoacherSkullFeatures.TextPrompt_UpdateGameOverString;
                On.Player.Grabbed += PoacherSkullFeatures.Player_Grabbed;
                On.Player.SpearStick += PoacherSkullFeatures.Player_SpearStick;
                #endregion
                #region PoacherGameplay
                On.DangleFruit.Update += PoacherGameplay.DangleFruit_Update;
                On.LanternMouse.Update += PoacherGameplay.LanternMouse_Update;
                On.MoreSlugcats.DandelionPeach.Update += PoacherGameplay.DandelionPeach_Update;
                On.Player.HeavyCarry += PoacherGameplay.Player_HeavyCarry;
                On.Player.ObjectEaten += PoacherGameplay.PlayerOnObjectEaten;
                #endregion
                #region DragonCrafts
                On.Player.GraspsCanBeCrafted += DragonCrafts.Player_GraspsCanBeCrafted;
                On.Player.SpitUpCraftedObject += DragonCrafts.Player_SpitUpCraftedObject;
                On.Weapon.NewRoom += DragonCrafts.Weapon_NewRoom;
                DragonCrafts.InitRecipes();
                #endregion
                Content.Register(new BoulderFisob());
                Content.Register(new LittleCrackerFisob());
                Content.Register(new BoomMineFisob());
                #endregion
                #region Noir
                #region NoirCatto
                On.AbstractPhysicalObject.Abstractize += NoirCatto.AbstractPhysicalObjectOnAbstractize;
                On.Menu.Menu.CommunicateWithUpcomingProcess += NoirCatto.MenuOnCommunicateWithUpcomingProcess;
                On.SeedCob.PlaceInRoom += NoirCatto.SeedCobOnPlaceInRoom;
                On.Spear.Update += NoirCatto.SpearOnUpdate;
                On.Player.AllowGrabbingBatflys += NoirCatto.PlayerOnAllowGrabbingBatflys;
                On.Player.Jump += NoirCatto.PlayerOnJump;
                On.Player.MovementUpdate += NoirCatto.PlayerOnMovementUpdate;
                On.Player.PickupCandidate += NoirCatto.PlayerOnPickupCandidate;
                On.Player.ThrowObject += NoirCatto.PlayerOnThrowObject;
                On.PlayerGraphics.AddToContainer += NoirCatto.PlayerGraphicsOnAddToContainer;
                On.PlayerGraphics.InitiateSprites += NoirCatto.PlayerGraphicsOnInitiateSprites;
                On.PlayerGraphics.Reset += NoirCatto.PlayerGraphicsOnReset;
                On.RainWorld.Update += NoirCatto.RainWorldOnUpdate;
                On.Room.AddObject += NoirCatto.RoomOnAddObject;
                On.SaveState.setDenPosition += NoirCatto.SaveStateOnsetDenPosition;
                IL.GhostWorldPresence.SpawnGhost += NoirCatto.GhostWorldPresenceILSpawnGhost;
                IL.Menu.KarmaLadderScreen.GetDataFromGame += NoirCatto.KarmaLadderScreenILGetDataFromGame;
                IL.SharedPhysics.TraceProjectileAgainstBodyChunks += NoirCatto.SharedPhysicsILTraceProjectileAgainstBodyChunks;
                IL.SeedCob.Update += NoirCatto.SeedCobILUpdate;
                IL.Spear.Update += NoirCatto.SpearILUpdate;
                IL.Weapon.Update += NoirCatto.WeaponILUpdate;
                #endregion
                #endregion
                #region Friend
                On.Player.MovementUpdate += FriendCrawlTurn.PlayerOnMovementUpdate;
                IL.Player.ThrowObject += CharacterThings.FriendThings.FriendGameplay.Player_ThrowObject;
                #endregion
                #endregion
                
                #region SolaceCustom
                On.PlayerProgression.ClearOutSaveStateFromMemory += SaveThings.SolaceCustom.PlayerProgressionOnClearOutSaveStateFromMemory;
                On.PlayerProgression.WipeSaveState += SaveThings.SolaceCustom.PlayerProgressionOnWipeSaveState;
                #endregion
                
                #region SolaceScarf
                //On.RoomSpecificScript.AddRoomSpecificScript += WorldChanges.ScarfScripts.RoomScript.RoomSpecificScriptOnAddRoomSpecificScript;
                //Content.Register(new SolaceScarfFisob());
                #endregion
                
                #region MainMenu
                On.Menu.IntroRoll.ctor += MainMenu.IntroRollOnctor;
                On.Menu.RainEffect.ctor += MainMenu.RainEffectOnctor;
                On.Menu.RainEffect.GrafUpdate += MainMenu.RainEffectOnGrafUpdate; 
                On.MenuMicrophone.PlaySound_SoundID_float_float_float += MainMenu.MenuMicrophoneOnPlaySound_SoundID_float_float_float;
                #endregion
                
                #region Shared Hooks
                On.Creature.Die += (orig, creature) =>
                {
                    orig(creature);
                    SaveThings.SolaceSaveData.CreatureOnDie(creature);
                };
                On.HUD.HUD.InitSinglePlayerHud += (orig, hud, cam) => 
                { 
                    orig(hud, cam);
                    HudHooks.HUDOnInitSinglePlayerHud(hud, cam);
                };
                On.Menu.Menu.Update += (orig, menu) =>
                {
                    orig(menu);
                    MainMenu.MenuOnUpdate(menu);
                    NoirCatto.MenuOnUpdate(menu);
                };
                On.Player.checkInput += (orig, player) =>
                {
                    var timer = player.GetFriend().poleSuperJumpTimer;
                    orig(player);
                    SlugcatGameplay.Player_checkInput(timer, player);
                    NoirCatto.PlayerOncheckInput(player);
                };
                On.Player.Grabability += (orig, player, obj) =>
                {
                    Player.ObjectGrabability? result = SlugcatGameplay.Player_Grabability(player, obj);
                    return result ?? orig(player, obj);
                };
                On.Player.GraphicsModuleUpdated += (orig, player, actuallyviewed, eu) =>
                {
                    orig(player, actuallyviewed, eu);
                    NoirCatto.PlayerOnGraphicsModuleUpdated(player, actuallyviewed, eu);
                    LizardRideFixes.Player_GraphicsModuleUpdated(player, actuallyviewed, eu);
                };
                On.Player.GrabUpdate += (orig, self1, eu) =>
                {
                    orig(self1, eu);
                    SlugcatGameplay.Player_GrabUpdate(self1, eu);
                    NoirCatto.PlayerOnGrabUpdate(self1, eu);
                };
                On.Player.ThrownSpear += (orig, self, spear) =>
                {
                    orig(self, spear);
                    DragonCrafts.Player_ThrownSpear(self, spear);
                    NoirCatto.PlayerOnThrownSpear(self, spear);
                };
                On.Player.UpdateAnimation += (orig, self) =>
                {
                    orig(self);
                    SlugcatGameplay.Player_UpdateAnimation(self);
                    NoirCatto.PlayerOnUpdateAnimation(self);
                };
                On.Player.Update += (orig, self, eu) =>
                {
                    orig(self, eu);
                    SlugcatGameplay.Player_Update(self, eu);
                    NoirCatto.PlayerOnUpdate(self, eu);
                    SaveThings.SolaceSaveData.PlayerOnUpdate(self, eu);
                };
                On.Player.UpdateBodyMode += (orig, self) =>
                {
                    orig(self);
                    FriendCrawlTurn.PlayerOnUpdateBodyMode(self);
                    SlugcatGameplay.Player_UpdateBodyMode(self);
                    NoirCatto.PlayerOnUpdateBodyMode(self);
                };
                On.PlayerGraphics.ApplyPalette += (orig, self, sleaser, rcam, palette) =>
                {
                    orig(self, sleaser, rcam, palette);
                    SlugcatGraphics.PlayerGraphics_ApplyPalette(self, sleaser, rcam, palette);
                    NoirCatto.PlayerGraphicsOnApplyPalette(self, sleaser, rcam, palette);
                };
                On.PlayerGraphics.ctor += (orig, self, ow) =>
                {
                    orig(self, ow);
                    SlugcatGraphics.PlayerGraphics_ctor(self, ow);
                    NoirCatto.PlayerGraphicsOnctor(self, ow);
                };
                On.PlayerGraphics.DrawSprites += (orig, self, sleaser, rcam, timestacker, campos) =>
                {
                    orig(self, sleaser, rcam, timestacker, campos);
                    SlugcatGraphics.PlayerGraphics_DrawSprites(self, sleaser, rcam, timestacker, campos);
                    NoirCatto.PlayerGraphicsOnDrawSprites(self, sleaser, rcam, timestacker, campos);
                };
                On.PlayerGraphics.Update += (orig, self) =>
                {
                    orig(self);
                    SlugcatGraphics.PlayerGraphics_Update(self);
                    FriendCrawl.PlayerGraphics_Update(self);
                    NoirCatto.PlayerGraphicsOnUpdate(self);
                };
                On.SlugcatHand.EngageInMovement += (orig, self) =>
                {
                    bool? result = FriendCrawl.SlugcatHand_EngageInMovement(self);
                    result ??= NoirCatto.SlugcatHandOnEngageInMovement(self);
                    return result ?? orig(self);
                };
                On.SlugcatStats.ctor += (orig, self, slugcat, malnourished) =>
                {
                    orig(self, slugcat, malnourished);
                    SlugcatGameplay.SlugcatStats_ctor(self, slugcat, malnourished);
                    NoirCatto.SlugcatStatsOnctor(self, slugcat, malnourished);
                };
                On.StoryGameSession.ctor += (orig, self, savestatenumber, game) =>
                {
                    orig(self, savestatenumber, game);
                    //WorldChanges.ScarfScripts.RoomScript.StoryGameSessionOnctor(self, savestatenumber, game);
                };
                On.RainWorldGame.ctor += (orig, self, manager) =>
                {
                    orig(self, manager);
                    NoirCatto.RainWorldGameOnctor(self, manager);
                    QuickWorldData.RainWorldGameOnctor(self, manager);
                };
                
                // Crawl turn IL Hook for Friend & Noir
                IL.Player.UpdateAnimation += il =>
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
                };
                #endregion
                
                On.AbstractPhysicalObject.Realize += AbstractObjectType.AbstractPhysicalObjectOnRealize;
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError(ex);
            }
        }
    }
}