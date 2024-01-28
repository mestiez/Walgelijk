namespace Walgelijk;

public enum StencilTestMode
{
    // glStencilFunc(GL_EQUAL, 1, 0xFF)
    /// <summary>
    /// Only draw inside the mask, i.e where the stencil buffer is set to 1
    /// </summary>
    Inside,
    // glStencilFunc(GL_NOTEQUAL, 1, 0xFF)
    /// <summary>
    /// Only draw outside the mask, i.e where the stencil buffer is set to 0
    /// </summary>
    Outside
}