namespace TheFriend.CharacterThings.DelugeThings;

public class DelugeGraphics
{
    public static void Apply()
    {
    }

    public static void DelugeGraphicsUpdate(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser)
    {
        var face = sLeaser.sprites[9];
        if (self.player.dead) return;
        if (self.player.GetDeluge().sprinting && !face.element.name.Contains("Stunned"))
            face.element =
                Futile.atlasManager.GetElementWithName(face.element.name.Remove(face.element.name.Length-2, 2) + "Stunned");
    }
}