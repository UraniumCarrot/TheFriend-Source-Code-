using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using System.Globalization;
using Solace.Creatures;
using MonoMod.Cil;

namespace Solace.WorldChanges;

public class FriendWorldState
{
    public static bool FaminePlayer(RainWorldGame game) // Applies actual room changes 
    {
        if (game?.StoryCharacter == Solace.FriendName || game?.StoryCharacter == Solace.DragonName && game != null) { SolaceWorldstate = true; return true; }
        else { SolaceWorldstate = false; return false; }
    }
    public static bool SolaceWorldstate;
    public static bool FamineName(SlugcatStats.Name name) // For use in menus and region properties 
    {
        if (name == Solace.FriendName || name == Solace.DragonName) { SolaceName = true; return true; }
        else { SolaceName = false; return false; }
    }
    public static bool SolaceName;

    public static void Apply()
    {
        // General world hooks
        On.Room.SlugcatGamemodeUniqueRoomSettings += Room_SlugcatGamemodeUniqueRoomSettings;
        On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues;

        // Shaded Citadel hooks
        On.Room.Loaded += Room_Loaded;

        // Creature hooks
        On.AbstractCreature.setCustomFlags += AbstractCreature_setCustomFlags;
        On.ScavengerAbstractAI.InitGearUp += ScavengerAbstractAI_InitGearUp;
        On.FireFly.ctor += FireFly_ctor;
        On.CreatureCommunities.InfluenceLikeOfPlayer += CreatureCommunities_InfluenceLikeOfPlayer;
        On.CreatureCommunities.LikeOfPlayer += CreatureCommunitiesOnLikeOfPlayer;
    }
    
    #region deprecated code
    public static bool solaceGenPop;
    public static void WorldLoader_GeneratePopulation(On.WorldLoader.orig_GeneratePopulation orig, WorldLoader self, bool fresh)
    {
        try { solaceGenPop = true; orig(self, fresh); }
        finally { solaceGenPop = false; }
    }
    public static bool WorldLoader_OverseerSpawnConditions(On.WorldLoader.orig_OverseerSpawnConditions orig, WorldLoader self, SlugcatStats.Name character)
    {
        if (character == Solace.FriendName)
        {
            bool guideOverseerDead = (self.game.session as StoryGameSession).saveState.guideOverseerDead;
            bool angryWithPlayer = (self.game.session as StoryGameSession).saveState.miscWorldSaveData.playerGuideState.angryWithPlayer;
            return !guideOverseerDead && angryWithPlayer;
        }
        return orig(self, character);
    }

    public static void OverseerAbstractAI_SetAsPlayerGuide(On.OverseerAbstractAI.orig_SetAsPlayerGuide orig, OverseerAbstractAI self, int ownerOverride)
    {
        if (self.world.game.StoryCharacter == Solace.FriendName && ownerOverride != 0 && ownerOverride != 1 && solaceGenPop) ownerOverride = 1;
        orig(self, ownerOverride);
    }

    public static void RoomCamera_UpdateDayNightPalette(On.RoomCamera.orig_UpdateDayNightPalette orig, RoomCamera self)
    {
        orig(self);
        if (SolaceWorldstate && self.room.world.region.name == "SH" && !self.room.IsGateRoom())
        {
            float num = 1320f;
            float num2 = 1.47f;
            float num3 = 1.92f;
            if ((float)self.room.world.rainCycle.dayNightCounter < num)
            {
                if (self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.AboveCloudsView) > 0f && self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SkyAndLightBloom) > 0f)
                {
                    self.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.SkyAndLightBloom).amount = 0f;
                }
                float a = self.paletteBlend;
                self.paletteBlend = Mathf.Lerp(a, 1f, (float)self.room.world.rainCycle.dayNightCounter / num);
                self.ApplyFade();
                self.paletteBlend = a;
            }
            else if ((float)self.room.world.rainCycle.dayNightCounter == num)
            {
                self.ChangeBothPalettes(self.paletteB, self.room.world.rainCycle.duskPalette, 0f);
            }
            else if ((float)self.room.world.rainCycle.dayNightCounter < num * num2)
            {
                if (self.paletteBlend == 1f || self.paletteB != self.room.world.rainCycle.duskPalette || self.dayNightNeedsRefresh)
                {
                    self.ChangeBothPalettes(self.paletteB, self.room.world.rainCycle.duskPalette, 0f);
                }
                self.paletteBlend = Mathf.InverseLerp(num, num * num2, self.room.world.rainCycle.dayNightCounter);
                self.ApplyFade();
            }
            else if ((float)self.room.world.rainCycle.dayNightCounter == num * num2)
            {
                self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette, 0f);
            }
            else if ((float)self.room.world.rainCycle.dayNightCounter < num * num3)
            {
                if (self.paletteBlend == 1f || self.paletteB != self.room.world.rainCycle.nightPalette || self.paletteA != self.room.world.rainCycle.duskPalette || self.dayNightNeedsRefresh)
                {
                    self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette, 0f);
                }
                self.paletteBlend = Mathf.InverseLerp(num * num2, num * num3, self.room.world.rainCycle.dayNightCounter) * (self.effect_dayNight * 0.99f);
                self.ApplyFade();
            }
            else if ((float)self.room.world.rainCycle.dayNightCounter == num * num3)
            {
                self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette, self.effect_dayNight * 0.99f);
            }
            else if ((float)self.room.world.rainCycle.dayNightCounter > num * num3)
            {
                if (self.paletteBlend == 1f || self.paletteB != self.room.world.rainCycle.nightPalette || self.paletteA != self.room.world.rainCycle.duskPalette || self.dayNightNeedsRefresh)
                {
                    self.ChangeBothPalettes(self.room.world.rainCycle.duskPalette, self.room.world.rainCycle.nightPalette, self.effect_dayNight);
                }
                self.paletteBlend = self.effect_dayNight * 0.99f;
                self.ApplyFade();
            }
        }
        self.dayNightNeedsRefresh = false;
    }

    public static string Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
    {
        if (SolaceWorldstate)
        {
            if (baseAcronym == "DS") baseAcronym = "UG";
            if (baseAcronym == "SS") baseAcronym = "RM";
        }
        return orig(character, baseAcronym);
    } // Swaps in Undergrowth and the Rot
    public static bool RoomSettings_Load(On.RoomSettings.orig_Load orig, RoomSettings self, SlugcatStats.Name playerChar) // Complete room settings hijack
    {
        if (SolaceWorldstate)
        {
            playerChar = Solace.FriendName;
            var settings = new string[] { playerChar.value, "saint", "rivulet" };
            for (int i = 0; i < settings.Length; i++)
            {
                string path = WorldLoader.FindRoomFile(self.name, false, "_settings-" + settings[i] + ".txt");
                if (File.Exists(path))
                {
                    self.filePath = path;
                    break;
                }
            }
        }
        return orig(self, playerChar);
    }
    public static string Region_GetRegionFullName(On.Region.orig_GetRegionFullName orig, string regionAcro, SlugcatStats.Name slugcatIndex)
    {
        FamineName(slugcatIndex);
        if (SolaceName) slugcatIndex = Solace.FriendName;
        return orig(regionAcro, slugcatIndex);
    } // Changes region's name in character select screen
    #endregion
    public static void Room_SlugcatGamemodeUniqueRoomSettings(On.Room.orig_SlugcatGamemodeUniqueRoomSettings orig, Room self, RainWorldGame game)
    {
        orig(self, game);
        if (game.IsStorySession && SolaceWorldstate)
        {
            if (self.world.region.name == "SH")
            {
                if (self.roomSettings.DangerType == RoomRain.DangerType.Flood)
                    self.roomSettings.RainIntensity = 0f;
            }
            self.roomSettings.wetTerrain = false;
            self.roomSettings.CeilingDrips = 0f;
            /*if (self.world.region.name != "UG" && 
                self.world.region.name != "SS" && 
                self.world.region.name != "RM" && 
                self.world.region.name != "SB") 
                self.roomSettings.DangerType = MoreSlugcatsEnums.RoomRainDangerType.Blizzard;*/
        }
    } // Default room settings
    public static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
    {
        orig(self);
        if (SolaceWorldstate && self.world.region.name == "SH")
        {
            if (self.lightSources.Count > 0)
                for (int i = 0; i < self.lightSources.Count; i++) self.lightSources[i].fadeWithSun = false;
        }
    }
    public static void WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues(On.WorldLoader.orig_ctor_RainWorldGame_Name_bool_string_Region_SetupValues orig, WorldLoader self, RainWorldGame game, SlugcatStats.Name playerCharacter, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues)
    {
        FaminePlayer(game);
        if (game != null) FamineWorld.HasFamines(game);
        orig(self, game, playerCharacter, singleRoomWorld, worldName, region, setupValues);
    }

    public static void Region_ctor(On.Region.orig_ctor orig, Region self, string name, int firstRoomIndex, int regionNumber, SlugcatStats.Name storyIndex) // Adjusts region parameters
    {
        FamineName(storyIndex);
        orig(self, name, firstRoomIndex, regionNumber, storyIndex);
        if (SolaceName)
        {
            Debug.Log("Applying regional changes...");
            var regionParams = self.regionParams;
            regionParams.earlyCycleChance = 0f;
            regionParams.earlyCycleFloodChance = 0f;
            if (regionParams.earlyCycleChance == 0f && regionParams.earlyCycleFloodChance == 0f) Debug.Log("Precycle chance destroyed for famine characters!");
            if (self.name != "UG" || self.name != "SB")
            {
                regionParams.batDepleteCyclesMax = 0;
                regionParams.batDepleteCyclesMin = 0;
                regionParams.batsPerActiveSwarmRoom = 0;
                regionParams.batsPerInactiveSwarmRoom = 0;
                regionParams.batDepleteCyclesMaxIfLessThanFiveLeft = 0;
                regionParams.batDepleteCyclesMaxIfLessThanTwoLeft = 0;
                regionParams.slugPupSpawnChance = 0.05f;
                if (regionParams.batDepleteCyclesMax == 0f && regionParams.batDepleteCyclesMin == 0 && regionParams.batsPerActiveSwarmRoom == 0) Debug.Log("Regional changes applied to " + self.name + "!");
            }
        }
    }

    public static void ScavengerAbstractAI_InitGearUp(On.ScavengerAbstractAI.orig_InitGearUp orig, ScavengerAbstractAI self)
    {
        orig(self);
        bool random = (Random.value > 0.7) ? true : false;
        if (self.world.game.IsStorySession && SolaceWorldstate && random && self.world.region.name != "SH" && self.world.region.name != "SB")
        {
            AbstractPhysicalObject obj = new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, self.parent.pos, self.world.game.GetNewID());
            self.world.GetAbstractRoom(self.parent.pos).AddEntity(obj);
            new AbstractPhysicalObject.CreatureGripStick(self.parent, obj, 1, carry: true);
        }
    } // Makes scavengers spawn with lanterns
    public static void AbstractCreature_setCustomFlags(On.AbstractCreature.orig_setCustomFlags orig, AbstractCreature self)
    {
        orig(self);
        if (self.creatureTemplate.type == CreatureTemplateType.YoungLizard) self.Winterized = true;
        if (SolaceWorldstate)
        {
            var type = self.creatureTemplate;
            if (type.type == CreatureTemplateType.YoungLizard || 
                type.type == CreatureTemplate.Type.Centipede ||
                type.type == CreatureTemplate.Type.Centiwing ||
                type.type == CreatureTemplate.Type.RedCentipede ||
                type.type == CreatureTemplate.Type.Scavenger ||
                type.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite) 
                self.HypothermiaImmune = false;
            else self.HypothermiaImmune = true;
            if (type.type == CreatureTemplate.Type.Centipede || type.type == CreatureTemplateType.MotherLizard || type.type == CreatureTemplateType.YoungLizard) self.ignoreCycle = false;
            else self.ignoreCycle = true;
            if (type.type == CreatureTemplate.Type.BigSpider && self.world.region.name != "SB" && self.world.region.name != "UG") self.Winterized = true;
            else if (type.type == CreatureTemplate.Type.SpitterSpider && self.world.region.name != "SB" && self.world.region.name != "UG") self.Winterized = true;
            else if (type.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider && self.world.region.name != "SB" && self.world.region.name != "UG") self.Winterized = true;
        }
    } // Stops creatures from running or dying when blizzard starts (or should, anyway)

    public static void FireFly_ctor(On.FireFly.orig_ctor orig, FireFly self, Room room, Vector2 pos)
    {
        orig(self, room, pos);
        if (self.col != null && SolaceWorldstate) self.col = new Color(0.8f, Random.Range(0.8f, 1f), 1f); ;
    } // Silver fireflies
    public static void CreatureCommunities_InfluenceLikeOfPlayer(On.CreatureCommunities.orig_InfluenceLikeOfPlayer orig, CreatureCommunities self, CreatureCommunities.CommunityID commID, int region, int playerNumber, float influence, float interRegionBleed, float interCommunityBleed)
    {
        try
        {
            if (SolaceWorldstate &&
                commID == CreatureCommunities.CommunityID.Lizards &&
                self.session.game.GetStorySession.saveState.cycleNumber == 0 &&
                self.session.game.StoryCharacter == Solace.FriendName &&
                !self.session.game.IsArenaSession &&
                Solace.FriendRepLock())
            {
                return;
            }
            if (!Solace.LocalLizRep())
            {
                orig(self, commID, region, playerNumber, influence, interRegionBleed, interCommunityBleed);
                return;
            }
            if (commID == CreatureCommunities.CommunityID.Lizards && (SolaceWorldstate || Solace.LocalLizRepAll()))
                interCommunityBleed = 0f;

            orig(self, commID, region, playerNumber, influence, interRegionBleed, interCommunityBleed);
        }
        catch (Exception e) { Debug.Log("Solace: Something bad happened! CreatureCommunities.InfluenceLikeOfPlayer broke!" + e); }
    } // Localized lizard reputation
    public static float CreatureCommunitiesOnLikeOfPlayer(On.CreatureCommunities.orig_LikeOfPlayer orig, CreatureCommunities self, CreatureCommunities.CommunityID commid, int region, int playernumber)
    {
        if (!Solace.LocalLizRep()) return orig(self, commid, region, playernumber);
        if ((SolaceWorldstate || Solace.LocalLizRepAll()) && commid == CreatureCommunities.CommunityID.All) return 0f;
        else return orig(self, commid, region, playernumber);
    } // Explode the ALL community likeofplayer because it ruins everything
}
