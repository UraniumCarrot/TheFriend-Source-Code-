using System;
using System.Collections.Generic;
using System.Linq;
using On.MoreSlugcats;
using RWCustom;
using SlugBase.SaveData;
using TheFriend.SlugcatThings;
using TheFriend.WorldChanges;
using UnityEngine;

namespace TheFriend.SaveThings;

public class SolaceSaveData
{
    public static List<int> MothersRegions = new List<int>();
    public static List<string> MothersRegionsStr = new List<string>();
    public static void Apply()
    {
        On.Creature.Die += CreatureOnDie;
        On.RainWorldGame.Update += RainWorldGameOnUpdate;
    }

    public static void RainWorldGameOnUpdate(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);
        if (self.session == null || 
            self.world == null || 
            !self.IsStorySession) 
            return;
        for (int i = 0; i < self.Players.Count; i++)
        {
            if (self.Players[i].realizedCreature == null || !(self.Players[i].realizedCreature is Player)) return;
            Player player = self.Players[i].realizedCreature as Player;
            var comm = self.session.creatureCommunities;
            var liz = CreatureCommunities.CommunityID.Lizards;
            if (!self.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData()
                .TryGet("MothersKilledInRegion", out List<int> regionsKilledIn))
                return;
            if (regionsKilledIn.Contains(self.world.RegionNumber)) player.GetPoacher().HatedHere = true;
            else player.GetPoacher().HatedHere = false;

            if (player.GetPoacher().HatedHere && comm.LikeOfPlayer(liz, self.world.RegionNumber, player.playerState.playerNumber) > -1)
            {
                comm.SetLikeOfPlayer(CreatureCommunities.CommunityID.Lizards, self.world.RegionNumber, player.playerState.playerNumber,-1);
                comm.InfluenceLikeOfPlayer(CreatureCommunities.CommunityID.Lizards, self.world.RegionNumber, player.playerState.playerNumber, -1,1,0);
            }

            if (!Plugin.LocalLizRep()) return;
            if (FriendWorldState.SolaceWorldstate || Plugin.LocalLizRepAll()) comm.SetLikeOfPlayer(CreatureCommunities.CommunityID.All, -1,player.playerState.playerNumber, 0);
        }
    }

    public static void CreatureOnDie(On.Creature.orig_Die orig, Creature self)
    {
        orig(self);
        if (self.Template.type == CreatureTemplateType.MotherLizard && self.killTag.realizedCreature is Player && self.room.game.IsStorySession)
        {
            int region = self.room.world.RegionNumber;
            string name = self.room.world.regionState.regionName;
            
            if (!self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData().TryGet("MothersKilledInRegion", out List<int> regionsKilledIn))
                regionsKilledIn = new List<int>();
            if (!self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData().TryGet("MothersKilledInRegionStr", out List<string> regionsKilledInStr))
                regionsKilledInStr = new List<string>();

            self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData().TryGet("MotherKillCount", out int count);

            count += 1;
            if (!regionsKilledIn.Contains(region)) regionsKilledIn.Add(region);
            if (!regionsKilledInStr.Contains(name)) regionsKilledInStr.Add(name.ToLower());

            self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData().Set("MotherKillCount", count);
            self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData().Set("MothersKilledInRegion",regionsKilledIn);
            self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData().Set("MothersKilledInRegionStr",regionsKilledInStr);
        }
    }
}