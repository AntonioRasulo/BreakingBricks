using System;
using System.Collections.Generic;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using BrickBreak.Utility;

namespace BrickBreak.GameObjects;

public class CollectibleHandler
{
    private static List<Collectible> _collectibles;

    private static Random _collectibleRand;

    public static readonly Vector2 SCALE = new(4.0f, 4.0f);
    private readonly Vector2 INV_SCALE = new(0.2f, 0.2f);

    public static Animation _speedUpAnimation;
    public static Animation _speedDownAnimation;
    public static Texture2D _stickySprite;
    public static Texture2D _enlargeSprite;
    public static Texture2D _shrinkSprite;

    private static SoundEffect _collectSound;

    private const int SPEED_UP_PROB = 33;
    private const int SPEED_DOWN_PROB = 66;
    private const int STICKY_PROB = 100;

    private static collectibleType[] _marbleBagContent;
    private static MarbleBag<collectibleType> _marbleBag;

    public CollectibleHandler()
    {
        LoadContent();
    }

    public static void LoadContent()
    {
        _collectibles = new List<Collectible>();

        _collectibleRand = new Random();

        TextureAtlas speedUpAtlas = TextureAtlas.FromFile(Core.Content, "images/PowerUp/SpeedUp/SpeedUpAtlas.xml");
        TextureAtlas speedDownAtlas = TextureAtlas.FromFile(Core.Content, "images/PowerUp/SpeedDown/SpeedDownAtlas.xml");

        _speedUpAnimation = speedUpAtlas.GetAnimation("speedup-bonus-animation");
        _speedDownAnimation = speedDownAtlas.GetAnimation("speeddown-bonus-animation");
        _stickySprite = Core.Content.Load<Texture2D>("images/PowerUp/Sticky");
        _enlargeSprite = Core.Content.Load<Texture2D>("images/PowerUp/Blue two sides arrow Black outline");
        _shrinkSprite = Core.Content.Load<Texture2D>("images/PowerUp/BlueTwoSidedArrowInverted");

        _marbleBagContent =
        [
            collectibleType.SPEED_UP,
            collectibleType.SPEED_DOWN,
            collectibleType.STICKY,
            collectibleType.ENLARGE,
            collectibleType.SHRINK,
            collectibleType.NONE,
            collectibleType.NONE,
            collectibleType.NONE,
            collectibleType.NONE,
            collectibleType.SPEED_UP,
            collectibleType.SPEED_DOWN,
            collectibleType.STICKY,
            collectibleType.ENLARGE,
            collectibleType.SHRINK,
            collectibleType.NONE,
            collectibleType.NONE,
            collectibleType.NONE,
            collectibleType.NONE,
            collectibleType.SPEED_UP,
            collectibleType.SPEED_DOWN,
            collectibleType.STICKY,
            collectibleType.ENLARGE,
            collectibleType.SHRINK,
            collectibleType.NONE,
            collectibleType.NONE,
            collectibleType.NONE,
            collectibleType.NONE
        ];
        _marbleBag = new MarbleBag<collectibleType>(_marbleBagContent);

    }

    public static void Update(GameTime gameTime)
    {
        List<Collectible> toRemove = new List<Collectible>();
        foreach(Collectible collectible in _collectibles)
        {
            collectible.Update(gameTime);
            if(collectible.getBounds().Top > Core.GraphicsDevice.PresentationParameters.BackBufferHeight)
            {
                toRemove.Add(collectible);
            }
        }
        _collectibles.RemoveAll(toRemove.Contains);
    }

    public static void Draw()
    {
        foreach(Collectible collectible in _collectibles)
        {
            collectible.Draw();
        }
    }

    static public void GenerateCollectible(Vector2 position)
    {
        collectibleType generatedType = _marbleBag.Next;
        switch (generatedType)
        {
            case collectibleType.SPEED_UP:
                _collectibles.Add(new PowerUpAnimation(position, collectibleType.SPEED_UP, 2.0f, (float)(Math.PI * 0.5f)));
            break;
            case collectibleType.SPEED_DOWN:
                _collectibles.Add(new PowerUpAnimation(position, collectibleType.SPEED_DOWN, 2.0f));
            break;
            case collectibleType.STICKY:
                _collectibles.Add(new PowerUpSprite(position, collectibleType.STICKY, 0.3f, 0.3f));
            break;
            case collectibleType.ENLARGE:
                _collectibles.Add(new PowerUpSprite(position, collectibleType.ENLARGE, 0.2f, 0.35f));
            break;
            case collectibleType.SHRINK:
                _collectibles.Add(new PowerUpSprite(position, collectibleType.SHRINK, 0.2f, 0.35f));
            break;
        }
    }

    public static collectibleType CheckPaddleCollision(Rectangle charBounds)
    {
        foreach(Collectible collectible in _collectibles)
        {
            Rectangle collectibleBounds = collectible.getBounds();

            if (collectibleBounds.Intersects(charBounds))
            {
                Core.Input.GamePads[(int)PlayerIndex.One].SetVibration(0.1f, TimeSpan.FromMilliseconds(100));
                //Core.Audio.PlaySoundEffect(_collectSound);
                _collectibles.Remove(collectible);
                return collectible.GetCollectibleType();
            }

        }

        return collectibleType.NONE;

    }

    public static void PlayCollectibleSound()
    {
        //Core.Audio.PlaySoundEffect(_collectSound);
    }

    public static void ResetMarbleBag()
    {
        _marbleBag.Reset(_marbleBagContent);
    }

}