using UnityEngine;

namespace TheFriend.HudThings;

public class FSpriteEx : FSprite
{
    public Vector2 lastPos;
    public Color lastColor;
    public float lastAlpha;

    //Base sprite functionality
    public FSpriteEx(string elementName, bool quadType = true) : base(elementName, quadType) { }
    public FSpriteEx (FAtlasElement element, bool quadType = true) : base(element, quadType) { }

    public Vector2 pos
    {
        get => GetPosition();
        set => SetPosition(value);
    }

    public Vector2 pixelSize => element?.sourcePixelSize ?? Vector2.zero;

    public virtual void Update()
    {
        lastPos = pos;
        lastColor = color;
        lastAlpha = alpha;
    }

    public virtual void Draw(float timestacker)
    {
        pos = Vector2.Lerp(lastPos, pos, timestacker);
        color = Color.Lerp(lastColor, color, timestacker);
        alpha = Mathf.Lerp(lastAlpha, alpha, timestacker);
    }
}

public class CreatureSprite : FSpriteEx
{
    public CreatureTemplate.Type CreatureType;
    public Color CreatureColor;
    public bool? SlatedForDeletion;

    public CreatureSprite(CreatureTemplate.Type type) : this(SymbolDataFromType(type)) { }
    public CreatureSprite(IconSymbol.IconSymbolData iconData) : base(CreatureSymbol.SpriteNameOfCreature(iconData))
    {
        CreatureColor = CreatureSymbol.ColorOfCreature(iconData);
        CreatureType = iconData.critType;
        color = CreatureColor;
    }

    public static IconSymbol.IconSymbolData SymbolDataFromType(CreatureTemplate.Type type, int intData = 0)
    {
        if (type == CreatureTemplate.Type.Centipede && intData == 0) intData = 2; //why, rain world
        return new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, intData);
    }
}
