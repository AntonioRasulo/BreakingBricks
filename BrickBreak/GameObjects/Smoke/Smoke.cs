using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

public class Smoke
{
    private Vector2 _position;
    private static Animation _smokeAnimation;
    private AnimatedSprite _smokeAnimatedSprite;
    private static readonly TimeSpan ANIMATION_DELAY = new(50);

    private static float SCALE = 0.1f;

    public Smoke(Vector2 position)
    {
        _position = position;
        _smokeAnimatedSprite = new AnimatedSprite(_smokeAnimation)
        {
            Scale = new Vector2(SCALE, SCALE)
        };
        _smokeAnimatedSprite.CenterOrigin();
    }

    public static void LoadContent()
    {
        List<TextureRegion> smokeImages =
        [
            new TextureRegion(Core.Content.Load<Texture2D>("images/smoke/Smoke_Frame_01")),
            new TextureRegion(Core.Content.Load<Texture2D>("images/smoke/Smoke_Frame_02")),
            new TextureRegion(Core.Content.Load<Texture2D>("images/smoke/Smoke_Frame_03")),
            new TextureRegion(Core.Content.Load<Texture2D>("images/smoke/Smoke_Frame_04")),
            new TextureRegion(Core.Content.Load<Texture2D>("images/smoke/Smoke_Frame_05")),
            new TextureRegion(Core.Content.Load<Texture2D>("images/smoke/Smoke_Frame_06")),
            new TextureRegion(Core.Content.Load<Texture2D>("images/smoke/Smoke_Frame_07")),
            new TextureRegion(Core.Content.Load<Texture2D>("images/smoke/Smoke_Frame_08")),
            new TextureRegion(Core.Content.Load<Texture2D>("images/smoke/Smoke_Frame_09")),
            new TextureRegion(Core.Content.Load<Texture2D>("images/smoke/Smoke_Frame_10"))
        ];

        _smokeAnimation = new Animation(smokeImages, ANIMATION_DELAY);
    }

    public void Update(GameTime gameTime)
    {
        if(!_smokeAnimatedSprite.IsComplete)
        {
            _smokeAnimatedSprite.Update(gameTime);
        }
    }

    public void Draw()
    {
        _smokeAnimatedSprite.Draw(Core.SpriteBatch, _position);
    }

    public bool isAnimationComplete()
    {
        return _smokeAnimatedSprite.IsComplete;
    }

}