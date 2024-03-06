using UnityEngine;
using System.Linq;
using System;
using RWCustom;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;
using JollyColorMode = Options.JollyColorMode;
using SlugBase.DataTypes;
using TheFriend.CharacterThings;
using TheFriend.SlugcatThings;

namespace TheFriend.PoacherThings;

public class PoacherGraphics
{
    public static readonly SlugcatStats.Name DragonName = Plugin.DragonName;
    
    public static Color blackColor;

    public static void PoacherSpritesInit(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        Array.Resize<FSprite>(ref sLeaser.sprites, sLeaser.sprites.Length + 3);
        self.player.GetGeneral().customSprite1 = sLeaser.sprites.Length - 3;
        self.player.GetGeneral().customSprite2 = sLeaser.sprites.Length - 2;
        self.player.GetGeneral().customSprite3 = sLeaser.sprites.Length - 1;

        // Set default sprites
        sLeaser.sprites[self.player.GetGeneral().customSprite1] = new FSprite("dragonskull2A0");
        sLeaser.sprites[self.player.GetGeneral().customSprite2] = new FSprite("dragonskull2A11");
        sLeaser.sprites[self.player.GetGeneral().customSprite3] = new FSprite("dragonskull3A7");
        self.AddToContainer(sLeaser, rCam, null);
    }

    public static void PoacherSpritesContainer(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (self.player.GetGeneral().customSprite1 < sLeaser.sprites.Length)
        {
            if (newContainer == null) newContainer = rCam.ReturnFContainer("Midground");
            newContainer.AddChild(sLeaser.sprites[self.player.GetGeneral().customSprite1]);
            newContainer.AddChild(sLeaser.sprites[self.player.GetGeneral().customSprite2]);
            newContainer.AddChild(sLeaser.sprites[self.player.GetGeneral().customSprite3]);
            sLeaser.sprites[self.player.GetGeneral().customSprite2].MoveBehindOtherNode(sLeaser.sprites[0]);
            sLeaser.sprites[self.player.GetGeneral().customSprite3].MoveInFrontOfOtherNode(sLeaser.sprites[self.player.GetGeneral().customSprite1]);
        }
    }
    
    public static void PoacherPalette(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam,
        RoomPalette palette)
    {
        if (self.player.room?.game?.IsArenaSession == null) return;
        blackColor = palette.blackColor;
        var color = new PlayerColor("Skull").GetColor(self);
        var color2 = new PlayerColor("Symbol").GetColor(self);
        if (Custom.rainWorld.options.jollyColorMode == JollyColorMode.AUTO &&
            self.player.playerState.playerNumber != 0 && color != null && color2 != null)
        {
            Color jolly = PlayerGraphics.JollyColor(self.player.playerState.playerNumber, 2);
            color = new Color(jolly.r - 0.4f, jolly.b - 0.4f, jolly.g - 0.4f);
            Color colorvar = CharacterTools.ColorMaker(0.3f, 1f, 0.5f, ToolMethods.MathMode.add, ToolMethods.MathMode.set, ToolMethods.MathMode.set, jolly);
            color2 = colorvar;
        }

        if (color != null) self.player.GetGeneral().customColor1 = color.Value;
        if (color2 != null) self.player.GetGeneral().customColor2 = color2.Value;
    }
    
    public static void PoacherFlicker(Player self, bool stopSound = false)
    {
        self.GetPoacher().flicker = Custom.IntClamp(200 / 3, 3, 15);
        if (!stopSound) self.room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, self.firstChunk, false, 1, 1);
    }
    public static void PoacherThinness(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * (float)Math.PI * 2f);
        Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
        Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
        float num2 = Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(Custom.DirVec(vector2, vector).y));

        sLeaser.sprites[0].scaleX = 0.8f + Mathf.Lerp(Mathf.Lerp(Mathf.Lerp(-0.05f, -0.15f, self.malnourished), 0.05f, num) * num2, 0.15f, self.player.sleepCurlUp);
        sLeaser.sprites[1].scaleX = 0.8f + self.player.sleepCurlUp * 0.2f + 0.05f * num - 0.05f * self.malnourished;
        for (int i = 0; i < 2; i++)
        {
            float num9 = 4.5f / (self.hands[i].retractCounter + 1f);
            Vector2 vector10 = Vector2.Lerp(self.hands[i].lastPos, self.hands[i].pos, timeStacker);
            Vector2 vector11 = vector + Custom.RotateAroundOrigo(new Vector2((-1f + 2f * (float)i) * (num9 * 0.6f), -3.5f), Custom.AimFromOneVectorToAnother(vector2, vector));
            sLeaser.sprites[5 + i].element = Futile.atlasManager.GetElementWithName("PlayerArm" + Mathf.RoundToInt(Mathf.Clamp(Vector2.Distance(vector10, vector11) / 2f, 0f, 12f)));
            sLeaser.sprites[5 + i].rotation = Custom.AimFromOneVectorToAnother(vector10, vector11) + 90f;
        }
    }

    public static Color PoacherHypothermiaColor(Player self, Color oldCol)
    {
        float hypothermia = (self.abstractPhysicalObject as AbstractCreature).Hypothermia;
        Color b = !(hypothermia < 1f) ? Color.Lerp(new Color(0.8f, 0.8f, 1f), new Color(0.15f, 0.15f, 0.3f), hypothermia - 1f) : Color.Lerp(oldCol, new Color(0.8f, 0.8f, 1f), hypothermia);
        return Color.Lerp(oldCol, b, 0.92f);
    }
    public static void PoacherAnimator(PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var head = sLeaser.sprites[3];
        
        var general = self.player.GetGeneral();
        var poacher = self.player.GetPoacher(); 
        var skullpos1 = general.customSprite1;
        var skullpos2 = general.customSprite2;
        var skullpos3 = general.customSprite3;
        var flicker = poacher.flicker;
        var headToBody = (new Vector2(sLeaser.sprites[1].x, sLeaser.sprites[1].y) - new Vector2(sLeaser.sprites[3].x, sLeaser.sprites[3].y)).normalized;
        var skullPos = new Vector2(sLeaser.sprites[3].x + headToBody.x * 7.5f, sLeaser.sprites[3].y + headToBody.y * 7.5f);
        float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBreath, self.breath, timeStacker) * (float)Math.PI * 2f);
        Vector2 vector = Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker);
        Vector2 vector2 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
        float num2 = Mathf.InverseLerp(0.3f, 0.5f, Mathf.Abs(Custom.DirVec(vector2, vector).y));

            // Redone skull animation code
        Color origColor = general.customColor1;
        Color origColor2 = general.customColor2;
        Color origColorDark = Color.Lerp(origColor, blackColor, 0.2f);
        var atlas = Futile.atlasManager;
        string dragon = "dragonskull2A";
        string dragon3 = "dragonskull3A";
        float mainscale = 1.2f; 
        float mainX = self.player.flipDirection * mainscale;
        float mainY = mainscale;
        float roll = self.player.rollDirection * mainscale;
        float speed = Mathf.Sign(self.player.firstChunk.vel.x) * mainscale;
        var headname = sLeaser.sprites[3].element.name;
        for (int h = 0; h < 3; h++)
        { 
            sLeaser.sprites[skullpos1 + h].rotation = Mathf.Lerp(sLeaser.sprites[3].rotation, sLeaser.sprites[3].rotation * 8.5f, self.player.sleepCurlUp * 0.1f);
            sLeaser.sprites[skullpos1 + h].scaleX = mainX;
            sLeaser.sprites[skullpos1 + h].scaleY = mainY;
            sLeaser.sprites[skullpos1 + h].x = skullPos.x;
            sLeaser.sprites[skullpos1 + h].y = skullPos.y;
        }
        if (flicker > 0)
        {
            poacher.flicker--;
            Color white = Color.Lerp(Color.white, blackColor, 0.3f);
            float blink = UnityEngine.Random.value;
            if (blink > 0.5f) { sLeaser.sprites[skullpos1].color = Color.white; sLeaser.sprites[skullpos2].color = white; sLeaser.sprites[skullpos3].color = white; }
            else { sLeaser.sprites[skullpos1].color = origColor; sLeaser.sprites[skullpos2].color = origColorDark; sLeaser.sprites[skullpos3].color = origColor2; }
        }
        else { sLeaser.sprites[skullpos1].color = origColor; sLeaser.sprites[skullpos2].color = origColorDark; sLeaser.sprites[skullpos3].color = origColor2; }

        //layer2 visibility
        if (sLeaser.sprites[skullpos1].element.name != "dragonskull2A7")
        {
            sLeaser.sprites[skullpos2].isVisible = true;
            if (sLeaser.sprites[skullpos1].element.name == "dragonskull2A4") sLeaser.sprites[skullpos2].element = atlas.GetElementWithName(dragon + "11");
            else if (sLeaser.sprites[skullpos1].element.name == "dragonskull2A5") sLeaser.sprites[skullpos2].element = atlas.GetElementWithName(dragon + "12");
            else sLeaser.sprites[skullpos2].element = atlas.GetElementWithName(dragon + "10");
        }
        else sLeaser.sprites[skullpos2].isVisible = false;

        //layer3 animation match
        var element = sLeaser.sprites[skullpos1].element.name;
        if (element == "dragonskull2A17") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "17"); sLeaser.sprites[skullpos3].isVisible = true; }
        else if (element == "dragonskull2A16") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "16"); sLeaser.sprites[skullpos3].isVisible = true; }
        else if (element == "dragonskull2A15") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "15"); sLeaser.sprites[skullpos3].isVisible = true; }
        else if (element == "dragonskull2A14") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "14"); sLeaser.sprites[skullpos3].isVisible = true; }
        else if (element == "dragonskull2A9") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "9"); sLeaser.sprites[skullpos3].isVisible = true; }
        else if (element == "dragonskull2A8") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "8"); sLeaser.sprites[skullpos3].isVisible = true; }
        else if (element == "dragonskull2A7") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "7"); sLeaser.sprites[skullpos3].isVisible = true; }
        else if (element == "dragonskull2A5") { sLeaser.sprites[skullpos3].element = atlas.GetElementWithName(dragon3 + "5"); sLeaser.sprites[skullpos3].isVisible = true; }
        else sLeaser.sprites[skullpos3].isVisible = false;



        //default frames
        string str = headname.Substring(headname.IndexOf("C"));
        switch (str)
        {
            case "C0" or "C1" or "C2" or "C3": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "0"); break;
            case "C4": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "4"); break;
            case "C5" or "C6": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "5"); break;
            case "C14": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "14"); sLeaser.sprites[skullpos1].scaleX = sLeaser.sprites[3].scaleX * mainscale; break;
            case "C15": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "15"); sLeaser.sprites[skullpos1].scaleX = sLeaser.sprites[3].scaleX * mainscale; break;
            case "C16": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "16"); break;
            case "C17": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "17"); break;
            case "C7" or "C8" or "C9": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "7"); sLeaser.sprites[skullpos1].scaleX = sLeaser.sprites[3].scaleX * mainscale; break;
        }
        var a = self.player.animation;
        var b = self.player.bodyMode;
        // Dynamics
        string anim = a.value;
        if (a.value.Any())
        {
            switch (anim)
            {
                case "Flip": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "5"); sLeaser.sprites[skullpos1].scaleX = self.player.ThrowDirection * mainscale; break;
                case "Roll": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "5"); sLeaser.sprites[skullpos1].scaleX = roll; break;
                case "RocketJump": sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "0"); break;
                case "HangUnderVerticalBeam": sLeaser.sprites[skullpos1].scaleX = -mainX; sLeaser.sprites[skullpos2].scaleX = self.player.ThrowDirection * mainscale; break;
                case "GetUpOnBeam": sLeaser.sprites[skullpos1].scaleX = -speed; break; 
                case "AntlerClimb": sLeaser.sprites[skullpos1].scaleX = speed; break;
                case "CorridorTurn":
                    sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "7");
                    sLeaser.sprites[skullpos1].scaleX = Mathf.Sign(self.player.corridorTurnDir.Value.x) * -mainscale;
                    break;
            }
        }
        switch (self.player.sleepCurlUp)
        {
            case < 0.2f and not 0: sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "4"); break;
            case > 0.2f and < 0.9f: sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "5"); break;
            case > 0.9f: sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "7"); break;
        }
        if (self.player.Sleeping) 
            sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "7");
        
        if (b == bod.CorridorClimb && (headname.EndsWith("C9") || headname.EndsWith("C8") || headname.EndsWith("C7") || headname.EndsWith("C6") || headname.EndsWith("C9") || headname.EndsWith("C10") || headname.EndsWith("C5")))
        {
            sLeaser.sprites[skullpos1].element = atlas.GetElementWithName(dragon + "7");
            sLeaser.sprites[skullpos1].scaleX = sLeaser.sprites[3].scaleX * mainscale;
        }
        //layer3 scale match
        sLeaser.sprites[skullpos3].scaleX = sLeaser.sprites[skullpos1].scaleX;
    }
}