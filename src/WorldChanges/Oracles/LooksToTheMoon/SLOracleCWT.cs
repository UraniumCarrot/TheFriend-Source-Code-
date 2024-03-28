using System.Runtime.CompilerServices;
using TheFriend.WorldChanges.WorldStates.General;
using UnityEngine;

namespace TheFriend.WorldChanges.Oracles.LooksToTheMoon;

public class MoonScene : ExtEnum<MoonScene>
{
    public MoonScene(string name, bool register = false) : base(name,register)
    {
    }
    public static readonly MoonScene MoonStageless = new(nameof(MoonStageless), true);
    public static readonly MoonScene MoonExamine = new(nameof(MoonExamine), true);
    public static readonly MoonScene MoonResearch = new(nameof(MoonResearch), true);
    public static readonly MoonScene MoonTalk = new(nameof(MoonTalk), true);
    public static readonly MoonScene MoonGiveMark = new(nameof(MoonGiveMark), true);
    public static readonly MoonScene MoonExhausted = new(nameof(MoonExhausted), true);
}


public static class SLOracleCWT
{
    public class Moon
    {
        public MoonScene stage;
        public int miniStage;
        public int stageCounter;
        public int speechCounter;
        public bool restMode;
        public Moon(AbstractPhysicalObject self)
        {
            stage = MoonScene.MoonStageless;
            miniStage = 0;
        }
    }
    public static readonly ConditionalWeakTable<AbstractPhysicalObject, Moon> CWT = new();
    public static Moon MoonCutsceneData(this Oracle moon) => CWT.GetValue(moon.abstractPhysicalObject, _ => new(moon.abstractPhysicalObject));

    public static bool TryMoonData(this AbstractPhysicalObject self, out Moon data)
    {
        if (self.type == AbstractPhysicalObject.AbstractObjectType.Oracle)
        {
            if ((self.realizedObject as Oracle)?.ID == Oracle.OracleID.SL)
            {
                data = (self.realizedObject as Oracle).MoonCutsceneData();
                return true;
            }
        }
        data = null;
        return false;
    }
    public static bool TryMoonData(this SLOracleBehavior behav, out Moon data) => behav.oracle.abstractPhysicalObject.TryMoonData(out data);
    public static bool TryMoonData(this Oracle self, out Moon data) => self.abstractPhysicalObject.TryMoonData(out data);
    
    public static void ChangeStage(this Moon data, MoonScene stage, float Seconds)
    {
        if (data.stageCounter > Seconds * 40)
        {
            Debug.Log("changing stage");
            data.stageCounter = 0;
            data.stage = stage;
            data.miniStage = 0;
        }
    }
    public static void ChangeStage(this Moon data, MoonScene stage, bool condition)
    {
        if (condition)
        {
            Debug.Log("changing stage");
            data.stageCounter = 0;
            data.stage = stage;
            data.miniStage = 0;
        }
    }
    public static void ChangeMiniStage(this Moon data, int miniStage, float Seconds)
    {
        if (data.stageCounter > Seconds * 40)
        {
            Debug.Log("changing ministage");
            data.stageCounter = 0;
            data.miniStage = miniStage;
        }
    }
    public static void ChangeMiniStage(this Moon data, int miniStage, bool condition)
    {
        if (condition)
        {
            Debug.Log("changing ministage");
            data.stageCounter = 0;
            data.miniStage = miniStage;
        }
    }

    public static void DestroyStage(this Moon data, MoonScene stageOverrider = null, int miniStageOverrider = 0)
    {
        if (stageOverrider == null) stageOverrider = MoonScene.MoonStageless;
        Debug.Log("destroying");
        data.stageCounter = 0;
        data.stage = stageOverrider;
        data.miniStage = miniStageOverrider;
    }
}