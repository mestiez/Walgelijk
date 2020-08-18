using System;
using System.Collections.Generic;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// Shader specific link between the engine and the graphics API
    /// </summary>
    public interface IShaderManager
    {
        /// <summary>
        /// Set a shader program uniform
        /// </summary>
        void SetUniform(Material material, string uniformName, object data);

        /// <summary>
        /// Try to get a shader program uniform.
        /// </summary>
        /// <returns>Boolean indicating whether the operation was successful</returns>
        bool TryGetUniform<T>(Material material, string uniformName, out T data);
    }
}
