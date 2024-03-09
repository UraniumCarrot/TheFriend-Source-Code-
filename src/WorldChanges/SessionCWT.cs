using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Expedition;
using UnityEngine;

namespace TheFriend.WorldChanges;

public static class SessionCWT
{ // Allows room scripts to be found and stored without holding them in memory permanently
    public class GameCWT
    {
        public List<string> RoomsWithNewScripts = new List<string>();
        public StoryGameSession self;
        public RainWorldGame game => self.game;
        public bool TrueSolaceCampaign => game.StoryCharacter == Plugin.FriendName ||
                                          game.StoryCharacter == Plugin.DragonName ||
                                          game.StoryCharacter == Plugin.NoirName;

        public bool UnnaturalFamines => RWCustom.Custom.rainWorld.ExpeditionMode &&
                                        ExpeditionGame.activeUnlocks.Contains(Expedition.ExpeditionBurdens.famine) 
                                        && !TrueSolaceCampaign;
        public bool NaturalFamines => TrueSolaceCampaign && (!Configs.NoFamine || ExpeditionGame.activeUnlocks.Contains(Expedition.ExpeditionBurdens.famine));
        
        public GameCWT(StoryGameSession game)
        {
            Plugin.LogSource.LogWarning("Solace: GameCWT constructed");
            self = game;
        }
    }
    public static readonly ConditionalWeakTable<StoryGameSession, GameCWT> CWT = new();
    public static GameCWT SessionData(this StoryGameSession game) => CWT.GetValue(game, _ => new GameCWT(game));
    
    public static bool GetSessionData(this RainWorldGame game, out GameCWT data)
    {
        if (game?.GetStorySession != null)
        {
            data = game.GetStorySession.SessionData();
            return true;
        }
        data = null;
        return false;
    }
    public static bool GetSessionData(this Room room, out GameCWT data) => room.world.game.GetSessionData(out data);
    public static bool GetSessionData(this AbstractWorldEntity obj, out GameCWT data) => obj.world.game.GetSessionData(out data);
    public static bool GetSessionData(this UpdatableAndDeletable obj, out GameCWT data) => obj.room.game.GetSessionData(out data);
    public static bool GetSessionData(this RainWorld obj, out GameCWT data) => (obj.processManager.currentMainLoop as RainWorldGame).GetSessionData(out data);
    public static bool GetSessionData(this OracleBehavior obj, out GameCWT data) => obj.oracle.room.game.GetSessionData(out data);
}