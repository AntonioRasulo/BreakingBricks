using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;

public class CollisionString
{
    private Vector2 _position;

    private static SpriteFont _font;

    private String _string;

    private static float TIME_ON_SCREEN_MS = 500;

    private float _screenTime;
    private Vector2 _size;

    private static float SPEED_UP = 0.25F;

    private static float lastRotation;
    private float _rotation;

    public CollisionString(String str, Vector2 position)
    {
        _string = str;
        _position = position;
        _screenTime = TIME_ON_SCREEN_MS;
        _size = _font.MeasureString(_string);
        //_position.X -= _size.X * 0.5f;

        _rotation = lastRotation switch
        {
            -45 => 0,
            0 => 45,
            _ => -45
        };

        lastRotation = _rotation;

    }

    public static void LoadContent()
    {
        _font = Core.Content.Load<SpriteFont>("fonts/cartoonFonts/cartoon");
    }

    public void Update(GameTime gameTime)
    {
        if(_screenTime > 0)
        {        
            float deltaMs = gameTime.ElapsedGameTime.Milliseconds;
            _screenTime -= deltaMs;
            _position.Y -= SPEED_UP;
        }
    }

    public void Draw()
    {
        Vector2 originRotation = new(_size.X * 0.5f, 0.0f);
        Core.SpriteBatch.DrawString(_font, _string, _position, Color.White, _rotation, originRotation, 1.0f, SpriteEffects.None, 0f);
    }

    public bool IsComplete()
    {
        return (_screenTime <= 0);
    }

}