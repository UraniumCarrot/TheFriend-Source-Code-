using UnityEngine;

namespace TheFriend.Creatures.LizardThings.PilgrimLizard;

public class PilgrimLizardMethods
{
    public static CreatureTemplate PilgrimLizardStats(CreatureTemplate temp, LizardBreedParams breedParams)
    {
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
    public static void PilgrimLizardGraphicsCtor(LizardGraphics self, PhysicalObject ow)
    {
        
    }
}