using System;
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

            var old = graphics.CurrentTarget;
            graphics.CurrentTarget = dst;
            graphics.Draw(PrimitiveMeshes.Quad, material);
            graphics.CurrentTarget = old;

            dst.ViewMatrix = view;
            dst.ProjectionMatrix = proj;
        }

        /// <summary>
        /// Just draw a screen space quad
        /// </summary>
        public static void DrawQuadScreenspace(this IGraphics graphics, Rect rect, Material mat)
        {
            graphics.CurrentTarget.ModelMatrix =
                new Matrix4x4(
                    Matrix3x2.CreateScale(rect.GetSize()) *
                    Matrix3x2.CreateTranslation(rect.BottomLeft)
                );
            graphics.Draw(PrimitiveMeshes.Quad, mat);
        }

        /// <summary>
        /// Just draw a quad
        /// </summary>
        public static void DrawQuad(this IGraphics graphics, Rect rect, Material mat)
        {
            graphics.CurrentTarget.ModelMatrix =
                new Matrix4x4(
                    Matrix3x2.CreateScale(rect.Width, -rect.Height) *
                    Matrix3x2.CreateTranslation(rect.TopLeft)
                );
            graphics.Draw(PrimitiveMeshes.Quad, mat);
        }

        /// <summary>
        /// Just draw screen space text
        /// </summary>
        public static TextMeshResult DrawTextScreenspace(this IGraphics graphics, ReadOnlySpan<char> text, Vector2 point, TextMeshGenerator gen, VertexBuffer mesh, Material mat)
        {
            if (text.IsEmpty || text.IsWhiteSpace())
                return default;

            graphics.CurrentTarget.ModelMatrix =
                new Matrix4x4(
                    Matrix3x2.CreateScale(1, -1) *
                    Matrix3x2.CreateTranslation(point)
                );

            var result = gen.Generate(text, mesh.Vertices, mesh.Indices);
            mesh.AmountOfIndicesToRender = result.IndexCount;
            mesh.ForceUpdate();

            graphics.Draw(mesh, mat);

            return result;
        }

        /// <summary>
        /// Perform some actions on a render target without having to set <see cref="IGraphics.CurrentTarget"/> manually
        /// </summary>
        public static void ActOnTarget(this IGraphics graphics, RenderTarget renderTarget, Action<IGraphics> action)
        {
            var p = graphics.CurrentTarget;
            graphics.CurrentTarget = renderTarget;
            action(graphics);
            graphics.CurrentTarget = p;
        }
    }
}
