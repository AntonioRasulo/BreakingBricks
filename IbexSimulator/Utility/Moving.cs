using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace IbexGame.Utility;

public enum MovingDir
{
    LEFT = -1,
    IDLE = 0,
    RIGHT = 1
}

public class Moving
{
    private static KeyboardState currentKeyboardState;
    private static GamePadState currentGamePadState;
    private static KeyboardState previousKeyboardState;
    private static GamePadState previousGamePadState;
    private const float THUMBSTICK_DEADZONE = 0.2f;

    public static MovingDir IsMoving()
    {

        bool IsMovingLeft = currentKeyboardState.IsKeyDown(Keys.A) ||
                             currentKeyboardState.IsKeyDown(Keys.Left) ||
                             currentGamePadState.DPad.Left == ButtonState.Pressed ||
                             currentGamePadState.ThumbSticks.Left.X < -THUMBSTICK_DEADZONE;

        if (IsMovingLeft)
            return MovingDir.LEFT;

        bool IsMovingRight = currentKeyboardState.IsKeyDown(Keys.D) ||
                              currentKeyboardState.IsKeyDown(Keys.Right) ||
                              currentGamePadState.DPad.Right == ButtonState.Pressed ||
                              currentGamePadState.ThumbSticks.Left.X > THUMBSTICK_DEADZONE;

        if (IsMovingRight)
            return MovingDir.RIGHT;

        return MovingDir.IDLE;
    }

    public static void readInput()
    {
        currentKeyboardState = Keyboard.GetState();
        currentGamePadState = GamePad.GetState(PlayerIndex.One);
    }

    public static void updatePrevInputState()
    {
        // Update keyboard and gamepad state
        previousKeyboardState = currentKeyboardState;
        previousGamePadState = currentGamePadState;
    }

    public static bool IsShootingPressed()
    {

        bool isButtonPressedKeyBoard = currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space);
        bool isButtonPressedGamePad = currentGamePadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A);
        bool isButtonPressed = isButtonPressedKeyBoard || isButtonPressedGamePad;

        return isButtonPressed;
    }
}