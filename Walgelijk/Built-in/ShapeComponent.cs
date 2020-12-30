namespace Walgelijk
{
    /// <summary>
    /// Interface that provides a <see cref="VertexBuffer"/> and <see cref="ShapeRenderTask"/>
    /// </summary>
    [RequiresComponents(typeof(TransformComponent))]
    public abstract class ShapeComponent
    {
        /// <summary>
        /// VertexBuffer that is generated. It's best not to edit this unless you really need to.
        /// </summary>
        public VertexBuffer VertexBuffer { get; set; }
        /// <summary>
        /// The render task that is generated. It's best not to edit this unless you really need to.
        /// </summary>
        public ShapeRenderTask RenderTask { get; set; }
        /// <summary>
        /// Determines if the shape should be rendered in screenspace
        /// </summary>
        public bool ScreenSpace { get; set; }
        /// <summary>
        /// Order of the rendering task
        /// </summary>
        public RenderOrder RenderOrder { get; set; }
        /// <summary>
        /// Whether or not the component is rendered by the system
        /// </summary>
        public bool Visible { get; set; } = true;
    }
}
