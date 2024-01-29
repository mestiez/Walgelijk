using System.Numerics;
using System.Runtime.ExceptionServices;
using Walgelijk;
using Walgelijk.SimpleDrawing;

namespace Playgrounds;

public struct CustomVertexScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new CustomVertexTestSystem());

        game.UpdateRate = 120;
        game.FixedUpdateRate = 8;

        return scene;
    }

    public class CustomVertexTestSystem : Walgelijk.System
    {
        TestTask task = new();

        public override void Render()
        {
            Game.Compositor.Enabled = false;
            RenderQueue.Add(new ClearRenderTask(), RenderOrder.Bottom);
            RenderQueue.Add(task);
        }
    }

    public struct FunVertex
    {
        public Vector3 Position;
        public Vector2 TexCoords;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiTangent;
        public Vector4 Color;

        public FunVertex(Vector3 position, Vector2 texCoords, Vector3 normal, Vector3 tangent, Vector3 biTangent, Color color)
        {
            Position = position;
            TexCoords = texCoords;
            Normal = normal;
            Tangent = tangent;
            BiTangent = biTangent;
            Color = color;
        }

        public FunVertex(float x, float y)
        {
            Position = new Vector3(x, y, 0);
            TexCoords = new Vector2(x,y);
            Normal = new Vector3(Utilities.RandomFloat(), Utilities.RandomFloat(), Utilities.RandomFloat() );
            Tangent = new Vector3(Utilities.RandomFloat(), Utilities.RandomFloat(), Utilities.RandomFloat());
            BiTangent = new Vector3(Utilities.RandomFloat(), Utilities.RandomFloat(), Utilities.RandomFloat());
            Color = Utilities.RandomColour(1);
        }

        public readonly struct Descriptor : IVertexDescriptor
        {
            public IEnumerable<VertexAttributeDescriptor> GetAttributes()
            {
                yield return new VertexAttributeDescriptor(AttributeType.Vector3); // position
                yield return new VertexAttributeDescriptor(AttributeType.Vector2); // uv
                yield return new VertexAttributeDescriptor(AttributeType.Vector3); // normal
                yield return new VertexAttributeDescriptor(AttributeType.Vector3); // tangent
                yield return new VertexAttributeDescriptor(AttributeType.Vector3); // bitangent
                yield return new VertexAttributeDescriptor(AttributeType.Vector4); // color
            }
        }
    }

    public class TestTask : IRenderTask
    {
        Material FunMaterial = new Material(new Shader(FunVertexShader, FunFragmentShader));

        VertexBuffer<FunVertex> FunBuffer = new(
            [
                new FunVertex(-1, -1),
                new FunVertex(2, -1),
                new FunVertex(-1, 1),
                new FunVertex(1, 1),
            ],
            [
                0,
                2,
                1,
                1,
                2,
                3
            ],
            new FunVertex.Descriptor()
        );

        public void Execute(IGraphics g)
        {
            g.CurrentTarget.ModelMatrix = Matrix4x4.CreateScale(100);
            g.CurrentTarget.ViewMatrix = Matrix4x4.Identity;
            g.CurrentTarget.ProjectionMatrix = g.CurrentTarget.OrthographicMatrix;

            g.Draw(FunBuffer, FunMaterial);
        }
    }

    public const string FunVertexShader =
@"#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec3 in_normal;
layout(location = 3) in vec3 in_tangent;
layout(location = 4) in vec3 in_biTangent;
layout(location = 5) in vec4 color;

out vec2 uv;
out vec4 vertexColor;
out vec3 normal;
out vec3 tangent;
out vec3 biTangent;
out vec4 worldPosition;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{   
   uv = texcoord;
   vertexColor = color;
   normal = (model * vec4(in_normal, 1)).xyz;
   tangent = (model * vec4(in_tangent, 1)).xyz;
   biTangent = (model * vec4(in_biTangent, 1)).xyz;

   gl_Position = projection * view * model * vec4(position, 1.0);
}";

    public const string FunFragmentShader =
@"#version 330 core

in vec2 uv;
in vec4 vertexColor;
in vec3 normal;
in vec3 tangent;
in vec3 biTangent;
in vec4 worldPosition;

out vec4 color;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    color = vec4(1, 1, 1, 1);
}";
}
