using Microsoft.Xna.Framework;
using Resources;

namespace SideGamePrototype
{
    public class GameLogic
    {
        public int ScorePlayer1 = 0;
        public int ScorePlayer2 = 0;

        internal Wizard Player1;
        internal Wizard Player2;

        private Vector2 Player1SpawnPosition;
        private Vector2 Player2SpawnPosition;

        public void Init()
        {
            this.Player1SpawnPosition = new Vector2(100, 500);
            this.Player2SpawnPosition = new Vector2(R.System.Window.Right - 100, 500);

            this.RespawnPlayer1();
            this.RespawnPlayer2();
        }

        private void RespawnPlayer1()
        {
            this.Player1 = new Wizard(
                pos: this.Player1SpawnPosition,
                input: new KeyboardLayout1InputHandler());
            GameState.Entities.Add(this.Player1);
        }

        private void RespawnPlayer2()
        {
            this.Player2 = new Wizard(
                pos: this.Player2SpawnPosition,
                input: new KeyboardLayout2InputHandler());
            GameState.Entities.Add(this.Player2);
        }

        public void Update(float gt)
        {
            if (this.Player1.Dead)
            {
                this.ScorePlayer2++;
                this.RespawnPlayer1();
            }

            if (this.Player2.Dead)
            {
                this.ScorePlayer1++;
                this.RespawnPlayer2();
            }
        }
    }
}