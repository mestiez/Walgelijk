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
        void SetUniform(Shader shader, string uniformName, object data);

        /// <summary>
        /// Try to get a shader program uniform.
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="uniformName"></param>
        /// <param name="data"></param>
        /// <returns>Boolean indicating whether the operation was successful</returns>
        bool TryGetUniform<T>(Shader shader, string uniformName, out T data);
    }
}
