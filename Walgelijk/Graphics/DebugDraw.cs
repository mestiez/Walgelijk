using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Debug drawing utility class. Usually accessed using <see cref="System.DebugDraw"/> or <see cref="Game.DebugDraw"/>
    /// </summary>
    public sealed class DebugDraw
    {
        private Game game;
        private readonly Material debugMaterial;
        private readonly VertexBuffer circle = PrimitiveMeshes.GenerateCircle(12, 1);

        private HashSet<Drawing> drawings = new();

        /// <summary>
        /// Construct debug draw
        /// </summary>
        /// <param name="game"></param>
        public DebugDraw(Game game)
        {
            this.game = game;

            circle.PrimitiveType = Primitive.LineLoop;

            debugMaterial = new Material(new Shader(
                @"#version 460

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec4 color;

out vec2 uv;
out vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = texcoord;
   vertexColor = color;
   gl_Position = projection * view * model * vec4(position, 1.0);
}",
                @"#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform vec4 tint;

void main()
{
    color = vertexColor * tint;
}"
                ));
        }

        private static Color GetColor(Color? c)
        {
            return c ?? Colors.Yellow;
        }

        private static float GetDuration(float? d)
        {
            return d ?? -1;
        }

        /// <summary>
        /// Update the debug drawer and remove expired drawings
        /// </summary>
        public void Render()
        {
            float t = game.Time.SecondsSinceLoad;
            foreach (var drawing in drawings)
            {
                game.RenderQueue.Add(drawing.Task, drawing.Order);
            }
            drawings.RemoveWhere(d => t >= d.ExpirationTime);
        }

        /// <summary>
        /// Draw a line
        /// </summary>
        public void Line(Vector3 from, Vector3 to, Color? color = null, float? duration = null, int renderOrder = 0)
        {
            var delta = (to - from);
            var distance = delta.Length();
            var normalised = delta / distance;

            var rotation = MathF.Atan2(normalised.Y, normalised.X);
            var model = Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateScale(distance) * Matrix4x4.CreateTranslation(from);

            var task = new DebugDrawTask(PrimitiveMeshes.Line, model, debugMaterial, GetColor(color));
            drawings.Add(new Drawing
            {
                ExpirationTime = game.Time.SecondsSinceLoad + GetDuration(duration),
                Order = renderOrder,
                Task = task
            });
        }

        /// <summary>
        /// Draw a cross
        /// </summary>
        public void Cross(Vector3 center, float size = 0.5f, Color? color = null, float? duration = null, int renderOrder = 0)
        {
            var right = new Vector3(size, 0, 0);
            var up = new Vector3(0, size, 0);

            Line(center - right, center + right, color, duration, renderOrder);
            Line(center - up,    center + up,    color, duration, renderOrder);
        }

        /// <summary>
        /// Draw a wire circle
        /// </summary>
        public void Circle(Vector3 center, float radius = 1f, Color? color = null, float? duration = null, int renderOrder = 0)
        {
            var model = Matrix4x4.CreateScale(radius) * Matrix4x4.CreateTranslation(center);
            var task = new DebugDrawTask(circle, model, debugMaterial, GetColor(color));
            drawings.Add(new Drawing
            {
                ExpirationTime = game.Time.SecondsSinceLoad + GetDuration(duration),
                Order = renderOrder,
                Task = task
            });
        }

        private struct Drawing
        {
            public IRenderTask Task;
            public int Order;
            public float ExpirationTime;
        }

        private class DebugDrawTask : ShapeRenderTask
        {
            public Color Tint;

            public DebugDrawTask(VertexBuffer vertexBuffer, Matrix4x4 modelMatrix, Material material, Color tint) : base(vertexBuffer, modelMatrix, material)
            {
                Tint = tint;
            }

            protected override void Draw(IGraphics graphics)
            {
                Material.SetUniform("tint", Tint);
                base.Draw(graphics);
            }
        }
    }
}
