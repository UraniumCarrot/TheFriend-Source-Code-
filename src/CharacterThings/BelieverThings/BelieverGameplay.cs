using System;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using SlugBase;
using MoreSlugcats;
using UnityEngine;
using Random = UnityEngine.Random;
using bod = Player.BodyModeIndex;
using ind = Player.AnimationIndex;

namespace TheFriend.CharacterThings.BelieverThings;

public class BelieverGameplay
{
    public static void Apply()
    {
    }

    public static void PacifistThrow(Player self, int grasp, bool eu)
    {
        #region throwvalues
        bool flag = self.input[0].y < 0;
        if (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value)
        {
            flag = self.input[0].y != 0;
        }
        IntVector2 throwDir = new IntVector2(self.ThrowDirection, 0);
        if (self.animation == Player.AnimationIndex.Flip && flag && self.input[0].x == 0)
        {
            throwDir = new IntVector2(0, (ModManager.MMF && MMF.cfgUpwardsSpearThrow.Value) ? self.input[0].y : (-1));
        }
        if (ModManager.MMF && self.bodyMode == Player.BodyModeIndex.ZeroG && MMF.cfgUpwardsSpearThrow.Value)
        {
            int y = self.input[0].y;
            throwDir = ((y == 0) ? new IntVector2(self.ThrowDirection, 0) : new IntVector2(0, y));
        }
        Vector2 vector = self.firstChunk.pos + throwDir.ToVector2() * 10f + new Vector2(0f, 4f);
        if (self.room.GetTile(vector).Solid)
        {
            vector = self.mainBodyChunk.pos;
        }
        #endregion
        if (self.grasps[grasp].grabbed is Spear spear && !spear.bugSpear && !self.room.game.rainWorld.ExpeditionMode)
        {
            self.TossObject(grasp, eu);
            self.ReleaseGrasp(grasp);
        }
        else if (self.grasps[grasp].grabbed is not Rock)
        {
            (self.grasps[grasp].grabbed as Weapon)?.Thrown(self, vector, self.mainBodyChunk.pos - throwDir.ToVector2() * 10f, throwDir, Mathf.Lerp(0.5f, 0.75f, self.Adrenaline), eu);
        }
    }
}