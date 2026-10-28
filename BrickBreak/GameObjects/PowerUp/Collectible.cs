using Microsoft.Xna.Framework;
using MonoGameLibrary;

namespace BrickBreak.GameObjects;

public enum collectibleType
{
    NONE,
    SPEED_UP,
    SPEED_DOWN,
    STICKY
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

    public virtual void Update(GameTime gameTime)
    {
        int screenHeight = Core.GraphicsDevice.PresentationParameters.BackBufferHeight;
        if(getBounds().Top <= screenHeight)
        {
            _position += VELOCITY_Y;
        }
    }

    public abstract void Draw();

    public abstract Rectangle getBounds();

    public collectibleType GetCollectibleType()
    {
        return _type;
    }

}