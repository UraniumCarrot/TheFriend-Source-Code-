using UnityEngine;
using System.Linq;
using System;
using RWCustom;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;
using JollyColorMode = Options.JollyColorMode;
using SlugBase.DataTypes;
using TheFriend.SlugcatThings;

namespace TheFriend.CharacterThings.BelieverThings;

public class BelieverGraphics
{
    public static void BelieverSpritesInit(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        Array.Resize<FSprite>(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
        self.player.GetGeneral().customSprite1 = sLeaser.sprites.Length - 1;
        sLeaser.sprites[self.player.GetGeneral().customSprite1] = new FSprite("ForeheadSpotsA0");
        self.AddToContainer(sLeaser, rCam, null);
    }

    public static void BelieverSpritesContainer(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (self.player.GetGeneral().customSprite1 < sLeaser.sprites.Length)
        {
            if (newContainer == null) newContainer = rCam.ReturnFContainer("Midground");
            newContainer.AddChild(sLeaser.sprites[self.player.GetGeneral().customSprite1]);
            sLeaser.sprites[self.player.GetGeneral().customSprite1].MoveBehindOtherNode(sLeaser.sprites[9]);
        }
    }

    public static void BelieverPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (self.player.room?.game?.IsArenaSession == null) return;
        var color = new PlayerColor("Spots").GetColor(self);
        if (Custom.rainWorld.options.jollyColorMode == JollyColorMode.AUTO &&
            self.player.playerState.playerNumber != 0 && color != null)
        {
            Color jolly = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 2);
            Color colorvar = CharacterTools.ColorMaker(0.1f,1f,1f, CharacterTools.colormode.add, CharacterTools.colormode.mult, CharacterTools.colormode.mult, jolly);
            color = colorvar;
        }
        if (color != null) self.player.GetGeneral().customColor1 = color.Value;
        sLeaser.sprites[self.player.GetGeneral().customSprite1].color = self.player.GetGeneral().customColor1;
    }

    public static void BelieverDrawSprites(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, FSprite head, FSprite face)
    {
        if (!self.RenderAsPup)
        {
            if (!head.element.name.Contains("HeadB") && 
                head.element.name.StartsWith("HeadA")) 
                head.SetElementByName("HeadB" + (head.element.name.Remove(0,5)));
        }
        if (face.element.name.StartsWith("Face")) 
            sLeaser.sprites[self.player.GetGeneral().customSprite1].SetElementByName("ForeheadSpots" + (face.element.name.Remove(0,4)));
        sLeaser.sprites[self.player.GetGeneral().customSprite1].SetPosition(face.GetPosition());
        sLeaser.sprites[self.player.GetGeneral().customSprite1].scaleX = face.scaleX;
    }
}