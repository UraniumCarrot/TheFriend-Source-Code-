using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using MoreSlugcats;

namespace TheFriend;

public class DragonRiding
{
    public class AbstractDragonRider : AbstractPhysicalObject.AbstractObjectStick
    {
        public AbstractPhysicalObject self
        {
            get { return A; }
            set { A = value; }
        }
        public AbstractPhysicalObject liz
        {
            get { return B; }
            set { B = value; }
        }
        public AbstractDragonRider(AbstractPhysicalObject self, AbstractPhysicalObject liz) : base(self, liz) { }
    }
    public static void DragonRiderSafety(Player self, Creature crit, Vector2 seat) // Values for the rider of mother lizard
    {
        if (!(crit as Lizard).GetLiz().IsRideable && (crit as Lizard).GetLiz() != null) { DragonRideReset(crit, self); return; }
        if (self?.GetPoacher()?.rideStick == null) self.GetPoacher().rideStick = new AbstractDragonRider(self.abstractPhysicalObject,crit.abstractPhysicalObject);
        self.GetPoacher().isRidingLizard = true;
        self.GetPoacher().grabCounter = 15;
        self.bodyChunks[1].pos = seat;
        self.bodyChunks[0].pos = Vector2.Lerp(seat,seat + new Vector2(0,crit.firstChunk.rad),0.5f);
        self.CollideWithTerrain = false;
        self.CollideWithObjects = false;
        if (!self.abstractCreature.stuckObjects.Contains(self?.GetPoacher()?.rideStick)) self.abstractCreature.stuckObjects.Add(self?.GetPoacher()?.rideStick);
    }
    public static void DragonRidden(Creature crit, Player player) // Values for the lizard being ridden
    {
        var self = crit as Lizard;
        if (!self.GetLiz().IsRideable) { DragonRideReset(crit,player); return; }
        self.GetLiz().IsBeingRidden = true;
        self.abstractCreature.controlled = true;
    }
    public static void DragonRideReset(Creature crit, Player player) // Performed after riding stops
    {
        player.CollideWithTerrain = true;
        player.CollideWithObjects = true;
        player.GetPoacher().dragonSteed = null;
        if ((crit as Lizard).GetLiz != null)
        {
            (crit as Lizard).GetLiz().boolseat0 = false;
            (crit as Lizard).GetLiz().IsBeingRidden = false;
            (crit as Lizard).GetLiz().rider = null;
        }
        player.GetPoacher().isRidingLizard = false;
        crit.abstractCreature.controlled = false;
        if (player?.GetPoacher()?.rideStick != null)
        {
            player.GetPoacher().rideStick.Deactivate();
            player.GetPoacher().rideStick = null;
            player.abstractCreature.stuckObjects.Remove(player?.GetPoacher()?.rideStick);
        }

    }
}
