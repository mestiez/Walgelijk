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
        private Vector3 pivot = Vector3.Zero;
        private Entity? parent;

        /// <summary>
        /// Parent entity with a transform
        /// </summary>
        public Entity? Parent
        {
            get => parent;
            set
            {
                parent = value;
                IsMatrixCached = false;
            }
        }

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
        /// The local pivot point that is the center of all transformations.
        /// </summary>
        public Vector2 LocalPivot
        {
            get => new Vector2(pivot.X, pivot.Y);

            set
            {
                pivot = new Vector3(value.X, value.Y, 0);
                IsMatrixCached = false;
            }
        }

        /// <summary>
        /// The generated model matrix
        /// </summary>
        public Matrix4x4 LocalToWorldMatrix;

        /// <summary>
        /// The inverse model matrix
        /// </summary>
        public Matrix4x4 WorldToLocalMatrix;

        /// <summary>
        /// All transform matrices separated by step. Useful for transforming specific vectors to/from local space
        /// </summary>
        public SeparateTransformMatrices SeparateMatrices;

        /// <summary>
        /// Returns if the model matrix is aligned to the transformation
        /// </summary>
        public bool IsMatrixCached;

        /// <summary>
        /// Recalculate the model matrix considering a containing matrix. This is usually <see cref="Matrix4x4.Identity"/>
        /// </summary>
        public void RecalculateModelMatrix(Matrix4x4 containingMatrix)
        {
            var matrix = Matrix4x4.CreateTranslation(-pivot);
            SeparateMatrices.AfterPivot = matrix;

            matrix *= Matrix4x4.CreateRotationZ(rotation * Utilities.DegToRad);
            SeparateMatrices.AfterRotation = matrix;

            matrix *= Matrix4x4.CreateScale(scale.X, scale.Y, 1);
            SeparateMatrices.AfterScale = matrix;

            matrix *= Matrix4x4.CreateTranslation(position.X, position.Y, 0);
            SeparateMatrices.AfterTranslation = matrix;

            //matrix *= Matrix4x4.CreateTranslation(-pivot);

            if (!containingMatrix.IsIdentity)
                matrix *= containingMatrix;

            SeparateMatrices.FinalModel = matrix;

            LocalToWorldMatrix = matrix;

            if (Matrix4x4.Invert(LocalToWorldMatrix, out var result))
                WorldToLocalMatrix = result;
            else
                WorldToLocalMatrix = LocalToWorldMatrix * -1;

            IsMatrixCached = true;
        }
    }
}
