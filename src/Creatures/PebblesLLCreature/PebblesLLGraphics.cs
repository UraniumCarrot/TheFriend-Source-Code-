using Color = UnityEngine.Color;
using Vector2 = UnityEngine.Vector2;

namespace TheFriend.Creatures.PebblesLLCreature;

public class PebblesLLGraphics : DaddyGraphics
{
    public static void Apply()
    {
        On.DaddyGraphics.DaddyTubeGraphic.ApplyPalette += DaddyTubeGraphic_ApplyPalette;
        On.DaddyGraphics.DaddyDeadLeg.ApplyPalette += DaddyDeadLeg_ApplyPalette;
        On.DaddyGraphics.DaddyDeadLeg.DrawSprite += DaddyDeadLeg_DrawSprite;
        On.DaddyGraphics.DaddyDangleTube.ApplyPalette += DaddyDangleTube_ApplyPalette;
    }
    public PebblesLLGraphics(PhysicalObject ow) : base(ow)
    {
    }
    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        blackColor = new Color(1f, 0.3f, 0.9f);
        for (int i = 0; i < daddy.bodyChunks.Length; i++)
        {
            sLeaser.sprites[BodySprite(i)].color = blackColor;
        }
    }

    public static void DaddyDangleTube_ApplyPalette(On.DaddyGraphics.DaddyDangleTube.orig_ApplyPalette orig, DaddyDangleTube self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        Color color = new Color(1f, 0.3f, 0.9f);
        if (self.owner.daddy.Template.type == CreatureTemplateType.PebblesLL)
        {
            var tri = sLeaser.sprites[self.firstSprite] as TriangleMesh;
            for (int i = 0; tri != null && i < tri.vertices.Length; i++)
            {
                //if (self?.Bcon?.chunk?.pos.y > 0) (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(palette.blackColor, color, i * 0.05f);
                //if (self?.Acon?.chunk?.pos.y > 0) (sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, palette.blackColor, i * 0.05f);
                if (i < tri.vertices.Length * 0.25f)
                {
                    tri.verticeColors[i] = Color.red;
                }
                else if (i < (tri.vertices.Length * 0.5f))
                {
                    tri.verticeColors[i] = Color.green;
                }
                else if (i < tri.vertices.Length * 0.75f)
                {
                    tri.verticeColors[i] = Color.blue;
                    // No changes needed
                }
                else if (i < tri.vertices.Length)
                {
                    //(sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.yellow;
                    if (self.Acon?.chunk?.pos.y > 0) tri.verticeColors[i] = Color.Lerp(color, palette.blackColor, i * 0.1f);
                    else tri.verticeColors[i] = color;
                }


                //float floatPos = Mathf.InverseLerp(0.3f, 1f, (float)i / (float)((sLeaser.sprites[self.firstSprite] as TriangleMesh).vertices.Length - 1));
                //(sLeaser.sprites[self.firstSprite] as TriangleMesh).verticeColors[i] = Color.Lerp(color, palette.blackColor, self.OnTubeEffectColorFac(floatPos)*4);
            }
        }
    }
    public static void DaddyDeadLeg_DrawSprite(On.DaddyGraphics.DaddyDeadLeg.orig_DrawSprite orig, DaddyDeadLeg self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.owner.daddy.Template.type == CreatureTemplateType.PebblesLL) { self.bumps[7].pos = self.bumps[3].pos; }
    }
    public static void DaddyDeadLeg_ApplyPalette(On.DaddyGraphics.DaddyDeadLeg.orig_ApplyPalette orig, DaddyDeadLeg self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        Color color = new Color(1f, 0.3f, 0.9f);
        var tri = sLeaser.sprites[self.firstSprite] as TriangleMesh;
        if (self.owner.daddy.Template.type == CreatureTemplateType.PebblesLL)
        {
            for (int i = 0; tri != null && i < tri.vertices.Length; i++)
            {
                if (i < tri.vertices.Length / 2) tri.verticeColors[i] = Color.Lerp(color, palette.blackColor, i * 0.01f - 0.1f);
            }
            for (int j = 0; j < self.bumps.Length; j++)
            {
                if (self.bumps[j].pos.y < 0.5f) sLeaser.sprites[self.firstSprite + 1 + j].color = Color.Lerp(color, palette.blackColor, self.OnTubeEffectColorFac(self.bumps[j].pos.y) * 0.4f);
            }
        }
    }
    public static void DaddyTubeGraphic_ApplyPalette(On.DaddyGraphics.DaddyTubeGraphic.orig_ApplyPalette orig, DaddyTubeGraphic self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        Color color = new Color(1f, 0.3f, 0.9f);
        var tri = sLeaser.sprites[self.firstSprite] as TriangleMesh;
        if (self.owner.daddy.Template.type == CreatureTemplateType.PebblesLL)
        {
            for (int i = 0; tri != null && i < tri.vertices.Length; i++)
            {
                if (i < tri.vertices.Length / 2) tri.verticeColors[i] = Color.Lerp(color, palette.blackColor, i * 0.01f - 0.1f);
            }
        }
    }
}