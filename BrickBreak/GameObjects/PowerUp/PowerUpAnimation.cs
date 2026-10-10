using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace BrickBreak.GameObjects;

public class PowerUpAnimation : Collectible
{
    AnimatedSprite _animatedSprite;

    public PowerUpAnimation(Vector2 position, collectibleType type, float scale, float rotation = 0f):
    base(position, type)
    {
        Animation powerAnimation = type switch
        {
            collectibleType.SPEED_UP => CollectibleHandler._speedUpAnimation,
            _ => null
        };

        _animatedSprite = new AnimatedSprite(powerAnimation)
        {
            Scale = new Vector2(scale, scale),
            Rotation = rotation
        };

        _animatedSprite.CenterOrigin();
    }

    public override void Update(GameTime gameTime)
    {
        _position += VELOCITY_Y;

        _animatedSprite.Update(gameTime);
    }

    public override Rectangle getBounds()
    {
        Rectangle bounds = new Rectangle(
            (int)_position.X,
            (int)_position.Y,
            (int)_animatedSprite.Width,
            (int)_animatedSprite.Height
        );

        return bounds;    
    }

    public override void Draw()
    {
        _animatedSprite.Draw(Core.SpriteBatch, _position);
    }

}