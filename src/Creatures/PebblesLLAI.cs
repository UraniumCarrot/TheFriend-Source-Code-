using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using DevInterface;
using MoreSlugcats;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using Fisobs.Properties;

namespace TheFriend.Creatures;

public class PebblesLLAI : DaddyAI
{
    public PebblesLLAI(AbstractCreature crit, World world) : base(crit, world)
    {
    }
}
