using System.Collections.Generic;
using LizardCosmetics;
using UnityEngine;
using RWCustom;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.CustomTemps;

public class BlandTailScales : Template
{
    public int rows;
    public int lines;
    public bool usingBigScales => type != GeckoType.littleScale;
    public GeckoType type;
    public enum GeckoType
    {
	    trueRandom, // Does not force scale type
        littleScale, // Uses the math for the tiny dots found on cyan and eel lizard tails
        bigScaleRandom, // Uses the math for big scales, but doesn't limit their color variances
        bigScaleShiny, // Big scales, forces them to have a shiny color
        bigScaleFlat // Big scales, NEVER has a shiny color
    }
    public BlandTailScales(LizardGraphics lGraphics, int startSprite, GeckoType type = GeckoType.trueRandom, int rows = 11, int lines = 3) : base(lGraphics, startSprite)
    {
	    if (type == GeckoType.trueRandom)
	    {
		    float random = Random.value;
		    if (random > 0.5f) this.type = GeckoType.littleScale;
		    else this.type = GeckoType.bigScaleRandom;
	    }
        spritesOverlap = SpritesOverlap.BehindHead;
        this.rows = rows;
        this.lines = lines;
        numberOfSprites = rows * lines;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        for (int i = 0; i < rows; i++)
            for (int a = 0; a < lines; a++)
                if (usingBigScales)
                {
                    sLeaser.sprites[startSprite + i * lines + a] = new FSprite("Circle20");
                    sLeaser.sprites[startSprite + i * lines + a].scaleY = 0.3f;
                }
                else
                {
	                sLeaser.sprites[startSprite + i * lines + a] = new FSprite("tinyStar");
                }
    }
    
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (type != GeckoType.littleScale)
		{
			LizardGraphics.LizardSpineData lastDt = lGraphics.SpinePosition(0.4f, timeStacker);
			for (int j = 0; j < rows; j++)
			{
				float f = Mathf.InverseLerp(0f, rows - 1, j);
				float spinePos = Mathf.Lerp(0.5f, 0.99f, Mathf.Pow(f, 0.8f));
				LizardGraphics.LizardSpineData dt = lGraphics.SpinePosition(spinePos, timeStacker);
				Color bodcol = lGraphics.BodyColor(spinePos);
				for (int a = 0; a < lines; a++)
				{
					float sidePos = (a + ((j % 2 == 0) ? 0.5f : 0f)) / (lines - 1);
					sidePos = -1f + 2f * sidePos;
					sidePos += Mathf.Lerp(lGraphics.lastDepthRotation, lGraphics.depthRotation, timeStacker);
					if (sidePos < -1f)
						sidePos += 2f;
					
					else if (sidePos > 1f)
						sidePos -= 2f;
					
					Vector2 lastDrwPos = lastDt.pos + lastDt.perp * (lastDt.rad + 0.5f) * sidePos;
					Vector2 drwPos = dt.pos + dt.perp * (dt.rad + 0.5f) * sidePos;
					sLeaser.sprites[startSprite + j * lines + a].x = (lastDrwPos.x + drwPos.x) * 0.5f - camPos.x;
					sLeaser.sprites[startSprite + j * lines + a].y = (lastDrwPos.y + drwPos.y) * 0.5f - camPos.y;
					sLeaser.sprites[startSprite + j * lines + a].rotation = Custom.AimFromOneVectorToAnother(lastDrwPos, drwPos);
					sLeaser.sprites[startSprite + j * lines + a].scaleX = Custom.LerpMap(Mathf.Abs(sidePos), 0.4f, 1f, dt.rad * 3.5f / rows, 0f) / 10f;
					sLeaser.sprites[startSprite + j * lines + a].scaleY = Vector2.Distance(lastDrwPos, drwPos) * 1.1f / 20f;
					if (type != GeckoType.bigScaleFlat && (lGraphics.iVars.tailColor > 0f || type == GeckoType.bigScaleShiny))
					{
						float tailCol = lGraphics.iVars.tailColor > 0f ? lGraphics.iVars.tailColor : 0.5f;
						float shine = Mathf.InverseLerp(0.5f, 1f, Mathf.Abs(Vector2.Dot(Custom.DirVec(drwPos, lastDrwPos), Custom.DegToVec(-45f + 120f * sidePos))));
						shine = Custom.LerpMap(Mathf.Abs(sidePos), 0.5f, 1f, 0.3f, 0f) + 0.7f * Mathf.Pow(shine * Mathf.Pow(tailCol, 0.3f), Mathf.Lerp(2f, 0.5f, f));
						if (f < 0.5f)
							shine *= Custom.LerpMap(f, 0f, 0.5f, 0.2f, 1f);
						
						shine = Mathf.Pow(shine, Mathf.Lerp(2f, 0.5f, f));
						if (shine < 0.5f)
							sLeaser.sprites[startSprite + j * lines + a].color = Color.Lerp(bodcol, lGraphics.effectColor, Mathf.InverseLerp(0f, 0.5f, shine));
						else
							sLeaser.sprites[startSprite + j * lines + a].color = Color.Lerp(lGraphics.effectColor, Color.white, Mathf.InverseLerp(0.5f, 1f, shine));
					}
					else
						sLeaser.sprites[startSprite + j * lines + a].color = Color.Lerp(bodcol, lGraphics.effectColor, Custom.LerpMap(f, 0f, 0.8f, 0.2f, Custom.LerpMap(Mathf.Abs(sidePos), 0.5f, 1f, 0.8f, 0.4f), 0.8f));
				}
				lastDt = dt;
			}
			return;
		}
		for (int i = 0; i < rows; i++)
		{
			float f2 = Mathf.InverseLerp(0f, rows - 1, i);
			float spinePos2 = Mathf.Lerp(0.4f, 0.95f, Mathf.Pow(f2, 0.8f));
			LizardGraphics.LizardSpineData dt2 = lGraphics.SpinePosition(spinePos2, timeStacker);
			Color col = Color.Lerp(lGraphics.BodyColor(spinePos2), lGraphics.effectColor, 0.2f + 0.8f * Mathf.Pow(f2, 0.5f));
			for (int a2 = 0; a2 < lines; a2++)
			{
				float sidePos2 = (a2 + ((i % 2 == 0) ? 0.5f : 0f)) / (lines - 1);
				sidePos2 = -1f + 2f * sidePos2;
				sidePos2 += Mathf.Lerp(lGraphics.lastDepthRotation, lGraphics.depthRotation, timeStacker);
				if (sidePos2 < -1f)
					sidePos2 += 2f;
				
				else if (sidePos2 > 1f)
					sidePos2 -= 2f;
				
				sidePos2 = Mathf.Sign(sidePos2) * Mathf.Pow(Mathf.Abs(sidePos2), 0.6f);
				Vector2 drwPos2 = dt2.pos + dt2.perp * (dt2.rad + 0.5f) * sidePos2;
				sLeaser.sprites[startSprite + i * lines + a2].x = drwPos2.x - camPos.x;
				sLeaser.sprites[startSprite + i * lines + a2].y = drwPos2.y - camPos.y;
				sLeaser.sprites[startSprite + i * lines + a2].rotation = Custom.VecToDeg(dt2.dir);
				sLeaser.sprites[startSprite + i * lines + a2].scaleX = Custom.LerpMap(Mathf.Abs(sidePos2), 0.4f, 1f, 1f, 0f);
				sLeaser.sprites[startSprite + i * lines + a2].color = col;
			}
		}
	}
}

public class FlavoredTailScales : BlandTailScales
{
	public string newSprite; // Allows a cosmetic's sprites to be overwritten
	public FreedCosmeticTemplate.LizColorMode[] colorMode = new FreedCosmeticTemplate.LizColorMode[2];
	public List<Color> BaseColors; // Colors from scales base to scales middle
	public List<Color> ShineColors; // Colors from scales middle to scales shine (if the lizard has shiny scales)
	public ToolMethods.MathMode sizeMathMode;
	public Vector2 drawSizeBonus; // Grants a size bonus to a cosmetic's sprites that is applied every frame

	public FlavoredTailScales(BlandTailScales template) : base(template.lGraphics, template.startSprite)
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < rows; i++)
			for (int a = 0; a < lines; a++)
			{
				if (usingBigScales)
				{
					sLeaser.sprites[startSprite + i * lines + a] = new FSprite(newSprite);
					sLeaser.sprites[startSprite + i * lines + a].scaleY = 0.3f;
				}
				else sLeaser.sprites[startSprite + i * lines + a] = new FSprite(newSprite);

				switch (sizeMathMode)
				{
					case ToolMethods.MathMode.add:
						sLeaser.sprites[startSprite + i * lines + a].scaleX += drawSizeBonus.x;
						sLeaser.sprites[startSprite + i * lines + a].scaleY += drawSizeBonus.y;
						break;
					case ToolMethods.MathMode.mult:
						sLeaser.sprites[startSprite + i * lines + a].scaleX *= drawSizeBonus.x;
						sLeaser.sprites[startSprite + i * lines + a].scaleY *= drawSizeBonus.y;
						break;
				}
			}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
		Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (type == GeckoType.littleScale)
			for (int i = 0; i < rows; i++)
			{
				float f2 = Mathf.InverseLerp(0f, rows - 1, i);
				float spinePos2 = Mathf.Lerp(0.4f, 0.95f, Mathf.Pow(f2, 0.8f));
				Color col = Color.Lerp(lGraphics.BodyColor(spinePos2), lGraphics.effectColor,
					0.2f + 0.8f * Mathf.Pow(f2, 0.5f));
				for (int a2 = 0; a2 < lines; a2++)
				{
					sLeaser.sprites[startSprite + i * lines + a2].color = col;
					switch (sizeMathMode)
					{
						case ToolMethods.MathMode.add:
							sLeaser.sprites[startSprite + i * lines + a2].scaleX += drawSizeBonus.x;
							break;
						case ToolMethods.MathMode.mult:
							sLeaser.sprites[startSprite + i * lines + a2].scaleX *= drawSizeBonus.x;
							break;
					}
				}
			}
		else
		{
			LizardGraphics.LizardSpineData lastDt = lGraphics.SpinePosition(0.4f, timeStacker);
			for (int j = 0; j < rows; j++)
			{
				float f = Mathf.InverseLerp(0f, rows - 1, j);
				float spinePos = Mathf.Lerp(0.5f, 0.99f, Mathf.Pow(f, 0.8f));
				LizardGraphics.LizardSpineData dt = lGraphics.SpinePosition(spinePos, timeStacker);
				for (int a = 0; a < lines; a++)
				{
					switch (sizeMathMode)
					{
						case ToolMethods.MathMode.add:
							sLeaser.sprites[startSprite + j * lines + a].scaleX += drawSizeBonus.x;
							sLeaser.sprites[startSprite + j * lines + a].scaleY += drawSizeBonus.y;
							break;
						case ToolMethods.MathMode.mult:
							sLeaser.sprites[startSprite + j * lines + a].scaleX *= drawSizeBonus.x;
							sLeaser.sprites[startSprite + j * lines + a].scaleY *= drawSizeBonus.y;
							break;
					}
					float sidePos = (a + ((j % 2 == 0) ? 0.5f : 0f)) / (lines - 1);
					sidePos = -1f + 2f * sidePos;
					sidePos += Mathf.Lerp(lGraphics.lastDepthRotation, lGraphics.depthRotation, timeStacker);
					if (sidePos < -1f)
						sidePos += 2f;

					else if (sidePos > 1f)
						sidePos -= 2f;

					Vector2 lastDrwPos = lastDt.pos + lastDt.perp * (lastDt.rad + 0.5f) * sidePos;
					Vector2 drwPos = dt.pos + dt.perp * (dt.rad + 0.5f) * sidePos;
					if (type != GeckoType.bigScaleFlat &&
					    (lGraphics.iVars.tailColor > 0f || type == GeckoType.bigScaleShiny))
					{
						float tailCol = lGraphics.iVars.tailColor > 0f ? lGraphics.iVars.tailColor : 0.5f;
						float shine = Mathf.InverseLerp(0.5f, 1f,
							Mathf.Abs(Vector2.Dot(Custom.DirVec(drwPos, lastDrwPos),
								Custom.DegToVec(-45f + 120f * sidePos))));
						shine = Custom.LerpMap(Mathf.Abs(sidePos), 0.5f, 1f, 0.3f, 0f) +
						        0.7f * Mathf.Pow(shine * Mathf.Pow(tailCol, 0.3f), Mathf.Lerp(2f, 0.5f, f));
						if (f < 0.5f)
							shine *= Custom.LerpMap(f, 0f, 0.5f, 0.2f, 1f);

						shine = Mathf.Pow(shine, Mathf.Lerp(2f, 0.5f, f));
						if (shine < 0.5f)
							switch (colorMode[0])
							{
								case FreedCosmeticTemplate.LizColorMode.HSL:
									sLeaser.sprites[startSprite + j * lines + a].color =
										Extensions.HSLMultiLerp(BaseColors, Mathf.InverseLerp(0f, 0.5f, shine));
									break;
								case FreedCosmeticTemplate.LizColorMode.RGB: 
									sLeaser.sprites[startSprite + j * lines + a].color =
										Extensions.RGBMultiLerp(BaseColors, Mathf.InverseLerp(0f, 0.5f, shine));
									break;
							}
						else
							switch (colorMode[1])
							{
								case FreedCosmeticTemplate.LizColorMode.HSL: 
									sLeaser.sprites[startSprite + j * lines + a].color =
										Extensions.HSLMultiLerp(ShineColors, Mathf.InverseLerp(0f, 0.5f, shine));
									break;
								case FreedCosmeticTemplate.LizColorMode.RGB: 
									sLeaser.sprites[startSprite + j * lines + a].color =
										Extensions.RGBMultiLerp(ShineColors, Mathf.InverseLerp(0f, 0.5f, shine));
									break;
							}
					}
					else switch (colorMode[0])
					{
						case FreedCosmeticTemplate.LizColorMode.HSL:
						sLeaser.sprites[startSprite + j * lines + a].color =
							Extensions.HSLMultiLerp(BaseColors, Mathf.InverseLerp(0f, 0.5f, Custom.LerpMap(f, 0f, 0.8f, 0.2f, Custom.LerpMap(Mathf.Abs(sidePos), 0.5f, 1f, 0.8f, 0.4f),
								0.8f)));
						break;
						case FreedCosmeticTemplate.LizColorMode.RGB: 
						sLeaser.sprites[startSprite + j * lines + a].color =
							Extensions.RGBMultiLerp(BaseColors, Mathf.InverseLerp(0f, 0.5f, Custom.LerpMap(f, 0f, 0.8f, 0.2f, Custom.LerpMap(Mathf.Abs(sidePos), 0.5f, 1f, 0.8f, 0.4f),
								0.8f)));
						break;
					}
				}
				lastDt = dt;
			}
		}
	}
}
