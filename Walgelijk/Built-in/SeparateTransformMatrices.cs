using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// All transform matrices separated by step
    /// </summary>
    public struct SeparateTransformMatrices
    {
        /// <summary>
        /// 0: The initial model matrix. A translation matrix for the transform pivot.
        /// </summary>
        public Matrix4x4 AfterPivot;

        /// <summary>
        /// 1: The matrix after scaling
        /// </summary>
        public Matrix4x4 AfterScale;

        /// <summary>
        /// 2: The matrix after rotating
        /// </summary>
        public Matrix4x4 AfterRotation;

        /// <summary>
        /// 3: The matrix after translating
        /// </summary>
        public Matrix4x4 AfterTranslation;

        /// <summary>
        /// 4: The final model matrix
        /// </summary>
        public Matrix4x4 FinalModel;
    }
}
