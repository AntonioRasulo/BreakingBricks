using Microsoft.Xna.Framework;

namespace BrickBreak.GameObjects;

public enum collectibleType
{
    NONE,
    SPEED_UP
}

public abstract class Collectible
{
    protected Vector2 _position;
    protected readonly Vector2 VELOCITY_Y = new(0f, 4.0f);

    protected collectibleType _type;

    public Collectible(Vector2 position, collectibleType type)
    {
        _position = position;
        _type = type;
    }

    public abstract void Update(GameTime gameTime);

    public abstract void Draw();

    public abstract Rectangle getBounds();

    public collectibleType GetCollectibleType()
    {
        return _type;
    }

}