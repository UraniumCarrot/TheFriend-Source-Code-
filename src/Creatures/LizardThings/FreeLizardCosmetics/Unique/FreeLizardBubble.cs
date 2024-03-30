using JetBrains.Annotations;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Unique;

public class FreeLizardBubble : LizardBubble
{
    public Color? color;
    public Color? secondColor;
    public string spriteReplacement = "";

    public FreeLizardBubble(LizardGraphics graphics, float intensity, float stickiness, float extraSpeed) : base(
        graphics, intensity, stickiness, extraSpeed)
    {
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
        Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        Color col1 = color.HasValue ? color.Value : lizardGraphics.HeadColor(timeStacker);
        Color col2 = secondColor.HasValue ? secondColor.Value : lizardGraphics.palette.blackColor;
        sLeaser.sprites[0].color = Color.Lerp(col1, col2, 1f - Mathf.Clamp(Mathf.Lerp(lastLife, life, timeStacker) * 2f, 0f, 1f));

        int frame = 0;
        string sprite = spriteReplacement.Length > 0 ? spriteReplacement : "LizardBubble";
        if (hollow) frame = RWCustom.Custom.IntClamp((int)(Mathf.Pow(Mathf.InverseLerp(lifeTimeWhenFree, 0f, life), hollowNess) * 7f), 1, 7);
        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName(sprite + frame);
    }
}