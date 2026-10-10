using System;
using BrickBreak.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace BrickBreak.GameObjects;

public enum PaddleState
{
    NORMAL,
    FAST,
    SLOW,
    MAGNETIC,
    STICKY,
    GOLDEN
}

public static class SizeScale
{
    public const float NORMAL = 0.15f;
    public const float BIG = 0.2f;
    public const float SMALL = 0.1f;

}

public class Paddle
{
    private Vector2 _position;

    private Vector2 _velocity;

    private Sprite _paddleSprite;

    private Texture2D _fastPaddle;
    private Texture2D _goldenPaddle;
    private Texture2D _magneticPaddle;
    private Texture2D _normalPaddle;
    private Texture2D _slowPaddle;
    private Texture2D _stickyPaddle;

    private float xSizeScale;

    private Vector2 SCALE;

    private const float NORMAL_SPEED = 5f;
    private const float SLOW_SPEED = 4f;
    private const float FAST_SPEED = 6.0f;

    //private float _speed = NORMAL_SPEED;

    private int _lives = 3;

    private PaddleState _paddleState;

    private Rectangle _roomBounds;

    private float POWER_UP_TIME_DURATION = 10.0f;
    private float _powerUpTimer = 0f;

    public Paddle(Rectangle roomBounds)
    {
        _paddleState = PaddleState.NORMAL;

        float windowHeight = Core.GraphicsDevice.PresentationParameters.BackBufferHeight;
        float windowWidth = Core.GraphicsDevice.PresentationParameters.BackBufferWidth;

        _position = new Vector2(
            windowWidth * 0.5f,
            windowHeight * 0.975f);

        xSizeScale = SizeScale.BIG;

        SCALE = new(xSizeScale, 0.15f);

        _roomBounds = roomBounds;
    }

    public void LoadContent()
    {
        _fastPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_fast_outline");
        _magneticPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_magnetic_outline");
        _goldenPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_golden_outline");
        _normalPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_normal_outline");
        _slowPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_slow_outline");
        _stickyPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_sticky_outline");

        _paddleSprite = new Sprite(_normalPaddle)
        {
            Scale = SCALE
        };
        _paddleSprite.CenterOrigin();
    }

    public void Update(GameTime gameTime)
    {

        float direction = (float)Moving.IsMoving();

        Rectangle paddleBounds = getBounds();

        if(paddleBounds.Left <= _roomBounds.Left && direction < 0)
        {
            direction = 0.0f;
        }

        if(paddleBounds.Right >= _roomBounds.Right && direction > 0)
        {
            direction = 0.0f;
        }

        _velocity = _paddleState switch
        {
            PaddleState.FAST => new Vector2(direction * FAST_SPEED, 0f),
            PaddleState.SLOW => new Vector2(direction * SLOW_SPEED, 0f),
            _ => new Vector2(direction * NORMAL_SPEED, 0f),
        };

        _position += _velocity;

        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if(_powerUpTimer > 0)
        {
            _powerUpTimer -= delta;
            if(_powerUpTimer <= 0)
            {
                _powerUpTimer = 0;
                setState(PaddleState.NORMAL);
            }
        }

    }

    public void Draw(Action configureSpriteBatch)
    {
        configureSpriteBatch();

        _paddleSprite.Draw(Core.SpriteBatch, _position);
    }

    public Rectangle getBounds()
    {
        // Creating a bounding rectangle for the paddle
        Rectangle bounds = new Rectangle(
            (int)(_position.X - _paddleSprite.Width*0.5f),
            (int)(_position.Y - _paddleSprite.Height*0.5f),
            (int)_paddleSprite.Width,
            (int)_paddleSprite.Height
        );

        return bounds;
    }

    public float getDirection()
    {
        return _velocity.X;
    }

    public Vector2 getPosition()
    {
        return _position;
    }

    public float getPaddleHeight()
    {
        return _paddleSprite.Height;
    }

    public PaddleState getState()
    {
        return _paddleState;
    }

    public float getVelocity()
    {
        return _velocity.X;
    }

    public int getLives()
    {
        return _lives;
    }

    public void setLives(int lives)
    {
        _lives = lives;
    }

    public bool isDead()
    {
        return (_lives == 0);
    }

    public void setFastSpeed()
    {
        setState(PaddleState.FAST);
        _powerUpTimer += POWER_UP_TIME_DURATION;
    }

    public void setState(PaddleState newState)
    {
        _paddleState = newState;
        UpdatePaddleSprite();
    }

    private void UpdatePaddleSprite()
    {

        Texture2D newSprite = _paddleState switch
        {
            PaddleState.FAST => _fastPaddle,
            _ => _normalPaddle

        };
        _paddleSprite.SetRegion(newSprite);
    }

}
