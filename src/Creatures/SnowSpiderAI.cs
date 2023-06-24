using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using IL;
using On;
using RWCustom;
using MoreSlugcats;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

namespace TheFriend.Creatures;

public class SnowSpiderAI : BigSpiderAI
{
    public SnowSpiderAI(AbstractCreature crit, World world) : base(crit, world) 
    {
    }
    public override void Update()
    {
        base.Update();
        bug.runSpeed = 0f;
        if (bug.sitting) bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0f, 0.01f, 1f / 60f);
        if (behavior == Behavior.Idle && !bug.sitting) bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.15f, 0.01f, 1f / 60f);
        if (behavior == Behavior.Hunt && !bug.sitting) bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.15f, 0.01f, 0.1f);
        if (behavior == Behavior.Flee && !bug.sitting) bug.runSpeed = Custom.LerpAndTick(bug.runSpeed, 0.2f, 0.01f, 0.1f);
    }
}
