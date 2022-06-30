using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui
{
    public struct ColourPickerRendering
    {
        public static readonly ColourPickerMaterialCache ColourPickerMaterialCache = new ColourPickerMaterialCache();

        public static void FillRainbow(Texture texture)
        {
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                {
                    var c = ColourUtils.HSVtoRGB(y / (float)texture.Height, 1, 1);
                    texture.SetPixel(x, y, c);
                }
            texture.FilterMode = FilterMode.Linear;
            texture.WrapMode = WrapMode.Mirror;
            texture.ForceUpdate();
        }

        public struct Shaders
        {
            public const string HueUniform = "hue";
            public const string FragmentShader =
    @$"#version 460

in vec2 uv;
in vec4 vertexColor;

out vec4 color;

uniform vec2 {DrawingMaterialCreator.ScaleUniform};
uniform float {DrawingMaterialCreator.RoundednessUniform} = 0;
uniform float {HueUniform} = 0;

uniform float {DrawingMaterialCreator.OutlineWidthUniform} = 0;
uniform vec4 {DrawingMaterialCreator.OutlineColourUniform} = vec4(0,0,0,0);

uniform sampler2D {DrawingMaterialCreator.MainTexUniform};
uniform vec4 {DrawingMaterialCreator.TintUniform} = vec4(1, 1, 1, 1);

//https://stackoverflow.com/questions/15095909/from-rgb-to-hsv-in-opengl-glsl
vec3 hsv2rgb(vec3 c)
{{
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}}

void main()
{{
    vec3 saturationValue = hsv2rgb(vec3({HueUniform}, uv.x, uv.y));

    float corner = 1;
    float clampedRoundness = clamp({DrawingMaterialCreator.RoundednessUniform}, 0, min(scale.x / 2, scale.y / 2));

    if (clampedRoundness >= 1)
    {{
        vec2 round = vec2(clampedRoundness);
        vec2 scaledRoundedness = round / {DrawingMaterialCreator.ScaleUniform};
        vec2 scaledUv = {DrawingMaterialCreator.ScaleUniform} * uv;
    
        if (uv.x < scaledRoundedness.x && uv.y < scaledRoundedness.y)
            corner = distance(round, scaledUv) < clampedRoundness ? 1 : 0;    
        else if (uv.x < scaledRoundedness.x && 1 - uv.y < scaledRoundedness.y)
            corner = distance(vec2(round.x, {DrawingMaterialCreator.ScaleUniform}.y - round.y), scaledUv) < clampedRoundness ? 1 : 0;    
        else if (1 - uv.x < scaledRoundedness.x && 1 - uv.y < scaledRoundedness.y)
            corner = distance(vec2({DrawingMaterialCreator.ScaleUniform}.x - round.x, {DrawingMaterialCreator.ScaleUniform}.y - round.y), scaledUv) < clampedRoundness ? 1 : 0;    
        else if (1 - uv.x < scaledRoundedness.x && uv.y < scaledRoundedness.y)
            corner = distance(vec2({DrawingMaterialCreator.ScaleUniform}.x - round.x, round.y), scaledUv) < clampedRoundness ? 1 : 0; 
    }}
    color = vertexColor * texture({DrawingMaterialCreator.MainTexUniform}, uv) * tint;
    color.a *= corner;
    color.rgb = saturationValue;
}}";

            public static readonly Shader DefaultShader = new(ShaderDefaults.WorldSpaceVertex, FragmentShader);

            public static readonly Material SaturationValueMaterial = Create();

            public static Material Create()
            {
                var m = new Material(DefaultShader);
                return m;
            }
        }
    }
}
