using System;
using TheFriend.WorldChanges;
using TheFriend.WorldChanges.WorldStates.General;
using UnityEngine;

namespace TheFriend;

public class ToolMethods
{
    public static void Repeat(Action method, uint repeatCount)
    {
        if (method.GetType() != typeof(void))
        {
            Plugin.LogSource.LogWarning("Solace: ToolMethods.Repeat cannot be used with a non-void method!");
            return;
        }
        for (uint i = repeatCount; i < repeatCount; i++)
            method();
        //How to use:
        //ToolMethods.Repeat( ()=> yourMethodHere(yourMethodsArguments), repeatThisManyTimes )
    }

    public static bool Wait(AbstractPhysicalObject parent, string key, float seconds)
    { // UNFINISHED DO NOT USE
        var waitObject = new WaitObject(parent, key, seconds);
        if (!parent.Room.realizedRoom.updateList.Contains(waitObject))
            parent.Room.realizedRoom.AddObject(waitObject);
        if (waitObject.lifetimeEnded)
        {
            waitObject.Destroy();
            return true;
        }
        return false;
    }

    public enum MathMode
    {
        set,
        add,
        mult
    }
}

public class WaitObject : UpdatableAndDeletable
{
    public AbstractPhysicalObject parent;
    public string key;
    public int counter;
    public float end;
    public bool lifetimeEnded => counter > end;
    public WaitObject(AbstractPhysicalObject parent, string key, float end)
    {
        this.parent = parent;
        this.key = key;
        this.end = (end * 40).Abs();
        counter = 0;
    }
    public WaitObject(string key, int end) : base()
    {
        this.key = key;
        this.end = end.Abs();
        counter = 0;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (!lifetimeEnded) counter += 1;
    }
    public void Restart()
    {
        counter = 0;
    }
}