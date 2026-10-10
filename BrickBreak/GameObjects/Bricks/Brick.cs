using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace BrickBreak.GameObjects;

public enum BrickType
{
    BIG,
    SMALL,
    SQUARE
}

public enum BrickColor
{
    GRAY,
    GREEN,
    YELLOW,
    ORANGE,
    RED,
    VIOLET
}

public enum NumCollisions
{
    ONE = 1,
    TWO = 2,
    THREE = 3
}

public class Brick
{

    private Vector2 _position;

    private Sprite _brickSprite;

    private BrickColor _color;
    private BrickType _type;

    private int _numLives;

    private static Dictionary<  BrickColor,
                                Dictionary<BrickType,
                                Dictionary<NumCollisions,
                                TextureRegion>>> _brickTextures;

    private Vector2 SCALE = new Vector2(2.0f, 2.0f);

    private int _brickScore = 5;

    public Brick(BrickColor color, BrickType type, NumCollisions lives, Vector2 position)
    {
        _color = color;
        _type = type;
        _numLives = (int)lives;
        _brickSprite = new Sprite(_brickTextures[color][type][lives]);
        _brickSprite.CenterOrigin();
        _brickSprite.Scale = SCALE;
        _position = position;
    }

    public static void LoadContent()
    {
        _brickTextures = [];
        
        for(BrickColor colorIndex = BrickColor.GRAY; colorIndex <= BrickColor.VIOLET; colorIndex++)
        {
            _brickTextures[colorIndex] = [];
            for (BrickType typeIndex = BrickType.BIG; typeIndex <= BrickType.SQUARE; typeIndex++)
            {
                _brickTextures[colorIndex][typeIndex] = [];
                //TODO?
            }

        }

        TextureAtlas bricksAtlas = TextureAtlas.FromFile(Core.Content, "images/bricks/BricksAtlas.xml");

        Dictionary<string, BrickColor> stringToBrickColor = new Dictionary<string, BrickColor>
        {
            {"Gray", BrickColor.GRAY },
            {"Green", BrickColor.GREEN},
            {"Yellow", BrickColor.YELLOW},
            {"Orange", BrickColor.ORANGE},
            {"Red", BrickColor.RED},
            {"Violet", BrickColor.VIOLET}
        };

        Dictionary<string, BrickType> stringToBrickType = new Dictionary<string, BrickType>
        {
          {"small", BrickType.SMALL},
          {"big", BrickType.BIG},
          {"square", BrickType.SQUARE}  
        };

        foreach (var (typeStr, typeEnum) in stringToBrickType)
        {
            int maxLives = 3;

            if(typeEnum == BrickType.SQUARE)
            {
                maxLives = 2;
            }

            for (int lives = 1; lives <= maxLives; lives++)
            {
                foreach (var (colorStr, colorEnum) in stringToBrickColor)
                {
                    String textureText = typeStr + colorStr + lives.ToString();

                    _brickTextures[colorEnum][typeEnum][(NumCollisions)lives] = bricksAtlas.GetRegion(textureText);
                }
            }
        }
    }

    public void Draw()
    {
        _brickSprite.Draw(Core.SpriteBatch, _position);
    }

    public bool IsToRemove()
    {
        return (_numLives == 0);
    }

    public int IsHit()
    {
        int returnScore = _brickScore;
        _numLives--;
        if(_numLives != 0)
        {
            _brickSprite.Region = _brickTextures[_color][_type][(NumCollisions)_numLives];
        }
        else
        {
            _brickScore *= 2;
        }
        return returnScore;
    }

    public Rectangle GetBounds()
    {
        // Creating a bounding rectangle for the paddle
        Rectangle bounds = new Rectangle(
            (int)(_position.X - _brickSprite.Width*0.5f),
            (int)(_position.Y - _brickSprite.Height*0.5f),
            (int)_brickSprite.Width,
            (int)_brickSprite.Height
        );

        return bounds;
    }

    public Vector2 GetPosition()
    {
        return _position;
    }

}