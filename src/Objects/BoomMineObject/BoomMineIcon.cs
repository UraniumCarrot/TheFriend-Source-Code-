using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Core;
using TheFriend.Objects.BoomMineObject;
using UnityEngine;

namespace TheFriend.Objects.BoomMineObject;

public class BoomMineIcon : Icon
{
    public override int Data(AbstractPhysicalObject apo)
    {
        return apo is BoomMineAbstract ? 0 : 0;
    }
    public override Color SpriteColor(int data)
    {
        return new Color(0.36862746f, 0.36862746f, 37f / 85f);
    }
    public override string SpriteName(int data)
    {
        return "icon_BoomMine";
    }
} // DONE (TECHNICALLY)
