using System;
using System.Linq;
using Menu;
using On.RWCustom;
using UnityEngine;
using MenuObject = Menu.MenuObject;
using MusicPlayer = Music.MusicPlayer;
using Random = UnityEngine.Random;

namespace TheFriend;

public class MainMenu
{
    public static void Apply()
    {
        On.MenuMicrophone.PlaySound_SoundID_float_float_float += MenuMicrophoneOnPlaySound_SoundID_float_float_float;
        On.Menu.RainEffect.ctor += RainEffectOnctor;
        On.Menu.RainEffect.GrafUpdate += RainEffectOnGrafUpdate; 
        On.Menu.Menu.Update += MenuOnUpdate;
        On.Menu.IntroRoll.ctor += IntroRollOnctor;
    }

    private static bool shouldPlayBlizzard;
    private static void IntroRollOnctor(On.Menu.IntroRoll.orig_ctor orig, IntroRoll self, ProcessManager manager)
    {
        orig(self, manager);
        var oldSplashScreen = self.pages[0].subObjects.FirstOrDefault(x => x is MenuIllustration illu && illu.fileName.Contains("Intro_Roll_C_"));
        if (oldSplashScreen == null) return;
        if (Random.value > 0.5f) return;
        var validNames = new[] { "friend", "noir", "poacher" };
        var index = Random.Range(0, validNames.Length);
        self.pages[0].RemoveSubObject(oldSplashScreen);
        self.illustrations[2] = new MenuIllustration(self, self.pages[0], "", "Intro_Roll_C_" + validNames[index], new Vector2(0f, 0f), true, false);
        self.pages[0].subObjects.Add(self.illustrations[2]);
        self.illustrations[2].sprite.isVisible = true;
        shouldPlayBlizzard = true;
    }

    private static float blizzardTicker = 0f;
    private static MenuMicrophone.MenuSoundLoop BlizzLoop;
    public static bool blizzardFinished = false;
    public static bool IntroOrSelect;
    public static float raindropfader;

    private static void MenuOnUpdate(On.Menu.Menu.orig_Update orig, Menu.Menu self)
    { // Handles blizzard's audio
        orig(self);
        IntroOrSelect = self is IntroRoll || self is SlugcatSelectMenu;

        if (self.manager.menuMic != null && !blizzardFinished && IntroOrSelect)
        {
            var music = self.manager.musicPlayer?.song as Music.IntroRollMusic;
            if (music != null && music.subTracks[0] != null) music.subTracks[0].volume = 0;
            
            float? vol = music?.rainVol;
            if (vol != null)
            { 
                if (BlizzLoop == null)
                    BlizzLoop = self.PlayLoop(
                        MoreSlugcats.MoreSlugcatsEnums.MSCSoundID.Sleep_Blizzard_Loop,
                        0,
                        Mathf.Clamp(vol.Value,0,0.6f),
                        1,
                        true);
                else BlizzLoop.loopVolume = vol.Value;
            }
            else if (BlizzLoop != null)
            {
                BlizzLoop.Destroy();
                blizzardFinished = true;
            }
        }
    }

    public static void RainEffectOnGrafUpdate(On.Menu.RainEffect.orig_GrafUpdate orig, Menu.RainEffect self, float timestacker)
    { // Blizzard or snow graphics
        orig(self, timestacker);

        if (!IntroOrSelect) return;
        var menu = self.menu as SlugcatSelectMenu;
        bool yes = menu != null;
        bool IHaveSnow = (self.menu is SlugcatSelectMenu) && DoIHaveSnow(menu.slugcatPages[menu.slugcatPageIndex].name);
        
        // Make old raineffect appearance explode!
        self.lightning = 0;
        self.lightningIntensity = 0;
        self.lastLightning = 0;
        foreach (FSprite i in self.sprites)
        {
            if (i == self.bkg ||
                i == self.fadeSprite) continue;
            if (yes) i.color = Color.Lerp(Color.white, Color.black, raindropfader);
            else i.isVisible = false;
        }
        self.bkg.scale = 50;
        self.bkg.scaleX = 200;
        self.bkg.scaleY = 200;
        self.bkg.x = self.menu.manager.rainWorld.screenSize.x/2;
        self.bkg.y = self.menu.manager.rainWorld.screenSize.y/2;
        
        if (blizzardTicker < 1) blizzardTicker += 0.01f;
        if (self.menu is IntroRoll)
        {
            self.bkg.shader = RWCustom.Custom.rainWorld.Shaders["LocalBlizzard"];
            var sin = 330 + Mathf.Sin(Time.time * Mathf.Deg2Rad * 50f) * 6;
            self.bkg.rotation = sin;
            self.bkg.color = Color.Lerp(Color.black, Color.white, blizzardTicker);
        }
        else if (yes)
        {
            if (raindropfader < 1 && IHaveSnow) raindropfader += 0.01f;
            else if (raindropfader > 0 && !IHaveSnow) raindropfader -= 0.01f;
            self.bkg.shader = RWCustom.Custom.rainWorld.Shaders["SnowFall"];
            self.bkg.color = Color.Lerp(Color.black, Color.white, raindropfader);
        }
    }

    public static void RainEffectOnctor(On.Menu.RainEffect.orig_ctor orig, Menu.RainEffect self, Menu.Menu menu, MenuObject owner)
    { // Initialize important ingredients for Blizzard and Snow shaders
        orig(self, menu, owner);
        if (menu is not IntroRoll && menu is not SlugcatSelectMenu) return;

        var palTex = new Texture2D(32, 8, TextureFormat.ARGB32, false);
        palTex.anisoLevel = 0;
        palTex.filterMode = FilterMode.Point;
        palTex.wrapMode = TextureWrapMode.Clamp;

        var snowTex = new RenderTexture(1400, 800, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        snowTex.filterMode = FilterMode.Point;

        Shader.EnableKeyword("SNOW_ON");
        Shader.SetGlobalTexture("_SnowTex", snowTex);
        Shader.SetGlobalTexture("_PalTex", palTex);
        Shader.SetGlobalTexture("_GrabTexture", Texture2D.whiteTexture);
        Shader.SetGlobalTexture("_LevelTex", Texture2D.whiteTexture);
        Shader.SetGlobalTexture("_MainTex", Texture2D.whiteTexture);
        Shader.SetGlobalTexture("_WindTexRendered", Texture2D.whiteTexture);

        Shader.SetGlobalFloat("_waterLevel", 20);
        Shader.SetGlobalFloat("_RAIN", 20);
        Shader.SetGlobalFloat("_fogAmount", 20);
    }
    public static void MenuMicrophoneOnPlaySound_SoundID_float_float_float(On.MenuMicrophone.orig_PlaySound_SoundID_float_float_float orig, MenuMicrophone self, SoundID soundid, float pan, float vol, float pitch)
    { // Remove lightning
        if ((soundid == SoundID.Thunder || 
            soundid == SoundID.Thunder_Close &&
            self.manager.currentMainLoop.ID == ProcessManager.ProcessID.IntroRoll) && IntroOrSelect)
            soundid = SoundID.None;
        orig(self, soundid, pan, vol, pitch);
    }


    public static bool DoIHaveSnow(string name)
    {
        if (name.Contains(Plugin.FriendName.value) ||
            name.Contains(Plugin.DragonName.value) ||
            name.Contains(Plugin.NoirName.value) ||
            name.Contains(Plugin.BelieverName.value) ||
            name.Contains(Plugin.DelugeName.value) ||
            name.Contains(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Saint.value))
            return true;
        return false;
    }
}