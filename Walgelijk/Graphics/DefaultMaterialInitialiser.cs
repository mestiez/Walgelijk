namespace Walgelijk
{
    internal struct DefaultMaterialInitialiser
    {
        private static Material material;

        public static Material GetMaterial()
        {
            if (material == null)
            {
                material = new Material();
                material.SetUniform(ShaderDefaults.MainTextureUniform, Texture.White);
            }
            return material;
        }
    }
}
