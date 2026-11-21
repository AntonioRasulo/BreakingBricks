using System;
using System.Collections.Generic;
using BrickBreak.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace BrickBreak.GameObjects;

public enum SpeedState
{
    NORMAL_SPEED,
    FAST,
    SLOW
}

public enum SizeState
{
    NORMAL_SIZE,
    BIG,
    SMALL
}

public enum SpecialState
{
    NONE,
    MAGNETIC,
    STICKY
}

public static class SizeScale
{
    public const float NORMAL = 0.15f;
    public const float BIG = 0.2f;
    public const float SMALL = 0.1f;

}

public class Paddle
{
    private static Vector2 _position;

    private static Vector2 _velocity;

    private static Sprite _paddleSprite;

    private const float SCALE_Y = 0.15f; 

    private static Vector2 _scale;

    private const float NORMAL_SPEED = 8f;
    private const float SLOW_SPEED = 6f;
    private const float FAST_SPEED = 10.0f;

    private static int _lives;

    private static Rectangle _roomBounds;

    private float POWER_UP_TIME_DURATION = 10.0f;

    private static float _powerUpSpeedTimer = 0f;
    private static float _powerUpSizeTimer = 0f;
    private static float _powerUpSpecialTimer = 0f;

    private const float tweenTimeMs = 100;
    private static float _tweenTimer = 0f;
    private const int tweenPercentage = 10;

    private static float _clampingXFactor = 360.0f;

    private static SpeedState _speedState;
    private static SizeState _sizeState;
    private static SpecialState _specialState;

    private static Dictionary<SpecialState, Dictionary<SpeedState, Texture2D>> _textureDict;

    public Paddle(Rectangle roomBounds)
    {
        _speedState = SpeedState.NORMAL_SPEED;
        _sizeState = SizeState.NORMAL_SIZE;
        _specialState = SpecialState.NONE;

        float windowHeight = Core.GraphicsDevice.PresentationParameters.BackBufferHeight;
        float windowWidth = Core.GraphicsDevice.PresentationParameters.BackBufferWidth;

        _position = new Vector2(
            windowWidth * 0.5f,
            windowHeight * 0.975f);

        _scale = new(SizeScale.NORMAL, SCALE_Y);

        _lives = 3;

        _roomBounds = roomBounds;
    }

    public void LoadContent()
    {
        Texture2D fastPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_fast_outline");
        Texture2D magneticPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_magnetic_outline");
        Texture2D goldenPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_golden_outline");
        Texture2D normalPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_normal_outline");
        Texture2D slowPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_slow_outline");
        Texture2D stickyPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_sticky_outline");
        Texture2D magneticFastPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_magnetic_fast_outline");
        Texture2D magneticSlowPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_magnetic_slow_outline");
        Texture2D stickyFastPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_sticky_fast_outline");
        Texture2D stickySlowPaddle = Core.Content.Load<Texture2D>("images/paddles/paddle_sticky_slow_outline");

        _textureDict = new Dictionary<SpecialState, Dictionary<SpeedState, Texture2D>>
        {
            {SpecialState.NONE, new Dictionary<SpeedState, Texture2D>
            {
                {SpeedState.NORMAL_SPEED, normalPaddle},
                {SpeedState.FAST, fastPaddle},
                {SpeedState.SLOW, slowPaddle}
            }
            },
            {SpecialState.MAGNETIC, new Dictionary<SpeedState, Texture2D>
            {
                {SpeedState.NORMAL_SPEED, magneticPaddle},
                {SpeedState.FAST, magneticFastPaddle},
                {SpeedState.SLOW, magneticSlowPaddle}
            }
            },
            {SpecialState.STICKY, new Dictionary<SpeedState, Texture2D>
            {
                {SpeedState.NORMAL_SPEED, stickyPaddle},
                {SpeedState.FAST, stickyFastPaddle},
                {SpeedState.SLOW, stickySlowPaddle}
            }
            }
        };

        _paddleSprite = new Sprite(normalPaddle)
        {
            Scale = _scale
        };
        _paddleSprite.CenterOrigin();
    }

    public static void Update(GameTime gameTime)
    {

        float direction = (float)Moving.IsMoving();

        Rectangle paddleBounds = getBounds();

        if(paddleBounds.Left <= _roomBounds.Left && direction < 0)
        {
            float offset = _roomBounds.Left - paddleBounds.Left;
            _position.X += offset;
            direction = 0.0f;
        }

        if(paddleBounds.Right >= _roomBounds.Right && direction > 0)
        {
            float offset = paddleBounds.Right - _roomBounds.Right;
            _position.X -= offset;
            direction = 0.0f;
        }

        _velocity = _speedState switch
        {
            SpeedState.FAST => new Vector2(direction * FAST_SPEED, 0f),
            SpeedState.SLOW => new Vector2(direction * SLOW_SPEED, 0f),
            _ => new Vector2(direction * NORMAL_SPEED, 0f),
        };

        _position += _velocity;

        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if(_powerUpSpeedTimer > 0)
        {
            _powerUpSpeedTimer -= delta;
            if(_powerUpSpeedTimer <= 0)
            {
                _powerUpSpeedTimer = 0;
                setSpeedState(SpeedState.NORMAL_SPEED);
            }
        }

        if(_powerUpSizeTimer > 0)
        {
            _powerUpSizeTimer -= delta;
            if(_powerUpSizeTimer <= 0)
            {
                _powerUpSizeTimer = 0;
                setSizeState(SizeState.NORMAL_SIZE);
            }
        }

        if(_powerUpSpecialTimer > 0)
        {
            _powerUpSpecialTimer -= delta;
            if(_powerUpSpecialTimer <= 0)
            {
                _powerUpSpecialTimer = 0;
                setSpecialState(SpecialState.NONE);
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

    public static Rectangle getBounds()
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

    public static float getDirection()
    {
        return _velocity.X;
    }

    public static Vector2 getPosition()
    {
        return _position;
    }

    public static float getPaddleHeight()
    {
        return _paddleSprite.Height;
    }

    public static float getVelocity()
    {
        return _velocity.X;
    }

    public static int getLives()
    {
        return _lives;
    }

    public static void setLives(int lives)
    {
        _lives = lives;
    }

    public bool isDead()
    {
        return (_lives == 0);
    }

    public void hitPowerUp(collectibleType type)
    {
        bool updatePaddleSprite = true;

        _speedState = type switch
        {
            collectibleType.SPEED_UP => SpeedState.FAST,
            collectibleType.SPEED_DOWN => SpeedState.SLOW,
            _ => _speedState
        };

        _sizeState = type switch
        {
            collectibleType.ENLARGE => SizeState.BIG,
            collectibleType.SHRINK => SizeState.SMALL,
            _ => _sizeState
        };

        SpecialState oldSpecialState = _specialState;

        _specialState = type switch
        {
            collectibleType.STICKY => SpecialState.STICKY,
            collectibleType.MAGNETIC => SpecialState.MAGNETIC,
            _ => _specialState
        };

        switch(type)
        {
            case collectibleType.SPEED_UP:
            case collectibleType.SPEED_DOWN:
                _powerUpSpeedTimer = POWER_UP_TIME_DURATION;
            break;
            case collectibleType.ENLARGE:
            case collectibleType.SHRINK:
                _powerUpSizeTimer = POWER_UP_TIME_DURATION;
            break;
            case collectibleType.STICKY:
            case collectibleType.MAGNETIC:
                _powerUpSpecialTimer = POWER_UP_TIME_DURATION;
            break;
            case collectibleType.BALL_SPLIT:
                BallsHandler.SplitBalls();
                updatePaddleSprite = false;
            break;
        }

        if(_specialState != SpecialState.STICKY && oldSpecialState == SpecialState.STICKY)
        {
            Ball.Detach();
        }

        if(updatePaddleSprite)
        {
            UpdatePaddleSprite();
        }
    }

    private static void setSpeedState(SpeedState speedState)
    {
        _speedState = speedState;
        UpdatePaddleSprite();
    }

    private static void setSizeState(SizeState sizeState)
    {
        _sizeState = sizeState;
        UpdatePaddleSprite();
    }

    private static void setSpecialState(SpecialState specialState)
    {
        _specialState = specialState;
        UpdatePaddleSprite();
    }

    private static void UpdatePaddleSprite()
    {
        Texture2D newSprite = _textureDict[_specialState][_speedState];

        restoreScale();

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

    private static void restoreScale()
    {
        float scaleX = _sizeState switch
        {
            SizeState.BIG => SizeScale.BIG,
            SizeState.SMALL => SizeScale.SMALL,
            _ => SizeScale.NORMAL
        };
        _scale = new Vector2(scaleX, SCALE_Y); 
        _paddleSprite.Scale = _scale;
    }

    public static bool isSitcky()
    {
        if(_specialState == SpecialState.STICKY)
        {
            return true;
        }
        return false;
    }

    public static bool isMagnetic()
    {
        if(_specialState == SpecialState.MAGNETIC)
        {
            return true;
        }
        return false;
    }

    public static float getClampingXFactor()
    {
        return _clampingXFactor * _scale.X;
    }

}
