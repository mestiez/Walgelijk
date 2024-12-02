using System.Numerics;

namespace Walgelijk.Prism;

[RequiresComponents(typeof(PrismTransformComponent))]
public class PrismCameraComponent : Component
{
    public float FieldOfView = 80;
    public float NearClip = 0.1f;
    public float FarClip = 1000;
    public Color ClearColour;
    public bool Clear = true;
    public Frustum Frustum;

    public bool Active = true;

    public static void SetActive(Scene scene, PrismCameraComponent camera)
    {
        foreach (var item in scene.GetAllComponentsOfType<PrismCameraComponent>())
            item.Active = camera == item;
    }
}

public class PrismSunLightComponent : Component, IDisposable
{
    public Vector3 Direction = new Vector3(-1,-1,);
    public float Intensity;
    public Color Color;

    public Matrix4x4 LightSpaceTransform;
    public float ShadowMapSize = 100;
    public float NearClip = 1;
    public float FarClip = 1000;
    public readonly RenderTexture ShadowMap;

    public PrismSunLightComponent(int shadowResolution)
    {
        ShadowMap = new RenderTexture(shadowResolution, shadowResolution, WrapMode.Clamp, FilterMode.Linear, RenderTargetFlags.DepthStencil);
    }

    public void UpdateProjection(Vector3 observerPos)
    {
        LightSpaceTransform = Matrix4x4.CreateOrthographic(ShadowMapSize, ShadowMapSize, NearClip, FarClip) * Matrix4x4.CreateLookAt();
    }

    public void Dispose()
    {
        ShadowMap.Dispose();
    }
}
