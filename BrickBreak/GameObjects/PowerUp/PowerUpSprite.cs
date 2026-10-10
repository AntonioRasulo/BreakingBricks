using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework;

namespace BrickBreak.GameObjects;

public class PowerUpSprite : Collectible
{
    private Sprite _sprite;

    public PowerUpSprite(Vector2 position, collectibleType type):
    base(position, type)
    {
        _sprite = type switch
        {
            //collectibleType.SPEED_UP => CollectibleHandler._speedUpAnimation,
            _ => null
        };   
    }

    public override void Update(GameTime gameTime)
    {
        int screenHeight = Core.GraphicsDevice.PresentationParameters.BackBufferHeight;
        if(_position.Y < screenHeight - _sprite.Height)
        {
            _position += VELOCITY_Y;
        }
        else
        {
            _position.Y = screenHeight - _sprite.Height;
        }   
    }

    public override void Draw()
    {
        _sprite.Draw(Core.SpriteBatch, _position);
    }

    public override Rectangle getBounds()
    {
        Rectangle bounds = new Rectangle(
            (int)_position.X,
            (int)_position.Y,
            (int)_sprite.Width,
            (int)_sprite.Height
        );

        return bounds;

    }

}