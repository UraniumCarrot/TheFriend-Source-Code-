namespace TheFriend.Creatures.LizardThings.DragonRideThings;

public class WormGrassImmunizer
{
    public static void WormGrassPatch_Update(On.WormGrass.WormGrassPatch.orig_Update orig, WormGrass.WormGrassPatch self)
    {
        orig(self);
        WormGrassStopTrackingRiders(self);
        var mother = self.wormGrass?.room?.abstractRoom?.creatures?.Find(x => x.creatureTemplate.type == CreatureTemplateType.MotherLizard);
        if (mother != null)
            if (!self.wormGrass.repulsiveObjects.Contains(mother.realizedObject))
                self.wormGrass.repulsiveObjects.Add(mother.realizedObject);
    }

    public static void WormGrassStopTrackingRiders(WormGrass.WormGrassPatch self)
    {
        if (self.trackedCreatures.Count > 0)
            for (int i = 0; i < self.trackedCreatures.Count; i++)
            {
                var puller = self.trackedCreatures[i];
                if (puller.creature.abstractCreature.GetAllConnectedObjects()
                    .Exists(x => 
                        x.realizedObject is Lizard liz && 
                        liz.Template.wormGrassImmune && 
                        liz.grasps[0]?.grabbed != puller.creature)) // Doesnt work if the creature was food
                    self.trackedCreatures.RemoveAt(self.trackedCreatures.IndexOf(puller));
                // If an abstract creature is connected to a wormgrassimmune lizard, theyll inherit immunity
            }
    }
}