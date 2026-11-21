using Microsoft.Xna.Framework;
using MonoGameLibrary;
using System;

namespace BrickBreak.Utility;

public enum PartCollided
{
    NONE,
    SIDEX,
    SIDEY,
    ANGLE
}

public class Functions
{
    public static (bool, PartCollided) AreIntersecting(Circle circle, Rectangle rectangle)
    {
        int distanceX = Math.Abs(circle.X - rectangle.Center.X);
        int distanceY = Math.Abs(circle.Y - rectangle.Center.Y);

        float halfRectWidth = rectangle.Width * 0.5f;
        float halfRectHeight = rectangle.Height * 0.5f;

        if((distanceX > (halfRectWidth + circle.Radius)) ||
            (distanceY > (halfRectHeight + circle.Radius)))
        {
            return (false, PartCollided.NONE);
        }

        if(distanceX <= halfRectWidth) return  (true, PartCollided.SIDEX);

        if(distanceY <= halfRectHeight) return (true, PartCollided.SIDEY);

        double cornerDistanceSquare = Math.Pow(distanceX-halfRectWidth, 2) + Math.Pow(distanceY-halfRectHeight, 2);

        return (cornerDistanceSquare <= Math.Pow(circle.Radius, 2), PartCollided.ANGLE);
    }

}