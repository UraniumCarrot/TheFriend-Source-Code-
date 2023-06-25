﻿using System;
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
        IL.Spear.Update += Spear_Update;
    }

    public static void Spear_Update(MonoMod.Cil.ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(i => i.MatchIsinst<Deer>(),
                       i => i.MatchBrtrue(out label));
            c.GotoPrev(i => i.MatchLdarg(0));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Spear spear) => 
            {
                return spear.stuckInObject is PebblesLL ? true : false;
            }
            );
            c.Emit(OpCodes.Brtrue, label);
        }
        catch(Exception e) { Debug.Log("Solace: IL hook SpearUpdatePLLStab failed!" + e); }
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
        catch(Exception e) { Debug.Log("Solace: IL hook PLLTentaCount failed!" + e); }
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
    public override void Update(bool eu)
    {
        base.Update(eu);
        State.alive = true;
        if (dead == true) dead = false;
        for (int i = 0; i < tentacles.Length; i++)
            if ((State as DaddyState).tentacleHealth[i] < 1f) (State as DaddyState).tentacleHealth[i] += 0.005f;
    }
    public override void Die()
    {
        killTag = null;
        base.Die();
    }
    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (hitAppendage != null) 
        { 
            damage = 0;
            tentacles[hitAppendage.appendage.appIndex].stun = (type == DamageType.Explosion || type == DamageType.Blunt) ? 200 : 20;
        }
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }
}