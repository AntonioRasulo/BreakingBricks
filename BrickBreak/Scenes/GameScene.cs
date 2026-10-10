using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Content;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using BrickBreak.UI;
using MonoGameGum;
using BrickBreak.GameObjects;
using BrickBreak.Config;
using BrickBreak.Utility;
using BrickBreak.Backgrounds;

namespace BrickBreak.Scenes;

public class GameScene : Scene
{
    private enum GameState
    {
        Playing,
        Paused
    }

    private Paddle _paddle;

    private Rectangle _roomBounds;

    // The SpriteFont Description used to draw text.
    private SpriteFont _font;

    // Tracks the players score.
    private int _score;

    private GameSceneUI _ui;

    private GameState _state;

    // The color swap shader material.  
    private Material _colorSwapMaterial;

    // The amount of saturation to provide the grayscale shader effect.
    private float _saturation = 1.0f;

    // The speed of the fade to grayscale effect.
    private const float FADE_SPEED = 0.02f;

    private List<Background> _levelBackground;
    private List<Ball> _balls;

    private Random _platformRand;

    private Vector2 lastGenPlatformCoord;

    private List<Brick> _bricks;

    private int _currentLevelIndex;

    private const int SCORE_LEVEL = 100;
    private const int LOST_BALL_SCORE = 20;

    private Texture2D _colorMap;

    private RedColorMap _bricksColorMap;

    private TimeSpan _lastBallPaddleCollTime;
    private double _blinkTimerPaddleMs = 0;

    private SpriteCamera3d _camera;

    // Defines the tilemap to draw.
    private Tilemap _tilemap;

    public GameScene(int startingLevel)
    {
        _currentLevelIndex = startingLevel;
    }

    public override void Initialize()
    {

        base.Initialize();

        // During the game scene, we want to disable exit on escape. Instead,
        // the escape key will be used to return back to the title screen
        Core.ExitOnEscape = false;

        // Rectangle screenBounds = Core.GraphicsDevice.PresentationParameters.Bounds;

        // // //_roomBounds = Core.GraphicsDevice.PresentationParameters.Bounds;
        // _roomBounds = new Rectangle(
        //     (int)_tilemap.TileWidth,
        //     (int)_tilemap.TileHeight,
        //     screenBounds.Width - (int)_tilemap.TileWidth * 2,
        //     screenBounds.Height - (int)_tilemap.TileHeight * 2
        // );

        // _roomBounds.Inflate(-_tilemap.TileWidth, -_tilemap.TileHeight);

        // Create any UI elements from the root element created in previous
        // scenes.
        GumService.Default.Root.Children.Clear();

        // Initialize the user interface for the game scene.
        InitializeUI();

        // Initialize a new game to be played.
        InitializeNewGame();

    }

    private void InitializeUI()
    {
        // Clear out any previous UI element incase we came here
        // from a different scene.
        GumService.Default.Root.Children.Clear();

        // Create the game scene ui instance.
        _ui = new GameSceneUI();
        _ui.UpdateLivesText(_paddle.getLives());

        // Subscribe to the events from the game scene ui.
        _ui.ResumeButtonClick += OnResumeButtonClicked;
        _ui.QuitButtonClick += OnQuitButtonClicked;
    }

    // TODO complete implementation
    private void InitializeNewGame()
    {
        _state = GameState.Playing;
    }

    private void OnResumeButtonClicked(object sender, EventArgs args)
    {
        // Change the game state back to playing.
        _state = GameState.Playing;
    }

    private void OnQuitButtonClicked(object sender, EventArgs args)
    {
        // Player has chosen to quit, so return back to the title scene.
        Core.ChangeScene(new TitleScene());
    }

    public override void LoadContent()
    {
        try
        {
            // Load the background theme music
            Song theme = Content.Load<Song>("audio/Music/4-Winter2-night-zwinzlergames");
            //Core.Audio.PlaySong(theme);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load theme music: {ex.Message}");
        }

        // Create the tilemap from the XML configuration file.
        _tilemap = Tilemap.FromFile(Content, "images/Tilemap/border_tilemap.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        Rectangle screenBounds = Core.GraphicsDevice.PresentationParameters.Bounds;

        _roomBounds = new Rectangle(
            (int)(_tilemap.TileWidth * 1f),
            (int)(_tilemap.TileHeight * 1f),
            screenBounds.Width - (int)(_tilemap.TileWidth * 2f),
            screenBounds.Height
        );

        _paddle = new Paddle(_roomBounds);
        _paddle.LoadContent();

        Ball.LoadContent();
        Brick.LoadContent();
        CollectibleHandler.LoadContent();

        _balls = new List<Ball>();
        _bricks = new List<Brick>();

        float initialPlatformPosY = Core.GraphicsDevice.PresentationParameters.BackBufferHeight * 0.5f;
        float initialPlatformPosX = Core.GraphicsDevice.PresentationParameters.BackBufferWidth * 0.1f;
        Vector2 initialPlatformPos = new Vector2(initialPlatformPosX, initialPlatformPosY);

        _balls.Add(new Ball(_paddle.getPosition(), _paddle.getPaddleHeight(), _paddle.getDirection(), AttachedStatus.ATTACHED));

        _platformRand = new Random();

        lastGenPlatformCoord = initialPlatformPos + new Vector2(400.0f, -250.0f);

        LoadLevel(LevelRegistry.AllLevels[_currentLevelIndex]);

        // Load the font
        _font = Content.Load<SpriteFont>("fonts/mountain_and_nature/Mountain_and_Nature_small");

        // Load the colorSwap material
        _colorSwapMaterial = Content.WatchMaterial("effects/colorSwapEffect");
        _colorSwapMaterial.IsDebugVisible = true;

        _colorMap = Core.Content.Load<Texture2D>("images/effects/color-map-1");
        _bricksColorMap = new RedColorMap();

        _bricksColorMap.SetColorsByRedValue(new Dictionary<int, Color>
        {
            // main color
            [97] = Color.Blue,
            [64] = Color.DarkBlue
        }, false);

        _colorSwapMaterial.SetParameter("ColorMap", _bricksColorMap.ColorMap);
        _camera = new SpriteCamera3d();
        _colorSwapMaterial.SetParameter("MatrixTransform", _camera.CalculateMatrixTransform());
        _colorSwapMaterial.SetParameter("ScreenSize", new Vector2(Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height));
    }

    public override void Update(GameTime gameTime)
    {
        Moving.readInput();

        // Update the colorSwap material if it was changed
        _colorSwapMaterial.Update();

        // float paddlePosX = _paddle.getPosition().X;
        // if(paddlePosX > Core.GraphicsDevice.Viewport.Width * 0.75f)
        // {
        //     paddlePosX = Core.GraphicsDevice.Viewport.Width * 0.75f;
        // }
        // else if(paddlePosX < Core.GraphicsDevice.Viewport.Width * 0.25f)
        // {
        //     paddlePosX = Core.GraphicsDevice.Viewport.Width * 0.25f;
        // }

        // var spinAmount = paddlePosX / (float)Core.GraphicsDevice.Viewport.Width;
        // spinAmount = MathHelper.SmoothStep(-.1f, .1f, spinAmount);
        // _colorSwapMaterial.SetParameter("SpinAmount", spinAmount);

        // Ensure the UI is always updated.
        _ui.Update(gameTime);

        if (_state != GameState.Playing)
        {
            // The game is in either a paused or game over state, so
            // gradually decrease the saturation to create the fading grayscale.
            _saturation = Math.Max(0.0f, _saturation - FADE_SPEED);

        }
        else
        {
            _saturation = 1.0f;
        }

        // If the pause button is pressed, toggle the pause state. TODO implement GameController
        if(Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape) || Core.Input.GamePads[(int)PlayerIndex.One].WasButtonJustPressed(Buttons.Start))
        {
            TogglePause();
        }

        // At this point, if the game is paused, just return back early.
        if (_state == GameState.Paused)
        {
            return;
        }

        _paddle.Update(gameTime);

        foreach(Ball ball in _balls)
        {
            ball.Update(gameTime, _paddle.getVelocity());
        }

        CollectibleHandler.Update(gameTime);

        CollisionChecks(gameTime);

        _bricks.RemoveAll(brick => brick.IsToRemove());
        _balls.RemoveAll(ball => ball.toRemove);

        Moving.updatePrevInputState();

        if((_balls.Count == 0) && (_paddle.isDead() == false))
        {
            _balls.Add(new Ball(_paddle.getPosition(), _paddle.getBounds().Height, _paddle.getDirection(), AttachedStatus.ATTACHED));
        }

        checkChangeScene();

        foreach(Background bg in _levelBackground)
        {
            bg.Update(gameTime);
        }

    }

    private void TogglePause()
    {
        if (_state == GameState.Paused)
        {
            // We're now unpausing the game, so hide the pause panel.
            _ui.HidePausePanel();

            // And set the state back to playing.
            _state = GameState.Playing;
        }
        else
        {
            // We're now pausing the game, so show the pause panel.
            _ui.ShowPausePanel();

            // And set the state to paused.
            _state = GameState.Paused;

            // Set the grayscale effect saturation to 1.0f
            _saturation = 1.0f;
        }
    }

    private void CollisionChecks(GameTime gameTime)
    {
        Rectangle paddleBounds = _paddle.getBounds();

        foreach(Ball ball in _balls)
        {
            if(ball.getAttachedStatus() == AttachedStatus.FREE)
            {
                Circle ballBounds = ball.GetBounds();

                /*Balls - paddle collision*/
                bool collision = false;
                PartCollided sideColl = PartCollided.NONE;
                (collision, sideColl) = areIntersecting(ballBounds, paddleBounds);

                if(collision)
                {
                    ball.CalculateBallBounce(paddleBounds, true);
                    _lastBallPaddleCollTime = gameTime.TotalGameTime;
                }

                /*Balls - Bricks collision*/
                foreach(Brick brick in _bricks)
                {
                    Rectangle brickBounds = brick.GetBounds();

                    if(!brick.IsToRemove())
                    {
                        collision = false;
                        sideColl = PartCollided.NONE;
                        (collision, sideColl) = areIntersecting(ballBounds, brickBounds);

                        if (collision)
                        {
                            ball.CalculateBallBounce(brickBounds);
                            _score += brick.IsHit();
                            _ui.UpdateScoreText(_score);
                            if(brick.IsToRemove())
                            {
                                Vector2 position = brick.GetPosition();
                                Vector2 genPosition = new Vector2(position.X, position.Y + brickBounds.Height);
                                CollectibleHandler.GenerateCollectible(genPosition);
                            }
                            break;
                        }
                    }
                }
            }
        }

        /*Balls - walls collision*/
        foreach(Ball ball in _balls)
        {
            if(ball.getAttachedStatus() == AttachedStatus.FREE)
            {
                Circle ballBounds = ball.GetBounds();

                if (ballBounds.Top < _roomBounds.Top)
                {
                    ball.Bounce(Vector2.UnitY);
                }
                else if (ballBounds.Bottom > _roomBounds.Bottom)
                {
                    ball.toRemove = true;
                    int paddleLives = _paddle.getLives();
                    paddleLives--;
                    _paddle.setLives(paddleLives);
                    _ui.UpdateLivesText(paddleLives);
                    _score -= LOST_BALL_SCORE;
                    if(_score < 0)
                        _score = 0;
                    _ui.UpdateScoreText(_score);
                }

                if (ballBounds.Left < _roomBounds.Left)
                {
                    ball.Bounce(Vector2.UnitX);
                }
                else if (ballBounds.Right > _roomBounds.Right)
                {
                    ball.Bounce(-Vector2.UnitX);
                }
            }
        }

        /* Character - Collectible collision */
        collectibleType collectibleCollided = CollectibleHandler.CheckPaddleCollision(paddleBounds);

        switch (collectibleCollided)
        {
            case collectibleType.SPEED_UP:
            _paddle.setFastSpeed();
            break;
        }

    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(57, 120, 168));

        _colorSwapMaterial.SetParameter("Saturation", _saturation);

        SpriteSortMode spriteSortMode = SpriteSortMode.Immediate;

        if (_state != GameState.Playing)
        {
            spriteSortMode = SpriteSortMode.Deferred;
        }

        // Draw the background
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        foreach (var layer in _levelBackground)   // far layers first, near layers last
            layer.Draw(Color.White * 0.5f);
        Core.SpriteBatch.End();

        // Draw the background and apply no effects to it
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        //Core.SpriteBatch.Draw(_levelBackground, Core.GraphicsDevice.PresentationParameters.Bounds, Color.White);
        // Draw the tilemap
        //_tilemap.Draw(Core.SpriteBatch);
        _tilemap.DrawBorder(Core.SpriteBatch);
        Core.SpriteBatch.End();

        // Begin the sprite batch to prepare for rendering.
        Core.SpriteBatch.Begin( samplerState: SamplerState.PointClamp,
                                sortMode: spriteSortMode,
                                effect: _colorSwapMaterial.Effect);

        // Update the colorMap for the slime  
        _colorSwapMaterial.SetParameter("ColorMap", _colorMap);

        foreach(Ball ball in _balls)
        {
            ball.Draw();
        }

        foreach(Brick brick in _bricks)
        {
            brick.Draw();
        }

        // Draw lives sprite
        int roomWidth = Core.GraphicsDevice.PresentationParameters.BackBufferWidth;
        float distanceFromTopWall = 23.0f;
        float xOffset = 35.0f;
        Vector2 livesSpritePosition = new Vector2(roomWidth * 0.5f - xOffset, distanceFromTopWall);

        Core.SpriteBatch.Draw(Ball.whiteTexture, livesSpritePosition, Ball.whiteTexture.Bounds, Color.White, 0.0f, Vector2.Zero, new Vector2(0.05f, 0.05f), SpriteEffects.None, 0.0f);

        _paddle.Draw
        (
            () =>
            {
                const int flashTimeMs = 1000;
                const int blinkIntervalMs = 100;
                double delta = gameTime.ElapsedGameTime.TotalMilliseconds;
                Texture2D map = _colorMap;
                var elapsedMs = gameTime.TotalGameTime.TotalMilliseconds - _lastBallPaddleCollTime.TotalMilliseconds;
                var intervalsAgo = (int)(elapsedMs / flashTimeMs);

                if(elapsedMs < flashTimeMs)
                {
                    _blinkTimerPaddleMs += delta;

                    if(_blinkTimerPaddleMs >= blinkIntervalMs)
                    {
                        map = _bricksColorMap.ColorMap;
                        _blinkTimerPaddleMs = 0;
                    }
                }

                _colorSwapMaterial.SetParameter("ColorMap", map);
            }
        );

        CollectibleHandler.Draw();

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();

        // Draw the UI.
        _ui.Draw();

        base.Draw(gameTime);
    }

    public static (bool, PartCollided) areIntersecting(Circle circle, Rectangle rectangle)
    {
        int distanceX = Math.Abs(circle.X - rectangle.Center.X);
        int distanceY = Math.Abs(circle.Y - rectangle.Center.Y);

        float halfRectWidth = rectangle.Width * 0.5f;
        float halfRectHeight = rectangle.Height * 0.5f;

        if((distanceX > (halfRectWidth + circle.Radius)) ||
           (distanceY > (halfRectHeight + circle.Radius)))
        {
            return (false, PartCollided.NONE);
        }

        if(distanceX <= halfRectWidth) return  (true, PartCollided.SIDEX);

        if(distanceY <= halfRectHeight) return (true, PartCollided.SIDEY);

        double cornerDistanceSquare = Math.Pow(distanceX-halfRectWidth, 2) + Math.Pow(distanceY-halfRectHeight, 2);

        return (cornerDistanceSquare <= Math.Pow(circle.Radius, 2), PartCollided.ANGLE);
    }

    private void LoadLevel(LevelConfig config)
    {
//        _balls.Clear();
        _bricks.Clear();

        foreach(var row in config.Bricks)
        {
            foreach(var brickSpawn in row)
            {
                _bricks.Add(new Brick(brickSpawn.Color, brickSpawn.Type, NumCollisions.THREE, brickSpawn.Position));
            }
        }

        List<Texture2D> texturesBG = new List<Texture2D>();

        _levelBackground = [];

        foreach(var bgStrSpeed in config.bgDict)
        {
            _levelBackground.Add(new(Content.Load<Texture2D>(bgStrSpeed.Key), bgStrSpeed.Value));
        }

    }

    private void checkChangeScene()
    {
        if(_bricks.Count == 0)
        {
            _score -= _ui.getTimer();
            if(_score < 0)
                _score = 0;
            _score += SCORE_LEVEL;
            _ui.UpdateScoreText(_score);
            _currentLevelIndex++;
            if(_currentLevelIndex >= LevelRegistry.AllLevels.Count)
            {
                Core.ChangeScene(new GameOver(_score));
            }
            else
            {
                _ui.resetTimer();
                LoadLevel(LevelRegistry.AllLevels[_currentLevelIndex]);
            }
        }

        if(_paddle.isDead())
        {
            Core.ChangeScene(new GameOver(_score));
        }
    }

}
