using RWCustom;
using TheFriend.RemixMenus;

namespace TheFriend.CharacterThings.NoirThings;

public partial class NoirCatto
{
    public enum CustomStartMode // Needs to be translatable
    {
        StoryAndExpedition = 0,
        Story = 1,
        Disabled = 2,
    }

    private const string StartingRoom = "SI_B12";

    public static void SaveStateOnsetDenPosition(On.SaveState.orig_setDenPosition orig, SaveState self)
    {
        orig(self);

        if (Custom.rainWorld.ExpeditionMode && RemixMain.NoirUseCustomStart.Value != CustomStartMode.StoryAndExpedition) return;
        if (self.saveStateNumber != Plugin.NoirName) return;
        if (self.cycleNumber == 0)
        {
            self.denPosition = StartingRoom;
        }
    }

    public static void RainWorldGameOnctor(RainWorldGame self, ProcessManager manager)
    {
        if (!self.IsStorySession) return;
        if (self.StoryCharacter != Plugin.NoirName) return;
        if (Custom.rainWorld.ExpeditionMode && RemixMain.NoirUseCustomStart.Value != CustomStartMode.StoryAndExpedition) return;
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
            if (RemixMain.NoirUseCustomStart.Value != CustomStartMode.Disabled && startRoom != null)
            {
                startRoom.AddObject(new NoirStart());
            }
        }
    }
}