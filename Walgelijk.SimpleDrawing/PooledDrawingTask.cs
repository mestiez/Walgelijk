using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.SimpleDrawing
{
    /// <summary>
    /// A drawing task that is meant to be pooled as to avoid constant allocation
    /// </summary>
    public class PooledDrawingTask : IRenderTask
    {
        private readonly Dictionary<string, int> textFrequencyCounter = new();

        /// <summary>
        /// Reference to the drawing
        /// </summary>
        public Drawing Drawing;

        public PooledDrawingTask(Drawing drawing)
        {
            Drawing = drawing;
        }

        public void Execute(IGraphics graphics)
        {
            Material material = Drawing.Material;

            if (Drawing.VertexBuffer != null)
            {
                if (Drawing.TextDrawing.HasValue)//yo ik heb text om te genereren
                {
                    var text = Drawing.TextDrawing.Value.Text;
                    var textDrawing = Drawing.TextDrawing.Value;

                    if (string.IsNullOrWhiteSpace(text))
                        return;

                    if (string.IsNullOrWhiteSpace(text))
                        Drawing.VertexBuffer.AmountOfIndicesToRender = 0;
                    else
                    {
                        if (textFrequencyCounter.TryGetValue(text, out int count))
                            textFrequencyCounter[text] = count + 1;
                        else
                        {
                            textFrequencyCounter.Add(text, 1);
                            count = 1;
                        }

                        var cachable = new CachableTextDrawing
                        {
                            Color = Drawing.Color,
                            Font = textDrawing.Font ?? Font.Default,
                            HorizontalAlign = textDrawing.HorizontalAlign,
                            VerticalAlign = textDrawing.VerticalAlign,
                            Text = text,
                            TextBoxWidth = textDrawing.TextBoxWidth
                        };

                        if (count > 240)
                            Drawing.VertexBuffer = Draw.TextMeshCache.Load(cachable); // this text is common! use cache
                        else
                            TextMeshCache.Prepare(cachable, Drawing.VertexBuffer, true); // this text has only appeared infrequently.
                    }
                }
                else
                {
                    material.SetUniform(DrawingMaterialCreator.MainTexUniform, Drawing.Texture ?? Texture.White);
                    material.SetUniform(DrawingMaterialCreator.ScaleUniform, new Vector2(MathF.Abs(Drawing.Scale.X), MathF.Abs(Drawing.Scale.Y)));
                    material.SetUniform(DrawingMaterialCreator.RoundednessUniform, Drawing.Roundness);
                    material.SetUniform(DrawingMaterialCreator.OutlineWidthUniform, Drawing.OutlineWidth);
                    material.SetUniform(DrawingMaterialCreator.OutlineColourUniform, Drawing.OutlineColour);
                    material.SetUniform(DrawingMaterialCreator.TintUniform, Drawing.Color);
                }

                if (Drawing.ScreenSpace)
                {
                    var target = graphics.CurrentTarget;

                    var view = target.ViewMatrix;
                    var proj = target.ProjectionMatrix;
                    target.ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, target.Size.X, target.Size.Y, 0, -10, 10);
                    target.ViewMatrix = Matrix4x4.Identity;
                    Drawing.Scale.Y *= -1;
                    draw(graphics);
                    target.ViewMatrix = view;
                    target.ProjectionMatrix = proj;
                }
                else
                    draw(graphics);

                if (Drawing.ReturnVertexBufferToPool)
                    Draw.PolygonPool.ReturnToPool(Drawing.VertexBuffer);
            }

            Draw.TaskPool.ReturnToPool(this);

            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            void draw(IGraphics graphics)
            {
                graphics.DrawBounds = Drawing.DrawBounds;
                graphics.CurrentTarget.ModelMatrix = new Matrix4x4(CreateMatrix(Drawing.Position, Drawing.Scale, Drawing.RotationRadians) * Drawing.Transformation);
                var oldBm = material.BlendMode;
                if (Drawing.BlendMode.HasValue)
                    material.BlendMode = Drawing.BlendMode.Value;
                graphics.Draw(Drawing.VertexBuffer, material);
                material.BlendMode = oldBm;
                graphics.DrawBounds = DrawBounds.DisabledBounds;
            }
        }

        [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static Matrix3x2 CreateMatrix(Vector2 pos, Vector2 scale, float rotationRadians) =>
            Matrix3x2.CreateScale(scale.X, scale.Y) *
            Matrix3x2.CreateRotation(-rotationRadians) *
            Matrix3x2.CreateTranslation(pos.X, pos.Y);
    }
}