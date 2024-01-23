using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace Walgelijk.OpenTK;

public class LoadedMaterial
{
    public int ProgramHandle { get; private set; }

    public Material Material { get; private set; }

    private readonly Dictionary<string, int> uniformLocations = new Dictionary<string, int>();
    private readonly Dictionary<int, int> textureUnitAssignments = new Dictionary<int, int>();

    public LoadedMaterial(Material material)
    {
        SetFromMaterial(material);
    }

    private void SetFromMaterial(Material material)
    {
        var shader = material.Shader;
        var loadedShader = GPUObjects.ShaderCache.Load(shader);
        int programIndex = GL.CreateProgram();

        LinkShaders(loadedShader, programIndex);

        GL.ValidateProgram(programIndex);

        Material = material;
        ProgramHandle = programIndex;
    }

    private static void LinkShaders(LoadedShader shader, int programIndex)
    {
        GL.AttachShader(programIndex, shader.VertexShaderHandle);
        GL.AttachShader(programIndex, shader.FragmentShaderHandle);

        GL.LinkProgram(programIndex);
        GL.GetProgram(programIndex, GetProgramParameterName.LinkStatus, out int linkStatus);

        bool linkingFailed = linkStatus == (int)All.False;

        if (linkingFailed)
        {
            GL.DeleteProgram(programIndex);
            throw new Exception("Shader program failed to link");
        }
    }

    public int GetUniformLocation(string name)
    {
        if (uniformLocations.TryGetValue(name, out int loc))
            return loc;

        loc = GL.GetUniformLocation(ProgramHandle, name);
        uniformLocations.Add(name, loc);
        return loc;
    }

    public TextureUnit GetTextureUnitForUniform(int uniformLocation)
    {
        if (textureUnitAssignments.TryGetValue(uniformLocation, out var index))
            return TypeConverter.Convert(index);

        index = textureUnitAssignments.Count;

        if (index > GLUtilities.GetMaximumAmountOfTextureUnits())
            Logger.Error($"Exceeded maximum texture unit count. Your material cannot have more than {GLUtilities.GetMaximumAmountOfTextureUnits()} textures.");

        textureUnitAssignments.Add(uniformLocation, index);

        return TypeConverter.Convert(index);
    }
}
