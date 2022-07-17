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
        private Vector2 pivot = Vector2.Zero;
        private Vector2 rotationPivot = Vector2.Zero;
        private Entity? parent;

        /// <summary>
        /// Parent entity with a transform
        /// </summary>
        public Entity? Parent
        {
            get => parent;
            set
            {
                if (parent == value) 
                    return;
                parent = value;
                IsMatrixCached = false;
            }
        }

        /// <summary>
        /// Position of the transform in parent space (global if no parent)
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
                pivot = new(value.X, value.Y);
                IsMatrixCached = false;
            }
        }

        /// <summary>
        /// The local pivot point that is the center of rotation.
        /// </summary>
        public Vector2 LocalRotationPivot
        {
            get => new(rotationPivot.X, rotationPivot.Y);

            set
            {
                rotationPivot = new(value.X, value.Y);
                IsMatrixCached = false;
            }
        }

        /// <summary>
        /// The generated model matrix
        /// </summary>
        public Matrix3x2 LocalToWorldMatrix;

        /// <summary>
        /// The inverse model matrix
        /// </summary>
        public Matrix3x2 WorldToLocalMatrix;

        /// <summary>
        /// All transform matrices separated by step. Useful for transforming specific vectors to/from local space
        /// </summary>
        public SeparateTransformMatrices SeparateMatrices;

        /// <summary>
        /// Returns if the model matrix is aligned to the transformation
        /// </summary>
        public bool IsMatrixCached;

        /// <summary>
        /// Recalculate the model matrix considering a containing matrix. This is usually <see cref="Matrix3x2.Identity"/>
        /// </summary>
        public void RecalculateModelMatrix(Matrix3x2 containingMatrix)
        {
            var matrix = Matrix3x2.CreateTranslation(-pivot.X, -pivot.Y);
            SeparateMatrices.AfterPivot = matrix;

            matrix *= Matrix3x2.CreateScale(scale.X, scale.Y);
            SeparateMatrices.AfterScale = matrix;

            matrix *= Matrix3x2.CreateRotation(rotation * Utilities.DegToRad, new Vector2(rotationPivot.X * scale.X, rotationPivot.Y * scale.Y));
            SeparateMatrices.AfterRotation = matrix;

            matrix *= Matrix3x2.CreateTranslation(position.X, position.Y);
            SeparateMatrices.AfterTranslation = matrix;

            //matrix *= Matrix4x4.CreateTranslation(-pivot);

            if (!containingMatrix.IsIdentity)
                matrix *= containingMatrix;

            SeparateMatrices.FinalModel = matrix;

            LocalToWorldMatrix = matrix;

            if (Matrix3x2.Invert(LocalToWorldMatrix, out var result))
                WorldToLocalMatrix = result;
            else
                WorldToLocalMatrix = LocalToWorldMatrix * -1;

            IsMatrixCached = true;
        }
    }
}
