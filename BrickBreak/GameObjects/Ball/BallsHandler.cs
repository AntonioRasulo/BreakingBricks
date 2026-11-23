using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BrickBreak.GameObjects;

public class BallsHandler
{
    private static List<Ball> _balls;

    public static void LoadContent()
    {
        _balls = new List<Ball>();
        _balls.Add(new Ball(Paddle.getPosition(), Paddle.getPaddleHeight(), Paddle.getDirection(), AttachedStatus.ATTACHED));
    }

    public static void Update(GameTime gameTime)
    {
        bool areAllBallsFree = true;
        foreach(Ball ball in _balls)
        {
            ball.Update(gameTime, Paddle.getVelocity());
            if(ball.getAttachedStatus() == AttachedStatus.ATTACHED)
            {
                areAllBallsFree = false;
            }
        }
        if(areAllBallsFree)
        {
            Ball.ResetDetach();
        }
    }

    public static void RemoveBalls()
    {
        _balls.RemoveAll(ball => ball.toRemove);
        if (_balls.Count == 0)
        {
            int paddleLives = Paddle.getLives();
            paddleLives--;
            Paddle.setLives(paddleLives);
        }
    }

    public static int GetBallsCount()
    {
        return _balls.Count;
    }

    public static void AddBall(Ball ball)
    {
        _balls.Add(ball);
    }

    public static void Draw()
    {
        foreach(Ball ball in _balls)
        {
            ball.Draw();
        }
    }

    public static List<Ball> GetBalls()
    {
        return _balls;
    }

    public static void SplitBalls()
    {
        List<Ball> newBalls = [];
        
        foreach(Ball ball in _balls)
        {
            if(ball.getAttachedStatus() == AttachedStatus.FREE)
            {
                Vector2 ballDirection = ball.getDirection();
                Ball newBall = new Ball(ball.getPosition(), -ballDirection.X, ballDirection.Y);
                newBalls.Add(newBall);
            }
        }

        _balls.AddRange(newBalls);

    }

    public static void MakeBallsFire()
    {
        // TODO
    }

}