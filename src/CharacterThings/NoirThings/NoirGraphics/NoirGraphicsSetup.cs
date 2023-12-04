using System.Collections.Generic;
using System.Linq;
using RWCustom;
using TheFriend.SlugcatThings;
using UnityEngine;

namespace TheFriend.NoirThings;

public partial class NoirCatto
{
    public static void LoadAtlases()
    {
        Futile.atlasManager.LoadAtlas("atlases/NoirHead");
        Futile.atlasManager.LoadAtlas("atlases/NoirFace");
        Futile.atlasManager.LoadAtlas("atlases/NoirEars");
        Futile.atlasManager.LoadAtlas("atlases/NoirBodyA");
        Futile.atlasManager.LoadAtlas("atlases/NoirHipsA");
        Futile.atlasManager.LoadAtlas("atlases/NoirLeftHipsA");
        Futile.atlasManager.LoadAtlas("atlases/NoirRightHipsA");
        Futile.atlasManager.LoadAtlas("atlases/NoirLegs");
        Futile.atlasManager.LoadAtlas("atlases/NoirPlayerArm");
        Futile.atlasManager.LoadAtlas("atlases/NoirTail");
    }

    #region Consts
    private static readonly string[] ValidSpriteNames = new[] { "Head", "Face", "PFace", "BodyA", "HipsA", "PlayerArm", "Legs", "Tail", "Futile_White" };

    private const string NoirHead = "NoirHead";
    private const string NoirEars = "NoirEars";
    private const string NoirFace = "NoirFace";
    private const string NoirBodyA = "NoirBodyA";
    private const string NoirPlayerArm = "NoirPlayerArm";
    private const string NoirHipsA = "NoirHipsA";
    private const string NoirLegs = "NoirLegs";
    private const string NoirTail = "NoirTail";
    private const string Noir = "Noir"; //Prefix for sprite replacement

    private const int HeadSpr = 3;
    private const int FaceSpr = 9;
    private const int BodySpr = 0;
    private const int ArmSpr = 5;
    private const int ArmSpr2 = 6;
    private const int OTOTArmSpr = 7;
    private const int OTOTArmSpr2 = 8;
    private const int HipsSpr = 1;
    private const int LegsSpr = 4;
    private const int TailSpr = 2;

    private const int TailLength = 7;
    public static readonly Color NoirWhite = new Color(0.695f, 0.695f, 0.695f);
    public static readonly Color NoirBlue = new Color(0.392156863f, 0.584313725f, 0.929411765f);
    #endregion

    private static List<int> SprToReplace = new List<int>()
    {
        HeadSpr, FaceSpr, BodySpr, ArmSpr, ArmSpr2, HipsSpr, LegsSpr, TailSpr
    };

    private const int NewSprites = 2;
    private static int TotalSprites;
    private static readonly int[] EarSpr = new int[2];


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
                    sleaser.sprites[num].element = Futile.atlasManager.GetElementWithName(NoirTail);
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

    private static Color ReColor(Color color, bool useNoirWhite)
    {
        color = Color.Lerp(color, Color.white, 0.5f);
        if (useNoirWhite)
        {
            color = Color.Lerp(color, NoirWhite, 0.5f);
        }
        return color;
    }

    private static Vector2 EarAttachPos(NoirData noirData, int earNum, float timestacker)
    {
        var graphics = (PlayerGraphics)noirData.Cat.graphicsModule;
        var numXMod = earNum == 0 ? -4 : 4;
        return Vector2.Lerp(graphics.head.lastPos + new Vector2(numXMod, 1.5f), graphics.head.pos + new Vector2(numXMod, 1.5f), timestacker) + Vector3.Slerp(noirData.LastHeadRotation, graphics.head.connection.Rotation, timestacker).ToVector2InPoints() * 15f;
    }
}