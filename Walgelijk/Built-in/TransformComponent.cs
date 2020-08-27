using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Basic component that holds transformation data
    /// </summary>
    public class TransformComponent
    {
        private Vector2 position;
        private float rotation;
        private Vector2 scale = Vector2.One;

        /// <summary>
        /// Position of the transform in world space
        /// </summary>
        public Vector2 Position
        {
            get => position;

            set
            {
                position = value;
                IsMatrixCached = false;
            }
        }

        /// <summary>
        /// Rotation in degrees of the transform in world space
        /// </summary>
        public float Rotation
        {
            get => rotation;

            set
            {
                rotation = value;
                IsMatrixCached = false;
            }
        }
        
        /// <summary>
        /// Scale multiplier of the transform
        /// </summary>
        public Vector2 Scale
        {
            get => scale;

            set
            {
                scale = value;
                IsMatrixCached = false;
            }
        }

        /// <summary>
        /// The generated model matrix
        /// </summary>
        public Matrix4x4 LocalToWorldMatrix { get; private set; }

        /// <summary>
        /// The inverse model matrix
        /// </summary>
        public Matrix4x4 WorldToLocalMatrix { get; private set; }

        /// <summary>
        /// Returns if the model matrix is aligned to transformation
        /// </summary>
        public bool IsMatrixCached { get; private set; }

        /// <summary>
        /// Recalculate the model matrix
        /// </summary>
        public void RecalculateModelMatrix()
        {
            var matrix = Matrix4x4.CreateRotationZ(rotation * Utilities.DegToRad);
            matrix *= Matrix4x4.CreateScale(scale.X, scale.Y, 1);
            matrix *= Matrix4x4.CreateTranslation(position.X, position.Y, 0);

            LocalToWorldMatrix = matrix;

            if (Matrix4x4.Invert(LocalToWorldMatrix, out var result))
                WorldToLocalMatrix = result;
            else
                WorldToLocalMatrix = LocalToWorldMatrix * -1;

            IsMatrixCached = true;
        }
    }
}
