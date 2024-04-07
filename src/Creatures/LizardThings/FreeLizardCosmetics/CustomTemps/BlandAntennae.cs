using System.Collections.Generic;
using LizardCosmetics;
using UnityEngine;
using RWCustom;
using TheFriend.Creatures.LizardThings.DragonRideThings;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.CustomTemps;

public class BlandAntennae : Template
{
    public GenericBodyPart[,] antennae;
    public int segments;
    public float length;
    public float alpha;
    public float shiver;
    public int Sprite(int side, int part)
    {
        return startSprite + part * 2 + side;
    }
    
    public BlandAntennae(LizardGraphics lGraphics, int startSprite, float length = 0) : base(lGraphics, startSprite)
    {
        spritesOverlap = SpritesOverlap.InFront;
        this.length = (length > 0) ? length : Random.value;
        segments = Mathf.FloorToInt(Mathf.Lerp(3f, 8f, Mathf.Pow(length, Mathf.Lerp(1f, 6f, length))));
        antennae = new GenericBodyPart[2, segments];
        alpha = length * 0.9f + Random.value * 0.1f;

        for (int i = 0; i < segments; i++)
        {
            antennae[0, i] = new GenericBodyPart(lGraphics, 1f, 0.6f, 0.9f, lGraphics.lizard.mainBodyChunk);
            antennae[1, i] = new GenericBodyPart(lGraphics, 1f, 0.6f, 0.9f, lGraphics.lizard.mainBodyChunk);
        }
        numberOfSprites = 4;
    }

    public override void Reset()
    {
        base.Reset();
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < segments; j++)
                antennae[i, j].Reset(AnchorPoint(i, 1f));
    }
    public override void Update()
    {
        float flicker = shiver;
        if (!lGraphics.lizard.Consious)
            shiver = 0f;
        
        float idealDist = Mathf.Lerp(10f, 7f, length);
        for (int a = 0; a < 2; a++)
            for (int i = 0; i < segments; i++)
            {
                float f = (float)i / (segments - 1);
                f = Mathf.Lerp(f, Mathf.InverseLerp(0f, 5f, i), 0.2f);
                antennae[a, i].vel += AntennaDir(a, 1f) * (1f - f + 0.6f * flicker);
                if (lGraphics.lizard.room.PointSubmerged(antennae[a, i].pos))
                    antennae[a, i].vel *= 0.8f;
                else
                    antennae[a, i].vel.y -= 0.4f * f * (1f - flicker);
                antennae[a, i].Update();
                antennae[a, i].pos += Custom.RNV() * 3f * flicker;
                Vector2 pushPos;
                if (i == 0)
                {
                    antennae[a, i].vel += AntennaDir(a, 1f) * 5f;
                    pushPos = lGraphics.head.pos;
                    antennae[a, i].ConnectToPoint(AnchorPoint(a, 1f), idealDist, push: true, 0f, lGraphics.lizard.mainBodyChunk.vel, 0f, 0f);
                }
                else
                {
                    pushPos = ((i != 1) ? antennae[a, i - 2].pos : AnchorPoint(a, 1f));
                    Vector2 dir = Custom.DirVec(antennae[a, i].pos, antennae[a, i - 1].pos);
                    float dist = Vector2.Distance(antennae[a, i].pos, antennae[a, i - 1].pos);
                    antennae[a, i].pos -= dir * (idealDist - dist) * 0.5f;
                    antennae[a, i].vel -= dir * (idealDist - dist) * 0.5f;
                    antennae[a, i - 1].pos += dir * (idealDist - dist) * 0.5f;
                    antennae[a, i - 1].vel += dir * (idealDist - dist) * 0.5f;
                }
                antennae[a, i].vel += Custom.DirVec(pushPos, antennae[a, i].pos) * 3f * Mathf.Pow(1f - f, 0.3f);
                if (i > 1)
                    antennae[a, i - 2].vel += Custom.DirVec(antennae[a, i].pos, antennae[a, i - 2].pos) * 3f * Mathf.Pow(1f - f, 0.3f);
                if (!Custom.DistLess(lGraphics.head.pos, antennae[a, i].pos, 200f))
                    antennae[a, i].pos = lGraphics.head.pos;
            }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
	    for (int j = 0; j < 2; j++)
		    for (int a = 0; a < 2; a++)
			    sLeaser.sprites[Sprite(j, a)] = TriangleMesh.MakeLongMesh(segments, pointyTip: true, customColor: true);
	    for (int i = 0; i < 2; i++)
		    sLeaser.sprites[Sprite(i, 1)].shader = rCam.room.game.rainWorld.Shaders["LizardAntenna"];
    }
	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		for (int side = 0; side < 2; side++)
		{
			sLeaser.sprites[startSprite + side].color = lGraphics.HeadColor(timeStacker);
			Vector2 lastSegmentPos = Vector2.Lerp(Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker), AnchorPoint(side, timeStacker), 0.5f);
			float lastRad = 1f;
			for (int i = 0; i < segments; i++)
			{
				float f = (float)i / (segments - 1);
				Vector2 segmentDrawPos = Vector2.Lerp(antennae[side, i].lastPos, antennae[side, i].pos, timeStacker);
				Vector2 dir = (segmentDrawPos - lastSegmentPos).normalized;
				Vector2 perpendicular = Custom.PerpendicularVector(dir);
				float dist = Vector2.Distance(segmentDrawPos, lastSegmentPos) / 5f;
				float rad = Mathf.Lerp(3f, 1f, Mathf.Pow(f, 0.8f));
				for (int a = 0; a < 2; a++)
				{
					(sLeaser.sprites[Sprite(side, a)] as TriangleMesh)!.MoveVertice(i * 4, lastSegmentPos - perpendicular * (lastRad + rad) * 0.5f + dir * dist - camPos);
					(sLeaser.sprites[Sprite(side, a)] as TriangleMesh)!.MoveVertice(i * 4 + 1, lastSegmentPos + perpendicular * (lastRad + rad) * 0.5f + dir * dist - camPos);
					(sLeaser.sprites[Sprite(side, a)] as TriangleMesh)!.verticeColors[i * 4] = Color.red;
					(sLeaser.sprites[Sprite(side, a)] as TriangleMesh)!.verticeColors[i * 4 + 1] = Color.blue;
					(sLeaser.sprites[Sprite(side, a)] as TriangleMesh)!.verticeColors[i * 4 + 2] = Color.green;
					if (i < segments - 1)
					{
						(sLeaser.sprites[Sprite(side, a)] as TriangleMesh)!.MoveVertice(i * 4 + 2, segmentDrawPos - perpendicular * rad - dir * dist - camPos);
						(sLeaser.sprites[Sprite(side, a)] as TriangleMesh)!.MoveVertice(i * 4 + 3, segmentDrawPos + perpendicular * rad - dir * dist - camPos);
						(sLeaser.sprites[Sprite(side, a)] as TriangleMesh)!.verticeColors[i * 4 + 3] = Color.yellow;
					}
					else (sLeaser.sprites[Sprite(side, a)] as TriangleMesh)!.MoveVertice(i * 4 + 2, segmentDrawPos - camPos);
				}
				lastRad = rad;
				lastSegmentPos = segmentDrawPos;
			}
		}
	}
    
    public void AntennaeShiver(float incrementAmount = 0.0001f)
    {
        if (shiver < 1) shiver += incrementAmount;
    }

    public void AntennaeUnShiver(float decrementAmount = 0.0001f)
    {
        if (shiver > 0) shiver -= decrementAmount;
    }
    
    #region position help
    public Vector2 AntennaDir(int side, float timeStacker)
    {
        float num = Mathf.Lerp(lGraphics.lastHeadDepthRotation, lGraphics.headDepthRotation, timeStacker);
        return Custom.RotateAroundOrigo(new Vector2(((side == 0) ? (-1f) : 1f) * (1f - Mathf.Abs(num)) * 1.5f + num * 3.5f, -1f).normalized, Custom.AimFromOneVectorToAnother(Vector2.Lerp(lGraphics.drawPositions[0, 1], lGraphics.drawPositions[0, 0], timeStacker), Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker)));
    }
    public Vector2 AnchorPoint(int side, float timeStacker)
    {
        return Vector2.Lerp(lGraphics.drawPositions[0, 1], lGraphics.drawPositions[0, 0], timeStacker) + AntennaDir(side, timeStacker) * 3f * lGraphics.iVars.headSize;
    }
    #endregion
}

public class FreeBlandAntennae : BlandAntennae
{
	public bool ImReactive;
	public float dark;
	public float CycleSpeed;
	public FreedCosmeticTemplate.LizColorMode colorMode;
	public List<Color> AntennaeColors;
	public FreeBlandAntennae(LizardGraphics lGraphics, int startSprite) : base(lGraphics, startSprite)
	{
		colorMode = FreedCosmeticTemplate.LizColorMode.HSL;
		AntennaeColors = [Color.red];
	}
	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
		Vector2 camPos)
	{
		// Controls color/fadesprite reactivity, can be either be synced with lizard's head, staticly at 0, or on a timer
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

		if (ImReactive) dark = lGraphics.lizard.Liz().dark;
		else if (CycleSpeed == 0) dark = 0;
		else
		{
			dark += CycleSpeed;
			if (dark > 1) dark = 0;
		}
		FreeAntennaeColor(sLeaser,timeStacker);
	}
	public void FreeAntennaeColor(RoomCamera.SpriteLeaser sLeaser, float timeStacker)
	{
		Color col = sLeaser.sprites[lGraphics.SpriteHeadStart].color;
		
		for (int i = 0; i < 2; i++)
			for (int k = 0; k < 2; k++)
			{
				var mesh = (TriangleMesh)sLeaser.sprites[Sprite(i, k)];
				for (var a = 0; a < mesh.verticeColors.Length; a++)
					switch (colorMode)
					{
						case FreedCosmeticTemplate.LizColorMode.HSL:
							mesh.verticeColors[a] = Color.Lerp(col,
								Extensions.HSLMultiLerp(AntennaeColors, ((float)a / mesh.verticeColors.Length) + dark),
								Mathf.Pow(a / (float)mesh.verticeColors.Length, 1.5f));
							break;
						case FreedCosmeticTemplate.LizColorMode.RGB:
							mesh.verticeColors[a] = Color.Lerp(col,
								Extensions.RGBMultiLerp(AntennaeColors, ((float)a / mesh.verticeColors.Length) + dark),
								Mathf.Pow(a / (float)mesh.verticeColors.Length, 1.5f));
							break;
					}
			}
	}
}