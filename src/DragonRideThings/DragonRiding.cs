using System;
using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.DragonRideThings;

public class DragonRiding
{
    public class AbstractDragonRider : AbstractPhysicalObject.AbstractObjectStick
    {
        public AbstractPhysicalObject self
        {
            get => A;
            set => A = value;
        }
        public AbstractPhysicalObject obj
        {
            get => B;
            set => B = value;
        }
        public AbstractDragonRider(AbstractPhysicalObject self, AbstractPhysicalObject obj) : base(self, obj) { }
    }
    public static void DragonRiderSafety(Player self, Creature crit, Vector2 seat) // Values for the rider of mother lizard
    {
        if (!(crit as Lizard).GetLiz().IsRideable && (crit as Lizard).GetLiz() != null) { DragonRideReset(crit, self); return; }
        self.GetGeneral().rideStick ??= new AbstractDragonRider(self.abstractPhysicalObject, crit.abstractPhysicalObject);
        self.GetGeneral().isRidingLizard = true;
        self.GetGeneral().grabCounter = 15;
        self.bodyChunks[1].pos = seat;
        self.bodyChunks[0].pos = Vector2.Lerp(seat,seat + new Vector2(0,crit.firstChunk.rad),0.5f);
        self.CollideWithTerrain = false;
        self.CollideWithObjects = false;
        if (!self.abstractCreature.stuckObjects.Contains(self.GetGeneral()?.rideStick)) self.abstractCreature.stuckObjects.Add(self.GetGeneral()?.rideStick);
    }
    public static void DragonRidden(Creature crit, Player player) // Values for the lizard being ridden
    {
        var self = crit as Lizard;
        if (!self.GetLiz().IsRideable) { DragonRideReset(crit,player); }
        //self.GetLiz().IsBeingRidden = true;
    }
    public static void DragonRideReset(Creature crit, Player player) // Performed after riding stops
    {
        player.CollideWithTerrain = true;
        player.CollideWithObjects = true;
        player.GetGeneral().dragonSteed = null;
        if (crit is Lizard liz)
        {
            liz.GetLiz().boolseat0 = false;
            //liz.GetLiz().IsBeingRidden = false;
            liz.GetLiz().rider = null;
        }
        player.GetGeneral().isRidingLizard = false;
        if (player.GetGeneral()?.rideStick != null)
        {
            player.GetGeneral().rideStick.Deactivate();
            player.GetGeneral().rideStick = null;
            player.abstractCreature.stuckObjects.Remove(player.GetGeneral().rideStick);
        }
    }

    public static void DragonRideCommands(Lizard liz, Player rider)
    {
        var input = rider.GetGeneral().UnchangedInputForLizRide;

        // Drop it!
        if (input[0].y < 0)
        {
            if (!(input[1].y < 0))
            {
                for (int i = 2; i < input.Length - 1; i++)
                {
                    if (input[i].y < 0 && !(input[i + 1].y < 0))
                    {
                        liz.ReleaseGrasp(0);
                        liz.voice.MakeSound(LizardVoice.Emotion.Submission);
                        rider.Blink(12);
                        rider.room.PlaySound(SoundID.Vulture_Grab_Player, rider.firstChunk.pos,0.5f,1);
                        rider.room.AddObject(new ExplosionSpikes(rider.room, rider.bodyChunks[1].pos + new Vector2(0.0f, -rider.bodyChunks[1].rad), 8, 7f, 5f, 5.5f, 40f, new Color(1f, 1f, 1f, 0.5f)));
                    }
                }
            }
        }
    }

    public static void DragonRiderPoint(Player self)
    {
        var oldinput = self.GetGeneral().UnchangedInputForLizRide;
        Vector2 pointPos = new Vector2(oldinput[0].x * 50, oldinput[0].y * 50) + self.bodyChunks[0].pos;
        var graph = self.graphicsModule as PlayerGraphics;
        var hand = ((pointPos - self.mainBodyChunk.pos).x < 0 || self.grasps[0]?.grabbed is Spear) ? 0 : 1;
        if (self.grasps[1]?.grabbed is Spear) hand = 1;
        var nothand = (hand == 1) ? 0 : 1;

        for (int i = 0; i < 2; i++)
        {
            if (self.grasps[i]?.grabbed is Spear && self.grasps[0]?.grabbed != self.grasps[1]?.grabbed) hand = i;
        }
        try
        {
            if (graph == null) return;
            graph.LookAtPoint(pointPos, 0f);
            graph.hands[hand].absoluteHuntPos = pointPos;
            if (self.GetGeneral().dragonSteed != null) graph.hands[nothand].absoluteHuntPos = self.GetGeneral().dragonSteed.firstChunk.pos;
            graph.hands[hand].reachingForObject = true;
            graph.hands[nothand].reachingForObject = true;
        }
        catch (Exception) { Debug.Log("Solace: Harmless exception happened in Player.Update riderHand"); }
    }

    public static void DragonRiderSpearPoint(Player self)
    { // Spear pointing specifically needed a fix so that the correct hand would be used
        if (self != null && self.GetGeneral().dragonSteed != null && self.GetGeneral().isRidingLizard)
        {
            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i] != null && self.grasps[i]?.grabbed != null && self.grasps[i]?.grabbed is Weapon wep)
                {
                    float rotation = i == 1 ? self.GetGeneral().pointDir1 + 90 : self.GetGeneral().pointDir0 + 90f;
                    Vector2 vec = Custom.DegToVec(rotation);
                    (wep).setRotation = vec;
                    (wep).rotationSpeed = 0f;
                }
            }
        }
    }

    public static Player.ObjectGrabability LizardGrabability(Player self, Lizard liz)
    {
        if (liz.Template.type != CreatureTemplateType.YoungLizard)
        {
            if (liz.GetLiz().IsRideable)
            {
                if (liz.Template?.type != CreatureTemplateType.MotherLizard && 
                    liz.AI?.DynamicRelationship(self?.abstractCreature).type != CreatureTemplate.Relationship.Type.Attacks && 
                    liz.AI?.DynamicRelationship(self?.abstractCreature).type != CreatureTemplate.Relationship.Type.Eats && 
                    liz.AI?.friendTracker?.friend != null && 
                    liz.AI?.friendTracker?.friendRel?.like < 0.5f && 
                    !liz.dead && 
                    !liz.Stunned) 
                    return Player.ObjectGrabability.CantGrab;
                if ((liz.GetLiz().rider != null || 
                     self.GetGeneral().grabCounter > 0 || 
                     liz.AI?.LikeOfPlayer(liz.AI?.tracker?.RepresentationForCreature(self?.abstractCreature, true)) < 0) && 
                    !liz.dead && 
                    !liz.Stunned) 
                    return Player.ObjectGrabability.CantGrab;
                self.GetGeneral().grabCounter = 15;
                return Player.ObjectGrabability.OneHand;
            }
        }
        else if (liz.Template.type == CreatureTemplateType.YoungLizard)
        {
            for (int i = 0; i < self?.grasps?.Length; i++) 
                if ((self.grasps[i]?.grabbed as Creature)?.Template?.type == CreatureTemplateType.YoungLizard) 
                    return Player.ObjectGrabability.CantGrab; // If already holding a young lizard, you can't grab a second one
            return Player.ObjectGrabability.OneHand;
        }
        return Player.ObjectGrabability.TwoHands;
    }
}
