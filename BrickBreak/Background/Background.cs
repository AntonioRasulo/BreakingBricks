using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using System;

namespace BrickBreak.Backgrounds;

public class Background
{
    private readonly Texture2D _texture;
    private readonly float _scrollSpeed;   // pixels/sec. Positive = scrolls left, negative = right
    private float _offsetX;

    public Background(Texture2D texture, float scrollSpeed)
    {
        _texture = texture;
        _scrollSpeed = scrollSpeed;
    }

    // Width of one tile after scaling the texture to the screen height.
    private int GetTileWidth(int screenHeight)
    {
        float scale = (float)screenHeight / _texture.Height;
        return Math.Max(1, (int)Math.Round(_texture.Width * scale));
    }

    public void Update(GameTime gameTime)
    {
        int tileWidth = GetTileWidth(Core.GraphicsDevice.Viewport.Height);

        _offsetX += _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        _offsetX %= tileWidth;
        if (_offsetX < 0) _offsetX += tileWidth;
    }

    // Call between SpriteBatch.Begin/End.
    public void Draw(Color? tint = null)
    {
        Color color = tint ?? Color.White;

        int screenWidth = Core.GraphicsDevice.Viewport.Width;
        int screenHeight = Core.GraphicsDevice.Viewport.Height;
        int tileWidth = GetTileWidth(screenHeight);

        int startX = -(int)MathF.Floor(_offsetX);

        for (int x = startX; x < screenWidth; x += tileWidth)
        {
            // Integer rectangles on exact tile boundaries avoid hairline seams.
            var dest = new Rectangle(x, 0, tileWidth, screenHeight);
            Core.SpriteBatch.Draw(_texture, dest, color);
        }
    }
}