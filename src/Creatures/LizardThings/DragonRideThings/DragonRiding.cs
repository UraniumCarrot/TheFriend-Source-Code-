using System;
using System.Linq;
using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheFriend.Creatures.LizardThings.DragonRideThings;

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
    public static void DragonRiderSafety(Player self, Creature crit, Vector2 seat, bool eu)
    {
        if (crit.TryGetLiz(out var data)) 
            if (!data.DoILikeYou(self)) { DragonRideReset(crit, self); return; }

        for (int i = 0; i < self.grasps?.Length; i++)
            if (self.grasps[i]?.grabbed == crit) self.grasps[i].Release();
        
        self.GetGeneral().rideStick ??= new AbstractDragonRider(self.abstractPhysicalObject, crit.abstractPhysicalObject); // Make player and lizard go through rooms in sync
        self.GetGeneral().isRidingLizard = true;
        
        if (!self.standing && self.animation != Player.AnimationIndex.None && self.bodyMode != Player.BodyModeIndex.Stand) self.Jump();
        self.animation = Player.AnimationIndex.None;
        self.bodyMode = Player.BodyModeIndex.Stand;
        self.standing = true; // Make player unable to crouch because it would look weird
        
        foreach (BodyChunk chunk in self.bodyChunks) chunk.vel = new Vector2(0f, 1f); // Fix fucked up player physics
        self.bodyChunks[1].HardSetPosition(seat); // Set player's position to the seat with the same index as them
        self.bodyChunks[0].HardSetPosition(new Vector2(self.bodyChunks[1].lastPos.x,self.bodyChunks[1].lastPos.y + self.bodyChunkConnections[0].distance));

        //var connectedChunk = data.self.bodyChunks[data.seats.IndexOf(seat)];
        
        self.CollideWithTerrain = false; // Prevent terrain and objects from messing with riders
        self.CollideWithObjects = false;
        
        if (!self.abstractCreature.stuckObjects.Contains(self.GetGeneral()?.rideStick)) 
            self.abstractCreature.stuckObjects.Add(self.GetGeneral()?.rideStick);
        DragonRiderAddItems(self, self.GetGeneral().dragonSteed);
    }
    public static void DragonRideReset(Creature crit, Player player) // Performed after riding stops
    {
        player.CollideWithTerrain = true;
        player.CollideWithObjects = true;
        player.GetGeneral().dragonSteed = null;
        if (crit.TryGetLiz(out var data))
        {
            data.mainRiders.Remove(player);
            DragonRiderRemoveItems(player, data.self);
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
                for (int i = 2; i < input.Length - 1; i++)
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

    public static void DragonRideTerrainReset(Lizard self)
    { // Prevents lizards and players from falling outside of the map during lizard ride. It may surprise you how important this is
        if (self.room.GetTile(self.firstChunk.pos)?.Solid == true)
        {
            if (self.firstChunk?.collideWithTerrain == true && 
                self.room.GetTile(self.firstChunk.lastPos)?.Solid == true && 
                self.room.GetTile(self.firstChunk.lastLastPos)?.Solid == true && 
                self.Liz().lastOutsideTerrainPos.HasValue)
            {
                Debug.Log("Solace: Resetting ridden lizard to outside terrain");
                for (int i = 0; i < self.bodyChunks?.Length; i++)
                {
                    self.bodyChunks[i].HardSetPosition(self.Liz().lastOutsideTerrainPos.Value + Custom.RNV() * Random.value);
                    self.bodyChunks[i].vel /= 2f;
                }
            }
        }
        else self.Liz().lastOutsideTerrainPos = self.firstChunk.pos;
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
            if (self.grasps[i]?.grabbed is Spear && self.grasps[0]?.grabbed != self.grasps[1]?.grabbed) hand = i;
        
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
                if (self.grasps[i] != null && self.grasps[i]?.grabbed != null && self.grasps[i]?.grabbed is Spear wep)
                {
                    float rotation = i == 1 ? self.GetGeneral().pointDir1 + 90 : self.GetGeneral().pointDir0 + 90f;
                    Vector2 vec = Custom.DegToVec(rotation);
                    (wep).setRotation = vec;
                    (wep).rotationSpeed = 0f;
                }
            }
        }
    }

    public static void DragonRiderAddItems(Player self, Lizard mount)
    {
        var data = mount.Liz();

        foreach (AbstractPhysicalObject obj in self.abstractCreature.GetAllConnectedObjects())
            if (obj is AbstractCreature crit)
                data.riderFriends.Add(crit);
    }

    public static void DragonRiderRemoveItems(Player self, Lizard mount)
    {
        var data = mount.Liz();

        foreach (AbstractPhysicalObject obj in self.abstractCreature.GetAllConnectedObjects())
                if (obj is AbstractCreature crit)
                    data.riderFriends.Remove(crit);
    }
}
