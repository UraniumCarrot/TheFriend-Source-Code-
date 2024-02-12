using System.Diagnostics.CodeAnalysis;
using TheFriend.CharacterThings.NoirThings;

namespace TheFriend;

public static class DataPearlType
{
    [AllowNull] public static DataPearl.AbstractDataPearl.DataPearlType DelugePearl = new(nameof(DelugePearl), true);

    public static void UnregisterValues()
    {
        if (DelugePearl != null)
        {
            DelugePearl.Unregister();
            DelugePearl = null;
        }
    }
}

public static class DangerType
{
    [AllowNull] public static RoomRain.DangerType FloodAndAerie = new(nameof(FloodAndAerie), true);

    public static void UnregisterValues()
    {
        if (FloodAndAerie != null)
        {
            FloodAndAerie.Unregister();
            FloodAndAerie = null;
        }
    }
}
public static class CreatureTemplateType
{
    [AllowNull] public static CreatureTemplate.Type PebblesLL = new(nameof(PebblesLL), true);
    [AllowNull] public static CreatureTemplate.Type MotherLizard = new(nameof(MotherLizard), true);
    [AllowNull] public static CreatureTemplate.Type YoungLizard = new(nameof(YoungLizard), true);
    [AllowNull] public static CreatureTemplate.Type PilgrimLizard = new(nameof(PilgrimLizard), true);
    [AllowNull] public static CreatureTemplate.Type SnowSpider = new(nameof(SnowSpider), true);

    public static void UnregisterValues()
    {
        if (PebblesLL != null)
        {
            PebblesLL.Unregister();
            PebblesLL = null;
        }
        if (MotherLizard != null)
        {
            MotherLizard.Unregister();
            MotherLizard = null;
        }
        if (PilgrimLizard != null)
        {
            PilgrimLizard.Unregister();
            PilgrimLizard = null;
        }
        if (YoungLizard != null)
        {
            YoungLizard.Unregister();
            YoungLizard = null;
        }
        if (SnowSpider != null)
        {
            SnowSpider.Unregister();
            SnowSpider = null;
        }
    }
}

public static class AbstractObjectType
{
    internal static void AbstractPhysicalObjectOnRealize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
    {
        orig(self);
        if (self.type == CatSlash)
        {
            var absSlash = (NoirCatto.AbstractCatSlash)self;
            self.realizedObject = new NoirCatto.CatSlash(self, self.world, absSlash.Owner, absSlash.HandUsed, absSlash.SlashType);
        }
    }

    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType CatSlash = new (nameof(CatSlash), true);
    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType LittleCracker = new(nameof(LittleCracker), true);
    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType Boulder = new(nameof(Boulder), true);
    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType BoomMine = new(nameof(BoomMine), true);
    [AllowNull] public static AbstractPhysicalObject.AbstractObjectType SolaceScarf = new(nameof(SolaceScarf), true);
    public static void UnregisterValues()
    {
        if (LittleCracker != null)
        {
            LittleCracker.Unregister();
            LittleCracker = null;
        }
        if (Boulder != null)
        {
            Boulder.Unregister();
            Boulder = null;
        }
        if (BoomMine != null)
        {
            BoomMine.Unregister();
            BoomMine = null;
        }
        if (SolaceScarf != null)
        {
            SolaceScarf.Unregister();
            SolaceScarf = null;
        }
    }
}

public static class SandboxUnlockID
{
    //[AllowNull] public static MultiplayerUnlocks.SandboxUnlockID PebblesLL = new(nameof(PebblesLL), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID MotherLizard = new(nameof(MotherLizard), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID YoungLizard = new(nameof(YoungLizard), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID SnowSpider = new(nameof(SnowSpider), true);
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID UnlockLittleCracker = new(nameof(UnlockLittleCracker), true);
    //[AllowNull] public static MultiplayerUnlocks.SandboxUnlockID UnlockBoulder = new(nameof(UnlockBoulder), true);
    public static void UnregisterValues()
    {
        //if (PebblesLL != null)
        //{
        //    PebblesLL.Unregister();
        //    PebblesLL = null;
        //}
        if (MotherLizard != null)
        {
            MotherLizard.Unregister();
            MotherLizard = null;
        }
        if (YoungLizard != null)
        {
            YoungLizard.Unregister();
            YoungLizard = null;
        }
        if (SnowSpider != null)
        {
            SnowSpider.Unregister();
            SnowSpider = null;
        }
        if (UnlockLittleCracker != null)
        {
            UnlockLittleCracker.Unregister();
            UnlockLittleCracker = null;
        }
        //if (UnlockBoulder != null)
        //{
        //    UnlockBoulder.Unregister();
        //    UnlockBoulder = null;
        //}
    }
}