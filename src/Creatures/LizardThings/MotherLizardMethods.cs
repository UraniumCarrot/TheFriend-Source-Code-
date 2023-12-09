using UnityEngine;
using MoreSlugcats;

namespace TheFriend.Creatures.LizardThings;

public class MotherLizardMethods
{
    public static CreatureTemplate MotherLizardStats(CreatureTemplate temp, LizardBreedParams breedParams)
    {
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
            temp.damageRestistances[(int)Creature.DamageType.Explosion, 0] = 0.2f;
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

    public static CreatureTemplate.Relationship MotherLizardDynamicRelations(LizardAI self, CreatureTemplate.Relationship relationship, Creature trackedcreature)
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
}