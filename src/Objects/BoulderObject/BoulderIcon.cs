using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Core;
using UnityEngine;

namespace TheFriend.Objects.BoulderObject;

internal class BoulderIcon : Icon
{
    public override int Data(AbstractPhysicalObject apo)
    {
        return 0;
    }
    public override Color SpriteColor(int data)
    {
        return Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
    }
    public override string SpriteName(int data)
    {
        return "icon_Boulder";
    }
}
