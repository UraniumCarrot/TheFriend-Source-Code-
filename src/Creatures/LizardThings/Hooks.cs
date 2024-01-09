using System;
using System.Collections.Generic;
using System.Linq;
using LizardCosmetics;
using MoreSlugcats;
using RWCustom;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics;
using TheFriend.Creatures.LizardThings.MotherLizard;
using TheFriend.Creatures.LizardThings.PilgrimLizard;
using TheFriend.Creatures.LizardThings.YoungLizard;
using TheFriend.SlugcatThings;
using TheFriend.DragonRideThings;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;

namespace TheFriend.Creatures.LizardThings;
public class Hooks
{
    public static void Apply()
    {
        On.LizardLimb.ctor += LizardLimb_ctor;
        On.LizardVoice.GetMyVoiceTrigger += LizardVoice_GetMyVoiceTrigger;

        On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
        On.Lizard.ctor += Lizard_ctor;
        On.Lizard.Bite += Lizard_Bite;
        On.Lizard.Update += Lizard_Update;
        On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship;

        On.LizardAI.SocialEvent += LizardAI_SocialEvent;
        On.Creature.NewRoom += Creature_NewRoom;
        On.Player.IsCreatureLegalToHoldWithoutStun += Player_IsCreatureLegalToHoldWithoutStun;
        On.Creature.Grab += Creature_Grab;
        WormGrassImmunizer.Apply();

        On.LizardGraphics.ctor += LizardGraphics_ctor;
        On.LizardGraphics.ApplyPalette += LizardGraphics_ApplyPalette;
        On.LizardGraphics.BodyColor += LizardGraphics_BodyColor;
        On.LizardGraphics.DynamicBodyColor += LizardGraphics_DynamicBodyColor;
        On.LizardGraphics.InitiateSprites += LizardGraphics_InitiateSprites;
        On.LizardGraphics.AddToContainer += LizardGraphics_AddToContainer;
        On.LizardGraphics.DrawSprites += LizardGraphics_DrawSprites;
        LizardCosmeticHooks.Apply();
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
            self.GetLiz().IsRideable = true;

        // Is this lizard aquatic? (Allows swimming during LizRide)
        if (self.Template.type == CreatureTemplate.Type.Salamander ||
            self.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
            self.GetLiz().aquatic = true;

        // Properly randomize each kind of new lizard
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
    public static void Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
    {
        try { orig(self, eu); }
        catch (Exception e) { Debug.Log("Solace: Exception happened in Lizard.Update orig " + e); }
        if (self == null) return;
        if (self.Template.type == CreatureTemplateType.PilgrimLizard) self.Destroy(); // TODO: GET RID OF THIS LATER
        try
        {
            if (self.GetLiz() != null)
            {
                self.GetLiz().seat0 = Vector2.Lerp(self.bodyChunks[1].pos, self.bodyChunks[0].pos, 0.5f) + new Vector2(0, 15);
            }
            if (!self.dead && self.LizardState?.health > 0f && self.Template?.type == CreatureTemplateType.YoungLizard)
            {
                self.LizardState.health = Mathf.Min(0.5f, self.LizardState.health + 0.001f);
            }
            if (self.grabbedBy?.Count > 0 && self.grabbedBy[0]?.grabber is Player player && self.grabbedBy[0]?.grabber is not null)
            {
                if (self.Template?.type == CreatureTemplateType.YoungLizard)
                {
                    self.grabbedAttackCounter = 0;
                    self.JawOpen = 0;
                }
                if ((self.Template?.type == CreatureTemplateType.MotherLizard || 
                     self.GetLiz().IsRideable) && 
                    self.AI?.LikeOfPlayer(self.AI?.tracker?.RepresentationForCreature(player.abstractCreature, true)) > 0)
                {
                    self.grabbedAttackCounter = 0;
                    self.JawOpen = 0;
                }
            }
        }
        catch (Exception e) { Debug.Log("Solace: Exception happened in Lizard.Update GeneralLizardCode " + e); }
        if (self.GetLiz() != null && self?.GetLiz()?.rider != null)
        {
            if (self?.graphicsModule != null) (self?.graphicsModule as LizardGraphics)?.BringSpritesToFront();
            try
            {
                if (self?.room?.GetTile(self.firstChunk.pos)?.Solid == true)
                {
                    if (self?.firstChunk?.collideWithTerrain == true && self?.room?.GetTile(self.firstChunk.lastPos)?.Solid == true && self?.room?.GetTile(self.firstChunk.lastLastPos)?.Solid == true && self.GetLiz().lastOutsideTerrainPos.HasValue)
                    {
                        Debug.Log("Solace: Resetting ridden lizard to outside terrain");
                        for (int i = 0; i < self?.bodyChunks?.Length; i++)
                        {
                            self.bodyChunks[i].HardSetPosition(self.GetLiz().lastOutsideTerrainPos.Value + Custom.RNV() * Random.value);
                            self.bodyChunks[i].vel /= 2f;
                        }
                    }
                }
                else if (self != null) self.GetLiz().lastOutsideTerrainPos = self.firstChunk.pos;
            }
            catch (Exception e) { Debug.Log("Solace: Exception happened in Lizard.Update LizardRideTerrainPositionReset" + e); }
        }
        else if (self?.Template?.type == CreatureTemplateType.MotherLizard) for (int i = 0; i < self?.bodyChunks?.Length; i++) self.bodyChunks[i].mass = 10;
        try
        {
            WormGrassImmunizer.WormGrassLizardRepulsor(self);
        }
        catch (Exception e) { Debug.Log("Solace: Exception happened in Lizard.Update WormGrassRepulse " + e); }
    }
    #endregion
    #region misc data
    public static void LizardAI_SocialEvent(On.LizardAI.orig_SocialEvent orig, LizardAI self, SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
    {
        if (self.lizard.GetLiz().rider != null && subjectCrit is Player pl && pl?.GetGeneral()?.dragonSteed == self.lizard) return;
        else orig(self, ID, subjectCrit, objectCrit, involvedItem);
    }
    public static void Creature_NewRoom(On.Creature.orig_NewRoom orig, Creature self, Room newRoom)
    {
        orig(self, newRoom);
        if (self is Lizard liz && liz.GetLiz() != null) liz.GetLiz().lastOutsideTerrainPos = null;
    }
    public static bool Player_IsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabCheck)
    {
        return grabCheck is Lizard liz && 
            (liz.Template.type == CreatureTemplateType.YoungLizard || liz.Template.type == CreatureTemplateType.MotherLizard) 
            || orig(self, grabCheck);
    }
    public static bool Creature_Grab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspUsed, int chunkGrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        if (self is Player && 
            obj is Lizard liz && 
            (liz.Template.type == CreatureTemplateType.YoungLizard || 
             liz.Template.type == CreatureTemplateType.MotherLizard))
        {
            shareability = Creature.Grasp.Shareability.CanNotShare;
            pacifying = false;
        }
        return orig(self, obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
    }
    #endregion
    #region lizard cosmetics
    public static void LizardGraphics_ctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
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
