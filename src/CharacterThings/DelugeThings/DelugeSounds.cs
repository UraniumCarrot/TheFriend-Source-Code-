namespace TheFriend.CharacterThings.DelugeThings;

public class DelugeSounds
{
    public static SoundID DelugeHeartbeat;
    public static SoundID DelugeHeartbeatSine;
    public static void LoadSounds()
    {

        DelugeHeartbeat = new SoundID("Deluge_Heartbeat", true);
        DelugeHeartbeatSine = new SoundID("Deluge_Heartbeat_Sine", true);
    }
}