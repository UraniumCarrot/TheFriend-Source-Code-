using System;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings;

public class YoungLizardAI : LizardAI
{
    public static void Apply()
    {
        //new Hook(
        //    typeof(LizardAI).GetMethod("get_RoomLike", BindingFlags.Instance | BindingFlags.Public),
        //    typeof(Plugin).GetMethod("LizardRoomLike", BindingFlags.Static | BindingFlags.Public)
        //);
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
