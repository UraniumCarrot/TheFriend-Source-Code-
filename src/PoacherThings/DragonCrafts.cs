using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;
using Solace.Objects.BoomMineObject;
using Solace.Objects.LittleCrackerObject;
using ObjType = AbstractPhysicalObject.AbstractObjectType;
using Solace.SlugcatThings;

namespace Solace.PoacherThings;
public class DragonCrafts
{
    #region Crafting dictionary
    static Dictionary<(ObjType, ObjType), ObjType> craftingResults = new();
    static void AddRecipe(ObjType inputOne, ObjType inputTwo, ObjType output)
    {
        craftingResults.Add((inputOne, inputTwo), output);
    }
    static ObjType GetResult(ObjType inputOne, ObjType inputTwo)
    {
        if (inputOne == ObjType.WaterNut || inputTwo == ObjType.WaterNut) return null;
        if (inputOne == LittleCrackerFisob.LittleCracker && inputTwo == LittleCrackerFisob.LittleCracker) return null;
        if (craftingResults.TryGetValue((inputOne, inputTwo), out var output) || craftingResults.TryGetValue((inputTwo, inputOne), out output))
        {
            return output;
        }
        return null;
    }
    #endregion
    public static void Apply()
    {
        //On.Player.CraftingResults += Player_CraftingResults;
        On.Player.SpitUpCraftedObject += Player_SpitUpCraftedObject;
        On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted;
        On.Player.ThrownSpear += Player_ThrownSpear;
        On.Weapon.NewRoom += Weapon_NewRoom;

        AddRecipe(LittleCrackerFisob.LittleCracker, ObjType.Rock, BoomMineFisob.BoomMine);
        AddRecipe(ObjType.Lantern, ObjType.Rock, BoomMineFisob.BoomMine);
        AddRecipe(ObjType.FlareBomb, ObjType.Rock, BoomMineFisob.BoomMine);
        AddRecipe(ObjType.PuffBall, ObjType.Rock, BoomMineFisob.BoomMine);
        AddRecipe(ObjType.Mushroom, ObjType.Rock, BoomMineFisob.BoomMine);
        AddRecipe(ObjType.JellyFish, ObjType.Rock, BoomMineFisob.BoomMine);

        AddRecipe(LittleCrackerFisob.LittleCracker, BoomMineFisob.BoomMine, BoomMineFisob.BoomMine);
        AddRecipe(ObjType.Lantern, BoomMineFisob.BoomMine, BoomMineFisob.BoomMine);
        AddRecipe(ObjType.JellyFish, BoomMineFisob.BoomMine, BoomMineFisob.BoomMine);
        AddRecipe(ObjType.FlareBomb, BoomMineFisob.BoomMine, BoomMineFisob.BoomMine);
        AddRecipe(ObjType.PuffBall, BoomMineFisob.BoomMine, BoomMineFisob.BoomMine);
        AddRecipe(ObjType.Mushroom, BoomMineFisob.BoomMine, BoomMineFisob.BoomMine);

        AddRecipe(ObjType.Rock, ObjType.Rock, ObjType.Spear);
        AddRecipe(ObjType.Rock, ObjType.Spear, ObjType.Spear);
        AddRecipe(ObjType.FlareBomb, ObjType.Spear, ObjType.Spear);
        AddRecipe(ObjType.JellyFish, ObjType.Spear, ObjType.Spear);
        AddRecipe(LittleCrackerFisob.LittleCracker, ObjType.Spear, ObjType.Spear);

        AddRecipe(ObjType.FirecrackerPlant, ObjType.Spear, LittleCrackerFisob.LittleCracker);
        AddRecipe(ObjType.FirecrackerPlant, ObjType.Rock, ObjType.ScavengerBomb);
        AddRecipe(LittleCrackerFisob.LittleCracker, ObjType.DangleFruit, ObjType.Lantern);
    }

    #region Firespear changes
    public static void Player_ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
    {
        orig(self, spear);
        if ((self.room.world.game.StoryCharacter == Solace.FriendName || self.room.world.game.StoryCharacter == Solace.DragonName) && spear.bugSpear)
        {
            spear.spearDamageBonus *= 0.4f;
        }
    }
    public static void Weapon_NewRoom(On.Weapon.orig_NewRoom orig, Weapon self, Room newRoom)
    {
        orig(self, newRoom);
        if (self is Spear spear && spear.bugSpear)
        {
            spear.room.AddObject(new SpearHeatSource(spear, spear.firstChunk.pos));
        }
    }
    class SpearHeatSource : UpdatableAndDeletable, IProvideWarmth
    {
        public Spear spear;
        public Vector2 Position() { return pos; }
        public Room loadedRoom => room;
        public Vector2 pos;
        public float warmth => RainWorldGame.DefaultHeatSourceWarmth * 0.5f;
        public float range => 40f;
        public SpearHeatSource(Spear spear, Vector2 pos)
        {
            this.spear = spear;
            this.pos = pos;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            pos = spear.firstChunk.pos;
            if (!spear.bugSpear || spear.room == null || spear != spear.abstractPhysicalObject.realizedObject || spear.room != loadedRoom || loadedRoom != room.abstractRoom.realizedRoom) base.Destroy();
        }
    }
    #endregion

    #region Crafting
    public static bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
    {
        if (self.slugcatStats.name == Solace.DragonName && self.input[0].y > 0) return true;
        return orig(self);
    }
    public static void Player_SpitUpCraftedObject(On.Player.orig_SpitUpCraftedObject orig, Player self)
    {
        if (self.slugcatStats.name != Solace.DragonName) { orig(self); return; }
        int vargrasp = self?.grasps[1]?.grabbed is FirecrackerPlant || self?.grasps[1]?.grabbed is BoomMine ? 1 : 0;
        var vargraspmat = self?.grasps[vargrasp]?.grabbed;
        var obj0 = self.grasps[0]?.grabbed?.abstractPhysicalObject;
        var obj1 = self.grasps[1]?.grabbed?.abstractPhysicalObject;
        var resultEx = GetResult(obj0.type, obj1.type);

        if (self.slugcatStats.name == Solace.DragonName)
        {
            if (resultEx != LittleCrackerFisob.LittleCracker) self.GetPoacher().isMakingPoppers = false;
            if (resultEx == ObjType.Spear && !(obj0.type == ObjType.Rock && obj1.type == ObjType.Rock))
            {
                Debug.Log("Solace: SpearCrafting method begin!");
                SpearCrafting(self);
                return;
            }
            else if (resultEx == LittleCrackerFisob.LittleCracker && vargraspmat is FirecrackerPlant)
            {
                try
                {
                    //int lumps = (vargraspmat as FirecrackerPlant).lumps.Length;
                    self.GetPoacher().isMakingPoppers = true;
                    int lumps = (vargraspmat as FirecrackerPlant).lumpsPopped.Count(i => i == false);
                    Debug.Log("Solace: TearFirecracker using a plant with " + lumps + " lumps (originally " + (vargraspmat as FirecrackerPlant).lumps.Length + "). Using hand " + vargrasp);
                    TearFirecracker(self, lumps);
                    return;
                }
                catch (Exception e) { Debug.Log("Solace: Exception caught in Player.SpitUpCraftedObject " + e); }
            }
            else if (resultEx == BoomMineFisob.BoomMine)
            {
                Debug.Log("Solace: BoomMine crafting begun!");
                if (vargraspmat is not BoomMine) { MineCrafting1(self); return; }
                else { MineCrafting2(self); return; }
            }
            else if (resultEx != null)
            {
                AbstractPhysicalObject result; // Crafting result
                result = null;
                SoundID sound; // Cosmetic sound
                sound = null;
                float volume = 1f; // Volume of cosmetic sound
                CosmeticSprite effect; // Cosmetic visual
                effect = null;

                switch (resultEx.value) // Define resulting object from recipe
                {
                    case nameof(ObjType.Spear):
                        result = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), explosive: false);
                        sound = SoundID.Spear_Bounce_Off_Creauture_Shell;
                        effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * 60f * Random.value, color: new Color(1f, 1f, 1f), null, 20, 50);
                        break;

                    case nameof(ObjType.ScavengerBomb):
                        if ((vargraspmat as FirecrackerPlant).lumps.Length - (vargraspmat as FirecrackerPlant).lumpsPopped.Count(i => i == true) < 6 || (vargraspmat as FirecrackerPlant).lumps.Length <= 5)
                        {
                            CraftFail(self);
                            Debug.Log("Solace: Dragoncraft failure! Tried to make a scavenger bomb without a full firecracker. Working as intended.");
                            return;
                        }
                        result = new AbstractPhysicalObject(self.room.world, resultEx, null, self.abstractCreature.pos, self.room.game.GetNewID());
                        sound = SoundID.Firecracker_Burn;
                        volume = 0.2f;
                        effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value), color: new Color(1f, 0.4f, 0.3f), null, 7, 17);
                        break;

                    case nameof(ObjType.Lantern):
                        result = new AbstractPhysicalObject(self.room.world, resultEx, null, self.abstractCreature.pos, self.room.game.GetNewID());
                        sound = SoundID.Snail_Warning_Click;
                        volume = 0.9f;
                        effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value), color: new Color(1f, 0.4f, 0.3f), null, 7, 17);
                        break;
                }
                for (int i = 0; i < 2; i++) // Remove materials
                {
                    self.grasps[i].grabbed.RemoveFromRoom();
                    self.room.abstractRoom.RemoveEntity(self.grasps[i].grabbed.abstractPhysicalObject);
                    self.ReleaseGrasp(i);
                }
                // Create and grab resulting object
                if (result == null) Debug.Log("Solace: Uh oh! Recipe created a null object! Doom incoming?!");
                self.room.PlaySound(sound, self.firstChunk, loop: false, volume, 1f);
                self.room.AddObject(effect);
                self.room.abstractRoom.AddEntity(result);
                result.RealizeInRoom();
                if (self.FreeHand() != -1)
                {
                    self.SlugcatGrab(result.realizedObject, self.FreeHand());
                }

                else Debug.Log("Solace: Dragoncraft success! Combined " + obj0.type + " with " + obj1.type + " to make " + result.type + "!");
                return;
            }
            else
            {
                CraftFail(self);
                Debug.Log("Solace: Dragoncraft failure! These items have no recipe for Poacher! Working as intended. Generic.");
                return;
            }
        }
        else orig(self);
    }
    public static void CraftFail(Player self)
    {
        self.room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, self.firstChunk, loop: false, 1f, 1f);
        self.room.AddObject(new Spark(self.bodyChunks[0].pos, Custom.RNV() * 60f * Random.value, color: new Color(1f, 1f, 1f), null, 20, 50));
    }
    public static void MineCrafting1(Player self)
    {
        Creature.Grasp[] grasp = self.grasps;
        for (int i = 0; i < 2; i++)
        {
            if (grasp[i]?.grabbed?.abstractPhysicalObject.type != BoomMineFisob.BoomMine)
            {
                grasp[i]?.grabbed?.abstractPhysicalObject.realizedObject.RemoveFromRoom();
                self.ReleaseGrasp(i);
                self.room.abstractRoom.RemoveEntity(grasp[i]?.grabbed?.abstractPhysicalObject);
                Debug.Log("Solace: Mine recipe item destroyed");
            }
        }

        AbstractPhysicalObject result = new BoomMineAbstract(self.room.world, self.abstractCreature.pos, self.room.game.GetNewID());
        self.room.PlaySound(SoundID.Gate_Clamp_Lock, self.mainBodyChunk, false, 0.5f, 3f + Random.value);
        self.room.AddObject(new Spark(self.bodyChunks[0].pos, Custom.RNV() * 60f * Random.value, color: new Color(1f, 1f, 1f), null, 20, 50));
        self.room.abstractRoom.AddEntity(result);
        result.RealizeInRoom();
        if (self.FreeHand() != -1)
        {
            self.SlugcatGrab(result.realizedObject, self.FreeHand());
        }
        BoomMine mine = result.realizedObject as BoomMine;
        mine.owner = self;
        //if ((result.realizedObject as BoomMine).slot1 == 0)
        Debug.Log("Solace: Mine craft ended! Did it work?");
    }
    public static void MineCrafting2(Player self)
    {
        int grasp = self?.grasps[1]?.grabbed?.abstractPhysicalObject?.type == BoomMineFisob.BoomMine ? 0 : 1;
        int result = self?.grasps[1]?.grabbed?.abstractPhysicalObject?.type == BoomMineFisob.BoomMine ? 1 : 0;

        int? upgrade;
        int red = 1;
        int green = 2;
        int blue = 3;

        upgrade = self?.grasps[grasp]?.grabbed?.abstractPhysicalObject?.realizedObject switch
        {
            Lantern or LittleCracker => red,
            PuffBall or Mushroom => green,
            JellyFish or FlareBomb => blue,
            _ => null
        };
        if (upgrade == null) { CraftFail(self); Debug.Log("Solace: Item cannot be used to upgrade mine!"); return; }

        var mine = self?.grasps[result]?.grabbed?.abstractPhysicalObject?.realizedObject as BoomMine;
        if (mine?.Abstr?.slot1 == 0) mine.Abstr.slot1 = (int)upgrade;
        else if (mine?.Abstr?.slot2 == 0) mine.Abstr.slot2 = (int)upgrade;
        else if (mine?.Abstr?.slot3 == 0) mine.Abstr.slot3 = (int)upgrade;
        else { CraftFail(self); Debug.Log("Solace: Mine cannot be upgraded further!"); return; }

        var mat = self?.grasps[grasp]?.grabbed?.abstractPhysicalObject?.realizedObject;
        mat.RemoveFromRoom();
        self.ReleaseGrasp(grasp);
        self.room.abstractRoom.RemoveEntity(self?.grasps[grasp]?.grabbed?.abstractPhysicalObject);
        Debug.Log("Solace: Mine recipe item destroyed");

        self.room.PlaySound(SoundID.Gate_Clamp_Lock, self.mainBodyChunk, false, 0.5f, 3f + Random.value);
        Debug.Log("Solace: Mine craft2 ended!");
        return;
    }
    public static void TearFirecracker(Player self, int lumps)
    {
        try
        {
            Debug.Log("TearFirecracker started");
            int grasp = self?.grasps[1]?.grabbed is FirecrackerPlant ? 1 : 0;
            var mat = self?.grasps[grasp]?.grabbed?.abstractPhysicalObject?.realizedObject;
            FirecrackerPlant plant = mat as FirecrackerPlant;

            if (self != null &&
                self.room != null &&
                self.room.world != null &&
                self.room.abstractRoom != null &&
                self.grasps[grasp] != null &&
                self.grasps[grasp].grabbed is FirecrackerPlant &&
                self.grasps[grasp].grabbed != null &&
                self.grasps[grasp].grabbed.abstractPhysicalObject != null &&
                self.grasps[grasp].grabbed.abstractPhysicalObject.realizedObject != null)
            {
                for (int i = plant.lumps.Length - 1; i >= 0; i--)
                {
                    if (plant.lumpsPopped[i]) continue;

                    self.room.PlaySound(SoundID.Seed_Cob_Open, self.firstChunk, loop: false, 1f, 1f);
                    AbstractPhysicalObject result = new LittleCrackerAbstract(self?.room?.world, self.abstractCreature.pos, self.room.game.GetNewID());
                    self.room.abstractRoom.AddEntity(result);
                    result.RealizeInRoom();
                    plant.lumpsPopped[i] = true;
                    if (i == 0)
                    {
                        self.ReleaseGrasp(grasp);
                        mat.RemoveFromRoom();
                        self?.room?.abstractRoom?.RemoveEntity(self?.grasps[grasp]?.grabbed?.abstractPhysicalObject);
                        self.SlugcatGrab(result?.realizedObject, grasp);
                        Debug.Log("Solace: Final popper torn off, plant destroyed");
                        break;
                    }
                    break;
                }
            }
            return;
        }
        catch (Exception e) { Debug.Log("Solace: Exception caught in TearFirecracker" + e); }
    }
    public static void SpearCrafting(Player self)
    {
        CosmeticSprite effect = null;
        SoundID sound = null;
        float volume = 1f;

        int grasp = self?.grasps[1]?.grabbed is not Spear ? 1 : 0;
        int speargrasp = self?.grasps[1]?.grabbed is Spear ? 1 : 0;
        var mat = self?.grasps[grasp]?.grabbed?.abstractPhysicalObject?.realizedObject;
        AbstractSpear spear = self?.grasps[speargrasp]?.grabbed.abstractPhysicalObject as AbstractSpear;
        bool electricSet = false;
        float hueSet = 0f;
        int electricCharge = spear.electricCharge;

        if (
            spear?.hue != 0 ||
            spear?.electricCharge >= 3 ||
            spear?.explosive == true
           )
        { CraftFail(self); Debug.Log("Solace: SpearCrafting failed! Invalid spear type."); return; }

        switch (spear.electric)
        {
            case true:
                switch (mat)
                {
                    case FlareBomb:
                        electricCharge += 1;
                        break;
                    case JellyFish:
                        electricCharge = 3;
                        break;

                    default: CraftFail(self); Debug.Log("Solace: SpearCrafting failed! Invalid material. Case 1"); return;
                }
                sound = SoundID.Zapper_Zap;
                effect = new Explosion.ExplosionLight(self.bodyChunks[0].pos, 200f, 1f, 4, new Color(0.8f, 0.8f, 1f));
                break;

            case false:
                switch (mat)
                {
                    case Rock and not LittleCracker:
                        electricSet = true;
                        electricCharge = 0;
                        sound = SoundID.Spear_Bounce_Off_Creauture_Shell;
                        effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * 60f * Random.value, color: new Color(1f, 1f, 1f), null, 20, 50);
                        break;
                    case LittleCracker:
                        hueSet = 0.5f;
                        sound = SoundID.Firecracker_Burn;
                        volume = 0.2f;
                        effect = new Spark(self.bodyChunks[0].pos, Custom.RNV() * Mathf.Lerp(5f, 11f, Random.value), color: new Color(1f, 0.4f, 0.3f), null, 7, 17);
                        break;

                    default: CraftFail(self); Debug.Log("Solace: SpearCrafting failed! Invalid material. Case 2"); return;
                }
                break;
        }

        if (mat is not JellyFish)
        {
            mat?.RemoveFromRoom();
            self?.room?.abstractRoom?.RemoveEntity(self?.grasps[grasp]?.grabbed?.abstractPhysicalObject);
            self?.ReleaseGrasp(grasp);
        }
        self?.room?.PlaySound(sound, self?.firstChunk, loop: false, volume, 1f);
        self?.room?.AddObject(effect);

        if (!spear.electric)
        {
            Debug.Log("Solace: SpearCrafting: Spear wasn't electric! Making new spear...");
            self?.grasps[speargrasp]?.grabbed?.RemoveFromRoom();
            self?.room?.abstractRoom?.RemoveEntity(self?.grasps[speargrasp]?.grabbed?.abstractPhysicalObject);
            self?.ReleaseGrasp(speargrasp);

            spear = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false, electricSet);
            spear.electricCharge = electricCharge;
            spear.hue = hueSet;
            spear.RealizeInRoom();
            self.SlugcatGrab(spear.realizedObject, speargrasp);
        }
        else spear.electricCharge = electricCharge;
        Debug.Log("Solace: SpearCrafting ended.");
        return;
    }
    #endregion
}
