namespace SideGamePrototype
{
    internal static class GameState
    {
        public static IEntityCollection Entities { get; set; }
        public static ICollision Collision { get; set; }
        public static Camera2D Camera { get; set; }
        public static GameLogic Logic { get; set; }

        public static bool DEBUG { get; set; } = false;
        public static bool PAUSE { get; set; } = false;

        public static void Update(float dt)
        {
            if (PAUSE)
                return;

            GameState.Entities.Update(dt);
            //FollowCamera.UpdateCamera(dt);
            ZoomingCamera.UpdateCamera(dt);
            GameState.Logic.Update(dt);
        }
    }
}