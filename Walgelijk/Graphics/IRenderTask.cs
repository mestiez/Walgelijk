using System;
using System.Collections.Generic;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// A task that can be queued in the render queue
    /// </summary>
    public interface IRenderTask
    {
        /// <summary>
        /// Execute the render task on the given render target
        /// </summary>
        /// <param name="target"></param>
        void Execute(RenderTarget target);
    }

    /// <summary>
    /// Render task that renders a vertex buffer with a material
    /// </summary>
    public struct ShapeRenderTask : IRenderTask
    {
        public ShapeRenderTask(VertexBuffer vertexBuffer, Material material = null)
        {
            VertexBuffer = vertexBuffer;
            Material = material;
        }

        /// <summary>
        /// Vertex buffer to draw
        /// </summary>
        public VertexBuffer VertexBuffer { get; set; }
        /// <summary>
        /// Material to draw with
        /// </summary>
        public Material Material { get; set; }

        public void Execute(RenderTarget target)
        {
            target.Draw(VertexBuffer, Material);
        }
    }

    /// <summary>
    /// Render task that renders a collection of vertices immediately
    /// </summary>
    public struct ImmediateRenderTask : IRenderTask
    {
        public ImmediateRenderTask(Vertex[] vertices, Material material = null)
        {
            Vertices = vertices;
            Material = material;
        }

        /// <summary>
        /// Vertices to draw
        /// </summary>
        public Vertex[] Vertices { get; set; }
        /// <summary>
        /// Material to draw with
        /// </summary>
        public Material Material { get; set; }

        public void Execute(RenderTarget target)
        {
            target.Draw(Vertices, Material);
        }
    }

    /// <summary>
    /// Render task that will invoke the action you give. Useful for unique smaller actions
    /// </summary>
    public struct ActionRenderTask : IRenderTask
    {
        public ActionRenderTask(Action<RenderTarget> action)
        {
            Action = action;
        }

        /// <summary>
        /// The action to invoke
        /// </summary>
        public Action<RenderTarget> Action { get; set; }

        public void Execute(RenderTarget renderTarget)
        {
            Action?.Invoke(renderTarget);
        }
    }
}
