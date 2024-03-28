using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Unique;
using TheFriend.Creatures.LizardThings.MotherLizard;
using TheFriend.Creatures.LizardThings.PilgrimLizard;
using TheFriend.Creatures.LizardThings.YoungLizard;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;

namespace TheFriend.Creatures.LizardThings;
public class LizardHooks
{
    internal static void HandleYoungLizardMotherPassage(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallvirt<SocialMemory>("GetLike"),
                x => x.MatchLdcR4(0.0f),
                x => x.MatchBleUn(out _)
            )) {
            if (c.TryGotoNext(MoveType.After, x => x.MatchLdfld<CreatureTemplate>("type")))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldloc_3);
                c.EmitDelegate((CreatureTemplate.Type creature, RainWorldGame game, int index) =>
                {
                    if (creature != TheFriend.CreatureTemplateType.YoungLizard) return creature;
                    Plugin.LogSource.LogMessage("YoungLizard in den!, incrementing pupCountInDen");
                    game.GetStorySession.playerSessionRecords[index].pupCountInDen++;
                    return creature;
                });
            } else { Plugin.LogSource.LogError("failed to hook HandleYoungLizardMotherPassage pt2!"); }
        } else { Plugin.LogSource.LogError("failed to hook HandleYoungLizardMotherPassage pt1!"); }
    }

    #region misc cosmetics
    public static void LizardLimb_ctor(On.LizardLimb.orig_ctor orig, LizardLimb self, GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, LizardLimb otherLimbInPair)
    {
        orig(self, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair);
        if (owner is LizardGraphics l)
        {
            if (l.lizard?.Template.type == CreatureTemplateType.MotherLizard || 
                l.lizard?.Template.type == CreatureTemplateType.PilgrimLizard)
            {
                self.grabSound = SoundID.Lizard_Green_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_Green_Foot_Release;
            }
            else if (l.lizard?.Template.type == CreatureTemplateType.YoungLizard)
            {
                self.grabSound = SoundID.Lizard_PinkYellowRed_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_PinkYellowRed_Foot_Release;
            }
        }
    }
    public static SoundID LizardVoice_GetMyVoiceTrigger(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
    { // Determine the voice of the lizard, don't change it if it's not from Solace
        var res = orig(self);
        if (self.lizard != null)
        {
            switch (self.lizard.Template.type.value)
            {
                case nameof(CreatureTemplateType.MotherLizard): return MotherLizardCritob.MotherLizardVoice(self.lizard, res);
                case nameof(CreatureTemplateType.YoungLizard): return YoungLizardCritob.YoungLizardVoice(self.lizard, res);
                case nameof(CreatureTemplateType.PilgrimLizard): return PilgrimLizardCritob.PilgrimLizardVoice(self.lizard, res);
                default: return res;
            }
        }
        return res;
    }
    #endregion
    #region lizard data
    public static CreatureTemplate LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
    { // Generate the general stats for each new kind of lizard
        if (type == CreatureTemplateType.MotherLizard)
        {
            var temp = orig(CreatureTemplate.Type.GreenLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            var breedParams = (temp.breedParameters as LizardBreedParams)!;
            temp.type = type;
            return MotherLizardMethods.MotherLizardStats(temp, breedParams);
        }
        if (type == CreatureTemplateType.YoungLizard)
        {
            var temp = orig(CreatureTemplate.Type.PinkLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            var breedParams = (temp.breedParameters as LizardBreedParams)!;
            temp.type = type;
            return YoungLizardMethods.YoungLizardStats(temp, breedParams);
        }
        if (type == CreatureTemplateType.PilgrimLizard)
        {
            var temp = orig(CreatureTemplate.Type.PinkLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            var breedParams = (temp.breedParameters as LizardBreedParams)!;
            temp.type = type;
            return  PilgrimLizardMethods.PilgrimLizardStats(temp, breedParams);
        }
        return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
    }
    public static void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
    {
        // Is this lizard rideable?
        orig(self, abstractCreature, world);
        if (Configs.LizRideAll && 
            self.Template.type != CreatureTemplateType.YoungLizard && 
            self.Template.type != CreatureTemplateType.MotherLizard) 
            self.Liz().RideEnabled = true;

        // Properly randomize and construct each kind of new lizard
        if (self.Template.type == CreatureTemplateType.MotherLizard)
            MotherLizardCritob.MotherLizardCtor(self, abstractCreature, world);
        else if (self.Template.type == CreatureTemplateType.YoungLizard)
            YoungLizardCritob.YoungLizardCtor(self, abstractCreature, world);
        else if (self.Template.type == CreatureTemplateType.PilgrimLizard)
            PilgrimLizardCritob.PilgrimLizardCtor(self, abstractCreature, world);
    }
    public static void Lizard_Bite(On.Lizard.orig_Bite orig, Lizard self, BodyChunk chunk)
    {
        orig(self, chunk);
        if (self.Template.type == CreatureTemplateType.YoungLizard) 
            YoungLizardMethods.YoungLizardBite(self,chunk);
    }
    public static CreatureTemplate.Relationship LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.LizardAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, LizardAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        CreatureTemplate.Relationship relationship = orig(self, dRelation);
        var trackedcreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
        if (dRelation == null) return relationship;
        if (dRelation.trackerRep == null) return relationship;
        if (dRelation.trackerRep?.representedCreature == null) return relationship;
        if (dRelation.trackerRep?.representedCreature?.realizedCreature == null) return relationship;
        if (trackedcreature == null) return relationship;

        if (self?.lizard?.Template?.type == CreatureTemplateType.YoungLizard)
            return YoungLizardMethods.YoungLizardDynamicRelations(self, relationship, trackedcreature);
        
        if (self?.lizard?.Template?.type == CreatureTemplateType.MotherLizard && self?.lizard != null)
            return MotherLizardMethods.MotherLizardDynamicRelations(self, relationship, trackedcreature);
        
        return relationship;
    }
    public static void Lizard_Update(Lizard self, bool eu)
    {
        if (self.room == null) return;
        if (self.Template.type == CreatureTemplateType.PilgrimLizard) self.Destroy(); // TODO: GET RID OF THIS LATER
        
        if (!self.dead && self.LizardState?.health > 0f && self.Template?.type == CreatureTemplateType.YoungLizard)
            self.LizardState.health = Mathf.Min(0.5f, self.LizardState.health + 0.001f); // young lizard health regen
        
        if (!self.dead) LizardRideFixes.LizardRideabilityAndSeats(self);
        
        try
        {
            var data = self.Liz();

            if (data.TemporaryFriendshipTimer > 0) data.TemporaryFriendshipTimer--;
            if (data.TemporaryFriendshipTimer <= 0 && self.AI.friendTracker.friendRel != null)
            {
                self.AI.friendTracker.friendRel.like = 0;
                self.AI.friendTracker.friendRel.tempLike = 0;
                self.AI.friendTracker.friend = null;
                self.AI.friendTracker.friendRel = null;
                Debug.Log("Solace: Artificially tamed lizard has stopped following");
            }
            if (data.seats.Any())
                foreach (DragonRiderSeat seat in data.seats) seat.Update(eu);
            
            if (self.AI?.focusCreature?.representedCreature != null)
                data.target = self.AI.focusCreature.representedCreature;
            else data.target = null;
            
            if (self.grabbedBy?.Count > 0 && self.grabbedBy[0]?.grabber is Player pl)
                if (self.Template?.type == CreatureTemplateType.YoungLizard || data.DoILikeYou(pl))
                    LizardRideFixes.LizardNoBiting(self);
        }
        catch (Exception e) { Debug.Log("Solace: Exception happened in Lizard.Update GeneralLizardCode " + e); }
        
        if (self.Liz().mainRiders.Count > 0)
            DragonRiding.DragonRideTerrainReset(self);
    }
    #endregion
    #region lizard cosmetics
    public static void LizardGraphics_ctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
    {
        orig(self, ow);

        if (self.lizard.abstractCreature.Liz().blockCosmetics)
        {
            self.cosmetics.Clear();
            self.extraSprites = 0;
        }
        var state = Random.state;
        Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
        
        // Adds unique scale patterns and features
        if (self.lizard.Template.type == CreatureTemplateType.MotherLizard)
            MotherLizardGraphics.MotherLizardGraphicsCtor(self,ow);
        else if (self.lizard.Template.type == CreatureTemplateType.YoungLizard)
            YoungLizardGraphics.YoungLizardGraphicsCtor(self,ow);
        
        Random.state = state;
    }
    public static Color LizardGraphics_DynamicBodyColor(On.LizardGraphics.orig_DynamicBodyColor orig, LizardGraphics self, float f)
    {
        orig(self, f);
        if (self.lizard.Template.type == CreatureTemplateType.YoungLizard)
            return YoungLizardGraphics.YoungLizardBodyColor(self);
        else return orig(self, f);
    }
    public static Color LizardGraphics_BodyColor(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
    {
        orig(self, f);
        if (self.lizard.Template.type == CreatureTemplateType.YoungLizard)
            return YoungLizardGraphics.YoungLizardBodyColor(self);
        else return orig(self, f);
    }
    public static void LizardGraphics_ApplyPalette(On.LizardGraphics.orig_ApplyPalette orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        self.palette = palette;
        if (self.lizard.Template.type == CreatureTemplateType.YoungLizard)
            self.ColorBody(sLeaser, Color.Lerp(self.lizard.effectColor, Color.white, 0.8f));
    }
    public static void LizardGraphics_InitiateSprites(On.LizardGraphics.orig_InitiateSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        
        // Motherlizard custom head
        if (self.lizard.Template.type == CreatureTemplateType.MotherLizard)
            MotherLizardGraphics.MotherLizardSpritesInit(self,sLeaser,rCam);
    }
    public static void LizardGraphics_AddToContainer(On.LizardGraphics.orig_AddToContainer orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);
        
        // Motherlizard custom head
        if (self.lizard.Template.type == CreatureTemplateType.MotherLizard)
            MotherLizardGraphics.MotherLizardSpritesAddToContainer(self,sLeaser,newContainer);
    }
    public static void LizardGraphics_DrawSprites(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        
        // just... Don't touch this.
        float dark = 1f - Mathf.Pow(0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBlink, self.blink, timeStacker) * 2f * (float)Math.PI), 1.5f + self.lizard.AI.excitement * 1.5f);
        if (self.headColorSetter != 0f)
            dark = Mathf.Lerp(dark, (self.headColorSetter > 0f) ? 1f : 0f, Mathf.Abs(self.headColorSetter));
        if (self.flicker > 10)
            dark = self.flickerColor;
        dark = Mathf.Lerp(dark, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(self.lastVoiceVisualization, self.voiceVisualization, timeStacker)), 0.75f), Mathf.Lerp(self.lastVoiceVisualizationIntensity, self.voiceVisualizationIntensity, timeStacker));
        self.lizard.Liz().dark = dark;

        // Motherlizard custom head
        if (self.lizard.Template.type == CreatureTemplateType.MotherLizard)
            MotherLizardGraphics.MotherLizardDrawSprites(self,sLeaser);
        
        // Allow head and whiskers to have same reactive coloring
        var i = self.cosmetics.FirstOrDefault(x => x is FreeWhiskers i && i.IGlow);
        if (i != null && i is FreeWhiskers whisker)
            FreeWhiskers.WhiskerDrawSprites(self, whisker, sLeaser, timeStacker);
    }
    #endregion
}
