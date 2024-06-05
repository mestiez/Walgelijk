namespace Walgelijk
{
    /// <summary>
    /// Provides a <see cref="VertexBuffer"/> and <see cref="ShapeRenderTask"/>
    /// </summary>
    [RequiresComponents(typeof(TransformComponent))]
    public abstract class ShapeComponent : Component
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

        /// <summary>
        /// Sets the "tint" uniform before rendering. If null, does nothing.
        /// </summary>
        public Color? Color { get; set; }

        /// <summary>
        /// Apply horizontal flip
        /// </summary>
        public bool HorizontalFlip { get; set; } = false;
        /// <summary>
        /// Apply vertical flip
        /// </summary>
        public bool VerticalFlip { get; set; } = false;
    }
}
