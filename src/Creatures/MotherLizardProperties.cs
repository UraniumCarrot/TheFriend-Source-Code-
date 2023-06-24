using Fisobs.Properties;
using MoreSlugcats;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod;
using UnityEngine.PlayerLoop;
using SlugBase;
using Fisobs.Core;
using Fisobs.Items;

namespace TheFriend.Creatures;
public class MotherLizardProperties : ItemProperties
{
    public readonly Lizard motherLizard;

    public MotherLizardProperties(Lizard motherLizard)
    {
        this.motherLizard = motherLizard;
    }
    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.OneHand;
        //if ((player?.GetPoacher()?.dragonSteed as Lizard).GetLiz().boolseat0) grabability = Player.ObjectGrabability.CantGrab;
    }
}