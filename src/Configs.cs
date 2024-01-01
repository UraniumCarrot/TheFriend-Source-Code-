using TheFriend.CharacterThings.NoirThings;
using TheFriend.RemixMenus;
using UnityEngine;

namespace TheFriend;

public class Configs
{
    #region general
    public static bool NoFamine => RemixMain.GeneralNoFamine.Value;
    public static bool GlobalFamine => RemixMain.GeneralFaminesForAll.Value;
    public static bool ExpeditionFamine => RemixMain.GeneralExpeditionFamine.Value;
    
    public static bool RepMeterAll => RemixMain.GeneralLizRepMeterForAll.Value;
    public static bool LocalRepAll => RemixMain.GeneralLocalizedLizRepForAll.Value;
    
    public static bool CharSnow => RemixMain.GeneralCharselectSnow.Value;
    public static bool IntroBlizzard => RemixMain.GeneralIntroRollBlizzard.Value;
    public static bool CharHeight => RemixMain.GeneralCharCustomHeights.Value;
    
    public static bool BlizzTimer => RemixMain.GeneralSolaceBlizzTimer.Value;
    
    public static bool LizRideAll => RemixMain.GeneralLizRideAll.Value;
    #endregion
    
    #region friend
    public static bool FriendBackspear => RemixMain.FriendBackspear.Value;
    public static bool FriendUnNerf => RemixMain.FriendUnNerf.Value;
    public static bool FriendRepLock => RemixMain.FriendRepLock.Value;
    public static bool FriendAutoCrouch => RemixMain.FriendAutoCrouch.Value;
    public static bool FriendPoleCrawl => RemixMain.FriendPoleCrawl.Value;
    #endregion
    
    #region poacher
    public static bool PoacherBackspear => RemixMain.PoacherBackspear.Value;
    public static bool PoacherPupActs => RemixMain.PoacherPupActs.Value;
    public static bool PoacherFreezeFaster => RemixMain.PoacherFreezeFaster.Value;
    public static bool PoacherFoodParkour => RemixMain.PoacherFoodParkour.Value;
    public static bool PoacherJumpNerf => RemixMain.PoacherJumpNerf.Value;
    #endregion
    
    #region noir
    public static bool NoirAltSlashConditions => RemixMain.NoirAltSlashConditions.Value;
    public static bool NoirAutoSlash => RemixMain.NoirAutoSlash.Value;
    public static bool NoirBuffSlash => RemixMain.NoirBuffSlash.Value;
    
    public static NoirCatto.CustomStartMode NoirUseCustomStart => RemixMain.NoirUseCustomStart.Value;
    public static bool NoirHideEars => RemixMain.NoirHideEars.Value;
    public static bool NoirDisableAutoCrouch => RemixMain.NoirDisableAutoCrouch.Value;
    
    public static bool NoirAttractiveMeow => RemixMain.NoirAttractiveMeow.Value;
    public static KeyCode NoirMeowKey => RemixMain.NoirMeowKey.Value;
    #endregion
    
    #region deluge
    #endregion
    
    #region believer
    #endregion
}