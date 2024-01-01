

using System.Collections.Generic;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace TheFriend.RemixMenus.CustomRemixObjects;

public class OpComboBoxSolid : OpComboBox //Thanks Henpemaz
{
    public OpComboBoxSolid(ConfigurableBase configBase, Vector2 pos, float width, List<ListItem> list) : base(configBase,
        pos, width, list)
    {
    }

    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        if (this._rectList != null && !_rectList.isHidden)
        {
            for (int j = 0; j < 9; j++)
            {
                this._rectList.sprites[j].alpha = 1;
            }
        }
    }
}