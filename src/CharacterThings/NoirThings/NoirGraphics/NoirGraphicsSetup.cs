using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Menu;
using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.CharacterThings.NoirThings;

public partial class NoirCatto
{
    public static void LoadAtlases()
    {
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirHead");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightHead");
        var eyeAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirFace");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightFace");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightNoseFace");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirBodyA");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightBodyA");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirHipsA");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightHipsA");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLeftHipsA");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightLeftHipsA");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirRightHipsA");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightRightHipsA");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLegs");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightLegs");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightPawLegs");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirPlayerArm");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightPlayerArm");
        Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirLightPawPlayerArm");
        var tailAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirTail");
        var earAtlas = Futile.atlasManager.LoadAtlas("atlases/NoirCatto/NoirEars");
        EyeTexture = (Texture2D)eyeAtlas.texture;
        TailTexture = (Texture2D)tailAtlas.texture;
        EarTexture = (Texture2D)earAtlas.texture;
        NoirBlueEyes = EyeTexture.GetPixels32().Where(c => c.a == 255).Distinct().Select(color32 => (Color)color32).ToArray();
    }

    #region Consts
    private static readonly string[] ValidSpriteNames = new[] { "Head", "Face", "PFace", "BodyA", "HipsA", "PlayerArm", "OnTopOfTerrainHand", "Legs", "Tail", "Futile_White" };

    private const string Head = "Head";
    private const string Face = "Face";
    private const string BodyA = "BodyA";
    private const string PlayerArm = "PlayerArm";
    private const string HipsA = "HipsA";
    private const string Legs = "Legs";

    private const string NoirHead = "NoirHead";
    private const string NoirEars = "NoirEars";
    private const string NoirFace = "NoirFace";
    private const string NoirBodyA = "NoirBodyA";
    private const string NoirPlayerArm = "NoirPlayerArm";
    private const string NoirHipsA = "NoirHipsA";
    private const string NoirLegs = "NoirLegs";
    private const string NoirTail = "NoirTail";
    private const string Noir = "Noir"; //Prefix for sprite replacement
    private const string NoirLight = "NoirLight"; //Prefix

    private const int BodySpr = 0; //Midground
    private const int HipsSpr = 1;
    private const int TailSpr = 2;
    private const int HeadSpr = 3;
    private const int LegsSpr = 4;
    private const int ArmSpr = 5;
    private const int ArmSpr2 = 6; //Midground
    private const int OTOTArmSpr = 7; //Foreground
    private const int OTOTArmSpr2 = 8; //Foreground
    private const int FaceSpr = 9; //Midground

    private const int LightBodySpr = 0;
    private const int LightHipsSpr = 1;
    private const int LightHeadSpr = 2;
    private const int LightLegsSpr = 3;
    private const int LightPawLegsSpr = 4;
    private const int LightArmSpr = 5;
    private const int LightPawArmSpr = 6;
    private const int LightArmSpr2 = 7;
    private const int LightPawArmSpr2 = 8;
    private const int LightFaceSpr = 9;
    private const int LightNoseFaceSpr = 10;

    private const int TailLength = 7;

    public static int CustomColorBody = 0; //Slugbase custom colors
    public static int CustomColorEyes = 1;
    public static int CustomColorFluff = 2;
    public static int CustomColorNose = 3;
    public static int CustomColorPaws = 4;

    public static readonly Color NoirWhite = Extensions.ColorFromHEX("e6e1e5");
    public static readonly Color NoirBlack = Extensions.ColorFromHEX("2f2e34");
    public static readonly Color NoirBlackPaws = Extensions.ColorFromHEX("2e2d33");
    public static readonly Color NoirPurple = Extensions.ColorFromHEX("6f5569");
    public static Color[] NoirBlueEyes;

    public static Texture2D EyeTexture;
    public static Texture2D TailTexture;
    public static Texture2D EarTexture;
    #endregion

    private static List<int> SprToReplace = new List<int>()
    {
        HeadSpr, FaceSpr, BodySpr, ArmSpr, ArmSpr2, OTOTArmSpr, OTOTArmSpr2,  HipsSpr, LegsSpr, TailSpr
    };


    //Helper methods
    private static void ReplaceSprites(RoomCamera.SpriteLeaser sleaser, PlayerGraphics self)
    {
        var noirData = self.player.GetNoir();

        foreach (var num in SprToReplace)
        {
            var spr = sleaser.sprites[num].element;

            if (!spr.name.StartsWith(Noir))
            {
                if (!ValidSpriteNames.Any(spr.name.StartsWith)) //For DMS compatibility :)
                {
                    continue;
                }

                if (num == HeadSpr) //Pup Fix
                {
                    if (!sleaser.sprites[num].element.name.Contains("HeadA"))
                    {
                        sleaser.sprites[num].element.name = spr.name.Replace("HeadB", "HeadA");
                        sleaser.sprites[num].element.name = spr.name.Replace("HeadC", "HeadA");
                        sleaser.sprites[num].element.name = spr.name.Replace("HeadD", "HeadA");
                    }
                }
                if (num == FaceSpr) //Pup Fix
                {
                    if (sleaser.sprites[num].element.name.Contains("PFace"))
                    {
                        sleaser.sprites[num].element.name = spr.name.Replace("PFace", "Face");
                    }
                }

                if (num == TailSpr)
                {

                }
                else
                {
                    sleaser.sprites[num].element = Futile.atlasManager.GetElementWithName(Noir + spr.name);
                }
            }
        }

        //It gets a bit messy here
        if (sleaser.sprites[HipsSpr].element.name.StartsWith(Noir))
        {
            if (!self.player.standing && (self.player.animation == Player.AnimationIndex.None || self.player.animation == Player.AnimationIndex.CrawlTurn) ||
                self.player.animation == Player.AnimationIndex.StandOnBeam && noirData.CanCrawlOnBeam())
            {
                var angle = Custom.AimFromOneVectorToAnother(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);

                if (angle is > 0 and < 120)
                {
                    sleaser.sprites[HipsSpr].element = Futile.atlasManager.GetElementWithName(Noir + "Left" + "HipsA");
                }
                else if (angle is < 0 and > -120)
                {
                    sleaser.sprites[HipsSpr].element = Futile.atlasManager.GetElementWithName(Noir + "Right" + "HipsA");
                }
                else
                {
                    sleaser.sprites[HipsSpr].element = Futile.atlasManager.GetElementWithName(Noir + "HipsA");
                }
            }
            else
            {
                sleaser.sprites[HipsSpr].element = Futile.atlasManager.GetElementWithName(Noir + "HipsA");
            }
        }
    }

    private static void AttachLightSprite(int lightSprIndex, int targetSprIndex, RoomCamera.SpriteLeaser sleaser, string lightPrefix = NoirLight)
    {
        if (!sleaser.sprites[targetSprIndex].element.name.StartsWith(Noir)) return;
        if (!sleaser.sprites[targetSprIndex].isVisible)
        {
            sleaser.sprites[lightSprIndex].isVisible = false;
            return;
        }
        sleaser.sprites[lightSprIndex].isVisible = true;
        sleaser.sprites[lightSprIndex].x = sleaser.sprites[targetSprIndex].x;
        sleaser.sprites[lightSprIndex].y = sleaser.sprites[targetSprIndex].y;
        sleaser.sprites[lightSprIndex].anchorX = sleaser.sprites[targetSprIndex].anchorX;
        sleaser.sprites[lightSprIndex].anchorY = sleaser.sprites[targetSprIndex].anchorY;
        sleaser.sprites[lightSprIndex].scaleX = sleaser.sprites[targetSprIndex].scaleX;
        sleaser.sprites[lightSprIndex].scaleY = sleaser.sprites[targetSprIndex].scaleY;
        sleaser.sprites[lightSprIndex].rotation = sleaser.sprites[targetSprIndex].rotation;
        sleaser.sprites[lightSprIndex].element = Futile.atlasManager.GetElementWithName(lightPrefix + sleaser.sprites[targetSprIndex].element.name.Substring(Noir.Length));
    }

    private static Color ReColor(Color color)
    {
        return Color.Lerp(color, Color.white, 0.5f);
    }

    private static Vector2 EarAttachPos(NoirData noirData, int earNum, float timestacker)
    {
        var graphics = (PlayerGraphics)noirData.Cat.graphicsModule;
        var numXMod = earNum == 0 ? -4 : 4;
        return Vector2.Lerp(graphics.head.lastPos + new Vector2(numXMod, 1.5f), graphics.head.pos + new Vector2(numXMod, 1.5f), timestacker) + Vector3.Slerp(noirData.LastHeadRotation, graphics.head.connection.Rotation, timestacker).ToVector2InPoints() * 15f;
    }
}