namespace TheFriend.NoirThings;

public partial class NoirCatto
{
    public static void Apply()
    {
        On.SlugcatStats.ctor += SlugcatStatsOnctor;
        On.Player.checkInput += PlayerOncheckInput;
        On.Player.AllowGrabbingBatflys += PlayerOnAllowGrabbingBatflys;
        On.Player.ThrowObject += PlayerOnThrowObject;
        On.Player.ThrownSpear += PlayerOnThrownSpear;
        On.Player.Update += PlayerOnUpdate;
        On.Player.UpdateBodyMode += PlayerOnUpdateBodyMode;
        IL.Player.UpdateAnimation += PlayerILUpdateAnimation;
        On.Player.UpdateAnimation += PlayerOnUpdateAnimation;
        On.Player.MovementUpdate += PlayerOnMovementUpdate;
        On.Player.Jump += PlayerOnJump;
        On.Player.PickupCandidate += PlayerOnPickupCandidate;
        On.Player.GrabUpdate += PlayerOnGrabUpdate;

        On.Player.GraphicsModuleUpdated += PlayerOnGraphicsModuleUpdated;
        On.PlayerGraphics.ctor += PlayerGraphicsOnctor;
        On.PlayerGraphics.InitiateSprites += PlayerGraphicsOnInitiateSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphicsOnAddToContainer;
        On.PlayerGraphics.DrawSprites += PlayerGraphicsOnDrawSprites;
        On.PlayerGraphics.ApplyPalette += PlayerGraphicsOnApplyPalette;
        On.PlayerGraphics.Update += PlayerGraphicsOnUpdate;
        On.PlayerGraphics.Reset += PlayerGraphicsOnReset;
        On.SlugcatHand.EngageInMovement += SlugcatHandOnEngageInMovement;

        IL.Weapon.Update += WeaponILUpdate;
        IL.SharedPhysics.TraceProjectileAgainstBodyChunks += SharedPhysicsILTraceProjectileAgainstBodyChunks;
        IL.Spear.Update += SpearILUpdate;
        On.Spear.Update += SpearOnUpdate;

        On.SeedCob.PlaceInRoom += SeedCobOnPlaceInRoom;
        IL.SeedCob.Update += SeedCobILUpdate;
        On.AbstractPhysicalObject.Abstractize += AbstractPhysicalObjectOnAbstractize;

        On.RainWorld.Update += RainWorldOnUpdate;
        On.RainWorldGame.ctor += RainWorldGameOnctor;

        On.Room.AddObject += RoomOnAddObject;

        On.SaveState.setDenPosition += SaveStateOnsetDenPosition;

        IL.GhostWorldPresence.SpawnGhost += GhostWorldPresenceILSpawnGhost;
        IL.Menu.KarmaLadderScreen.GetDataFromGame += KarmaLadderScreenILGetDataFromGame;

        On.Menu.Menu.Update += MenuOnUpdate;
        On.Menu.Menu.CommunicateWithUpcomingProcess += MenuOnCommunicateWithUpcomingProcess;
    }
}