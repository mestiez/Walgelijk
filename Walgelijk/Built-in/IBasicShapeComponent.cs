namespace Walgelijk
{
    /// <summary>
    /// Interface that provides a <see cref="VertexBuffer"/> and <see cref="ShapeRenderTask"/>
    /// </summary>
    public interface IBasicShapeComponent
    {
        /// <summary>
        /// VertexBuffer that is generated. It's best not to edit this unless you really need to.
        /// </summary>
        public VertexBuffer VertexBuffer { get; }
        /// <summary>
        /// The render task that is generated. It's best not to edit this unless you really need to.
        /// </summary>
        public ShapeRenderTask RenderTask { get; }
    }
}
