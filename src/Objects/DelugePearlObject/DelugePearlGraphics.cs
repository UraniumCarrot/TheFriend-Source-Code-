using SlugBase.DataTypes;
using TheFriend.SlugcatThings;
using UnityEngine;
using RWCustom;
using JollyColorMode = Options.JollyColorMode;
namespace TheFriend.Objects.DelugePearlObject;

public class DelugePearlGraphics
{
    public static void Apply()
    {
        On.DataPearl.DrawSprites += DataPearlOnDrawSprites;
    }

    public static void DataPearlOnDrawSprites(On.DataPearl.orig_DrawSprites orig, DataPearl self, RoomCamera.SpriteLeaser sleaser, RoomCamera rcam, float timestacker, Vector2 campos)
    {
        orig(self, sleaser, rcam, timestacker, campos);
        if (self.AbstractPearl.dataPearlType == TheFriend.DataPearlType.DelugePearl)
        {
            Color mainCol = self.AbstractPearl.DelugePearlData().color;

            Vector3 lightColVec = Custom.RGB2HSL(mainCol);
            lightColVec.y = 1f;
            lightColVec.z += 0.4f;
            lightColVec.x += 0.1f;

            Color lightCol = Custom.HSL2RGB(lightColVec.x, lightColVec.y, lightColVec.z);

            sleaser.sprites[0].color = mainCol;
            sleaser.sprites[2].color = mainCol;
            sleaser.sprites[1].color = lightCol;
        }
    }
    
    public static Color DelugePearlColor(PlayerGraphics self)
    {
        var col = new PlayerColor("Bauble").GetColor(self);
        if (!col.HasValue) return Color.magenta;
        else
        {
            Color pearlcolor = col.Value;
            if (Custom.rainWorld.options.jollyColorMode == JollyColorMode.AUTO &&
                self.player.playerState.playerNumber != 0)
            {
                Color jolly = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 2);
                pearlcolor = new Color(jolly.r, jolly.b, jolly.g);
            
                Vector3 mainColVec = Custom.RGB2HSL(pearlcolor);
                mainColVec.y = 1f;
                mainColVec.z = 0.5f;
                pearlcolor = Custom.HSL2RGB(mainColVec.x, mainColVec.y, mainColVec.z);
            }
            Debug.Log("Solace: Bauble colored!");
            return pearlcolor;   
        }
    }
}