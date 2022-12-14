using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Extension methods for all implementations of <see cref="IGraphics"/>
    /// </summary>
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Blit the <paramref name="src"/> content to <paramref name="dst"/> using a fullscreen quad and the given material
        /// </summary>
        public static void BlitFullscreenQuad(this IGraphics graphics, IReadableTexture src, RenderTarget dst, int targetWidth, int targetHeight, Material material, string textureUniform)
        {
            material.SetUniform(textureUniform, src);

            dst.ModelMatrix = Matrix4x4.CreateTranslation(0, -1, 0) * Matrix4x4.CreateScale(targetWidth, -targetHeight, 1);

            var view = dst.ViewMatrix;
            var proj = dst.ProjectionMatrix;
            dst.ProjectionMatrix = dst.OrthographicMatrix;
            dst.ViewMatrix = Matrix4x4.Identity;

            graphics.CurrentTarget = dst;
            graphics.Draw(PrimitiveMeshes.Quad, material);

            dst.ViewMatrix = view;
            dst.ProjectionMatrix = proj;
        }
    }
}
