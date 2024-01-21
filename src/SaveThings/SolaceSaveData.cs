using System;
using System.Collections.Generic;
using System.Linq;
using On.MoreSlugcats;
using RWCustom;
using SlugBase.SaveData;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using TheFriend.SlugcatThings;
using TheFriend.WorldChanges;
using UnityEngine;

namespace TheFriend.SaveThings;

public class SolaceSaveData
{
    public static void Apply()
    {
        On.Creature.Die += CreatureOnDie;
        On.Player.Update += PlayerOnUpdate;
    }

    public static void PlayerOnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self.room == null) return;
        if (!self.room.game.IsStorySession) return;
        if (self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData()
            .TryGet(Plugin.MothersKilled, out List<string> regionsKilledInStr))
            if (regionsKilledInStr.Contains(self.room.world.name.ToLower()))
                FriendWorldState.customLock = true;
            else
                FriendWorldState.customLock = false;
        else FriendWorldState.customLock = false;
    }

    public static void CreatureOnDie(On.Creature.orig_Die orig, Creature self)
    {
        orig(self);
        if (self.room == null) return;
        if (!self.room.game.IsStorySession) return;

        if (self is Lizard liz)
        {
            if (self.Template.type == CreatureTemplateType.MotherLizard && self.killTag.realizedCreature is Player player)
                MotherKilled(player);
            if (self.TryGetLiz(out var data) && data.seats.Any())
                foreach (DragonRiderSeat seat in data.seats) seat.Destroy();
            liz.Liz().RideEnabled = false;
        }
    }

    public static void MotherKilled(Player self)
    {
        string name = self.room.world.name.ToLower();
            
        if (!self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData().TryGet(Plugin.MothersKilled, out List<string> regionsKilledInStr))
            regionsKilledInStr = new List<string>();
        self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData().TryGet(Plugin.MotherKillNum, out int count);
        count += 1;
            
        if (!regionsKilledInStr.Contains(name)) regionsKilledInStr.Add(name);

        self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData().Set(Plugin.MotherKillNum, count);
        self.room.game.GetStorySession.saveState.miscWorldSaveData.GetSlugBaseData()
            .Set(Plugin.MothersKilled, regionsKilledInStr);
        // MothersKilledInRegionStr is responsible for telling MotherKillTracker what regions have had an ML killed in - MUST BE IN LOWERCASE TO WORK.
        // MotherKillCount currently has no purpose
        MotherKilledRepChange(self);
    }

    public static void MotherKilledRepChange(Player self)
    {
        var liz = CreatureCommunities.CommunityID.Lizards;
        var community = self.room.game.session.creatureCommunities;
        var region = self.room.world.RegionNumber;
        var player = self.playerState.playerNumber;

        community.SetLikeOfPlayer(liz, region, player, -1);
        community.InfluenceLikeOfPlayer(liz, region, player, -1, 1, 0);
    }
}