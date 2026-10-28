using System;
using System.Collections.Generic;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

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

    private static SoundEffect _collectSound;

    private const int SPEED_UP_PROB = 33;
    private const int SPEED_DOWN_PROB = 66;
    private const int STICKY_PROB = 100;

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
        int rand = _collectibleRand.Next(0, 100);
                if (rand < SPEED_UP_PROB)
        {
            _collectibles.Add(new PowerUpAnimation(position, collectibleType.SPEED_UP, 2.0f, (float)(Math.PI * 0.5f)));
        }
        else if (rand < SPEED_DOWN_PROB)
        {
            _collectibles.Add(new PowerUpAnimation(position, collectibleType.SPEED_DOWN, 2.0f));
        }
        else if (rand < STICKY_PROB)
        {
            _collectibles.Add(new PowerUpSprite(position, collectibleType.STICKY, 0.3f));
        }
        // else if (rand < PlayerStatsManager.currentStats.bombProbability && rand > INVINCIBILITY_MAX_PROB)
        // {
        //     _collectibles.Add(new PowerUp(position, collectibleType.BOMB));
        // }
        // else if (rand < GOLD_COIN_PROB && rand > BOMB_MAX_PROB)
        // {
        //     _collectibles.Add(new Coin(position, collectibleType.GOLD_COIN));
        // }
        // else if (rand < SILVER_COIN_PROB && rand > GOLD_COIN_PROB)
        // {
        //     _collectibles.Add(new Coin(position, collectibleType.SILVER_COIN));
        // }
        // else if(rand < BRONZE_COIN_PROB && rand > SILVER_COIN_PROB)
        // {
        //     _collectibles.Add(new Coin(position, collectibleType.BRONZE_COIN));
        // }
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

}