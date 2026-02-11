using cfg;

using Vector2Int =  UnityEngine.Vector2Int;

namespace Luban
{
    public static class ExternalTypeUtil
    {
       

        public static (float, float) NewFloat2(Float2 deserializeFloat2)
        {
            return (deserializeFloat2.Item1, deserializeFloat2.Item2);
        }

        public static (int, int) NewInt2(Int2 deserializeInt2)
        {
            return (deserializeInt2.Item1, deserializeInt2.Item2);
        }


        public static Vector2Int NewVector2Int(cfg.Vector2Int vector2)
        {
            return new Vector2Int(vector2.X, vector2.Y);
        }
    }
}