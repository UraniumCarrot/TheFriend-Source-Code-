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
    public static void Apply()
    {
        
    }

    public static void BelieverPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (self.player.room?.game?.IsArenaSession == null) return;
        var color = new PlayerColor("Spots").GetColor(self);
        if (Custom.rainWorld.options.jollyColorMode == JollyColorMode.AUTO &&
            self.player.playerState.playerNumber != 0 && color != null)
        {
            Color jolly = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 2);
            Color colorvar = SlugcatGraphics.ColorMaker(0.1f,1f,1f, SlugcatGraphics.colormode.add, SlugcatGraphics.colormode.mult, SlugcatGraphics.colormode.mult, jolly);
            color = colorvar;
        }
        if (color != null) self.player.GetGeneral().customColor1 = color.Value;
        sLeaser.sprites[self.player.GetGeneral().customSprite1].color = self.player.GetGeneral().customColor1;
    }
}