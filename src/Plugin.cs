using System;
using System.Linq;
using BepInEx;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Security.Permissions;
using System.Security;
using BepInEx.Logging;
using TheFriend.SlugcatThings;
using TheFriend.RemixMenus;
using TheFriend.CharacterThings.NoirThings;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace TheFriend
{
    [BepInPlugin(MOD_ID, "The Friend", MOD_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string MOD_ID = "thefriend";
        public const string MOD_VERSION = "0.3.1.0";

        public static readonly PlayerFeature<float> SuperJump = PlayerFloat("friend/super_jump");
        public static readonly PlayerFeature<float> SuperCrawl = PlayerFloat("friend/super_crawl");
        public static readonly PlayerFeature<bool> FriendHead = PlayerBool("friend/friendhead");
        public static readonly PlayerFeature<bool> IsFriendChar = PlayerBool("friend/is_the_friend");
        public static readonly PlayerFeature<bool> CustomTail = PlayerBool("friend/fancytail");
        public static readonly PlayerFeature<bool> MaulEnabled = PlayerBool("friend/maul");

        #region hooks
        public static ManualLogSource LogSource { get; private set; }
        public static bool RotundWorld;
        public void OnEnable()
        {
            LogSource = Logger;
            On.RainWorld.OnModsInit += Hooks.RainWorldOnOnModsInit;
            On.RainWorld.PostModsInit += RainWorldOnPostModsInit;

            On.RainWorld.OnModsDisabled += (orig, self, newlyDisabledMods) =>
            {
                orig(self, newlyDisabledMods);
                for (var i = 0; i < newlyDisabledMods.Length; i++)
                {
                    if (newlyDisabledMods[i].id == "The Friend")
                    {
                        //if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.PebblesLL))
                        //    MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.PebblesLL);
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.MotherLizard))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.MotherLizard);
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.YoungLizard))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.YoungLizard);
                        if (MultiplayerUnlocks.CreatureUnlockList.Contains(SandboxUnlockID.SnowSpider))
                            MultiplayerUnlocks.CreatureUnlockList.Remove(SandboxUnlockID.SnowSpider);
                        if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.UnlockLittleCracker))
                            MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.UnlockLittleCracker);
                        //if (MultiplayerUnlocks.ItemUnlockList.Contains(SandboxUnlockID.UnlockBoulder))
                        //    MultiplayerUnlocks.ItemUnlockList.Remove(SandboxUnlockID.UnlockBoulder);
                        CreatureTemplateType.UnregisterValues();
                        SandboxUnlockID.UnregisterValues();
                        break;
                    }
                }
            };
        }

        private bool _postModsInit;
        private void RainWorldOnPostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            if (_postModsInit) return;

            try
            {
                _postModsInit = true;
                SlugcatNameFix.Apply();

                if (ModManager.ActiveMods.Any(x => x.id == "willowwisp.bellyplus"))
                {
                    RotundWorld = true;
                    Logger.LogInfo("Rotund World detected! Cats gonna be chonky...");
                }
                else
                {
                    RotundWorld = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        internal static void LoadResources()
        {
            MachineConnector.SetRegisteredOI("thefriend", new RemixMain());
            Futile.atlasManager.LoadAtlas("atlases/friendsprites");
            Futile.atlasManager.LoadAtlas("atlases/friendlegs");
            Futile.atlasManager.LoadAtlas("atlases/dragonskull2");
            Futile.atlasManager.LoadAtlas("atlases/dragonskull3");
            Futile.atlasManager.LoadAtlas("atlases/solacesymbols");
            Futile.atlasManager.LoadAtlas("atlases/ForeheadSpots");
            Futile.atlasManager.LoadAtlas("atlases/CentipedeLegB_Fade");
            NoirCatto.LoadSounds();
            NoirCatto.LoadAtlases();
        }
        #endregion

        public static readonly SlugcatStats.Name FriendName = new SlugcatStats.Name("Friend", false); // Makes Friend's campaign more accessible to me
        public static readonly SlugcatStats.Name DragonName = new SlugcatStats.Name("FriendDragonslayer", false); // Makes Poacher's campaign more accessible to me
        public static readonly SlugcatStats.Name NoirName = new SlugcatStats.Name("NoirCatto", false);
        
        public const string MothersKilled = "MothersKilledInRegionStr";
        public const string MotherKillNum = "MotherKillCount";
    }
}