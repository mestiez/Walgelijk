namespace Walgelijk;

public static class BatchMaterialCreator
{
    public static readonly Material DefaultInstancedMaterial;
    public static readonly Shader InstancedDefaultShader = new(BuiltInShaders.BatchVertex, BuiltInShaders.BatchFragment);
    static BatchMaterialCreator()
    {
        DefaultInstancedMaterial = new Material(InstancedDefaultShader);
        DefaultInstancedMaterial.SetUniform(ShaderDefaults.MainTextureUniform, Texture.White);
    }
}