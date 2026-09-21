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
    private static KeyboardState previousKeyboardState;
    private static GamePadState previousGamePadState;
    private const float THUMBSTICK_DEADZONE = 0.2f;

    public static MovingDir IsMoving()
    {
        KeyboardState currentKeyboardState = Keyboard.GetState();
        GamePadState currentGamepadState = GamePad.GetState(PlayerIndex.One);

        bool IsMovingLeft = currentKeyboardState.IsKeyDown(Keys.A) ||
                             currentKeyboardState.IsKeyDown(Keys.Left) ||
                             currentGamepadState.DPad.Left == ButtonState.Pressed ||
                             currentGamepadState.ThumbSticks.Left.X < -THUMBSTICK_DEADZONE;

        if (IsMovingLeft)
            return MovingDir.LEFT;

        bool IsMovingRight = currentKeyboardState.IsKeyDown(Keys.D) ||
                              currentKeyboardState.IsKeyDown(Keys.Right) ||
                              currentGamepadState.DPad.Right == ButtonState.Pressed ||
                              currentGamepadState.ThumbSticks.Left.X > THUMBSTICK_DEADZONE;

        if (IsMovingRight)
            return MovingDir.RIGHT;

        return MovingDir.IDLE;
    }

    public static void Update(GameTime gameTime)
    {
        KeyboardState currentKeyboardState = Keyboard.GetState();
        GamePadState currentGamepadState = GamePad.GetState(PlayerIndex.One);

        // Update keyboard and gamepad state
        previousKeyboardState = currentKeyboardState;
        previousGamePadState = currentGamepadState;
    }

    public static bool IsShootingPressed()
    {
        KeyboardState currentKeyboardState = Keyboard.GetState();
        GamePadState currentGamepadState = GamePad.GetState(PlayerIndex.One);

        bool isButtonPressedKeyBoard = currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space);
        bool isButtonPressedGamePad = currentGamepadState.IsButtonDown(Buttons.A) && previousGamePadState.IsButtonUp(Buttons.A);
        bool isButtonPressed = isButtonPressedKeyBoard || isButtonPressedGamePad;

        return isButtonPressed;
    }
}