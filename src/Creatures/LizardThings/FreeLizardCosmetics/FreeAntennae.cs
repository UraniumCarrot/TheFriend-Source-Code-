using System;
using LizardCosmetics;
using RWCustom;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics;

public class FreeAntennae : Antennae
{
	public bool darkenWithHead;
	public bool darkenWithRoom;
	public Color? tipColor;
	public FreeAntennae(LizardGraphics graf, int startsprite, Color tint, Color? tipCol = null, bool darkenWithHead = false, bool darkenWithRoom = false) : base(graf,startsprite)
	{
		this.darkenWithHead = darkenWithHead;
		this.darkenWithRoom = darkenWithRoom;
		this.tipColor = tipCol;
        redderTint = tint;
    }
	
	public override void Update()
    {
	    float num = 0;
	    if (!lGraphics.lizard.Consious)
	    {
		    num = 0f;
	    }
	    float num2 = Mathf.Lerp(10f, 7f, length);
	    for (int i = 0; i < 2; i++)
	    {
		    for (int j = 0; j < segments; j++)
		    {
			    float a = j / (float)(segments - 1);
			    a = Mathf.Lerp(a, Mathf.InverseLerp(0f, 5f, j), 0.2f);
			    antennae[i, j].vel += AntennaDir(i, 1f) * (1f - a + 0.6f * num);
			    if (lGraphics.lizard.room.PointSubmerged(antennae[i, j].pos))
			    {
				    antennae[i, j].vel *= 0.8f;
			    }
			    else
			    {
				    antennae[i, j].vel.y -= 0.4f * a * (1f - num);
			    }

			    antennae[i, j].Update();
			    antennae[i, j].pos += Custom.RNV() * 3f * num;
			    Vector2 p;
			    if (j == 0)
			    {
				    antennae[i, j].vel += AntennaDir(i, 1f) * 5f;
				    p = lGraphics.head.pos;
				    antennae[i, j].ConnectToPoint(AnchorPoint(i, 1f), num2, push: true, 0f,
					    lGraphics.lizard.mainBodyChunk.vel, 0f, 0f);
			    }
			    else
			    {
				    p = ((j != 1) ? antennae[i, j - 2].pos : AnchorPoint(i, 1f));
				    Vector2 vector = Custom.DirVec(antennae[i, j].pos, antennae[i, j - 1].pos);
				    float num3 = Vector2.Distance(antennae[i, j].pos, antennae[i, j - 1].pos);
				    antennae[i, j].pos -= vector * (num2 - num3) * 0.5f;
				    antennae[i, j].vel -= vector * (num2 - num3) * 0.5f;
				    antennae[i, j - 1].pos += vector * (num2 - num3) * 0.5f;
				    antennae[i, j - 1].vel += vector * (num2 - num3) * 0.5f;
			    }

			    antennae[i, j].vel += Custom.DirVec(p, antennae[i, j].pos) * 3f * Mathf.Pow(1f - a, 0.3f);
			    if (j > 1)
			    {
				    antennae[i, j - 2].vel += Custom.DirVec(antennae[i, j].pos, antennae[i, j - 2].pos) * 3f *
				                              Mathf.Pow(1f - a, 0.3f);
			    }

			    if (!Custom.DistLess(lGraphics.head.pos, antennae[i, j].pos, 200f))
			    {
				    antennae[i, j].pos = lGraphics.head.pos;
			    }
		    }
	    }
    }

	public void FreeAntennaeColor(RoomCamera.SpriteLeaser sLeaser, float timeStacker)
	{
		Color col = lGraphics.HeadColor(timeStacker);
		Color moddedTint = redderTint;
		Color? moddedTip = tipColor;
		for (int i = 0; i < 2; i++)
			for (int j = 0; j < 2; j++)
				for (int k = 0; k < 2; k++)
				{
					var mesh = (TriangleMesh)sLeaser.sprites[Sprite(i, k)];
					for (var a = 0; a < mesh.verticeColors.Length; a++)
					{
						if (darkenWithHead)
						{
							var alpha = 1f - Mathf.Pow(0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lGraphics.lastBlink, lGraphics.blink, timeStacker) * 2f * (float)Math.PI), 1.5f + lGraphics.lizard.AI.excitement * 1.5f);
							if (lGraphics.headColorSetter != 0f)
								alpha = Mathf.Lerp(alpha, (lGraphics.headColorSetter > 0f) ? 1f : 0f, Mathf.Abs(lGraphics.headColorSetter));
							if (lGraphics.flicker > 10)
								alpha = lGraphics.flickerColor;
							alpha = Mathf.Lerp(alpha, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lGraphics.lastVoiceVisualization, lGraphics.voiceVisualization, timeStacker)), 0.75f), Mathf.Lerp(lGraphics.lastVoiceVisualizationIntensity, lGraphics.voiceVisualizationIntensity, timeStacker));
							

							var baseBrightness = Custom.RGB2HSL(col).z;
							var tintVec = Custom.RGB2HSL(redderTint);
							var tintBrightness = Mathf.Lerp(baseBrightness, tintVec.z, alpha);
							moddedTint = Custom.HSL2RGB(tintVec.x, tintVec.y, tintBrightness);
							if (moddedTip != null)
							{
								var tipVec = Custom.RGB2HSL(tipColor.Value);
								var tipBrightness = Mathf.Lerp(baseBrightness, tipVec.z, alpha);
								moddedTip = Custom.HSL2RGB(tipVec.x, tipVec.y, tipBrightness);
							}
						}

						if (darkenWithRoom)
						{
							moddedTint = Color.Lerp(moddedTint, palette.blackColor, palette.darkness);
							if (moddedTip != null)
								moddedTip = Color.Lerp(moddedTip.Value, palette.blackColor, palette.darkness);
						}

						mesh.verticeColors[a] = Color.Lerp(lGraphics.HeadColor(timeStacker), moddedTint,
							Mathf.Pow(a / (float)mesh.verticeColors.Length, 1.5f));
						if (moddedTip != null)
							mesh.verticeColors[a] = Color.Lerp(mesh.verticeColors[a], moddedTip.Value,
								Mathf.Pow(a / (float)mesh.verticeColors.Length, 2.8f));
					}
				}
	}

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
	    FreeAntennaeColor(sLeaser, timeStacker);
	    for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[Sprite(i,0)].MoveBehindOtherNode(sLeaser.sprites[lGraphics.SpriteHeadStart]);
			sLeaser.sprites[startSprite + i].color = lGraphics.HeadColor(timeStacker);
			Vector2 vector = Vector2.Lerp(Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker), AnchorPoint(i, timeStacker), 0.5f);
			float num = 1f;
			for (int j = 0; j < segments; j++)
			{
				float num3 = j / (float)(segments - 1);
				Vector2 vector2 = Vector2.Lerp(antennae[i, j].lastPos, antennae[i, j].pos, timeStacker);
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num4 = Vector2.Distance(vector2, vector) / 5f;
				float num5 = Mathf.Lerp(3f, 1f, Mathf.Pow(num3, 0.8f));
				for (int k = 0; k < 2; k++)
				{
					(sLeaser.sprites[Sprite(i, k)] as TriangleMesh)!.MoveVertice(j * 4, vector - vector3 * (num + num5) * 0.5f + normalized * num4 - camPos);
					(sLeaser.sprites[Sprite(i, k)] as TriangleMesh)!.MoveVertice(j * 4 + 1, vector + vector3 * (num + num5) * 0.5f + normalized * num4 - camPos);
					if (j < segments - 1)
					{
						(sLeaser.sprites[Sprite(i, k)] as TriangleMesh)!.MoveVertice(j * 4 + 2, vector2 - vector3 * num5 - normalized * num4 - camPos);
						(sLeaser.sprites[Sprite(i, k)] as TriangleMesh)!.MoveVertice(j * 4 + 3, vector2 + vector3 * num5 - normalized * num4 - camPos);
					}
					else
					{
						(sLeaser.sprites[Sprite(i, k)] as TriangleMesh)!.MoveVertice(j * 4 + 2, vector2 - camPos);
					}
				}
				num = num5;
				vector = vector2;
			}
		}
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
	    base.InitiateSprites(sLeaser,rCam);
	    for (int j = 0; j < 2; j++)
	    {
		    sLeaser.sprites[Sprite(j, 1)].shader = Custom.rainWorld.Shaders["Basic"];
	    }
    }
}