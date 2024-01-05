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
            case 0: x = 28; break;
            case 1: x = label.Length*6.7f; break;
            case 2: x = label.Length*3.35f; break;
        }
        Label = new OpLabel(0, 0, label);
        myContainer.AddChild(Label.label);
        myContainer.GetChildAt(myContainer.GetChildIndex(Label.label)).x = x;
        myContainer.GetChildAt(myContainer.GetChildIndex(Label.label)).y = (side == 2) ? -5 : 12;
    }
    public OpCheckboxLabelled(Configurable<bool> config, float posX, float posY, string label, int side = 0) : base(config, posX, posY)
    {
        float x = 0;
        switch (side)
        {
            case 0: x = 28; break;
            case 1: x = label.Length*6.7f; break;
            case 2: x = label.Length*3.35f; break;
        }
        Label = new OpLabel(0, 0, label);
        myContainer.AddChild(Label.label);
        myContainer.GetChildAt(myContainer.GetChildIndex(Label.label)).x = x;
        myContainer.GetChildAt(myContainer.GetChildIndex(Label.label)).y = (side == 2) ? -5 : 12;
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