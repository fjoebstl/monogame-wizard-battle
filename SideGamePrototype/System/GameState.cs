using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideGamePrototype
{
    internal static class GameState
    {
        public static IEntityCollection Entities { get; set; }
        public static ICollision Collision { get; set; }
        public static Camera2D Camera { get; set; }

        public static bool DEBUG { get; set; } = false;
    }
}