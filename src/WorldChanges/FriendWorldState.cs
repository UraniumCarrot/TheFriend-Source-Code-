using MoreSlugcats;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace TheFriend.WorldChanges;

public class FriendWorldState
{
    public static bool FaminePlayer(RainWorldGame game) // Applies actual room changes 
    {
        if ((game?.StoryCharacter == Plugin.FriendName || 
             game?.StoryCharacter == Plugin.DragonName || 
             game?.StoryCharacter == Plugin.NoirName) && 
            game != null && !game.IsArenaSession) 
        { SolaceWorldstate = true; return true; }
        else { SolaceWorldstate = false; return false; }
    }
    public static bool SolaceWorldstate;

    public static void Apply()
    {
        // General world hooks
        On.Room.SlugcatGamemodeUniqueRoomSettings += Room_SlugcatGamemodeUniqueRoomSettings;
        On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues;
        On.Region.ctor += Region_ctor;

        // Shaded Citadel hooks
        On.Room.Loaded += Room_Loaded;

        // Creature hooks
        On.AbstractCreature.setCustomFlags += AbstractCreature_setCustomFlags;
        On.ScavengerAbstractAI.InitGearUp += ScavengerAbstractAI_InitGearUp;
        On.FireFly.ctor += FireFly_ctor;
        On.CreatureCommunities.InfluenceCell += CreatureCommunitiesOnInfluenceCell;
        On.CreatureCommunities.LoadDefaultCommunityAlignments += CreatureCommunitiesOnLoadDefaultCommunityAlignments;
    }
    
    public static void Room_SlugcatGamemodeUniqueRoomSettings(On.Room.orig_SlugcatGamemodeUniqueRoomSettings orig, Room self, RainWorldGame game)
    {
        orig(self, game);
        if (game.IsStorySession)
        {
            if (SolaceWorldstate)
            {
                if (self.world.region.name == "SH")
                {
                    if (self.roomSettings.DangerType == RoomRain.DangerType.Flood)
                        self.roomSettings.RainIntensity = 0f;
                }
                self.roomSettings.wetTerrain = false;
                self.roomSettings.CeilingDrips = 0f;
            }
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
        if (game != null)
        {
            FamineWorld.HasFamines(game);
        }
        orig(self, game, playerCharacter, singleRoomWorld, worldName, region, setupValues);
    }

    public static void Region_ctor(On.Region.orig_ctor orig, Region self, string name, int firstRoomIndex, int regionNumber, SlugcatStats.Name storyIndex) // Adjusts region parameters
    {
        //FamineName(storyIndex);
        orig(self, name, firstRoomIndex, regionNumber, storyIndex);
        if (SolaceWorldstate)
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
            if (self.world.region.name != "SB" && self.world.region.name != "UG")
            {
                if (type.type == CreatureTemplate.Type.BigSpider) self.Winterized = true;
                else if (type.type == CreatureTemplate.Type.SpitterSpider) self.Winterized = true;
                else if (type.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider) self.Winterized = true;   
            }
        }
    } // Stops creatures from running or dying when blizzard starts (or should, anyway)

    public static void FireFly_ctor(On.FireFly.orig_ctor orig, FireFly self, Room room, Vector2 pos)
    {
        orig(self, room, pos);
        if (self.col != null && SolaceWorldstate) self.col = new Color(0.8f, Random.Range(0.8f, 1f), 1f); ;
    } // Silver fireflies

    public static bool customLock;
    public static void CreatureCommunitiesOnInfluenceCell(On.CreatureCommunities.orig_InfluenceCell orig, CreatureCommunities self, int comm, int reg, int plr, float infl)
    {
        if (SolaceWorldstate &&
            self.session.characterStats.name == Plugin.FriendName &&
            self.session.game.GetStorySession.saveState.cycleNumber == 0 &&
            comm == 2 &&
            Configs.FriendRepLock)
            return;
        // Cycle 0 lizard rep lock

        if (SolaceWorldstate || Configs.LocalRepAll || customLock)
        {
            if (comm == 2 && reg == 0)
                return;
            if (comm == 0)
                return;
        }
        orig(self, comm, reg, plr, infl);
    } // Stops global lizard and All rep from mattering
    public static void CreatureCommunitiesOnLoadDefaultCommunityAlignments(On.CreatureCommunities.orig_LoadDefaultCommunityAlignments orig, CreatureCommunities self, SlugcatStats.Name savestatenumber)
    {
        orig(self, savestatenumber);
        if (savestatenumber == Plugin.FriendName)
        {
            for (int i = 1; i < self.playerOpinions.GetLength(1); i++) // region
            {
                for (int a = 0; a < self.playerOpinions.GetLength(2); a++) // player
                {
                    self.playerOpinions[2, i, a] = Mathf.Lerp(self.playerOpinions[2, i, a], 1,1);
                }
            }
        }
    } // Sets game-start reputation without slugbase's help (lizard global starts at 0 but rest start at 1)
}
