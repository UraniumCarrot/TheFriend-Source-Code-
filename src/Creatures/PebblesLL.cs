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
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Security.Cryptography;

namespace TheFriend.Creatures;

public class PebblesLL : DaddyLongLegs
{
    public static void Apply()
    {
        PebblesLLGraphics.Apply();
        On.AbstractCreature.ctor += AbstractCreature_ctor;
        IL.DaddyLongLegs.ctor += DaddyLongLegs_ctor;
    }

    public static void DaddyLongLegs_ctor(MonoMod.Cil.ILContext il)
    {
        try 
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After, x => x.MatchNewarr<DaddyTentacle>()) && c.TryGotoNext(x => x.MatchNewarr<DaddyTentacle>()))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((int length, DaddyLongLegs self) => self.Template.type == CreatureTemplateType.PebblesLL ? 3 : length);
            }
        }
        catch(Exception e) { Debug.Log("Solace: IL hook PebblesLongLegsCount failed!" + e); }
    }

    public static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
    {
        orig(self, world, creatureTemplate, realizedCreature, pos, ID);
        if (creatureTemplate.type == CreatureTemplateType.PebblesLL) self.ID = new EntityID(-1,5);
    }

    public PebblesLL(AbstractCreature acrit, World world) : base(acrit, acrit.world)
    {
        graphicsSeed = 1;
        effectColor = new Color(0,0,1);
        eyeColor = new Color(0,0,1);
        colorClass = true;
    }
    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new PebblesLLGraphics(this);
        graphicsModule.Reset();
    }
}