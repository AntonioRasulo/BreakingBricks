using Microsoft.Xna.Framework;
using System.Collections.Generic;
using IbexGame.GameObjects;
using MonoGameLibrary;
using System;

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
                generateRow(screenWidth * 0.25f, screenWidth * 0.05f, 11, screenHeight * 0.2f, BrickType.BIG, BrickColor.GREEN),
                generateRow(screenWidth * 0.25f, screenWidth * 0.05f, 11, screenHeight * 0.25f, BrickType.BIG, BrickColor.RED),
            },
        }
    };
}