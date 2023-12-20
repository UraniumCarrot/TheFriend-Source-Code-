using System;
using Menu;
using On.RWCustom;
using UnityEngine;
using MenuObject = Menu.MenuObject;

namespace TheFriend;

public class MainMenu
{
    public static void Apply()
    {
        On.Menu.IntroRoll.RawUpdate += IntroRollOnRawUpdate;
        On.MenuMicrophone.PlaySound_SoundID_float_float_float += MenuMicrophoneOnPlaySound_SoundID_float_float_float;
        On.Menu.RainEffect.ctor += RainEffectOnctor;
        On.Menu.RainEffect.GrafUpdate += RainEffectOnGrafUpdate;
        On.Menu.Menu.Update += MenuOnUpdate;
    }
    
    private static float blizzardTicker = 0f; // Represents your Y position on a cos wave
    private static float blizzardTimeTicker = 0f; // Responsible for blizzard brightness and volume; gets paused when player hasnt done an input yet. Represents your X position on a cos wave
    public static void MenuOnUpdate(On.Menu.Menu.orig_Update orig, Menu.Menu self)
    {
        orig(self);
        blizzardTicker = ((Mathf.Cos(blizzardTimeTicker*3.14f) * -1)/2) + 0.5f;
        float volume = blizzardTicker * 0.6f;
        Debug.Log("blizzticker: " + blizzardTicker + ", blizztimer: " + blizzardTimeTicker);
        if (self.manager.menuMic != null && self is IntroRoll)
            {
                if (self.soundLoop == null)
                    self.soundLoop = self.manager.menuMic.PlayLoop(
                    MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.Sleep_Blizzard_Loop, 
                    0f, 
                    volume, 
                    1, 
                    true);
            }
    }

    public static void RainEffectOnGrafUpdate(On.Menu.RainEffect.orig_GrafUpdate orig, Menu.RainEffect self, float timestacker)
    {
        orig(self, timestacker);
        var sin = 330 + Mathf.Sin(Time.time * Mathf.Deg2Rad * 50f) * 6;
        self.bkg.rotation = sin;
        
        self.bkg.scale = 50;
        self.bkg.scaleX = 200;
        self.bkg.scaleY = 200;
        self.bkg.x = self.menu.manager.rainWorld.screenSize.x/2;
        self.bkg.y = self.menu.manager.rainWorld.screenSize.y/2;
        self.bkg.shader = RWCustom.Custom.rainWorld.Shaders["LocalBlizzard"];
        
        if (blizzardTicker !>= 1) self.bkg.color = Color.Lerp(Color.black, Color.white, blizzardTicker);
    }

    public static void RainEffectOnctor(On.Menu.RainEffect.orig_ctor orig, Menu.RainEffect self, Menu.Menu menu, MenuObject owner)
    {
        orig(self, menu, owner);
        
        var palTex = new Texture2D(32,8,TextureFormat.ARGB32,false);
        palTex.anisoLevel = 0;
        palTex.filterMode = FilterMode.Point;
        palTex.wrapMode = TextureWrapMode.Clamp;

        var snowTex = new RenderTexture(1400,800,0,RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        snowTex.filterMode = FilterMode.Point;
        Shader.SetGlobalTexture("_SnowTex", snowTex);
        Shader.SetGlobalTexture("_PalTex", palTex);
        
        Shader.SetGlobalFloat("_waterLevel", 20);
        Shader.SetGlobalFloat("_RAIN", 20);
        Shader.SetGlobalFloat("_fogAmount", 20);
    }
    public static void MenuMicrophoneOnPlaySound_SoundID_float_float_float(On.MenuMicrophone.orig_PlaySound_SoundID_float_float_float orig, MenuMicrophone self, SoundID soundid, float pan, float vol, float pitch)
    {
        if (soundid == SoundID.Thunder || 
            soundid == SoundID.Thunder_Close &&
            self.manager.currentMainLoop.ID == ProcessManager.ProcessID.IntroRoll) 
            soundid = SoundID.None;
        orig(self, soundid, pan, vol, pitch);
    }

    public static void IntroRollOnRawUpdate(On.Menu.IntroRoll.orig_RawUpdate orig, Menu.IntroRoll self, float dt)
    {
        orig(self, dt);
        if (!self.continueToMenu && !self.lastAnyButton && blizzardTimeTicker < 1) blizzardTimeTicker += 0.01f;
        else if ((!self.continueToMenu && self.lastAnyButton) && blizzardTimeTicker > 0) blizzardTimeTicker -= 0.05f;
        else if ((self.continueToMenu && !self.lastAnyButton) && blizzardTimeTicker > 0) blizzardTimeTicker -= 0.2f;
        
        foreach (FSprite i in self.rainEffect.sprites)
        {
            if (i == self.rainEffect.bkg ||
                i == self.rainEffect.fadeSprite) continue;
            i.isVisible = false;
            i.alpha = 0;
            i.color = Color.black;
        }
        self.rainEffect.lightning = 0;
        self.rainEffect.lightningIntensity = 0;
        self.rainEffect.lastLightning = 0;
        if (self.manager.musicPlayer.song is Music.IntroRollMusic)
            if (self.manager.musicPlayer != null) 
                (self.manager.musicPlayer.song as Music.IntroRollMusic).rainVol = 0;
        
    }
}