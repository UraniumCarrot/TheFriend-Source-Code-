using System.Collections.Generic;
using System.Globalization;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using Fisobs.Creatures;
using IL;
using LizardCosmetics;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using On;
using RWCustom;
using Noise;
using SlugBase.DataTypes;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using System.Linq;
using TheFriend.Objects.BoomMineObject;
using JollyCoop;
using TheFriend.WorldChanges;
using SlugBase.Features;
using System.Drawing;

namespace TheFriend.Creatures.LizardThings;

public class YoungLizardAI : LizardAI
{
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
                for (int i = 0; i < creature?.Room?.creatures?.Count; i++)
                {
                    if (creature?.Room?.creatures[i]?.creatureTemplate?.type == CreatureTemplateType.MotherLizard && creature.Room.creatures[i].state.alive)
                        mother = creature?.Room?.creatures[i];
                }
                for (int i = 0; i < creature?.Room?.entitiesInDens?.Count; i++)
                {
                    if ((creature?.Room?.entitiesInDens[i] as AbstractCreature)?.creatureTemplate?.type == CreatureTemplateType.MotherLizard && (creature?.Room?.entitiesInDens[i] as AbstractCreature).state.alive)
                        mother = creature.Room.entitiesInDens[i] as AbstractCreature;
                }
            }
            if (mother != null)
            {
                if (mother.state.dead || mother.Room == null) mother = null;
                creature.abstractAI.followCreature = mother;
                if (mother?.pos.room != creature?.pos.room) creature?.abstractAI?.SetDestination(mother.pos.WashTileData());
                else creature?.abstractAI?.SetDestination(mother.pos);
            }
        }
        else mother = null;
    }
}
