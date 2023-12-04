using RWCustom;

namespace TheFriend.NoirThings;

public partial class NoirCatto
{
    private const string StartingRoom = "SI_B12";
    private static void SaveStateOnsetDenPosition(On.SaveState.orig_setDenPosition orig, SaveState self)
    {
        orig(self);

        if (self.saveStateNumber != Plugin.NoirName) return;
        if (self.cycleNumber == 0)
        {
            self.denPosition = StartingRoom;
        }
    }

    private static void RainWorldGameOnctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
    {
        orig(self, manager);

        if (!self.IsStorySession) return;
        if (self.StoryCharacter != Plugin.NoirName) return;
        var session = self.GetStorySession;

        if (session.saveState.cycleNumber == 0)
        {
            Room startRoom = null;

            foreach (var player in self.Players)
            {
                if (player.Room.name != StartingRoom) break;
                var pState = (PlayerState)player.state;

                player.pos.Tile = new IntVector2(11 + pState.playerNumber, 54);

                startRoom ??= player.Room.realizedRoom;
            }
            if (Options.NoirUseCustomStart.Value && startRoom != null)
            {
                startRoom.AddObject(new NoirStart());
            }
        }
    }
}