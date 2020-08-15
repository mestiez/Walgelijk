using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Basic component that holds transformation data
    /// </summary>
    public class TransformComponent
    {
        /// <summary>
        /// Position of the entity in world space
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Rotation in degrees of the entity in world space
        /// </summary>
        public float Rotation { get; set; }
    }
}
