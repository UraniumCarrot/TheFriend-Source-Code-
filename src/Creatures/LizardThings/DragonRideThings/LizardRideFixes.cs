using System;
using System.Linq;
using TheFriend.SlugcatThings;
using UnityEngine;
using RWCustom;

namespace TheFriend.Creatures.LizardThings.DragonRideThings;

public class LizardRideFixes
{
    public static void Apply()
    {
        WormGrassImmunizer.Apply();
        
        On.LizardAI.SocialEvent += LizardAI_SocialEvent;
        On.Weapon.HitThisObject += Weapon_HitThisObject;
        On.Creature.NewRoom += Creature_NewRoom;
        On.Creature.SpitOutOfShortCut += CreatureOnSpitOutOfShortCut;
        On.Player.IsCreatureLegalToHoldWithoutStun += Player_IsCreatureLegalToHoldWithoutStun;
        On.Creature.Grab += Creature_Grab;
        On.Player.GraphicsModuleUpdated += Player_GraphicsModuleUpdated;
        On.Player.ReleaseGrasp += PlayerOnReleaseGrasp;
    }

    public static void PlayerOnReleaseGrasp(On.Player.orig_ReleaseGrasp orig, Player self, int grasp)
    { // Explode living creatures from riderFriends if theyve been thrown or dropped
        if (self.GetGeneral().dragonSteed != null)
            if (self.grasps[grasp].grabbed.abstractPhysicalObject is AbstractCreature crit &&
                self.GetGeneral().dragonSteed.Liz().riderFriends.Contains(crit)) 
                self.GetGeneral().dragonSteed.Liz().riderFriends.Remove(crit);
        orig(self, grasp);
    }

    public static void LizardAI_SocialEvent(On.LizardAI.orig_SocialEvent orig, LizardAI self, SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
    { // Lizard mount won't care about anything their riders throw
        if (self.TryGetLiz(out var data) && data.mainRiders.Contains(subjectCrit)) return;
        orig(self, ID, subjectCrit, objectCrit, involvedItem);
    }
    public static bool Weapon_HitThisObject(On.Weapon.orig_HitThisObject orig, Weapon self, PhysicalObject obj)
    { // Lizard mount will not be hit by rider's weapons
        if (obj is Lizard liz &&
            liz.Liz().mainRiders.Contains(self.thrownBy)) 
            return false;
        else if (self.thrownBy is Player pl && obj is Creature crit && pl.GetGeneral().dragonSteed != null)
        { // Stop players from screwing eachother over if riding a lizard together
            if (pl.GetGeneral().dragonSteed.Liz().mainRiders.Contains(crit)) return false;
            if (pl.GetGeneral().dragonSteed.Liz().riderFriends.Contains(crit.abstractPhysicalObject)) return false;
            return orig(self, obj);
        }
        return orig(self, obj);
    }
    public static void Player_GraphicsModuleUpdated(On.Player.orig_GraphicsModuleUpdated orig, Player self, bool actuallyViewed, bool eu)
    { // Spear pointing while riding a lizard
        orig(self, actuallyViewed, eu);
        if (self == null) return;
        try
        {
            if (self.GetGeneral().dragonSteed != null && self.GetGeneral().isRidingLizard)
                for (int i = 0; i < 2; i++)
                    if (self.grasps[i] != null && self.grasps[i]?.grabbed != null && self.grasps[i]?.grabbed is Spear spr)
                    {
                        float rotation = (i == 1) ? self.GetGeneral().pointDir1 + 90 : self.GetGeneral().pointDir0 + 90f;
                        Vector2 vec = Custom.DegToVec(rotation);
                        spr.setRotation = vec; //new Vector2(self.input[0].x*10, self.input[0].y*10);
                        spr.rotationSpeed = 0f;
                    }
        }
        catch (Exception e) { Debug.Log("Solace: Exception occurred in Player.GraphicsModuleUpdated" + e); }
    }
    public static void Creature_NewRoom(On.Creature.orig_NewRoom orig, Creature self, Room newRoom)
    {
        orig(self, newRoom);
        if (self.TryGetLiz(out var data))
        {
            data.lastOutsideTerrainPos = null; // Little thing for fixing up lastOutsideTerrainPos
            
            if (data.mainRiders.Count > 0) // Fixes players drawing in front of their lizard mount - Moving between rooms
                if (self.graphicsModule != null) (self.graphicsModule as LizardGraphics)?.BringSpritesToFront();
        }
    }
    public static void CreatureOnSpitOutOfShortCut(On.Creature.orig_SpitOutOfShortCut orig, Creature self, IntVector2 pos, Room newroom, bool spitoutallsticks)
    { // Fix shortcut stuff
        orig(self, pos, newroom, spitoutallsticks);
        if (self.TryGetLiz(out var data))
        {
            data.lastOutsideTerrainPos = null; // Little thing for fixing up lastOutsideTerrainPos
            
            if (data.mainRiders.Count > 0) // Fixes players drawing in front of their lizard mount - Moving through a room
                if (self.graphicsModule != null) (self.graphicsModule as LizardGraphics)?.BringSpritesToFront();
        }
    }
    public static bool Player_IsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabCheck)
    { // Don't break lizard brains when grabbed if tamed
        return grabCheck.TryGetLiz(out var data) && 
               (data.myTemplate.type == CreatureTemplateType.YoungLizard || data.DoILikeYou(self)) 
               || orig(self, grabCheck);
    }
    public static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    { // Don't break lizard brains when grabbed if tamed
        if (self is Player && 
            obj.TryGetLiz(out var data) && 
            (data.myTemplate.type == CreatureTemplateType.YoungLizard || 
             data.DoILikeYou(self as Player)))
        {
            shareability = Creature.Grasp.Shareability.CanNotShare;
            pacifying = false;
        }
        return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
    }
    public static Player.ObjectGrabability LizardGrabability(Player self, Lizard liz)
    { // Makes it possible to grab tamed lizards
        if (liz.Template.type == CreatureTemplateType.YoungLizard)
        {
            for (int i = 0; i < self?.grasps?.Length; i++) 
                if ((self.grasps[i]?.grabbed as Creature)?.Template?.type == CreatureTemplateType.YoungLizard) 
                    return Player.ObjectGrabability.CantGrab; // If already holding a young lizard, you can't grab a second one
            return Player.ObjectGrabability.OneHand;
        }
        else if (liz.TryGetLiz(out var data))
        {
            if ((data.mainRiders.Count >= data.seats.Count || // Are there too many riders?
                 !data.DoILikeYou(self)) || // Does the lizard like you?
                data.mainRiders.Contains(self)) // Are you already riding the lizard?
                return Player.ObjectGrabability.TwoHands; // Secretly, this is a return to orig.
            return Player.ObjectGrabability.OneHand;
        }
        return Player.ObjectGrabability.TwoHands;
    }
    public static void LizardNoBiting(Lizard self)
    { // Cure for being bitten by friendly lizards when you're holding them
        self.grabbedAttackCounter = 0;
        self.JawOpen = 0;
    }
    
    public static void LizardRideabilityAndSeats(Lizard self)
    { // Initialize lizard's seats and rideability when their bodychunks won't be returning null
        if (self.Liz().seats.Count > 0 || !self.Liz().RideEnabled) return;
        float length = 0;
        foreach (PhysicalObject.BodyChunkConnection connection in self.bodyChunkConnections)
            if (connection.chunk1.index == 0 && connection.chunk2.index == self.bodyChunks.Length-1) { }
            else length += connection.distance;
        
        int seatsLength = Mathf.FloorToInt((length-5) / 10f); 
        // change the number Length is divided by to change how many seats generate
        // the bigger the number, the less seats there will be

        if (seatsLength < 1)
        {
            Debug.Log("Solace: This lizard is too small to be rideable, no seats generated");
            self.Liz().RideEnabled = false;
            return;
        }

        for (int a = 0; a < seatsLength; a++)
        { // For the love of all that is holy do NOT touch the math here anymore
            float point = seatsLength > 1 ? (length * a / (seatsLength - 1f)) : (length / 2f);
            for (int i = 0; i < self.bodyChunks.Length - 1; i++)
            {
                var connectionLength = self.bodyChunkConnections[i].distance;
                if (point < connectionLength)
                {
                    var seat = new DragonRiderSeat(self,
                        self.bodyChunks[i],
                        self.bodyChunks[i + 1],
                        point / connectionLength);
                    self.Liz().seats.Add(seat);
                    // self.room.AddObject(seat); - Uncomment if testing must be done
                    break;
                }
                else point -= self.bodyChunkConnections[i].distance;
                Debug.Log($"i={i},a={a},point={point},connLength={connectionLength},point={point},seatsLength={seatsLength},length={length}");
            }
        }
        Debug.Log($"Solace: Seats made for rideable lizard, created {self.Liz().seats.Count}");
    }
}