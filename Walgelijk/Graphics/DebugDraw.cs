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
        private readonly Material fontMaterial;
        private readonly VertexBuffer circle = PrimitiveMeshes.GenerateCircle(12, 1);
        private readonly VertexBuffer rect = new VertexBuffer(PrimitiveMeshes.Quad.Vertices, PrimitiveMeshes.Quad.Indices);
        private readonly VertexBuffer text = new VertexBuffer();
        private readonly TextMeshGenerator textGenerator = new TextMeshGenerator();

        private HashSet<Drawing> drawings = new();

        /// <summary>
        /// Construct debug draw
        /// </summary>
        /// <param name="game"></param>
        public DebugDraw(Game game)
        {
            this.game = game;

            circle.PrimitiveType = Primitive.LineLoop;
            rect.PrimitiveType = Primitive.LineLoop;

            textGenerator.Font = Font.Default;
            textGenerator.Color = Colors.White;

            fontMaterial = new Material(Font.Default.Material ?? throw new Exception("Font.Default.Material is null somehow"));
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
            float t = game.State.Time.SecondsSinceLoad;
            foreach (var drawing in drawings)
            {
                game.RenderQueue.Add(drawing.Task, drawing.Order);
            }
            drawings.RemoveWhere(d => t >= d.ExpirationTime);
        }

        /// <summary>
        /// Draw a line
        /// </summary>
        public void Line(Vector2 from, Vector2 to, Color? color = null, float? duration = null, RenderOrder renderOrder = default)
        {
            if (!game.DevelopmentMode)
                return;

            var delta = (to - from);
            var distance = delta.Length();
            var normalised = delta / distance;

            var rotation = MathF.Atan2(normalised.Y, normalised.X);
            var model = Matrix3x2.CreateRotation(rotation) * Matrix3x2.CreateScale(distance) * Matrix3x2.CreateTranslation(from);

            var task = new DebugDrawTask(PrimitiveMeshes.Line, model, debugMaterial, GetColor(color));
            AddDrawing(duration, renderOrder, task);
        }

        /// <summary>
        /// Draw a cross
        /// </summary>
        public void Cross(Vector2 center, float size = 0.5f, Color? color = null, float? duration = null, RenderOrder renderOrder = default)
        {
            if (!game.DevelopmentMode)
                return;

            var right = new Vector2(size, 0);
            var up = new Vector2(0, size);

            Line(center - right, center + right, color, duration, renderOrder);
            Line(center - up, center + up, color, duration, renderOrder);
        }

        /// <summary>
        /// Draw a wire circle
        /// </summary>
        public void Circle(Vector2 center, float radius = 1f, Color? color = null, float? duration = null, RenderOrder renderOrder = default)
        {
            if (!game.DevelopmentMode)
                return;

            var model = Matrix3x2.CreateScale(radius) * Matrix3x2.CreateTranslation(center);
            var task = new DebugDrawTask(circle, model, debugMaterial, GetColor(color));
            AddDrawing(duration, renderOrder, task);
        }

        /// <summary>
        /// Draw a rectangle
        /// </summary>
        public void Rectangle(Vector2 topleft, Vector2 size, float rotationDegrees, Color? color = null, float? duration = null, RenderOrder renderOrder = default)
        {
            if (!game.DevelopmentMode)
                return;

            var model = Matrix3x2.CreateRotation(rotationDegrees * Utilities.DegToRad) *
                Matrix3x2.CreateScale(size.X, -size.Y) * Matrix3x2.CreateTranslation(topleft);
            var task = new DebugDrawTask(rect, model, debugMaterial, GetColor(color));
            AddDrawing(duration, renderOrder, task);
        }

        /// <summary>
        /// Draw a rectangle
        /// </summary>
        public void Rectangle(Rect rect, float rotationDegrees, Color? color = null, float? duration = null, RenderOrder renderOrder = default) =>
            Rectangle(rect.TopLeft, new Vector2(rect.Width, rect.Height), rotationDegrees, color, duration, renderOrder);

        /// <summary>
        /// Draw a rectangle
        /// </summary>
        public void Text(Vector2 topleft, string @string, float size = 1, Color? color = null, float? duration = null, RenderOrder renderOrder = default)
        {
            if (!game.DevelopmentMode)
                return;

            var model = Matrix3x2.CreateScale(size) * Matrix3x2.CreateTranslation(topleft);
            var task = new DebugTextDrawTask(text, model, fontMaterial, GetColor(color))
            {
                Generator = textGenerator,
                String = @string
            };

            AddDrawing(duration, renderOrder, task);
        }

        private void AddDrawing(float? duration, RenderOrder renderOrder, DebugDrawTask task)
        {
            drawings.Add(new Drawing
            {
                ExpirationTime = game.State.Time.SecondsSinceLoad + GetDuration(duration),
                Order = renderOrder,
                Task = task
            });
        }

        private struct Drawing
        {
            public IRenderTask Task;
            public RenderOrder Order;
            public float ExpirationTime;
        }

        private class DebugTextDrawTask : DebugDrawTask
        {
            public string String = string.Empty;
            public TextMeshGenerator? Generator;

            public DebugTextDrawTask(VertexBuffer vertexBuffer, Matrix3x2 modelMatrix, Material material, Color tint) : base(vertexBuffer, modelMatrix, material, tint)
            {
                Tint = tint;
            }

            protected override void Draw(IGraphics graphics)
            {
                if (Generator == null)
                    return;

                int vertexCount = String.Length * 4;

                if (VertexBuffer.Vertices == null || VertexBuffer.Vertices.Length != vertexCount)
                {
                    VertexBuffer.Vertices = new Vertex[vertexCount];
                    VertexBuffer.Indices = new uint[String.Length * 6];
                }

                Generator.Generate(String, VertexBuffer.Vertices, VertexBuffer.Indices);
                VertexBuffer.ForceUpdate();

                base.Draw(graphics);
            }
        }

        private class DebugDrawTask : ShapeRenderTask
        {
            public Color Tint;

            public DebugDrawTask(VertexBuffer vertexBuffer, Matrix3x2 modelMatrix, Material material, Color tint) : base(vertexBuffer, modelMatrix, material)
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
