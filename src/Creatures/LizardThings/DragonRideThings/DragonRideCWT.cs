using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.DragonRideThings;

public class DragonData
{
    public readonly AbstractCreature Crit;
    public readonly CreatureTemplate myTemplate;
    public readonly Lizard self;

    public int hybridHead;
    public bool MyHeadDoesntFlash;
    public bool blockCosmetics;
    public float dark;

    public Vector2? lastOutsideTerrainPos;
    public bool aquatic => myTemplate.waterRelationship == CreatureTemplate.WaterRelationship.Amphibious;
    public bool RideEnabled; // Could become rideable if tamed, must pass DoILikeYou check tho
    public float Encumberance;

    public bool DoILikeYou(Player player)
    {
        var ImMother = self.Template?.type == CreatureTemplateType.MotherLizard;
        bool data;
        data = self.AI?.LikeOfPlayer(self.AI?.tracker?.RepresentationForCreature(player.abstractCreature, true)) > 0 && ImMother;
        data = data || (self.AI?.friendTracker?.friend != null &&
                                              self.AI?.friendTracker?.friendRel?.like > 0.5f);
        data = data && (self.AI?.DynamicRelationship(player.abstractCreature).type != CreatureTemplate.Relationship.Type.Attacks &&
                        self.AI?.DynamicRelationship(player.abstractCreature).type != CreatureTemplate.Relationship.Type.Eats);
        data = !self.dead && data;
        return data;
    }

    public List<DragonRiderSeat> seats;
    public List<Player> mainRiders;
    public HashSet<AbstractCreature> riderFriends; // Any creatures the riders are holding or stuck to
    public AbstractCreature target;

    public DragonData(AbstractCreature crit)
    {
        if (crit.realizedCreature is not Lizard)
        {
            Plugin.LogSource.LogWarning("Solace: Abstract creature wasnt a lizard");
            return;
        }
        Crit = crit;
        bool IsLizard = crit.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate;
        if (!IsLizard)
            Plugin.LogSource.LogError("Solace: Somehow, this creature isn't a lizard! Very bad!");

        seats = new List<DragonRiderSeat>();
        mainRiders = new List<Player>();
        riderFriends = new HashSet<AbstractCreature>();
        self = (crit.realizedCreature as Lizard);
        myTemplate = crit.creatureTemplate;
        RideEnabled = false;
        lastOutsideTerrainPos = null;
    }
}

public static class DragonExtender
{
    private static readonly ConditionalWeakTable<AbstractCreature, DragonData> CWT = new();
    public static DragonData Liz(this Lizard lizard) => CWT.GetValue(lizard.abstractCreature, _ => new DragonData(lizard.abstractCreature));
    public static DragonData Liz(this AbstractCreature lizard) => CWT.GetValue(lizard, _ => new DragonData(lizard));

    // Bunch of different ways to quickly access the bool method at the bottom of this class
    public static bool TryGetLiz(this Creature lizard, out DragonData data) => lizard.abstractCreature.TryGetLiz(out data);
    public static bool TryGetLiz(this Lizard lizard, out DragonData data) => lizard.abstractCreature.TryGetLiz(out data);
    public static bool TryGetLiz(this LizardGraphics lizard, out DragonData data) => lizard.lizard.abstractCreature.TryGetLiz(out data);
    public static bool TryGetLiz(this LizardCosmetics.Template lizard, out DragonData data) => lizard.lGraphics.TryGetLiz(out data);
    public static bool TryGetLiz(this LizardAI lizard, out DragonData data) => lizard.lizard.TryGetLiz(out data);
    public static bool TryGetLiz(this PhysicalObject lizard, out DragonData data) => lizard.abstractPhysicalObject.TryGetLiz(out data);
    public static bool TryGetLiz(this AbstractPhysicalObject lizard, out DragonData data) 
    { data = null; return (lizard as AbstractCreature)?.TryGetLiz(out data) ?? false; }

    public static bool TryGetLiz(this AbstractCreature lizard, out DragonData data)
    { // One main bool method that all the rest are gonna lead to calling
        if (lizard.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate)
        {
            data = lizard.Liz();
            return true;
        }
        data = null;
        return false;
    }
}
