using System.Numerics;

namespace Walgelijk.ParticleSystem
{
    public struct Particle
    {
        public Vector2 Position;
        public float Angle;

        public Color InitialColor;
        public Color Color;

        public float InitialSize;
        public float Size;

        public Vector2 Velocity;
        public float RotationalVelocity;

        public Vector2 Gravity;

        public float Life;
        public float MaxLife;

        public float Dampening;
        public float RotationalDampening;

        public static readonly Shader DefaultShader = new Shader(
            @"
#version 460

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec4 color;
layout(location = 4) in vec4 instanceModel;

out vec2 uv;
out vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = texcoord;
   vertexColor = color;
   gl_Position = projection * view * instanceModel * model * vec4(position, 1.0);
}",
            @"
#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform sampler2D mainTex;

void main()
{
    color = vertexColor * texture(mainTex, uv);
}"
            );

        public static readonly Material DefaultMaterial = ParticleMaterialInitialiser.CreateDefaultMaterial();
    }
}
