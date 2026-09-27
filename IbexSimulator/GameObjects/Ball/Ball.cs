using IbexGame.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System;

namespace IbexGame.GameObjects;

public enum AttachedStatus
{
    ATTACHED,
    FREE
}

public enum PartCollided
{
    NONE,
    SIDEX,
    SIDEY,
    ANGLE
}

public class Ball
{
    private Vector2 _position;

    private Vector2 _velocity;

    private Sprite _ballSprite;

    private float _movementSpeed = 6.0f;

    private static Texture2D _whiteTexture;

    private Vector2 SCALE = new Vector2(0.04f, 0.04f);

    public bool toRemove {get; set;}

    private AttachedStatus _attachStatus;

    public Ball(Vector2 paddlePosition, float paddleHeight, float dirX, AttachedStatus attachStatus)
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

        _attachStatus = attachStatus;

    }

    public void Bounce(Vector2 normal, bool fromPaddle = false, float angle = 45)
    {
        if(fromPaddle && angle !=45)
        {
            angle -= (float)AngleBetween(Vector2.UnitX, _velocity);
            _velocity.Rotate((float)((Math.PI * angle)/180));
        }

        // Normalize before reflecting
        normal.Normalize();

        // Apply reflection based on the normal.
        _velocity = Vector2.Reflect(_velocity, normal);

        calculateNewPosition();

    }

    private void calculateNewPosition(float moveFactor = 1.0f)
    {
        Vector2 newPosition = _position;

        //Adjust the position based on the normal to prevent sticking to walls.
        if(_velocity.Y != 0)
        {
            newPosition.Y += _velocity.Y /Math.Abs(_velocity.Y) * moveFactor * (_ballSprite.Height * 0.5f);
        }

        if(_velocity.X != 0)
        {
            newPosition.X += _velocity.X /Math.Abs(_velocity.X)  * moveFactor * (_ballSprite.Height * 0.5f);
        }

        // Apply the new position
        _position = Vector2.Lerp(_position, newPosition, 0.5f);
    }

    public static void LoadContent()
    {
        _whiteTexture = Core.Content.Load<Texture2D>("images/Ball/ball_white_shaded_outline");
    }

    public void Update(GameTime gameTime, float paddleVelocity)
    {
        if(_attachStatus == AttachedStatus.ATTACHED)
        {
            _position += new Vector2(paddleVelocity, 0f);

            if(Moving.IsShootingPressed())
            {
                float directionX = (float)Moving.IsMoving();
                if(directionX != 0.0f)
                {
                    _velocity = new Vector2(directionX, -1) * _movementSpeed;
                }
                _attachStatus = AttachedStatus.FREE;
            }
        }

        if(_attachStatus == AttachedStatus.FREE)
        {
            _position += _velocity;
        }

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

    public void CalculateBallBounce(Rectangle paddleBounds, bool isPaddle = false)
    {
        float normalX = 0f;
        float normalY = 0f;
        bool insidePaddle = false;

        while(normalX ==0 && normalY==0)
        {
            float ballCenterX = _position.X;
            float ballCenterY = _position.Y;

            if (ballCenterY < paddleBounds.Top)
            {
                normalY = -1f;
            }
            else if (ballCenterY > paddleBounds.Bottom)
            {
                normalY = 1f;
            }

            if (ballCenterX > paddleBounds.Right)
            {
                normalX = 1f;
            }
            else if (ballCenterX < paddleBounds.Left)
            {
                normalX = -1f;
            }

            if(normalX != 0 || normalY != 0)
            {
                break;
            }

            if(isPaddle)
            {
                _position.Y = paddleBounds.Top - _ballSprite.Height * 0.5f;
                if(_position.X > paddleBounds.Center.X)
                {
                    _position.X = paddleBounds.Right + _ballSprite.Width * 0.5f;
                }
                else
                {
                    _position.X = paddleBounds.Left - _ballSprite.Width * 0.5f;
                }

                normalY = -1;
                insidePaddle = true;
            }
            else
            {
                while (isBallInside(paddleBounds, _position))
                {

                    if (_velocity.X != 0)
                    {
                        _position.X += -(_velocity.X / Math.Abs(_velocity.X)) * _ballSprite.Width /** 0.5f*/;
                    }

                    if (_velocity.Y != 0)
                    {
                        _position.Y += -(_velocity.Y / Math.Abs(_velocity.Y)) * _ballSprite.Height /** 0.5f*/;
                    }

                }
            }

        }

        Vector2 normal = new Vector2(normalX, normalY);

        if (isPaddle == true && insidePaddle == false)
        {
            float paddleSize = paddleBounds.Width; //204 when normal
            float paddleX = paddleBounds.X + paddleBounds.Width * 0.5f;
            float xDist = Math.Abs(_position.X - paddleX);
            float minThresh = (paddleSize * 10) / 100;
            float stdAngle = 45.0f;
            float minAngle = 30.0f;
            float halfPad = paddleSize * 0.5f;
            float maxAngle = 80.0f;
            float retAngle = stdAngle;

            // 30 + (30 * ((sogliaMinima-xDist))/(soglia - halfPad) <-- formula
            if(xDist >= minThresh)
            {
                float numAngle = (maxAngle - minAngle) * (minThresh - xDist);
                float denAngle = (minThresh - halfPad);
                retAngle = minAngle + numAngle / denAngle;
            }

            if (minThresh < paddleX - _position.X && retAngle != 45)
            {
                retAngle += 90;
            }

            Bounce(normal, true, retAngle);

        }
        else if (insidePaddle)
        {
            if(_position.X > paddleBounds.X)
            {
                _velocity = new Vector2(6.0f, -6.0f);
            }
            else
            {
                _velocity = new Vector2(-6.0f, -6.0f);
            }

        }
        else
        {
            Bounce(normal);
        }
    }

    private bool isBallInside(Rectangle paddleBounds, Vector2 position)
    {
        float ballX = position.X;
        float ballY = position.Y;
        if(ballX < paddleBounds.Right && ballX > paddleBounds.Left && ballY > paddleBounds.Top && ballY < paddleBounds.Bottom)
        {
            return true;
        }
        return false;
    }

    public static double AngleBetween(Vector2 vector1, Vector2 vector2)
    {
        double sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
        double cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

        return Math.Atan2(sin, cos) * (180 / Math.PI);
    }

    public AttachedStatus getAttachedStatus()
    {
        return _attachStatus;
    }

}