using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BrickBreak.GameObjects;

public class PowerUpSprite : Collectible
{
    private Sprite _sprite;

    public PowerUpSprite(Vector2 position, collectibleType type, float scale):
    base(position, type)
    {
        Texture2D texture = type switch
        {
            collectibleType.STICKY => CollectibleHandler._stickySprite,
            _ => null
        };
        _sprite = new Sprite(texture);
        _sprite.CenterOrigin();
        _sprite.Scale = new Vector2(scale);
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