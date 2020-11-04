namespace Walgelijk.ParticleSystem
{
    internal static class ParticleMaterialInitialiser
    {
        public static Material CreateDefaultMaterial()
        {
            Material mat = new Material(Particle.DefaultShader);
            mat.SetUniform(ShaderDefaults.MainTextureUniform, Texture.White);
            return mat;
        }
    }
}
