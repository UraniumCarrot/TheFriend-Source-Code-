using LizardCosmetics;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Unique;

public class FreeAntennae : Antennae, IFreedCosmetic
{ // Needs rework
	public LizColorMode[] colorMode => new LizColorMode[3];
	public bool darkenWithHead { get; }
	public float dark { get; set; }
	public bool darkenWithRoom;
	public Color? tipColor;
	public FreeAntennae(
		LizardGraphics graf, 
		int startsprite, 
		int length, 
		Color tint, 
		Color? tipCol = null, 
		bool darkenWithHead = false, 
		bool darkenWithRoom = false) : base(graf,startsprite)
	{
		this.length = length;
		segments = length;
		spritesOverlap = SpritesOverlap.BehindHead;
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
			    antennae[i, j].pos += RWCustom.Custom.RNV() * 3f * num;
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
				    Vector2 vector = RWCustom.Custom.DirVec(antennae[i, j].pos, antennae[i, j - 1].pos);
				    float num3 = Vector2.Distance(antennae[i, j].pos, antennae[i, j - 1].pos);
				    antennae[i, j].pos -= vector * (num2 - num3) * 0.5f;
				    antennae[i, j].vel -= vector * (num2 - num3) * 0.5f;
				    antennae[i, j - 1].pos += vector * (num2 - num3) * 0.5f;
				    antennae[i, j - 1].vel += vector * (num2 - num3) * 0.5f;
			    }

			    antennae[i, j].vel += RWCustom.Custom.DirVec(p, antennae[i, j].pos) * 3f * Mathf.Pow(1f - a, 0.3f);
			    if (j > 1)
			    {
				    antennae[i, j - 2].vel += RWCustom.Custom.DirVec(antennae[i, j].pos, antennae[i, j - 2].pos) * 3f *
				                              Mathf.Pow(1f - a, 0.3f);
			    }

			    if (!RWCustom.Custom.DistLess(lGraphics.head.pos, antennae[i, j].pos, 200f))
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
			for (int k = 0; k < 2; k++)
			{
				var mesh = (TriangleMesh)sLeaser.sprites[Sprite(i, k)];
				for (var a = 0; a < mesh.verticeColors.Length; a++)
				{
					var baseBrightness = RWCustom.Custom.RGB2HSL(col).z;
					var tintVec = RWCustom.Custom.RGB2HSL(redderTint);
					var tintBrightness = Mathf.Lerp(baseBrightness, tintVec.z, dark);
					moddedTint = RWCustom.Custom.HSL2RGB(tintVec.x, tintVec.y, tintBrightness);
					if (tipColor != null)
					{
						var tipVec = RWCustom.Custom.RGB2HSL(tipColor.Value);
						var tipBrightness = Mathf.Lerp(baseBrightness, tipVec.z, dark);
						moddedTip = RWCustom.Custom.HSL2RGB(tipVec.x, tipVec.y, tipBrightness);
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
			sLeaser.sprites[startSprite + i].color = lGraphics.HeadColor(timeStacker);
			Vector2 vector = Vector2.Lerp(Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker), AnchorPoint(i, timeStacker), 0.5f);
			float num = 1f;
			for (int j = 0; j < segments; j++)
			{
				float num3 = j / (float)(segments - 1);
				Vector2 vector2 = Vector2.Lerp(antennae[i, j].lastPos, antennae[i, j].pos, timeStacker);
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 vector3 = RWCustom.Custom.PerpendicularVector(normalized);
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
		    sLeaser.sprites[Sprite(j, 1)].shader = RWCustom.Custom.rainWorld.Shaders["Basic"];
	    }
    }
}