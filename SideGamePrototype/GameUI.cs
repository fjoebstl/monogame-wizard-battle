using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Resources;

namespace SideGamePrototype
{
    public static class GameUI
    {
        public static void Draw(SpriteBatch s)
        {
            s.DrawString(R.Fonts.Default, $"Player 1: {GameState.Logic.ScorePlayer1} kills", new Vector2(32, 16), Color.White);

            var p2 = $"Player 2: {GameState.Logic.ScorePlayer2} kills";
            var size = R.Fonts.Default.MeasureString(p2);

            s.DrawString(R.Fonts.Default, $"Player 2: {GameState.Logic.ScorePlayer2} kills", new Vector2(R.System.Window.Width - size.X - 32, 16), Color.White);
        }
    }
}