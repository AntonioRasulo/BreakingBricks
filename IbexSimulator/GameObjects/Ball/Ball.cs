using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace IbexGame.GameObjects;

public enum AttachedStatus
{
    ATTACHED,
    FREE
}

public class Ball
{
    private Vector2 _position;

    private Vector2 _velocity;

    private Sprite _ballSprite;

    private float _movementSpeed = 6.0f;

    private static Texture2D _whiteTexture;

    private Vector2 SCALE = new Vector2(0.05f, 0.05f);

    public bool toRemove {get; set;}

    private AttachedStatus _attachStatus;

    public Ball(Vector2 paddlePosition, float paddleHeight, float dirX)
    {
        _ballSprite = new Sprite(_whiteTexture)
        {
            Scale = SCALE
        };
        _ballSprite.CenterOrigin();

        float yPositionOffset = paddleHeight * 0.5f + _ballSprite.Height * 0.5f;

        _position = paddlePosition - new Vector2(0.0f, yPositionOffset);

        if(dirX == 0)
        {
            dirX = 1;
        }

        Vector2 direction = new Vector2(dirX, -1);

        _velocity = direction * _movementSpeed;

        toRemove = false;
    }

    public void Bounce(Vector2 normal)
    {
        Vector2 newPosition = _position;

        //Adjust the position based on the normal to prevent sticking to walls.
        if (normal.X != 0)
        {
            // We are bouncing off a vertical wall (left/right).
            // Move slightly away from the wall in the direction of the normal.
            newPosition.X += normal.X * (_ballSprite.Width * 0.5f);
        }

        if (normal.Y != 0)
        {
            // We are bouncing off a horizontal wall (top/bottom).
            // Move slightly way from the wall in the direction of the normal.
            newPosition.Y += normal.Y * (_ballSprite.Height * 0.5f);
        }

        // Apply the new position
        _position = newPosition;

        // Normalize before reflecting
        normal.Normalize();

        // Apply reflection based on the normal.
        _velocity = Vector2.Reflect(_velocity, normal);
    }

    public static void LoadContent()
    {
        _whiteTexture = Core.Content.Load<Texture2D>("images/Ball/ball_white_shaded_outline");
    }

    public void Update(GameTime gameTime)
    {
        // switch (_attachStatus)
        // {
        //     case AttachedStatus.ATTACHED:
        //     break;
        //     case AttachedStatus.FREE:
        //     break;
        // }
        if(_attachStatus == AttachedStatus.ATTACHED)
        {
            
        }
        _position += _velocity;
    }

    public void Draw()
    {
        _ballSprite.Draw(Core.SpriteBatch, _position);
    }

    public Circle GetBounds()
    {
        int radius = (int)(_ballSprite.Width * 0.5f);

        return new Circle((int)_position.X, (int)_position.Y, radius);
    }

    public Rectangle GetRectangleBounds()
    {
        Rectangle bounds = new Rectangle(
            (int)(_position.X - _ballSprite.Width*0.5f),
            (int)(_position.Y - _ballSprite.Height*0.5f),
            (int)_ballSprite.Width,
            (int)_ballSprite.Height
        );

        return bounds;
    }

    // public Vector2 GetPosition()
    // {
    //     return _position;
    // }

}