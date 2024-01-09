using System;
using System.Linq;
using RWCustom;

namespace TheFriend.Creatures.LizardThings.YoungLizard;

public class YoungLizardAI : LizardAI
{
    public static void Apply()
    {
        On.Creature.SuckedIntoShortCut += CreatureOnSuckedIntoShortCut;
        //On.Lizard.SpitOutOfShortCut += LizardOnSpitOutOfShortCut;
    }

    public static void LizardOnSpitOutOfShortCut(On.Lizard.orig_SpitOutOfShortCut orig, Lizard self, IntVector2 pos, Room newroom, bool spitoutallsticks)
    {
        orig(self, pos, newroom, spitoutallsticks);
        if (self.Template.type == CreatureTemplateType.YoungLizard && self.grabbedBy?.Count > 0)
        {
            self.bodyChunks[1].HardSetPosition(self.firstChunk.pos);
        }
    }

    public static void CreatureOnSuckedIntoShortCut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancepos, bool carriedbyother)
    { // Shortcut fix
        if (self.Template.type == CreatureTemplateType.YoungLizard && self.grabbedBy?.Count > 0)
        {
            self.SpitOutOfShortCut(entrancepos, self.room, false);
        }
        else orig(self, entrancepos, carriedbyother);
    }

    public static float LizardRoomLike(Func<LizardAI, float> orig, LizardAI self)
    {
        if (self is YoungLizardAI l && l.lizard.room.abstractRoom.index == l.mother.Room.index) return 1f;
        return orig(self);
    }

    public YoungLizardAI(AbstractCreature crit, World world) : base(crit, world)
    {
        //this.yellowAI = new YellowAI(this);
        //base.AddModule(yellowAI);
    }
    public AbstractCreature mother;
    public override void Update()
    {
        base.Update();
        if (behavior != Behavior.FollowFriend)
        {
            if (mother == null)
            {
                mother = creature.Room.creatures.FirstOrDefault(i => i.creatureTemplate.type == CreatureTemplateType.MotherLizard && i.state.alive);
                if (mother == null)
                    mother = (AbstractCreature)creature.Room.entitiesInDens.FirstOrDefault(i => i is AbstractCreature crit && crit.creatureTemplate.type == CreatureTemplateType.MotherLizard && crit.state.alive);
            }
            if (mother != null)
            {
                if (creature != null)
                {
                    if (mother.state.dead)
                    {
                        mother = null;
                        return;
                    }

                    creature.abstractAI.followCreature = mother;
                    creature.abstractAI.SetDestination(mother.pos.room != creature.pos.room ? mother.pos.WashTileData() : mother.pos);
                }
            }
        }
        else mother = null;
    }
}
