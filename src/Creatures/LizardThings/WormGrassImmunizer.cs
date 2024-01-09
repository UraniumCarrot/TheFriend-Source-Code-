using System;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings;

public class WormGrassImmunizer
{
    public static void Apply()
    {
        On.WormGrass.WormGrassPatch.InteractWithCreature += WormGrassPatch_InteractWithCreature;
        On.WormGrass.WormGrassPatch.Update += WormGrassPatch_Update;
        On.WormGrass.WormGrassPatch.AlreadyTrackingCreature += WormGrassPatch_AlreadyTrackingCreature;
    }

    public static void WormGrassLizardRepulsor(Lizard self)
    {
        if (self.Template.type == CreatureTemplateType.MotherLizard && self.room is Room room)
        {
            if (room.updateList.Count == 0) return;
            foreach (UpdatableAndDeletable update in self.room?.updateList)
            {
                if (update is WormGrass grass)
                {
                    if (grass?.repulsiveObjects?.Contains(self) != true) grass.AddNewRepulsiveObject(self);
                }
            }
        }
    }
    
    public static bool WormGrassPatch_AlreadyTrackingCreature(On.WormGrass.WormGrassPatch.orig_AlreadyTrackingCreature orig, WormGrass.WormGrassPatch self, Creature creature)
    {
        for (int i = 0; i < self.trackedCreatures.Count; i++)
        {
            try
            {
                if (creature is Player player && player.GetGeneral()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard)
                    return true;
                else if (creature is Player player0 && player0.GetGeneral()?.dragonSteed?.Template?.wormGrassImmune == true)
                    return true;
            }
            catch (Exception e) { Debug.Log("Solace: Exception occurred in WormGrassPatch.AlreadyTrackingCreature playerCode " + e); }
        }
        return orig(self, creature);
    }
    public static void WormGrassPatch_InteractWithCreature(On.WormGrass.WormGrassPatch.orig_InteractWithCreature orig, WormGrass.WormGrassPatch self, WormGrass.WormGrassPatch.CreatureAndPull creatureAndPull)
    { // um what is this i dont rember
        if (!(creatureAndPull?.creature is Player player && 
              (player.GetGeneral()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard || player.GetGeneral()?.dragonSteed?.Template?.wormGrassImmune == true)) 
            ||
            !(creatureAndPull.creature?.grabbedBy?.Count > 0 && 
              creatureAndPull.creature?.grabbedBy[0]?.grabber is Player pl && 
              (pl.GetGeneral()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard || player.GetGeneral()?.dragonSteed?.Template?.wormGrassImmune == true)))
            orig(self, creatureAndPull);
    }
    public static void WormGrassPatch_Update(On.WormGrass.WormGrassPatch.orig_Update orig, WormGrass.WormGrassPatch self)
    {
        orig(self);
        for (int i = 0; i < self.trackedCreatures.Count; i++)
        {
            Creature crit = self.trackedCreatures[i]?.creature;
            try
            {
                if (self.trackedCreatures[i]?.creature is Player player)
                {
                    if (player.GetGeneral()?.isRidingLizard == true && 
                        (player.GetGeneral()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard || 
                         player.GetGeneral()?.dragonSteed?.Template?.wormGrassImmune == true)) 
                        self.trackedCreatures.RemoveAt(i);
                }
            }
            catch (Exception e) { Debug.Log("Solace: Exception occurred in WormGrassPatch.Update playerCode " + e); }
            try
            {
                if (crit is not null && crit?.grabbedBy?.Count > 0 && self.trackedCreatures[i]?.creature?.grabbedBy[0]?.grabber is Player pl)
                {
                    if (pl.GetGeneral()?.dragonSteed != null && 
                        (pl.GetGeneral()?.dragonSteed?.Template?.type == CreatureTemplateType.MotherLizard || 
                         pl.GetGeneral()?.dragonSteed?.Template?.wormGrassImmune == true)) 
                        self.trackedCreatures.RemoveAt(i);
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
}