using Gum.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System;
using System.IO.Pipes;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

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

    private bool _collideWithPaddle;

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

        _collideWithPaddle = false;
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

        // Vector2 newPosition = _position;

        // //Adjust the position based on the normal to prevent sticking to walls.
        // // (normal.X != 0)
        // //{
        //     // We are bouncing off a vertical wall (left/right).
        //     // Move slightly away from the wall in the direction of the normal.
        //     //newPosition.X += normal.X * (_ballSprite.Width * 0.5f);
        // if(_velocity.Y != 0)
        // {
        //     newPosition.Y += _velocity.Y /Math.Abs(_velocity.Y)  * (_ballSprite.Height * 0.5f);
        // }

        // if(_velocity.X != 0)
        // {
        //     newPosition.X += _velocity.X /Math.Abs(_velocity.X)  * (_ballSprite.Height * 0.5f);
        // }

        // // Apply the new position
        // //_position = newPosition;
        // _position = Vector2.Lerp(_position, newPosition, 0.5f);

    }

    private void calculateNewPosition(float moveFactor = 1.0f)
    {
        Vector2 newPosition = _position;

        //Adjust the position based on the normal to prevent sticking to walls.
        // (normal.X != 0)
        //{
            // We are bouncing off a vertical wall (left/right).
            // Move slightly away from the wall in the direction of the normal.
            //newPosition.X += normal.X * (_ballSprite.Width * 0.5f);
        if(_velocity.Y != 0)
        {
            newPosition.Y += _velocity.Y /Math.Abs(_velocity.Y) * moveFactor * (_ballSprite.Height * 0.5f);
        }

        if(_velocity.X != 0)
        {
            newPosition.X += _velocity.X /Math.Abs(_velocity.X)  * moveFactor * (_ballSprite.Height * 0.5f);
        }

        // Apply the new position
        //_position = newPosition;
        _position = Vector2.Lerp(_position, newPosition, 0.5f);
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

    public void CalculateBallBounce(Rectangle paddleBounds, bool isPaddle = false, PaddleState paddleState = PaddleState.NORMAL)
    {
        Circle ballBounds = GetBounds();

        int[] distances =
        {
            Math.Abs(ballBounds.Top - paddleBounds.Bottom), // From bottom
            Math.Abs(ballBounds.Bottom -  paddleBounds.Top), // From top
            Math.Abs(ballBounds.Right - paddleBounds.Left), // From Left
            Math.Abs(ballBounds.Left - paddleBounds.Right), // From right
        };
        //Console.WriteLine("Printing distances");
        string text = "index: " + 0 + " value " + distances[0];
        //Console.WriteLine(text);
        int indexMin = 0;
        int min = distances[0];
        for (int i = 1; i < distances.Length; i++)
        {
            text = "index: " + i + " value " + distances[i];
            //Console.WriteLine(text);
            if (distances[i] < min)
            {
                min = distances[i];
                indexMin = i;
            }
        }

        switch (indexMin)
        {
            case 0:
                Bounce(Vector2.UnitY);
                if(!isPaddle)
                {
                    Console.WriteLine("Collided bottom");
                }
                break;
            case 1:
                if(!isPaddle)
                    Console.WriteLine("Collided top");
                if(isPaddle == true)
                {
                    float paddleSize = paddleBounds.Width; //204 when normal
                    float paddleX = paddleBounds.X + paddleBounds.Width * 0.5f;
                    float xDist = Math.Abs(_position.X - paddleX);
                    float minThresh = (paddleSize * 10) / 100;
                    float stdAngle = 45.0f;
                    float minAngle = 30.0f;
                    float halfPad = paddleSize * 0.5f;
                    float maxAngle = 80.0f;
                    //float maxAngle = 120.0f;
                    float retAngle = stdAngle;
                    if(xDist >= minThresh)
                    {
                        float numAngle = (maxAngle-minAngle) * (minThresh - xDist);
                        float denAngle = (minThresh - halfPad);
                        retAngle = minAngle + numAngle/denAngle;
                        //retAngle *= (-paddleX + _position.X)/xDist;
                    }
                    // xDist < sogliaMinima -> 45°
                    // sogliaMinima = (paddleSize * 10) / 100  -> 30°
                    // xDist == paddleSize * 0.5 -> max angle let's suppose ball.x bigger, 60°
                    // 30 + y * (20 - xDist) / (paddleSize * 0.5)
                    // y * (20 - paddleSize * 0.5) = 30 (paddleSize * 0.5)
                    // y = 30 (paddleSize * 0.5) / (20 - paddleSize * 0.5)
                    // y = 15*paddleSize/(20 - paddleSize * 0.5)
                    // 30 + (15 * paddleSize * ((sogliaMinima-xDist))/(halfPad * ((soglia - halfPad)) <-- formula
                    // 30 + (30 * ((sogliaMinima-xDist))/(soglia - halfPad) <-- formula

                    if(minThresh < paddleX - _position.X && retAngle != 45)
                    {
                        retAngle += 90;
                    }

                    Bounce(-Vector2.UnitY, true, retAngle);

                }
                else
                {
                    Bounce(-Vector2.UnitY);
                }
                break;
            case 2:
                Bounce(-Vector2.UnitX);
                if(!isPaddle)
                    Console.WriteLine("Collided left");
                // if(isPaddle && paddleState == PaddleState.FAST)
                // {
                //     calculateNewPosition(3.0f);
                // }
                break;
            case 3:
                
                    Bounce(Vector2.UnitX);
                if(!isPaddle)
                    Console.WriteLine("Collided right");
                // if(isPaddle && paddleState == PaddleState.FAST)
                // {
                //     calculateNewPosition(3.0f);
                // }
                break;
        }
    }

    public static double AngleBetween(Vector2 vector1, Vector2 vector2)
    {
        double sin = vector1.X * vector2.Y - vector2.X * vector1.Y;
        double cos = vector1.X * vector2.X + vector1.Y * vector2.Y;

        return Math.Atan2(sin, cos) * (180 / Math.PI);
    }

    public void setCollideWithPaddle(bool collided)
    {
        _collideWithPaddle = collided;
    }

    public bool getCollideWithPaddle()
    {
        return _collideWithPaddle;
    }

}