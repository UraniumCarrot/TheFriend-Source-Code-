using System.Collections.Generic;
using System.Linq;
using LizardCosmetics;
using static TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies.FreedCosmeticMethods;
using TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Dependencies;
using UnityEngine;

namespace TheFriend.Creatures.LizardThings.FreeLizardCosmetics.Unique;

public class FreeTailGeckoScales : TailGeckoScales, IFreedCosmetic, ISpriteOverridable
{
    public List<Color> MainColors = new List<Color>(); // Colors the main bulk of the tail
    public Color? BaseColor;
    public Color GlintColor; // Color for "reflective" light of shiny scales, unused by littlescale
    public GeckoType tailType;
    public LizColorMode[] colorMode => new LizColorMode[3];
    public bool darkenWithHead { get; }
    public float dark { get; set; }
    public string newSprite { get; }
    public float random; // Used for the world's tiniest adjustment for big shiny scales
    
    public enum GeckoType
    {
	    littleScale, // Uses the math for the tiny dots found on cyan and eel lizard tails
	    bigScaleRandom, // Uses the math for big scales, but doesn't limit their color variances
	    bigScaleShiny, // Big scales, forces them to have a shiny color
	    bigScaleFlat // Big scales, NEVER has a shiny color
    }

    public static GeckoType RandomGeckoType(FreeTailGeckoScales self, bool preferBigScales = false)
    { // Selects a random scale type, preferBigScales being true makes big scales have a 75% chance of generating
	    var a = preferBigScales ? Random.Range(0, 3) : Random.Range(0, 1);
	    switch (a)
	    {
		    case 0: return GeckoType.littleScale;
		    case 1: return GeckoType.bigScaleRandom;
		    case 2: return GeckoType.bigScaleShiny;
		    case 3: return GeckoType.bigScaleFlat;
	    }
	    return GeckoType.bigScaleRandom;
    }
    
    public FreeTailGeckoScales(
	    LizardGraphics lGraphics, 
	    int startSprite, 
	    GeckoType geckoType, 
	    IList<LizColorMode> colorMath, 
	    Color? baseColor = null, 
	    IList<Color> mainColors = null, 
	    Color? glintColor = default, 
	    bool pulseWithHead = false, 
	    uint lines = 0,
	    uint rows = 0,
	    string SpriteOverride = "Circle20") : base(lGraphics,startSprite)
    {
	    ColorModeGetter(this,colorMath);
	    if (rows != 0) this.rows = (int)rows; // how many rows and columns are present - left alone if not set
	    if (lines != 0) this.lines = (int)lines;
	    tailType = geckoType;
	    darkenWithHead = pulseWithHead;
	    newSprite = SpriteOverride;
	    bigScales = geckoType != GeckoType.littleScale;
	    random = Random.Range(0.05f, 1);
	    if (mainColors != null && mainColors.Any())
		    MainColors.AddRange(mainColors); 
	    if (!glintColor.HasValue) glintColor = Color.white;
	    GlintColor = glintColor.Value; // Color of the shiny white on bigscales. NOT USED by littleScale or bigScaleFlat
	    BaseColor = baseColor; // Color of the scales near the tail's base, usually darker than the rest of the tail
	    if (rows != 0 || lines != 0) numberOfSprites = this.rows * this.lines;
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < lines; j++)
                sLeaser.sprites[startSprite + i * lines + j].element = Futile.atlasManager.GetElementWithName(newSprite);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
        Vector2 camPos)
    {
	    if (bigScales)
		    DrawBigScale(sLeaser, rCam, timeStacker, camPos);
	    else DrawSmallScale(sLeaser, rCam, timeStacker, camPos);
    }

    public void DrawBigScale(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
	    Vector2 camPos)
    {
	    LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(0.4f, timeStacker);
	    for (int i = 0; i < rows; i++)
			{
				float num = Mathf.InverseLerp(0f, rows - 1, i);
				float num2 = Mathf.Lerp(0.5f, 0.99f, Mathf.Pow(num, 0.8f));
				LizardGraphics.LizardSpineData lizardSpineData2 = lGraphics.SpinePosition(num2, timeStacker);
				Color a = lGraphics.BodyColor(num2);
				a = BaseColor.HasValue ? BaseColor.Value : a;
				
				Color main = lGraphics.effectColor;
				if (MainColors.Any())
					if (colorMode[0]==LizColorMode.RGB)
						main = Extensions.RGBMultiLerp(MainColors, (float)i / rows);
					else main = Extensions.HSLMultiLerp(MainColors, (float)i / rows);
				List<Color> col1 = new List<Color>() { a, main };
				
				for (int j = 0; j < lines; j++)
				{
					float num3 = GeckoMathGen(i, j, timeStacker);

					Vector2 vector = lizardSpineData.pos + lizardSpineData.perp * (lizardSpineData.rad + 0.5f) * num3;
					Vector2 vector2 = lizardSpineData2.pos + lizardSpineData2.perp * (lizardSpineData2.rad + 0.5f) * num3;
					sLeaser.sprites[startSprite + i * lines + j].x = (vector.x + vector2.x) * 0.5f - camPos.x;
					sLeaser.sprites[startSprite + i * lines + j].y = (vector.y + vector2.y) * 0.5f - camPos.y;
					sLeaser.sprites[startSprite + i * lines + j].rotation = RWCustom.Custom.AimFromOneVectorToAnother(vector, vector2);
					sLeaser.sprites[startSprite + i * lines + j].scaleX = RWCustom.Custom.LerpMap(Mathf.Abs(num3), 0.4f, 1f, lizardSpineData2.rad * 3.5f / rows, 0f) / 10f;
					sLeaser.sprites[startSprite + i * lines + j].scaleY = Vector2.Distance(vector, vector2) * 1.1f / 20f;
					if (tailType == GeckoType.bigScaleShiny || (tailType == GeckoType.bigScaleRandom && lGraphics.iVars.tailColor > 0f))
					{ // Responsible for glinting tail scales
						float glisten = (lGraphics.iVars.tailColor > 0.05f) ? lGraphics.iVars.tailColor : random;
						float num4 = Mathf.InverseLerp(0.5f, 1f, Mathf.Abs(Vector2.Dot(RWCustom.Custom.DirVec(vector2, vector), RWCustom.Custom.DegToVec(-45f + 120f * num3))));
						num4 = RWCustom.Custom.LerpMap(Mathf.Abs(num3), 0.5f, 1f, 0.3f, 0f) + 0.7f * Mathf.Pow(num4 * Mathf.Pow(glisten, 0.3f), Mathf.Lerp(2f, 0.5f, num));
						if (num < 0.5f)
							num4 *= RWCustom.Custom.LerpMap(num, 0f, 0.5f, 0.2f, 1f);
						num4 = Mathf.Pow(num4, Mathf.Lerp(2f, 0.5f, num));

						List<Color> col2 = new List<Color>() { main, GlintColor };
						if (num4 < 0.5f)
							if (colorMode[1]==LizColorMode.RGB) // Base section
								sLeaser.sprites[startSprite + i * lines + j].color = Extensions.RGBMultiLerp(col1,Mathf.InverseLerp(0f, 0.5f, num4));
							else sLeaser.sprites[startSprite + i * lines + j].color = Extensions.HSLMultiLerp(col1,Mathf.InverseLerp(0f, 0.5f, num4));
						else if (colorMode[2]==LizColorMode.RGB) // Glint section
							sLeaser.sprites[startSprite + i * lines + j].color = Extensions.RGBMultiLerp(col2,Mathf.InverseLerp(0.5f, 1f, num4));
						else sLeaser.sprites[startSprite + i * lines + j].color = Extensions.HSLMultiLerp(col2,Mathf.InverseLerp(0.5f, 1f, num4));
					}
					else if (colorMode[1]==LizColorMode.RGB) // Flat coloration
						sLeaser.sprites[startSprite + i * lines + j].color = Extensions.RGBMultiLerp(col1,RWCustom.Custom.LerpMap(num, 0f, 0.8f, 0.2f, RWCustom.Custom.LerpMap(Mathf.Abs(num3), 0.5f, 1f, 0.8f, 0.4f), 0.8f));
					else sLeaser.sprites[startSprite + i * lines + j].color = Extensions.HSLMultiLerp(col1,RWCustom.Custom.LerpMap(num, 0f, 0.8f, 0.2f, RWCustom.Custom.LerpMap(Mathf.Abs(num3), 0.5f, 1f, 0.8f, 0.4f), 0.8f));
				}
				lizardSpineData = lizardSpineData2;
			}
    }

    public void DrawSmallScale(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker,
	    Vector2 camPos)
    {
	    for (int k = 0; k < rows; k++)
	    {
		    float f = Mathf.InverseLerp(0f, rows - 1, k);
		    float num5 = Mathf.Lerp(0.4f, 0.95f, Mathf.Pow(f, 0.8f));
		    LizardGraphics.LizardSpineData lizardSpineData3 = lGraphics.SpinePosition(num5, timeStacker);

		    var baseCol = BaseColor.HasValue ? BaseColor.Value : lGraphics.BodyColor(num5);
		    Color main = lGraphics.effectColor;
		    if (MainColors.Any())
			    if (colorMode[0]==LizColorMode.RGB)
				    main = Extensions.RGBMultiLerp(MainColors, (float)k / rows);
			    else main = Extensions.HSLMultiLerp(MainColors, (float)k / rows);
		    List<Color> col1 = new List<Color>() { baseCol, main };
		    
		    Color color;
		    if (colorMode[1] == LizColorMode.RGB)
			    color = Extensions.RGBMultiLerp(col1, 0.2f + 0.8f * Mathf.Pow(f, 0.5f));
		    else color = Extensions.HSLMultiLerp(col1, 0.2f + 0.8f * Mathf.Pow(f, 0.5f));
		    
		    for (int l = 0; l < lines; l++)
		    { 
			    float num6 = GeckoMathGen(k, l, timeStacker);
			    num6 = Mathf.Sign(num6) * Mathf.Pow(Mathf.Abs(num6), 0.6f);
			    
			    Vector2 vector3 = lizardSpineData3.pos + lizardSpineData3.perp * (lizardSpineData3.rad + 0.5f) * num6;
			    sLeaser.sprites[startSprite + k * lines + l].x = vector3.x - camPos.x;
			    sLeaser.sprites[startSprite + k * lines + l].y = vector3.y - camPos.y;
			    sLeaser.sprites[startSprite + k * lines + l].rotation = RWCustom.Custom.VecToDeg(lizardSpineData3.dir);
			    sLeaser.sprites[startSprite + k * lines + l].scaleX = RWCustom.Custom.LerpMap(Mathf.Abs(num6), 0.4f, 1f, 1f, 0f);
			    sLeaser.sprites[startSprite + k * lines + l].color = color;
		    }
	    }
    }

    public float GeckoMathGen(int row, int line, float timeStacker)
    {
	    float num = (line + ((row % 2 == 0) ? 0.5f : 0f)) / (lines - 1);
	    num = -1f + 2f * num;
	    num += Mathf.Lerp(lGraphics.lastDepthRotation, lGraphics.depthRotation, timeStacker);
	    if (num < -1f)
		    num += 2f;
	    else if (num > 1f)
		    num -= 2f;
	    return num;
    }
}