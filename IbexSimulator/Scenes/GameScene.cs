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
using IbexGame.UI;
using MonoGameGum;
using IbexGame.GameObjects;
using IbexGame.Config;

namespace IbexGame.Scenes;

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

    // The grayscale shader effect.
    private Material _grayscaleEffect;

    // The amount of saturation to provide the grayscale shader effect.
    private float _saturation = 1.0f;

    // The speed of the fade to grayscale effect.
    private const float FADE_SPEED = 0.02f;

    private Texture2D _levelBackground;
    private List<Ball> _balls;

    private Random _platformRand;

    private Vector2 lastGenPlatformCoord;

    private List<Brick> _bricks;

    private int _currentLevelIndex;

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

        _roomBounds = Core.GraphicsDevice.PresentationParameters.Bounds;

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

        _paddle = new Paddle();
        _paddle.LoadContent();

        Ball.LoadContent();
        Flower.LoadContent();
        Brick.LoadContent();

        _balls = new List<Ball>();
        _bricks = new List<Brick>();

        float initialPlatformPosY = Core.GraphicsDevice.PresentationParameters.BackBufferHeight * 0.5f;
        float initialPlatformPosX = Core.GraphicsDevice.PresentationParameters.BackBufferWidth * 0.1f;
        Vector2 initialPlatformPos = new Vector2(initialPlatformPosX, initialPlatformPosY);

        _balls.Add(new Ball(_paddle.getPosition(), _paddle.getPaddleHeight(), _paddle.getDirection()));

        _platformRand = new Random();

        lastGenPlatformCoord = initialPlatformPos + new Vector2(400.0f, -250.0f);

        LoadLevel(LevelRegistry.AllLevels[_currentLevelIndex]);

        _levelBackground = Core.Content.Load<Texture2D>("images/backgrounds/mountains/origbig");

        // Load the font
        _font = Content.Load<SpriteFont>("fonts/mountain_and_nature/Mountain_and_Nature_small");

        // Load the grayscale effect.
        _grayscaleEffect = Content.WatchMaterial("effects/grayscaleEffect");
        _grayscaleEffect.IsDebugVisible = false;
    }

    public override void Update(GameTime gameTime)
    {
        // Update the grayscale effect if it was changed
        _grayscaleEffect.Update();

        // Ensure the UI is always updated.
        _ui.Update(gameTime);

        if (_state != GameState.Playing)
        {
            // The game is in either a paused or game over state, so
            // gradually decrease the saturation to create the fading grayscale.
            _saturation = Math.Max(0.0f, _saturation - FADE_SPEED);

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
            ball.Update(gameTime);
        }

        _balls.RemoveAll(ball => ball.toRemove);

        CollisionChecks();

        _bricks.RemoveAll(brick => brick.IsToRemove());

        checkChangeScene();

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

    private void CollisionChecks()
    {
        Rectangle paddleBounds = _paddle.getBounds();

        foreach(Ball ball in _balls)
        {
            Circle ballBounds = ball.GetBounds();

            /*Balls - paddle collision*/
            bool collision = false;
            PartCollided sideColl = PartCollided.NONE;
            (collision, sideColl) = areIntersecting(ballBounds, paddleBounds);

            if (collision)
            {
                ball.CalculateBallBounce(paddleBounds, true);

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
                        brick.IsHit();
                        break;
                    }
                }
            }
        }

        /*Balls - walls collision*/
        foreach(Ball ball in _balls)
        {
            Circle ballBounds = ball.GetBounds();

            if (ballBounds.Top < _roomBounds.Top)
            {
                ball.Bounce(Vector2.UnitY);
            }
            else if (ballBounds.Bottom > _roomBounds.Bottom)
            {
                ball.toRemove = true;
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

        _balls.RemoveAll(ball => ball.toRemove);

        if(_balls.Count == 0)
        {
            Core.ChangeScene(new GameOver(_score));
        }

    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.White);

        if (_state != GameState.Playing)
        {
            // We are in a game over state, so apply the saturation parameter.
            _grayscaleEffect.SetParameter("Saturation", _saturation);

            // And begin the sprite batch using the grayscale effect.
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: _grayscaleEffect.Effect);
        }
        else
        {
            // Begin the sprite batch to prepare for rendering.
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }
        Core.SpriteBatch.Draw(_levelBackground, Core.GraphicsDevice.PresentationParameters.Bounds, Color.White);

        foreach(Ball ball in _balls)
        {
            ball.Draw();
        }

        foreach(Brick brick in _bricks)
        {
            brick.Draw();
        }

        _paddle.Draw();

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

        // List<Texture2D> clouds = new List<Texture2D>();

        // foreach(string backgroundStr in config.backgroundStr)
        // {
        //     clouds.Add(Content.Load<Texture2D>(backgroundStr));
        // }

        // _levelBackground = new Background(clouds);

    }

    private void checkChangeScene()
    {
        if(_bricks.Count == 0)
        {
            _score -= _ui.getTimer();
            if(_score < 0)
                _score = 0;
            //_score += SCORE_LEVEL;
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
    }

}
