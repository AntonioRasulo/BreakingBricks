using Microsoft.Xna.Framework;

namespace BrickBreak.GameObjects;

public class FreezeHandler
{
    private static float _freezeDuration = 4.0f;
    public static float freezeTimer = 0f;

    public static void updateFreeze(GameTime gameTime)
    {
        if (freezeTimer > 0f)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            freezeTimer -= delta;

            if (freezeTimer <= 0f)
            {
                freezeTimer = 0f;
            }
        }
    }

    public static void Freeze()
    {
        freezeTimer = _freezeDuration;
    }

    public static void resetFreeze()
    {
        freezeTimer = 0f;
    }
}