using UnityEngine;
using MoreSlugcats;

namespace TheFriend.Creatures.LizardThings;

public class YoungLizardMethods
{
    public static CreatureTemplate YoungLizardStats(CreatureTemplate temp, LizardBreedParams breedParams)
    {
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

    public static CreatureTemplate.Relationship YoungLizardDynamicRelations(LizardAI self, CreatureTemplate.Relationship relationship, Creature trackedcreature)
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

}