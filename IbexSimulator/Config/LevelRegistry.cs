using Microsoft.Xna.Framework;
using System.Collections.Generic;
using IbexGame.GameObjects;
using MonoGameLibrary;

namespace IbexGame.Config;

public static class LevelRegistry
{
    private static readonly float screenWidth = Core.GraphicsDevice.PresentationParameters.Bounds.Width;
    private static readonly float screenHeight = Core.GraphicsDevice.PresentationParameters.Bounds.Height;

    private static List<BrickConfig> generateRow(float firstRowPos, float rowDist, int numRows, float height, BrickType type, BrickColor color)
    {
        List<BrickConfig> returnList = new List<BrickConfig>();

        for (int rowNum = 0; rowNum < numRows; rowNum++)
        {
            float xPos = firstRowPos + rowDist * rowNum;
            returnList.Add(new BrickConfig
            {
                Position = new Vector2(xPos, height),
                Type = type,
                Color = color
            });
        }

        return returnList;
    }

    public static List<LevelConfig> AllLevels = new List<LevelConfig>
    {
        new LevelConfig // Level 0
        {
            Bricks = new List<List<BrickConfig>>
            {
                generateRow(screenWidth * 0.3f, screenWidth * 0.05f, 9, screenHeight * 0.2f, BrickType.BIG, BrickColor.GREEN),
                generateRow(screenWidth * 0.3f, screenWidth * 0.05f, 9, screenHeight * 0.25f, BrickType.BIG, BrickColor.RED),
                generateRow(screenWidth * 0.3f, screenWidth * 0.05f, 9, screenHeight * 0.3f, BrickType.BIG, BrickColor.GRAY),
                generateRow(screenWidth * 0.3f, screenWidth * 0.05f, 9, screenHeight * 0.35f, BrickType.BIG, BrickColor.ORANGE),
                generateRow(screenWidth * 0.3f, screenWidth * 0.05f, 9, screenHeight * 0.4f, BrickType.BIG, BrickColor.VIOLET),
                generateRow(screenWidth * 0.3f, screenWidth * 0.05f, 9, screenHeight * 0.45f, BrickType.BIG, BrickColor.YELLOW)
            },
        }
    };
}