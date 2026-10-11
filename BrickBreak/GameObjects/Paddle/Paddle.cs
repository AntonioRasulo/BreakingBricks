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
    GOLDEN,
    BIG,
    SMALL
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

    private const float SCALE_Y = 0.15f; 

    private Vector2 _scale;

    private const float NORMAL_SPEED = 8f;
    private const float SLOW_SPEED = 6f;
    private const float FAST_SPEED = 10.0f;

    private int _lives = 3;

    private PaddleState _paddleState;

    private Rectangle _roomBounds;

    private float POWER_UP_TIME_DURATION = 10.0f;
    private float _powerUpTimer = 0f;

    private const float tweenTimeMs = 100;
    private float _tweenTimer = 0f;
    private const int tweenPercentage = 10;

    public Paddle(Rectangle roomBounds)
    {
        _paddleState = PaddleState.NORMAL;

        float windowHeight = Core.GraphicsDevice.PresentationParameters.BackBufferHeight;
        float windowWidth = Core.GraphicsDevice.PresentationParameters.BackBufferWidth;

        _position = new Vector2(
            windowWidth * 0.5f,
            windowHeight * 0.975f);

        _scale = new(SizeScale.NORMAL, SCALE_Y);

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
            Scale = _scale
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

        if(_tweenTimer > 0)
        {
            float deltaMs = (float)gameTime.ElapsedGameTime.Milliseconds;
            _tweenTimer -= deltaMs;
            if(_tweenTimer <= 0)
            {
                restoreScale();
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

    public void hitPowerUp(collectibleType type)
    {
        PaddleState newState = type switch
        {
            collectibleType.SPEED_UP => PaddleState.FAST,
            collectibleType.SPEED_DOWN => PaddleState.SLOW,
            _ => PaddleState.NORMAL
        };
        _powerUpTimer = POWER_UP_TIME_DURATION;
        setState(newState);
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
            PaddleState.SLOW => _slowPaddle,
            _ => _normalPaddle

        };
        _paddleSprite.SetRegion(newSprite);
    }

    public void startTweenScale()
    {
        _tweenTimer = tweenTimeMs;
        float scaleX = _paddleSprite.Scale.X;
        float scaleY = _paddleSprite.Scale.Y;
        float newScaleX = scaleX + (scaleX * tweenPercentage) * 0.01f;
        float newScaleY = scaleY + (scaleY * tweenPercentage) * 0.01f;
        _scale = new Vector2(newScaleX, newScaleY);
        _paddleSprite.Scale = _scale;
    }

    private void restoreScale()
    {
        float scaleX = _paddleState switch
        {
            PaddleState.BIG => SizeScale.BIG,
            PaddleState.SMALL => SizeScale.SMALL,
            _ => SizeScale.NORMAL
        };
        _scale = new Vector2(scaleX, SCALE_Y);
        _paddleSprite.Scale = _scale;
    }

}
