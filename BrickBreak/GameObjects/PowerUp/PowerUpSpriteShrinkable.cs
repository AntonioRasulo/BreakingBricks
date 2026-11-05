using Microsoft.Xna.Framework;

namespace BrickBreak.GameObjects;

public class PowerUpSpriteShrinkable : PowerUpSprite
{
    private float _minShrink;
    private float _maxShrink;
    private float _currentShrink;
    private bool _shrink;
    private float _scaleY;
    private const float SHRINK_FACTOR = 0.002f;

    public PowerUpSpriteShrinkable(Vector2 position, collectibleType type, bool shrink, float scaleX = 1.0f, float scaleY = 1.0f):
    base(position, type, scaleX, scaleY)
    {
        _shrink = shrink;

        _minShrink = 0f;
        _maxShrink = scaleX;
        _scaleY = scaleY;

        if(_shrink)
        {            
            _currentShrink = _maxShrink;
        }
        else
        {
            _currentShrink = _minShrink;
            _sprite.Scale = new Vector2(_currentShrink, _scaleY);
        }
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if(_shrink)
        {
            _currentShrink -= SHRINK_FACTOR;
            if(_currentShrink <= 0.0f)
            {
                _currentShrink = _maxShrink;
            }
            
        }
        else
        {
            _currentShrink += SHRINK_FACTOR;
            if(_currentShrink >= _maxShrink)
            {
                _currentShrink = 0.0f;
            }
        }
        _sprite.Scale = new Vector2(_currentShrink, _scaleY);
    }
}