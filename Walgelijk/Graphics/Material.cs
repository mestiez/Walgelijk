using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Walgelijk
{
    /// <summary>
    /// Object that holds unique information specific to a shader
    /// </summary>
    [Serializable]
    public sealed class Material
    {
        /// <summary>
        /// The shader this material uses
        /// </summary>
        public Shader Shader { get; set; } = Shader.Default;

        private readonly Dictionary<string, object> uniforms = new Dictionary<string, object>();

        /// <summary>
        /// Create a material with a shader
        /// </summary>
        /// <param name="shader"></param>
        public Material(Shader shader)
        {
            Shader = shader;
        }

        /// <summary>
        /// New instance of the default shader
        /// </summary>
        public Material()
        {
            Shader = Shader.Default;
        }

        /// <summary>
        /// Set uniform data
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetUniform(string name, object value)
        {
            if (Game.Main == null)
                throw new InvalidOperationException("There is no main instance of Game. Setting uniforms can only be done once a game is running");

            uniforms.TryAdd(name, value);

            uniforms[name] = value;
            Game.Main.Window.ShaderManager.SetUniform(this, name, value);
        }

        /// <summary>
        /// Try to get the value of a uniform
        /// </summary>
        /// <returns>True if the uniform exists</returns>
        public bool TryGetUniform(string name, out object value)
        {
            if (uniforms.TryGetValue(name, out value))
                return true;
            return false;
        }

        /// <summary>
        /// Returns whether a uniform with the given name has been registered in the material
        /// </summary>
        public bool HasUniform(string name)
        {
            return uniforms.ContainsKey(name);
        }

        /// <summary>
        /// Get all uniforms and their values as an immutable dictionary
        /// </summary>
        public ImmutableDictionary<string, object> GetAllUniforms()
        {
            return uniforms.ToImmutableDictionary();
        }

        /// <summary>
        /// The default material with the default shader. This material is shared.
        /// </summary>
        public static Material DefaultTextured => DefaultMaterialInitialiser.GetMaterial();
    }

    internal struct DefaultMaterialInitialiser
    {
        private static Material material;

        public static Material GetMaterial()
        {
            if (material == null)
            {
                material = new Material();
                material.SetUniform(ShaderDefaults.MainTextureUniform, new Texture(1, 1, new[] { Color.White }));
            }
            return material;
        }
    }
}
