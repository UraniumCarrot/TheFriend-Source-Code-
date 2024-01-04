using Menu.Remix.MixedUI;
using UnityEngine;

namespace TheFriend.RemixMenus.CustomRemixObjects;

public class OpCheckboxLabelled : OpCheckBox
{
    public OpLabel Label;
    private bool _labelAdded = false;
    // 0 = left
    // 1 = right
    // 2 = middle?
    public OpCheckboxLabelled(Configurable<bool> config, Vector2 pos, string label, int side = 0) : base(config, pos)
    {
        float x = 0;
        switch (side)
        {
            case 0: x = pos.x + 28; break;
            case 1: x = pos.x - (label.Length*6.7f); break;
            case 2: x = pos.x - (label.Length*3.35f); break;
        }
        Label = new OpLabel(x, (side == 2) ? pos.y - 5 : pos.y + 2, label);
        Label.MoveBehindElement(this);
    }
    public OpCheckboxLabelled(Configurable<bool> config, float posX, float posY, string label, int side = 0) : base(config, posX, posY)
    {
        float x = 0;
        switch (side)
        {
            case 0: x = posX + 28; break;
            case 1: x = posX - (label.Length*6.7f); break;
            case 2: x = posX - (label.Length*3.35f); break;
        }
        Label = new OpLabel(x, (side == 2) ? pos.y - 5 : posY + 2, label);
        myContainer.AddChild(Label.myContainer);
    }

    public override void Update()
    {
        base.Update();
        if (!_labelAdded && tab != null)
        {
            tab.AddItems(Label);
            _labelAdded = true;
        }
    }
}