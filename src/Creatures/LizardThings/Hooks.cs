using System;
using System.Collections.Generic;
using LizardCosmetics;
using MoreSlugcats;
using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;

namespace TheFriend.Creatures.LizardThings;
public class Hooks
{
    public static void Apply()
    {
        On.LizardLimb.ctor += LizardLimb_ctor;
        On.LizardVoice.GetMyVoiceTrigger += LizardVoice_GetMyVoiceTrigger;
        On.LizardCosmetics.LongHeadScales.ctor += LongHeadScales_ctor;
        On.LizardCosmetics.ShortBodyScales.ctor += ShortBodyScales_ctor;
        On.LizardCosmetics.BodyScales.GeneratePatchPattern += BodyScales_GeneratePatchPattern;
        On.LizardCosmetics.BodyScales.GenerateSegments += BodyScales_GenerateSegments;
        On.LizardCosmetics.LongBodyScales.DrawSprites += LongBodyScales_DrawSprites;
        On.LizardCosmetics.BumpHawk.ctor += BumpHawk_ctor;

        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
        On.Lizard.ctor += Lizard_ctor;
        On.Lizard.Bite += Lizard_Bite;
        On.Lizard.Update += Lizard_Update;
        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship;

        On.LizardGraphics.ctor += LizardGraphics_ctor;
        On.LizardGraphics.ApplyPalette += LizardGraphics_ApplyPalette;
        On.LizardGraphics.BodyColor += LizardGraphics_BodyColor;
        On.LizardGraphics.DynamicBodyColor += LizardGraphics_DynamicBodyColor;
        On.LizardGraphics.InitiateSprites += LizardGraphics_InitiateSprites;
        On.LizardGraphics.AddToContainer += LizardGraphics_AddToContainer;
        On.LizardGraphics.DrawSprites += LizardGraphics_DrawSprites;

        On.LizardAI.SocialEvent += LizardAI_SocialEvent;
        On.Creature.NewRoom += Creature_NewRoom;
        On.Player.IsCreatureLegalToHoldWithoutStun += Player_IsCreatureLegalToHoldWithoutStun;
        On.Creature.Grab += Creature_Grab;
        On.WormGrass.WormGrassPatch.InteractWithCreature += WormGrassPatch_InteractWithCreature;
        On.WormGrass.WormGrassPatch.Update += WormGrassPatch_Update;
        On.WormGrass.WormGrassPatch.AlreadyTrackingCreature += WormGrassPatch_AlreadyTrackingCreature;
    }

    #region misc cosmetics
    public static void LizardLimb_ctor(On.LizardLimb.orig_ctor orig, LizardLimb self, GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, LizardLimb otherLimbInPair)
    {
        orig(self, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair);
        if (owner is LizardGraphics l && (l.lizard?.Template.type == CreatureTemplateType.MotherLizard || l.lizard?.Template.type == CreatureTemplateType.YoungLizard || l.lizard?.Template.type == CreatureTemplateType.PilgrimLizard))
        {
            self.grabSound = SoundID.Lizard_Green_Foot_Grab;
            self.releaseSeound = SoundID.Lizard_Green_Foot_Release;
        }
    }
    public static SoundID LizardVoice_GetMyVoiceTrigger(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
    {
        var res = orig(self);
        if (self.lizard is Lizard m && m.Template.type == CreatureTemplateType.MotherLizard)
        {
            var array = new[] { "A", "B", "C", "D", "E" };
            var list = new List<SoundID>();
            for (int i = 0; i < array.Length; i++)
            {
                var soundID = SoundID.None;
                var text2 = "Lizard_Voice_Red_" + array[i];
                if (SoundID.values.entries.Contains(text2))
                    soundID = new(text2);
                if (soundID != SoundID.None && soundID.Index != -1 && m.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                    list.Add(soundID);
            }
            if (list.Count == 0)
                res = SoundID.None;
            else
                res = list[Random.Range(0, list.Count)];
        }
        else if (self.lizard is Lizard y && y.Template.type == CreatureTemplateType.YoungLizard)
        {
            var array = new[] { "A", "B", "C", "D", "E" };
            var list = new List<SoundID>();
            for (int i = 0; i < array.Length; i++)
            {
                var soundID = SoundID.None;
                var text2 = "Lizard_Voice_Blue_" + array[i];
                if (SoundID.values.entries.Contains(text2))
                    soundID = new(text2);
                if (soundID != SoundID.None && soundID.Index != -1 && y.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                    list.Add(soundID);
            }
            if (list.Count == 0)
                res = SoundID.None;
            else
                res = list[Random.Range(0, list.Count)];
        }
        else if (self.lizard is Lizard z && z.Template.type == CreatureTemplateType.PilgrimLizard)
        {
            var array = new[] { "A", "B", "C", "D", "E" };
            var list = new List<SoundID>();
            for (int i = 0; i < array.Length; i++)
            {
                var soundID = SoundID.None;
                var text2 = "Lizard_Voice_Black_" + array[i];
                if (SoundID.values.entries.Contains(text2))
                    soundID = new(text2);
                if (soundID != SoundID.None && soundID.Index != -1 && z.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                    list.Add(soundID);
            }
            if (list.Count == 0)
                res = SoundID.None;
            else
                res = list[Random.Range(0, list.Count)];
        }
        return res;
    }
    #endregion
    #region lizard data
    public static CreatureTemplate LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
    {
        if (type == CreatureTemplateType.MotherLizard)
        {
            var temp = orig(CreatureTemplate.Type.GreenLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            var breedParams = (temp.breedParameters as LizardBreedParams)!;
            temp.type = type;
            temp.name = "MotherLizard";
            breedParams.baseSpeed = 4f;
            breedParams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[2] = new(.1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[3] = new(.1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[4] = new(.1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[5] = new(.1f, 1f, 1f, 1f);
            breedParams.swimSpeed = 0.5f;

            temp.visualRadius = 1000f;
            temp.waterVision = .3f;
            temp.movementBasedVision = .4f;
            temp.throughSurfaceVision = .95f;
            breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 0f);
            breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 7f / 12f);
            breedParams.framesBetweenLookFocusChange = 60;

            breedParams.headSize = 1.5f;
            breedParams.neckStiffness = 1f;
            breedParams.jawOpenAngle = 100f;
            breedParams.jawOpenLowerJawFac = .65f;
            breedParams.jawOpenMoveJawsApart = 18.5f;
            breedParams.headGraphics = new int[5] { 0, 0, 0, 5, 3 };

            breedParams.standardColor = new(1f, 1f, 1f);
            breedParams.bodyMass = 10f;
            breedParams.bodySizeFac = 1.8f;
            breedParams.bodyLengthFac = 1.7f;
            breedParams.bodyRadFac = 1f;
            breedParams.bodyStiffnes = .1f;
            temp.bodySize = 3f;

            breedParams.tailSegments = 7;
            breedParams.tailStiffness = 500f;
            breedParams.tailStiffnessDecline = .30f;
            breedParams.tailLengthFactor = 1.85f;
            breedParams.tailColorationStart = 0.5f;
            breedParams.tailColorationExponent = 0.3f;

            breedParams.biteDelay = 2;
            breedParams.biteInFront = 20f;
            breedParams.biteRadBonus = 5f;
            breedParams.biteHomingSpeed = 1f;
            breedParams.biteChance = 1f;
            breedParams.attemptBiteRadius = 150f;
            breedParams.getFreeBiteChance = 1f;
            breedParams.biteDamage = 10f;
            breedParams.biteDamageChance = 1f;
            breedParams.biteDominance = 1f;

            temp.dangerousToPlayer = breedParams.danger;
            breedParams.danger = .7f;
            breedParams.aggressionCurveExponent = .4f;
            breedParams.headShieldAngle = 80f;
            breedParams.toughness = 20f;
            breedParams.stunToughness = 8f;
            temp.damageRestistances[(int)Creature.DamageType.Explosion, 0] = 1.9f;
            temp.damageRestistances[(int)Creature.DamageType.Electric, 0] = 1.9f;
            temp.baseDamageResistance = 40;
            temp.baseStunResistance = 20;

            breedParams.legPairDisplacement = 0.8f;
            breedParams.limbSize = 1.4f;
            breedParams.limbSpeed = 3.15f;
            breedParams.limbThickness = 1.9f;
            breedParams.limbQuickness = 0.5f;
            breedParams.limbGripDelay = 2;
            breedParams.liftFeet = 0.2f;
            breedParams.feetDown = 0.2f;
            breedParams.stepLength = 0.5f;
            breedParams.regainFootingCounter = 11;
            breedParams.floorLeverage = 10f;
            breedParams.maxMusclePower = 14f;
            breedParams.wiggleSpeed = 0.2f;
            breedParams.wiggleDelay = 15;
            breedParams.idleCounterSubtractWhenCloseToIdlePos = 10;
            breedParams.noGripSpeed = .1f;
            breedParams.smoothenLegMovement = true;
            breedParams.walkBob = 1.9f;

            breedParams.canExitLounge = false;
            breedParams.canExitLoungeWarmUp = true;
            breedParams.loungeDistance = 100f;
            breedParams.preLoungeCrouch = 25;
            breedParams.preLoungeCrouchMovement = -.3f;
            breedParams.loungeSpeed = 2.8f;
            breedParams.loungeMaximumFrames = 20;
            breedParams.loungePropulsionFrames = 20;
            breedParams.loungeJumpyness = 1f;
            breedParams.findLoungeDirection = 1f;
            breedParams.loungeDelay = 60;
            breedParams.riskOfDoubleLoungeDelay = .4f;
            breedParams.postLoungeStun = 18;
            breedParams.loungeTendensy = 0.65f;

            breedParams.tamingDifficulty = 10f;
            breedParams.tongue = false;

            temp.meatPoints = 20;
            temp.wormGrassImmune = true;
            temp.waterPathingResistance = 1.5f;
            temp.requireAImap = true;
            temp.BlizzardAdapted = true;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard);
            temp.doPreBakedPathing = false;

            temp.throwAction = "Hiss";
            temp.pickupAction = "Bite";
            temp.jumpAction = "Lunge";
            return temp;
        }
        if (type == CreatureTemplateType.YoungLizard)
        {
            var temp = orig(CreatureTemplate.Type.PinkLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            var breedParams = (temp.breedParameters as LizardBreedParams)!;
            temp.type = type;
            temp.name = "YoungLizard";
            breedParams.baseSpeed = 10f;
            breedParams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[2] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[4] = new(.1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[5] = new(.1f, 1f, 1f, 1f);
            temp.waterPathingResistance = 3f;
            breedParams.swimSpeed = 0.5f;

            temp.visualRadius = 900f;
            temp.waterVision = .4f;
            temp.throughSurfaceVision = .85f;
            temp.movementBasedVision = .3f;
            breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 0f);
            breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 7f / 12f);
            breedParams.framesBetweenLookFocusChange = 80;

            breedParams.headSize = 0.85f;
            breedParams.neckStiffness = 0.2f;
            breedParams.jawOpenAngle = 100f;
            breedParams.jawOpenLowerJawFac = .65f;
            breedParams.jawOpenMoveJawsApart = 18.5f;
            breedParams.headGraphics = new int[5];

            breedParams.standardColor = new(1f, 1f, 1f);
            breedParams.standardColor = new Color(0.80f, 0.75f, 0.6f);
            breedParams.bodyMass = 0.5f;
            breedParams.bodySizeFac = 0.73f;
            breedParams.bodyLengthFac = 0.6f;
            breedParams.bodyRadFac = 1f;
            breedParams.bodyStiffnes = .25f;
            temp.bodySize = 1f;

            breedParams.tailSegments = 4;
            breedParams.tailStiffness = 250f;
            breedParams.tailStiffnessDecline = .20f;
            breedParams.tailLengthFactor = 0.6f;
            breedParams.tailColorationStart = .3f;
            breedParams.tailColorationExponent = 2f;

            breedParams.biteDelay = 12;
            breedParams.biteInFront = 25f;
            breedParams.biteRadBonus = 0f;
            breedParams.biteHomingSpeed = 1.7f;
            breedParams.biteChance = 0.5f;
            breedParams.attemptBiteRadius = 40f;
            breedParams.getFreeBiteChance = 0.65f;
            breedParams.biteDamage = 0.01f;
            breedParams.biteDamageChance = 100f;
            breedParams.biteDominance = 0.2f;

            temp.dangerousToPlayer = breedParams.danger;
            breedParams.danger = 0.01f;
            breedParams.aggressionCurveExponent = .4f;
            breedParams.headShieldAngle = 100f;
            breedParams.toughness = 0.5f;
            breedParams.stunToughness = 8f;
            temp.baseDamageResistance = 0.5f;
            temp.baseStunResistance = 1;

            breedParams.legPairDisplacement = 0.2f;
            breedParams.limbSize = 0.94f;
            breedParams.limbSpeed = 3.15f;
            breedParams.limbThickness = 0.94f;
            breedParams.limbQuickness = .5f;
            breedParams.limbGripDelay = 1;
            breedParams.liftFeet = .3f;
            breedParams.feetDown = 0.5f;
            breedParams.stepLength = 0.3f;
            breedParams.regainFootingCounter = 8;
            breedParams.floorLeverage = 0.5f;
            breedParams.maxMusclePower = 3f;
            breedParams.wiggleSpeed = .5f;
            breedParams.wiggleDelay = 15;
            breedParams.idleCounterSubtractWhenCloseToIdlePos = 10;
            breedParams.noGripSpeed = .1f;
            breedParams.smoothenLegMovement = true;
            breedParams.walkBob = 5.34f;

            breedParams.canExitLounge = true;
            breedParams.canExitLoungeWarmUp = true;
            breedParams.findLoungeDirection = .5f;
            breedParams.loungeDistance = 100f;
            breedParams.preLoungeCrouch = 25;
            breedParams.preLoungeCrouchMovement = -.4f;
            breedParams.loungeSpeed = 2.1f;
            breedParams.loungeMaximumFrames = 15;
            breedParams.loungePropulsionFrames = 40;
            breedParams.loungeJumpyness = 1f;
            breedParams.loungeDelay = 60;
            breedParams.riskOfDoubleLoungeDelay = .1f;
            breedParams.postLoungeStun = 18;
            breedParams.loungeTendensy = 1f;

            breedParams.tamingDifficulty = 0.2f;

            temp.meatPoints = 1;
            temp.doPreBakedPathing = false;
            temp.requireAImap = true;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard);
            temp.throwAction = "Hiss";
            temp.pickupAction = "Bite";
            return temp;
        }
        if (type == CreatureTemplateType.PilgrimLizard)
        {
            var temp = orig(CreatureTemplate.Type.PinkLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            var breedParams = (temp.breedParameters as LizardBreedParams)!;
            temp.type = type;
            temp.name = "PilgrimLizard";
            breedParams.baseSpeed = 4f;
            breedParams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[2] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[4] = new(.1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[5] = new(.1f, 1f, 1f, 1f);
            temp.waterPathingResistance = 3f;
            breedParams.swimSpeed = 0.5f;

            temp.visualRadius = 900f;
            temp.waterVision = .4f;
            temp.throughSurfaceVision = .85f;
            temp.movementBasedVision = .3f;
            breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 0f);
            breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 7f / 12f);
            breedParams.framesBetweenLookFocusChange = 80;

            breedParams.headSize = 1f;
            breedParams.neckStiffness = 0.2f;
            breedParams.jawOpenAngle = 100f;
            breedParams.jawOpenLowerJawFac = .65f;
            breedParams.jawOpenMoveJawsApart = 18.5f;
            breedParams.headGraphics = new int[5];

            breedParams.standardColor = new(1f, 1f, 1f);
            breedParams.standardColor = new Color(1f, 1f, 1f);
            breedParams.bodyMass = 1f;
            breedParams.bodySizeFac = 1f;
            breedParams.bodyLengthFac = 1f;
            breedParams.bodyRadFac = 1f;
            breedParams.bodyStiffnes = .25f;
            temp.bodySize = 1f;

            breedParams.tailSegments = 6;
            breedParams.tailStiffness = 250f;
            breedParams.tailStiffnessDecline = .20f;
            breedParams.tailLengthFactor = 0.6f;
            breedParams.tailColorationStart = .3f;
            breedParams.tailColorationExponent = 2f;

            breedParams.biteDelay = 12;
            breedParams.biteInFront = 25f;
            breedParams.biteRadBonus = 0f;
            breedParams.biteHomingSpeed = 1.7f;
            breedParams.biteChance = 0.5f;
            breedParams.attemptBiteRadius = 40f;
            breedParams.getFreeBiteChance = 0.65f;
            breedParams.biteDamage = 0.01f;
            breedParams.biteDamageChance = 100f;
            breedParams.biteDominance = 0.2f;

            temp.dangerousToPlayer = breedParams.danger;
            breedParams.danger = 0.01f;
            breedParams.aggressionCurveExponent = .4f;
            breedParams.headShieldAngle = 100f;
            breedParams.toughness = 0.5f;
            breedParams.stunToughness = 8f;
            temp.baseDamageResistance = 0.5f;
            temp.baseStunResistance = 1;

            breedParams.legPairDisplacement = 0.2f;
            breedParams.limbSize = 0.94f;
            breedParams.limbSpeed = 3.15f;
            breedParams.limbThickness = 0.94f;
            breedParams.limbQuickness = .5f;
            breedParams.limbGripDelay = 1;
            breedParams.liftFeet = .3f;
            breedParams.feetDown = 0.5f;
            breedParams.stepLength = 0.3f;
            breedParams.regainFootingCounter = 8;
            breedParams.floorLeverage = 0.5f;
            breedParams.maxMusclePower = 3f;
            breedParams.wiggleSpeed = .5f;
            breedParams.wiggleDelay = 15;
            breedParams.idleCounterSubtractWhenCloseToIdlePos = 10;
            breedParams.noGripSpeed = .1f;
            breedParams.smoothenLegMovement = true;
            breedParams.walkBob = 5.34f;

            breedParams.canExitLounge = true;
            breedParams.canExitLoungeWarmUp = true;
            breedParams.findLoungeDirection = .5f;
            breedParams.loungeDistance = 100f;
            breedParams.preLoungeCrouch = 25;
            breedParams.preLoungeCrouchMovement = -.4f;
            breedParams.loungeSpeed = 2.1f;
            breedParams.loungeMaximumFrames = 15;
            breedParams.loungePropulsionFrames = 40;
            breedParams.loungeJumpyness = 1f;
            breedParams.loungeDelay = 60;
            breedParams.riskOfDoubleLoungeDelay = .1f;
            breedParams.postLoungeStun = 18;
            breedParams.loungeTendensy = 1f;

            breedParams.tamingDifficulty = 0.2f;

            temp.meatPoints = 1;
            temp.doPreBakedPathing = false;
            temp.requireAImap = true;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard);
            temp.throwAction = "Hiss";
            temp.pickupAction = "Bite";
            return temp;
        }
        return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
    }
    public static void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (Plugin.LizRideAll() && 
            self.Template.type != CreatureTemplateType.YoungLizard && 
            self.Template.type != CreatureTemplateType.MotherLizard) 
            self.GetLiz().IsRideable = true;
        if (self.Template.type == CreatureTemplateType.MotherLizard)
        {
            var state = Random.state;
            Random.InitState(abstractCreature.ID.RandomSeed);
            self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.9f, 0.1f, 0.6f), 1f, Custom.ClampedRandomVariation(0.85f, 0.15f, 0.2f));
            if (Plugin.LizRide()) self.GetLiz().IsRideable = true;
            Random.state = state;
        }
        else if (self.Template.type == CreatureTemplateType.YoungLizard)
        {
            var state = Random.state;
            Random.InitState(abstractCreature.ID.RandomSeed);
            self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.1f, 1f, 1f), 1f, Custom.ClampedRandomVariation(0.8f, 0.2f, 0f));
            AbstractCreature creature = self.abstractCreature;
            self.GetLiz().IsRideable = false;
            Random.state = state;
        }
        else if (self.Template.type == CreatureTemplateType.PilgrimLizard)
        {
            var state = Random.state;
            Random.InitState(abstractCreature.ID.RandomSeed);
            self.effectColor = Color.red;
            AbstractCreature creature = self.abstractCreature;
            self.GetLiz().IsRideable = false;
            Random.state = state;
        }
        if (self.Template.type == CreatureTemplate.Type.Salamander ||
            self.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
            self.GetLiz().aquatic = true;
    }
    public static void Lizard_Bite(On.Lizard.orig_Bite orig, Lizard self, BodyChunk chunk)
    {
        orig(self, chunk);
        if (self.Template.type != CreatureTemplateType.YoungLizard) return;
        if (chunk.owner is Creature crit && 
            (crit.Template.type == CreatureTemplate.Type.Hazer || 
             crit.Template.type == CreatureTemplate.Type.VultureGrub || 
             crit.Template.type == CreatureTemplate.Type.Fly))
        {
            crit.Die();
        }
    }
    public static CreatureTemplate.Relationship LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.LizardAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, LizardAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        CreatureTemplate.Relationship relationship = orig(self, dRelation);
        var trackedcreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
        try
        {
            if (dRelation == null) return relationship;
            if (dRelation.trackerRep == null) return relationship;
            if (dRelation.trackerRep?.representedCreature == null) return relationship;
            if (dRelation.trackerRep?.representedCreature?.realizedCreature == null) return relationship;
            if (trackedcreature == null) return relationship;

            if (self?.lizard?.Template?.type == CreatureTemplateType.YoungLizard)
            {
                if ((trackedcreature.Template?.type == CreatureTemplate.Type.Slugcat || 
                     trackedcreature.Template?.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC) && 
                    (relationship.type == CreatureTemplate.Relationship.Type.Attacks || 
                     relationship.type == CreatureTemplate.Relationship.Type.Eats))
                {
                    if (self?.creature?.personality.aggression >= 0.7f || 
                        self?.creature?.personality.bravery >= 0.7f)
                        relationship.type = CreatureTemplate.Relationship.Type.Ignores;
                    else
                        relationship.type = CreatureTemplate.Relationship.Type.Afraid;
                }
                if (trackedcreature is Lizard liz && liz.Template?.type == CreatureTemplateType.YoungLizard && 
                    (relationship.type == CreatureTemplate.Relationship.Type.Attacks || 
                     relationship.type == CreatureTemplate.Relationship.Type.Eats || 
                     relationship.type == CreatureTemplate.Relationship.Type.AgressiveRival))
                    relationship.type = CreatureTemplate.Relationship.Type.Ignores;
                
                if (trackedcreature is Lizard && relationship.type == CreatureTemplate.Relationship.Type.Attacks || 
                    relationship.type == CreatureTemplate.Relationship.Type.Eats || 
                    relationship.type == CreatureTemplate.Relationship.Type.Afraid)
                {
                    if (relationship.type == CreatureTemplate.Relationship.Type.Attacks)
                        relationship.type = CreatureTemplate.Relationship.Type.Ignores;
                    
                    if (relationship.type == CreatureTemplate.Relationship.Type.Afraid && 
                        (self.creature.personality.aggression >= 0.7f || 
                         self.creature.personality.bravery >= 0.7f))
                        relationship.type = CreatureTemplate.Relationship.Type.Ignores;
                }
                return relationship;
            }
            if (self?.lizard?.Template?.type == CreatureTemplateType.MotherLizard && self?.lizard != null)
            {
                if ((trackedcreature.Template?.type == CreatureTemplate.Type.Slugcat || 
                     trackedcreature.Template?.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC))
                {
                    if (relationship.type == CreatureTemplate.Relationship.Type.Afraid) 
                        relationship.type = CreatureTemplate.Relationship.Type.Attacks;
                    if (relationship.type == CreatureTemplate.Relationship.Type.Eats && 
                        !self.lizard.room.game.IsArenaSession && 
                        ( self.creature?.personality.sympathy >= 0.75f && 
                          self.creature?.world?.game?.session?.creatureCommunities?.LikeOfPlayer(CreatureCommunities.CommunityID.Lizards, self.creature.world.region.regionNumber, 0) > 0.5f ||
                         self.lizard?.AI?.LikeOfPlayer(self.lizard?.AI?.tracker?.RepresentationForCreature(trackedcreature.abstractCreature, true)) > 0)
                        )
                        relationship.type = CreatureTemplate.Relationship.Type.Ignores;
                }
                if (trackedcreature is Lizard liz && 
                    liz != null && 
                    liz?.Template?.type == CreatureTemplateType.YoungLizard && 
                    relationship.type == CreatureTemplate.Relationship.Type.Attacks)
                    relationship.type = CreatureTemplate.Relationship.Type.Ignores;
                
                return relationship;
            }
            return relationship;
        }
        catch (Exception) { Debug.Log("Solace: Exception occurred in LizardAI.DynamicRelationship"); }
        return relationship;
    }
    public static void Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
    {
        try { orig(self, eu); }
        catch (Exception e) { Debug.Log("Solace: Exception happened in Lizard.Update orig " + e); }
        if (self == null) return;
        if (self.Template.type == CreatureTemplateType.PilgrimLizard) self.Destroy(); // TODO: GET RID OF THIS LATER
        try
        {
            if (self.GetLiz() != null)
            {
                self.GetLiz().seat0 = Vector2.Lerp(self.bodyChunks[1].pos, self.bodyChunks[0].pos, 0.5f) + new Vector2(0, 15);
            }
            if (!self.dead && self.LizardState?.health > 0f && self.Template?.type == CreatureTemplateType.YoungLizard)
            {
                self.LizardState.health = Mathf.Min(0.5f, self.LizardState.health + 0.001f);
            }
            if (self.grabbedBy?.Count > 0 && self.grabbedBy[0]?.grabber is Player player && self.grabbedBy[0]?.grabber is not null)
            {
                if (self.Template?.type == CreatureTemplateType.YoungLizard)
                {
                    self.grabbedAttackCounter = 0;
                    self.JawOpen = 0;
                }
                if ((self.Template?.type == CreatureTemplateType.MotherLizard || 
                     self.GetLiz().IsRideable) && 
                    self.AI?.LikeOfPlayer(self.AI?.tracker?.RepresentationForCreature(player?.abstractCreature, true)) > 0)
                {
                    self.grabbedAttackCounter = 0;
                    self.JawOpen = 0;
                }
            }
        }
        catch (Exception e) { Debug.Log("Solace: Exception happened in Lizard.Update GeneralLizardCode " + e); }
        if (self.GetLiz() != null && self?.GetLiz()?.rider != null)
        {
            if (self?.graphicsModule != null) (self?.graphicsModule as LizardGraphics)?.BringSpritesToFront();
            try
            {
                if (self?.room?.GetTile(self.firstChunk.pos)?.Solid == true)
                {
                    if (self?.firstChunk?.collideWithTerrain == true && self?.room?.GetTile(self.firstChunk.lastPos)?.Solid == true && self?.room?.GetTile(self.firstChunk.lastLastPos)?.Solid == true && self.GetLiz().lastOutsideTerrainPos.HasValue)
                    {
                        Debug.Log("Solace: Resetting ridden lizard to outside terrain");
                        for (int i = 0; i < self?.bodyChunks?.Length; i++)
                        {
                            self.bodyChunks[i].HardSetPosition(self.GetLiz().lastOutsideTerrainPos.Value + Custom.RNV() * Random.value);
                            self.bodyChunks[i].vel /= 2f;
                        }
                    }
                }
                else if (self != null) self.GetLiz().lastOutsideTerrainPos = self.firstChunk.pos;
            }
            catch (Exception e) { Debug.Log("Solace: Exception happened in Lizard.Update LizardRideTerrainPositionReset" + e); }
        }
        else if (self?.Template?.type == CreatureTemplateType.MotherLizard) for (int i = 0; i < self?.bodyChunks?.Length; i++) self.bodyChunks[i].mass = 10;
        try
        {
            if (self?.Template?.type == CreatureTemplateType.MotherLizard && self?.room != null)
            {
                foreach (UpdatableAndDeletable update in self?.room?.updateList)
                {
                    if (update is WormGrass grass)
                    {
                        if (grass?.repulsiveObjects?.Contains(self) != true) grass.AddNewRepulsiveObject(self);
                    }
                }
            }
        }
        catch (Exception e) { Debug.Log("Solace: Exception happened in Lizard.Update WormGrassRepulse " + e); }
    }
    #endregion
    #region misc data
    public static void LizardAI_SocialEvent(On.LizardAI.orig_SocialEvent orig, LizardAI self, SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
    {
        if (self.lizard.GetLiz().rider != null && subjectCrit is Player pl && pl?.GetGeneral()?.dragonSteed == self.lizard) return;
        else orig(self, ID, subjectCrit, objectCrit, involvedItem);
    }
    public static void Creature_NewRoom(On.Creature.orig_NewRoom orig, Creature self, Room newRoom)
    {
        orig(self, newRoom);
        if (self is Lizard liz && liz.GetLiz() != null) liz.GetLiz().lastOutsideTerrainPos = null;
    }
    public static bool Player_IsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabCheck)
    {
        return grabCheck is Lizard liz && (liz.Template.type == CreatureTemplateType.YoungLizard || liz.Template.type == CreatureTemplateType.MotherLizard) || orig(self, grabCheck);
    }
    public static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        if (self is Player && obj is Lizard liz && (liz.Template.type == CreatureTemplateType.YoungLizard || liz.Template.type == CreatureTemplateType.MotherLizard))
        {
            shareability = Creature.Grasp.Shareability.CanNotShare;
            pacifying = false;
        }
        return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
    }
    public static bool WormGrassPatch_AlreadyTrackingCreature(On.WormGrass.WormGrassPatch.orig_AlreadyTrackingCreature orig, WormGrass.WormGrassPatch self, Creature creature)
    {
        for (int i = 0; i < self.trackedCreatures.Count; i++)
        {
            try
            {
                if (creature is Player player && player?.GetGeneral()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard)
                    return true;
                else if (creature is Player player0 && player0?.GetGeneral()?.dragonSteed?.Template?.wormGrassImmune == true)
                    return true;
            }
            catch (Exception e) { Debug.Log("Solace: Exception occurred in WormGrassPatch.AlreadyTrackingCreature playerCode " + e); }
        }
        return orig(self, creature);
    }
    public static void WormGrassPatch_InteractWithCreature(On.WormGrass.WormGrassPatch.orig_InteractWithCreature orig, WormGrass.WormGrassPatch self, WormGrass.WormGrassPatch.CreatureAndPull creatureAndPull)
    {
        if (!(creatureAndPull?.creature is Player player && (player?.GetGeneral()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard || player?.GetGeneral()?.dragonSteed?.Template?.wormGrassImmune == true)) ||
            !(creatureAndPull?.creature?.grabbedBy?.Count > 0 && creatureAndPull?.creature?.grabbedBy[0]?.grabber is Player pl && (pl?.GetGeneral()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard || player?.GetGeneral()?.dragonSteed?.Template?.wormGrassImmune == true)))
            orig(self, creatureAndPull);
    }
    public static void WormGrassPatch_Update(On.WormGrass.WormGrassPatch.orig_Update orig, WormGrass.WormGrassPatch self)
    {
        orig(self);
        for (int i = 0; i < self.trackedCreatures.Count; i++)
        {
            Creature crit = self?.trackedCreatures[i]?.creature;
            try
            {
                if (self?.trackedCreatures[i]?.creature is Player player)
                {
                    if (player?.GetGeneral()?.isRidingLizard == true && (player?.GetGeneral()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard || player?.GetGeneral()?.dragonSteed?.Template?.wormGrassImmune == true)) self.trackedCreatures.RemoveAt(i);
                }
            }
            catch (Exception e) { Debug.Log("Solace: Exception occurred in WormGrassPatch.Update playerCode " + e); }
            try
            {
                if (crit is not null && crit?.grabbedBy?.Count > 0 && self?.trackedCreatures[i]?.creature?.grabbedBy[0]?.grabber is Player pl)
                {
                    if (pl?.GetGeneral()?.dragonSteed != null && (pl?.GetGeneral()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard || pl?.GetGeneral()?.dragonSteed?.Template?.wormGrassImmune == true)) self.trackedCreatures.RemoveAt(i);
                }
                /*else if (crit is not null && crit?.abstractCreature?.stuckObjects?.Count > 0 && crit?.abstractCreature?.stuckObjects[0]?.B?.realizedObject is Player pla)
                {
                    if (pla?.GetPoacher()?.dragonSteed != null && (pla?.GetPoacher()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard || pla?.GetPoacher()?.dragonSteed?.Template?.wormGrassImmune == true)) self.trackedCreatures.RemoveAt(i);
                }
                else if (crit is not null && crit?.abstractCreature?.stuckObjects?.Count > 0 && crit?.abstractCreature?.stuckObjects[0]?.A?.realizedObject is Player plr)
                {
                    if (plr?.GetPoacher()?.dragonSteed != null && (plr?.GetPoacher()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard || plr?.GetPoacher()?.dragonSteed?.Template?.wormGrassImmune == true)) self.trackedCreatures.RemoveAt(i);
                }*/
            }
            catch (Exception e) { Debug.Log("Solace: Exception occurred in WormGrassPatch.Update itemCode " + e); }
        }
    }
    #endregion
    #region lizard cosmetics
    public static void BodyScales_GeneratePatchPattern(On.LizardCosmetics.BodyScales.orig_GeneratePatchPattern orig, BodyScales self, float startPoint, int numOfScales, float maxLength, float lengthExponent)
    {
        if (self.lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            self.GenerateTwoLines(0.07f, 1f, 1.5f, 3f);
            Debug.Log("Solace: Patch scales got WRECKED, SON!");
            return;
        }
        else orig(self, startPoint, numOfScales, maxLength, lengthExponent);
        if (self.lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard) Debug.Log("Solace: My hook was no match. Patch scales not destroyed...");
    }
    public static void BodyScales_GenerateSegments(On.LizardCosmetics.BodyScales.orig_GenerateSegments orig, BodyScales self, float startPoint, float maxLength, float lengthExponent)
    {
        if (self.lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            self.GenerateTwoLines(0.07f, 1f, 1.5f, 3f);
            Debug.Log("Solace: Segment scales got WRECKED, SON!");
            return;
        }
        else orig(self, startPoint, maxLength, lengthExponent);
        if (self.lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard) Debug.Log("Solace: My hook was no match. Segment scales not destroyed...");
    }
    public static void BumpHawk_ctor(On.LizardCosmetics.BumpHawk.orig_ctor orig, BumpHawk self, LizardGraphics lGraphics, int startSprite)
    {
        if (lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            self.numberOfSprites = 0;
        }
    }
    public static void ShortBodyScales_ctor(On.LizardCosmetics.ShortBodyScales.orig_ctor orig, ShortBodyScales self, LizardGraphics lGraphics, int startSprite)
    {
        orig(self, lGraphics, startSprite);
        if (lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            Array.Resize(ref self.scalesPositions, self.scalesPositions.Length - self.numberOfSprites);
            self.numberOfSprites = 0;
        }
    }
    public static void LongHeadScales_ctor(On.LizardCosmetics.LongHeadScales.orig_ctor orig, LongHeadScales self, LizardGraphics lGraphics, int startSprite)
    {
        orig(self, lGraphics, startSprite);
        if (lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            Array.Resize(ref self.scaleObjects, self.scaleObjects.Length - self.scalesPositions.Length);
            Array.Resize(ref self.scalesPositions, self.scalesPositions.Length - (self.colored ? self.numberOfSprites / 2 : self.numberOfSprites));
            self.numberOfSprites = 0;
        }
    }
    public static void LongBodyScales_DrawSprites(On.LizardCosmetics.LongBodyScales.orig_DrawSprites orig, LongBodyScales self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics.lizard.Template.type == CreatureTemplateType.MotherLizard && self is LongShoulderScales)
        {
            var x = 1.5f;
            for (int i = self.startSprite + self.scalesPositions.Length - 1; i >= self.startSprite; i--)
            {
                sLeaser.sprites[i].scaleX *= x;
                if (self.colored) sLeaser.sprites[i + self.scalesPositions.Length].scaleX *= x;
            }
        }
    }
    public static void LizardGraphics_ctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (self.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            bool rCol = Random.value > 0.5f ? true : false;
            var state = Random.state;
            Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
            float r1 = Random.value;
            var num = self.startOfExtraSprites + self.extraSprites;

            var shoulder = new LongShoulderScales(self, num);
            shoulder.graphic = r1 > 0.5f ? 6 : 2;
            shoulder.graphicHeight /= r1 > 0.5f ? 3.5f : 2f;
            shoulder.numberOfSprites = rCol == true ? shoulder.scalesPositions.Length * 2 : shoulder.scalesPositions.Length;
            shoulder.colored = rCol == true ? true : false;
            shoulder.rigor = 10f;
            num = self.AddCosmetic(num, shoulder);

            var scale = new SpineSpikes(self, num);
            scale.graphic = r1 > 0.5f ? 6 : 2;
            scale.sizeRangeMin = r1 > 0.5f ? 0.6f : 0.8f;
            scale.sizeRangeMax = r1 > 0.5f ? 2.6f : 3f;
            scale.spineLength = Mathf.Lerp(0.7f, 0.95f, Random.value) * self.BodyAndTailLength;
            scale.sizeSkewExponent = Random.value;
            if (scale.bumps > 15) scale.bumps = 15;
            scale.colored = rCol == true ? 1 : 0;
            scale.numberOfSprites = scale.colored > 0 ? scale.bumps * 2 : scale.bumps;
            num = self.AddCosmetic(num, scale);
            Random.state = state;
        }
        if (self.lizard.Template.type == CreatureTemplateType.YoungLizard)
        {
            var state = Random.state;
            Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
            var num = self.startOfExtraSprites + self.extraSprites;
            if (Random.value < 0.2f)
                num = self.AddCosmetic(num, new TailTuft(self, num));

            Random.state = state;
        }
    }
    public static Color LizardGraphics_DynamicBodyColor(On.LizardGraphics.orig_DynamicBodyColor orig, LizardGraphics self, float f)
    {
        orig(self, f);
        if (self.lizard.Template.type == CreatureTemplateType.YoungLizard)
        {
            return Color.Lerp(self.lizard.effectColor, Color.white, 0.5f);
        }
        else return orig(self, f);
    }
    public static Color LizardGraphics_BodyColor(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
    {
        orig(self, f);
        if (self.lizard.Template.type == CreatureTemplateType.YoungLizard)
        {
            return Color.Lerp(self.lizard.effectColor, Color.white, 0.5f);
        }
        else return orig(self, f);
    }
    public static void LizardGraphics_ApplyPalette(On.LizardGraphics.orig_ApplyPalette orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        self.palette = palette;
        if (self.lizard.Template.type == CreatureTemplateType.YoungLizard)
        {
            self.ColorBody(sLeaser, Color.Lerp(self.lizard.effectColor, Color.white, 0.8f));
        }
    }
    public static void LizardGraphics_InitiateSprites(On.LizardGraphics.orig_InitiateSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
            if (self.lizard.GetLiz() != null) self.lizard.GetLiz().hybridHead = sLeaser.sprites.Length - 1;
            if (self.lizard.GetLiz() != null) sLeaser.sprites[self.lizard.GetLiz().hybridHead] = new FSprite("LizardHead3.0");
            self.AddToContainer(sLeaser, rCam, null);
        }
    }
    public static void LizardGraphics_AddToContainer(On.LizardGraphics.orig_AddToContainer orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);
        if (self.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            if (self.lizard.GetLiz() != null && self.lizard.GetLiz().hybridHead < sLeaser.sprites.Length)
            {
                if (newContainer == null) newContainer = sLeaser.sprites[self.SpriteHeadStart].container;
                newContainer.AddChild(sLeaser.sprites[self.lizard.GetLiz().hybridHead]);
            }
        }
    }
    public static void LizardGraphics_DrawSprites(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lizard.Template.type == CreatureTemplateType.MotherLizard)
        {
            sLeaser.sprites[self.SpriteHeadStart + 4].color = self.effectColor; // Eyes
            sLeaser.sprites[self.SpriteHeadStart + 1].color = self.effectColor; // Bottom teeth
            sLeaser.sprites[self.SpriteHeadStart + 2].color = self.effectColor; // Top teeth
            sLeaser.sprites[self.SpriteHeadStart].color = self.palette.blackColor; // Bottom jaw
            sLeaser.sprites[self.SpriteHeadStart + 3].color = self.palette.blackColor; // Top jaw

            // Custom head
            if (self.lizard.GetLiz() != null)
            {
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].color = sLeaser.sprites[self.SpriteHeadStart + 3].color;
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].scaleX = sLeaser.sprites[self.SpriteHeadStart + 3].scaleX;
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].scaleY = sLeaser.sprites[self.SpriteHeadStart + 3].scaleY * 0.6f;
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].rotation = sLeaser.sprites[self.SpriteHeadStart + 3].rotation;
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].SetPosition(sLeaser.sprites[self.SpriteHeadStart + 3].GetPosition());
                sLeaser.sprites[self.lizard.GetLiz().hybridHead].MoveBehindOtherNode(sLeaser.sprites[self.SpriteHeadStart]);

                switch (sLeaser.sprites[self.SpriteHeadStart + 3].element.name)
                {
                    case "LizardHead0.5":
                        sLeaser.sprites[self.lizard.GetLiz().hybridHead].element = Futile.atlasManager.GetElementWithName("LizardHead0.0");
                        break;
                    case "LizardHead1.5":
                        sLeaser.sprites[self.lizard.GetLiz().hybridHead].element = Futile.atlasManager.GetElementWithName("LizardHead1.0");
                        break;
                    case "LizardHead2.5":
                        sLeaser.sprites[self.lizard.GetLiz().hybridHead].element = Futile.atlasManager.GetElementWithName("LizardHead2.0");
                        break;
                    case "LizardHead3.5":
                        sLeaser.sprites[self.lizard.GetLiz().hybridHead].element = Futile.atlasManager.GetElementWithName("LizardHead3.0");
                        break;
                }
            }
        }
    }
    #endregion
}
