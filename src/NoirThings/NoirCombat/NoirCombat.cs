using RWCustom;
using UnityEngine;

namespace TheFriend.NoirThings;

public partial class NoirCatto
{
    private static PhysicalObject PlayerOnPickupCandidate(On.Player.orig_PickupCandidate orig, Player self, float favorspears)
    {
        if (self.SlugCatClass != Plugin.NoirName) return orig(self, favorspears);

        for (var i = 0; i < self.room.physicalObjects.Length; i++)
        {
            for (var j = 0; j < self.room.physicalObjects[i].Count; j++)
            {
                if (Custom.DistLess(self.bodyChunks[0].pos, self.room.physicalObjects[i][j].bodyChunks[0].pos, self.room.physicalObjects[i][j].bodyChunks[0].rad + 40f)
                    && (Custom.DistLess(self.bodyChunks[0].pos, self.room.physicalObjects[i][j].bodyChunks[0].pos, self.room.physicalObjects[i][j].bodyChunks[0].rad + 20f)
                        || self.room.VisualContact(self.bodyChunks[0].pos, self.room.physicalObjects[i][j].bodyChunks[0].pos)) && self.CanIPickThisUp(self.room.physicalObjects[i][j]))
                {
                    if (self.room.physicalObjects[i][j] is Creature crit) //Favor grabbing stunned creatures
                    {
                        if (crit.stun >= 20)
                        {
                            if (crit.State is HealthState healthState)
                            {
                                if (healthState.health > 0) return crit;
                            }
                            else if (!crit.State.dead) return crit;
                        }
                    }
                }
            }
        }

        return orig(self, favorspears);
    }

    private static void PlayerOnGrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self.SlugCatClass != Plugin.NoirName) return;
        NoirMaul(self, eu);
    }

    private static int HandToSlash(Player self)
    {
        var hand = self.flipDirection == -1 ? 0 : 1;
        if (self.grasps[hand] != null) hand = hand == 0 ? 1 : 0;
        return hand;
    }
    private static void CustomCombatUpdate(NoirData noirData, bool eu)
    {
        var self = noirData.Cat;

        if (!noirData.CanSlash) return;

        if (self.animation == Player.AnimationIndex.Flip)
        {
            if (noirData.AirSlashCooldown > 0) return;
            if (noirData.GraspsAllNull)
            {
                var airSlash = new AbstractCatSlash(self.room.world, AbstractObjectType.CatSlash, null, self.abstractCreature.pos, self.room.game.GetNewID(), self, 0, SlashType.AirSlash);
                var airSlash2 = new AbstractCatSlash(self.room.world, AbstractObjectType.CatSlash, null, self.abstractCreature.pos, self.room.game.GetNewID(), self, 1, SlashType.AirSlash2);
                airSlash.RealizeInRoom();
                airSlash2.RealizeInRoom();
            }
            else if (noirData.GraspsAnyNull)
            {
                var airSlash = new AbstractCatSlash(self.room.world, AbstractObjectType.CatSlash, null, self.abstractCreature.pos, self.room.game.GetNewID(), self, HandToSlash(self), SlashType.AirSlash);
                airSlash.RealizeInRoom();
            }
            noirData.AirSlashCooldown += 40;
        }
        else
        {
            if (noirData.SlashCooldown > 0) return;
            var slash = new AbstractCatSlash(self.room.world, AbstractObjectType.CatSlash, null, self.abstractCreature.pos, self.room.game.GetNewID(), self, HandToSlash(self));
            slash.RealizeInRoom();
            noirData.SlashCooldown += 40;
        }
    }
}
