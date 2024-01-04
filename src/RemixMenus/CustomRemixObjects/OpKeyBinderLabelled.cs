using Menu.Remix.MixedUI;
using UnityEngine;

namespace TheFriend.RemixMenus.CustomRemixObjects;

public class OpKeyBinderLabelled : OpKeyBinder
{
    public OpLabel Label;
    private bool _labelAdded = false;
    // 0 = left
    // 1 = right
    // 2 = middle?

    public OpKeyBinderLabelled(Configurable<KeyCode> config, Vector2 pos, Vector2 size, string label, int side = 2, bool collide = true, BindController controller = BindController.AnyController) : base(config, pos, size, collide, controller)
    {
        float x = 0;
        switch (side)
        {
            case 0: x = pos.x + 28; break;
            case 1: x = pos.x - (label.Length*6.7f); break;
            case 2: x = pos.x + (size.x / 3.2f); break;
        }
        Label = new OpLabel(x, pos.y-20, label);
        Label.MoveBehindElement(this);
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