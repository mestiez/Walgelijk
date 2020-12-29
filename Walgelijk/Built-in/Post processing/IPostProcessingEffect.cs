using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// A post processing effect
    /// </summary>
    public interface IPostProcessingEffect
    {
        /// <summary>
        /// Process the current graphics state
        /// </summary>
        public void Process(RenderTexture src, RenderTexture dst, IGraphics graphics, Scene scene);
    }
}
