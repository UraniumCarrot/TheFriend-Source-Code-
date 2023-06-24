using Fisobs.Properties;
using RWCustom;
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
public class YoungLizardProperties : ItemProperties
{
    public readonly Lizard youngLizard;

    public YoungLizardProperties(Lizard youngLizard)
    {
        this.youngLizard = youngLizard;
    }
}
