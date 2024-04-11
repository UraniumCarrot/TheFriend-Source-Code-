using System.Collections.Generic;
using LizardCosmetics;
using UnityEngine;
using RWCustom;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;


namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.CustomTemps;

public class BlandWhiskers : Template
{
    public float lightUp;
    public GenericBodyPart[,] whiskers;
    public Vector2[] whiskerDirections;
    public float[,] whiskerProps;
    public float[,,] whiskerLightUp;
    public int amount;
    
    public BlandWhiskers(LizardGraphics lGraphics, int startSprite, int amount = 4) : base(lGraphics, startSprite)
    {
        spritesOverlap = SpritesOverlap.InFront;
        this.amount = amount;
        whiskers = new GenericBodyPart[2, amount];
        whiskerDirections = new Vector2[amount];
        whiskerProps = new float[amount, 5];
        whiskerLightUp = new float[amount, 2, 2];
        for (int i = 0; i < amount; i++)
        {
            whiskers[0, i] = new GenericBodyPart(lGraphics, 1f, 0.6f, 0.9f, lGraphics.lizard.mainBodyChunk);
            whiskers[1, i] = new GenericBodyPart(lGraphics, 1f, 0.6f, 0.9f, lGraphics.lizard.mainBodyChunk);
            whiskerDirections[i] = Custom.DegToVec(Mathf.Lerp(4f, 100f, Random.value));
            whiskerProps[i, 0] = Custom.ClampedRandomVariation(0.5f, 0.4f, 0.5f) * 40f;
            whiskerProps[i, 1] = Mathf.Lerp(-0.5f, 0.8f, Random.value);
            whiskerProps[i, 2] = Mathf.Lerp(11f, 720f, Mathf.Pow(Random.value, 1.5f)) / whiskerProps[i, 0];
            whiskerProps[i, 3] = Random.value;
            whiskerProps[i, 4] = Mathf.Lerp(0.6f, 1.2f, Mathf.Pow(Random.value, 1.6f));
            if (i <= 0)
                continue;
            for (int a = 0; a < 5; a++)
                if (a != 1)
                    whiskerProps[i, a] = Mathf.Lerp(whiskerProps[i, a], whiskerProps[i - 1, a], Mathf.Pow(Random.value, 0.3f) * 0.6f);
        }
        numberOfSprites = amount * 2;
    }
    
    public override void Reset()
    {
        base.Reset();
        for (int a = 0; a < 2; a++)
            for (int i = 0; i < amount; i++)
                whiskers[a, i].Reset(AnchorPoint(a, i, 1f));
    }
    public override void Update()
    {
        for (int a = 0; a < 2; a++)
        {
            for (int i = 0; i < amount; i++)
            {
                whiskers[a, i].vel += whiskerDir(a, i, 1f) * whiskerProps[i, 2];
                if (lGraphics.lizard.room.PointSubmerged(whiskers[a, i].pos))
                    whiskers[a, i].vel *= 0.8f;
                else whiskers[a, i].vel.y -= 0.6f;
                
                whiskers[a, i].Update();
                whiskers[a, i].ConnectToPoint(AnchorPoint(a, i, 1f), whiskerProps[i, 0], push: false, 0f, lGraphics.lizard.mainBodyChunk.vel, 0f, 0f);
                if (!Custom.DistLess(lGraphics.head.pos, whiskers[a, i].pos, 200f))
                    whiskers[a, i].pos = lGraphics.head.pos;
                
                whiskerLightUp[i, a, 1] = whiskerLightUp[i, a, 0];
                if (whiskerLightUp[i, a, 0] < Mathf.InverseLerp(0f, 0.3f, lightUp))
                    whiskerLightUp[i, a, 0] = Mathf.Lerp(whiskerLightUp[i, a, 0], Mathf.InverseLerp(0f, 0.3f, lightUp), 0.7f) + 0.05f;
                else
                    whiskerLightUp[i, a, 0] -= 0.025f;
                whiskerLightUp[i, a, 0] += Mathf.Lerp(-1f, 1f, Random.value) * 0.03f * lightUp;
                whiskerLightUp[i, a, 0] = Mathf.Clamp(whiskerLightUp[i, a, 0], 0f, 1f);
            }
        }
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        for (int i = startSprite + amount * 2 - 1; i >= startSprite; i--)
            sLeaser.sprites[i] = TriangleMesh.MakeLongMesh(4, pointyTip: true, customColor: true);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 headDir = Custom.DegToVec(lGraphics.HeadRotation(timeStacker));
		for (int j = 0; j < amount; j++)
		{
			for (int side = 0; side < 2; side++)
			{
				Vector2 tipPos = Vector2.Lerp(whiskers[side, j].lastPos, whiskers[side, j].pos, timeStacker);
				Vector2 dr = whiskerDir(side, j, timeStacker);
				Vector2 anchorPos = AnchorPoint(side, j, timeStacker);
				dr = (dr + headDir).normalized;
				Vector2 lastSegmentPos = anchorPos;
				float lastRad = whiskerProps[j, 4];
				float bendFac = 1f;
				for (int i = 0; i < 4; i++)
				{
					Vector2 segmentDrawPos;
					if (i < 3)
					{
						segmentDrawPos = Vector2.Lerp(anchorPos, tipPos, (i + 1) / 4f);
						segmentDrawPos += dr * bendFac * whiskerProps[j, 0] * 0.2f;
					}
					else segmentDrawPos = tipPos;
					
					bendFac *= 0.7f;
					Vector2 dir = (segmentDrawPos - lastSegmentPos).normalized;
					Vector2 perpendicular = Custom.PerpendicularVector(dir);
					float dist = Vector2.Distance(segmentDrawPos, lastSegmentPos) / ((i == 0) ? 1f : 5f);
					float rad = Custom.LerpMap(i, 0f, 3f, whiskerProps[j, 4], 0.5f);
					for (int v = i * 4; v < i * 4 + ((i == 3) ? 3 : 4); v++)
						(sLeaser.sprites[startSprite + j * 2 + side] as TriangleMesh)!.verticeColors[v] = Color.Lerp(lGraphics.HeadColor(timeStacker), new Color(1f, 1f, 1f), (i - 1) / 2f * Mathf.Lerp(whiskerLightUp[j, side, 1], whiskerLightUp[j, side, 0], timeStacker));
					(sLeaser.sprites[startSprite + j * 2 + side] as TriangleMesh)!.MoveVertice(i * 4, lastSegmentPos - perpendicular * (rad + lastRad) * 0.5f + dir * dist - camPos);
					(sLeaser.sprites[startSprite + j * 2 + side] as TriangleMesh)!.MoveVertice(i * 4 + 1, lastSegmentPos + perpendicular * (rad + lastRad) * 0.5f + dir * dist - camPos);
					if (i < 3)
					{
						(sLeaser.sprites[startSprite + j * 2 + side] as TriangleMesh)!.MoveVertice(i * 4 + 2, segmentDrawPos - perpendicular * rad - dir * dist - camPos);
						(sLeaser.sprites[startSprite + j * 2 + side] as TriangleMesh)!.MoveVertice(i * 4 + 3, segmentDrawPos + perpendicular * rad - dir * dist - camPos);
					}
					else (sLeaser.sprites[startSprite + j * 2 + side] as TriangleMesh)!.MoveVertice(i * 4 + 2, segmentDrawPos + dir * 2.1f - camPos);
					
					lastRad = rad;
					lastSegmentPos = segmentDrawPos;
				}
			}
		}
	}
	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		for (int a = 0; a < 2; a++)
			for (int i = 0; i < amount; i++)
				for (int c = 0; c < (sLeaser.sprites[startSprite + i * 2 + a] as TriangleMesh)!.verticeColors.Length; c++)
					(sLeaser.sprites[startSprite + i * 2 + a] as TriangleMesh)!.verticeColors[c] = lGraphics.effectColor;
	}

    public void WhiskerLight(float incrementAmount = 0.0001f)
    {
        if (lightUp < 1) lightUp += incrementAmount;
    }
    public void WhiskerUnLight(float decrementAmount = 0.0001f)
    {
        if (lightUp > 0) lightUp -= decrementAmount;
    }
    #region position help
    public Vector2 whiskerDir(int side, int m, float timeStacker)
    {
        float useDR = Mathf.Lerp(lGraphics.lastHeadDepthRotation, lGraphics.headDepthRotation, timeStacker);
        return Custom.RotateAroundOrigo(new Vector2(((side == 0) ? (-1f) : 1f) * (1f - Mathf.Abs(useDR)) * whiskerDirections[m].x + useDR * whiskerProps[m, 1], whiskerDirections[m].y).normalized, Custom.AimFromOneVectorToAnother(Vector2.Lerp(lGraphics.drawPositions[0, 1], lGraphics.drawPositions[0, 0], timeStacker), Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker)));
    }

    public Vector2 AnchorPoint(int side, int m, float timeStacker)
    {
        return Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker) + Custom.DegToVec(lGraphics.HeadRotation(timeStacker)) * 2.85f * lGraphics.iVars.headSize + whiskerDir(side, m, timeStacker);
    }
    #endregion
}

public class FlavoredWhiskers : BlandWhiskers
{
	public List<Color> BaseColors;
	public List<Color> LightUpColors;
	public FreedCosmeticTemplate.LizColorMode[] colorMode;
	public BlandWhiskers temp;
	public FlavoredWhiskers(BlandWhiskers template) : base(template.lGraphics, template.startSprite)
	{
		temp = template;
		colorMode = [FreedCosmeticTemplate.LizColorMode.HSL, FreedCosmeticTemplate.LizColorMode.HSL];
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
		Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		for (int j = 0; j < amount; j++)
			for (int side = 0; side < 2; side++)
				for (int i = 0; i < 4; i++)
					for (int v = i * 4; v < i * 4 + ((i == 3) ? 3 : 4); v++)
						(sLeaser.sprites[startSprite + j * 2 + side] as TriangleMesh)!.verticeColors[v] = Color.Lerp(lGraphics.HeadColor(timeStacker), new Color(1f, 1f, 1f), (i - 1) / 2f * Mathf.Lerp(whiskerLightUp[j, side, 1], whiskerLightUp[j, side, 0], timeStacker));
	}
	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		for (int a = 0; a < 2; a++)
			for (int i = 0; i < amount; i++)
				for (int c = 0; c < (sLeaser.sprites[startSprite + i * 2 + a] as TriangleMesh)!.verticeColors.Length; c++)
					(sLeaser.sprites[startSprite + i * 2 + a] as TriangleMesh)!.verticeColors[c] = lGraphics.effectColor;
	}
}