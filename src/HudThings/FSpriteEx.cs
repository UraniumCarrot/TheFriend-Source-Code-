using UnityEngine;

namespace TheFriend.HudThings;

public class FSpriteEx : FSprite
{
    public Vector2 lastPos;
    public Color lastColor;
    public float lastAlpha;
    public Vector2 targetPos;
    public Color targetColor = Futile.white;
    public float targetAlpha = 1f;

    //Base sprite functionality
    public FSpriteEx(string elementName, bool quadType = true) : base(elementName, quadType) { }
    public FSpriteEx (FAtlasElement element, bool quadType = true) : base(element, quadType) { }

    public Vector2 pos
    {
        get => GetPosition();
        set => SetPosition(value);
    }

    public Vector2 pixelSize => element?.sourcePixelSize ?? Vector2.zero;
    public Vector2 LerpPos(float timeStacker) => Vector2.Lerp(lastPos, targetPos, timeStacker);
    public Color LerpColor(float timeStacker) => Color.Lerp(lastColor, targetColor, timeStacker);
    public float LerpAlpha(float timeStacker) => Mathf.Lerp(lastAlpha, targetAlpha, timeStacker);

    public virtual void LerpAll(float timeStacker) //Call this in Draw(), but ONLY if you do not set any of these in Draw()
    {
        pos = Vector2.Lerp(lastPos, targetPos, timeStacker);
        color = Color.Lerp(lastColor, targetColor, timeStacker);
        alpha = Mathf.Lerp(lastAlpha, targetAlpha, timeStacker);
    }

    public virtual void Update()
    {
        lastPos = pos;
        lastColor = color;
        lastAlpha = alpha;
    }

    public new void SetPosition(Vector2 newPosition)
    {
        base.SetPosition(newPosition);
        targetPos = newPosition;
    }
}

public class CreatureSprite : FSpriteEx
{
    public CreatureTemplate.Type CreatureType;
    public Color CreatureColor;

    public CreatureSprite(CreatureTemplate.Type type) : this(SymbolDataFromType(type)) { }
    public CreatureSprite(IconSymbol.IconSymbolData iconData) : base(CreatureSymbol.SpriteNameOfCreature(iconData))
    {
        CreatureColor = CreatureSymbol.ColorOfCreature(iconData);
        CreatureType = iconData.critType;
        targetColor = CreatureColor;
        color = CreatureColor;
    }

    public static IconSymbol.IconSymbolData SymbolDataFromType(CreatureTemplate.Type type, int intData = 0)
    {
        if (type == CreatureTemplate.Type.Centipede && intData == 0) intData = 2; //why, rain world
        return new IconSymbol.IconSymbolData(type, AbstractPhysicalObject.AbstractObjectType.Creature, intData);
    }
}
