using Microsoft.Xna.Framework;
using System.Collections.Generic;
using IbexGame.GameObjects;

namespace IbexGame.Config;

public class LevelConfig
{
    public List<List<BrickConfig>> Bricks{get; set;}

    //public List<string> backgroundStr;

    //public List<EnemyConfig> Enemies{get; set;}
    // later: tilemap, time limit, etc.

    public static readonly int STARTING_LEVEL = 0;

}

public class BrickConfig
{
    public Vector2 Position {get; set;}

    public BrickType Type {get; set;}

    public BrickColor Color{get; set;}
}
