namespace TheFriend.Objects;

public class GenericObjectStick : AbstractPhysicalObject.AbstractObjectStick
{
    public AbstractPhysicalObject self
    {
        get => A;
        set => A = value;
    }
    public AbstractPhysicalObject obj
    {
        get => B;
        set => B = value;
    }
    public GenericObjectStick(AbstractPhysicalObject self, AbstractPhysicalObject obj) : base(self, obj) { }
}