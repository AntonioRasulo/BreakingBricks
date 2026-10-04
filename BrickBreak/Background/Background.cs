using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using System.Collections.Generic;

namespace BrickBreak.Backgrounds;

public class Background
{
    // The textures used for the background pattern.
    private List<Texture2D> _textures;

    private float _backgroundWidth;
    private float _backgroundHeight; 

    // The destination rectangle for the background pattern to fill.
    private Rectangle _backgroundDestination;
    private Rectangle _textureDestination;

    // The offset to apply when drawing the background pattern so it appears to
    // be scrolling.
    private Vector2 _backgroundOffset;

    // The speed that the background pattern scrolls.
    private float _scrollSpeed;

    public Background(List<Texture2D> texture, float scrollSpeed = 25.0f)
    {
        _textures = texture;

        _backgroundWidth = _textures[0].Width;
        _backgroundHeight = _textures[0].Height;

        // Set the background pattern destination rectangle to fill the entire
        // screen background.
        _backgroundDestination = Core.GraphicsDevice.PresentationParameters.Bounds;

        float screenHeight = Core.GraphicsDevice.PresentationParameters.BackBufferHeight;

        _textureDestination= new Rectangle(
            0,
            (int)(screenHeight - _backgroundHeight),  // Y position on screen
            _backgroundDestination.Width,
            (int)_backgroundHeight
        );

        _scrollSpeed = scrollSpeed;

    }

    public void Update(GameTime gameTime)
    {
        // Update the offsets for the background pattern wrapping so that it
        // scrolls down and to the right.
        float offset = _scrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        _backgroundOffset.X -= offset;

        // Ensure that the offsets do not go beyond the texture bounds so it is
        // a seamless wrap.
        _backgroundOffset.X %= _backgroundWidth;
    }

    public void Draw(Effect effect = null)
    {

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: effect);
        Core.SpriteBatch.Draw(_textures[0], _backgroundDestination, _textures[0].Bounds, Color.White * 0.5f);
        Core.SpriteBatch.End();

        Rectangle source = new Rectangle(
            (int)_backgroundOffset.X,
            0,
            _backgroundDestination.Width,
            (int)_backgroundHeight
        );

        SamplerState samplerStateBackground = new SamplerState();
        samplerStateBackground.AddressU = TextureAddressMode.Wrap;
        samplerStateBackground.AddressV = TextureAddressMode.Clamp;
        samplerStateBackground.Filter = TextureFilter.Point;
        Core.SpriteBatch.Begin(samplerState: samplerStateBackground);

        for(int index = 1; index < _textures.Count; index++)
        {
            Core.SpriteBatch.Draw(_textures[index], _textureDestination, source, Color.White * 0.5f);
        }

        Core.SpriteBatch.End();
    }

}
