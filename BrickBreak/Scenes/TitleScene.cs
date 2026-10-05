using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Content;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using Microsoft.Xna.Framework.Media;
using MonoGameGum;
using BrickBreak.UI;
using BrickBreak.GameObjects;
using BrickBreak.Utility;
using BrickBreak.Backgrounds;
using System.Collections.Generic;

namespace BrickBreak.Scenes;

public class TitleScene : Scene
{
    // The font to use to render normal text.
    private SpriteFont _font;

    private Background _levelBackground;

    private static bool _volumeInitialized = false;

    // The 3d material  
    private Material _3dMaterial;

    public override void Initialize()
    {
        // LoadContent is called during base.Initialize().
        base.Initialize();

        // While on the title screen, we can enable exit on escape so the player
        // can close the game by pressing the escape key.
        Core.ExitOnEscape = true;

        if(_volumeInitialized == false)
        {
            Core.Audio.SongVolume = 0.5f;
            Core.Audio.SoundEffectVolume = 0.5f;
            _volumeInitialized = true;
        }

        InitializeUI();
    }

    public override void LoadContent()
    {
        // Load the font for the standard text.
        _font = Core.Content.Load<SpriteFont>("fonts/mountain_and_nature/Mountain_and_Nature_small");

        try
        {
            // Load the background theme music
            Song theme = Content.Load<Song>("audio/Music/3-Fall-evening-zwinzlergames");
            //Core.Audio.PlaySong(theme);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load theme music: {ex.Message}");
        }

        // Load the 3d effect 
        _3dMaterial = Core.SharedContent.WatchMaterial("effects/3dEffect");
        _3dMaterial.IsDebugVisible = false;

        var camera = new SpriteCamera3d();
        _3dMaterial.SetParameter("MatrixTransform", camera.CalculateMatrixTransform());
        _3dMaterial.SetParameter("ScreenSize", new Vector2(Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height));

        Goat.LoadSoundEffects();

        List<Texture2D> texturesBG =
        [
            Content.Load<Texture2D>("images/backgrounds/backgroundTitle/1"),
            Content.Load<Texture2D>("images/backgrounds/backgroundTitle/2"),
            Content.Load<Texture2D>("images/backgrounds/backgroundTitle/3"),
            Content.Load<Texture2D>("images/backgrounds/backgroundTitle/4"),
            Content.Load<Texture2D>("images/backgrounds/backgroundTitle/5"),
        ];

        _levelBackground = new Background(texturesBG, 0.0f);
    }

    public override void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
        Moving.readInput();

        _levelBackground.Update(gameTime);

        _3dMaterial.Update();

        float spinAmount = -150;
        _3dMaterial.SetParameter("SpinAmount", spinAmount);
        Moving.updatePrevInputState();
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(32, 40, 78, 255));

        // Draw the background
        _levelBackground.Draw();

        // Begin the sprite batch to prepare for rendering.
         Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp,
                                 rasterizerState: RasterizerState.CullNone,
                                 effect: _3dMaterial.Effect);

        TitlePanelManager.Draw();

        // Always end the sprite batch when finished.
        Core.SpriteBatch.End();

        GumService.Default.Draw();
    }

    private void InitializeUI()
    {
        // Clear out any previous UI in case we came here from
        // a different screen:
        GumService.Default.Root.Children.Clear();

        TitlePanelManager.LoadContent();
    }

}