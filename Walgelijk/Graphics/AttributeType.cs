namespace Walgelijk
{
    /// <summary>
    /// Possible vertex attribute types
    /// </summary>
    public enum AttributeType
    {
        /// <summary>
        /// Integer value
        /// </summary>
        Integer,
        /// <summary>
        /// Single precision value
        /// </summary>
        Float,
        /// <summary>
        /// Double precision value
        /// </summary>
        Double,
        /// <summary>
        /// Two single precision values
        /// </summary>
        Vector2,
        /// <summary>
        /// Three single precision values
        /// </summary>
        Vector3,
        /// <summary>
        /// Four single precision values
        /// </summary>
        Vector4,
        /// <summary>
        /// Four rows of Vector4
        /// </summary>
        Matrix4x4,
    }
}
