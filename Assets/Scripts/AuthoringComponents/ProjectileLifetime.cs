using Unity.Entities;

[GenerateAuthoringComponent]
public struct ProjectileLifetime : IComponentData
{
    public ProjectileLifetime(float lifetime)
    {
        maxLifetime = lifetime;
        this.lifetime = 0f;
    }

    public float lifetime;
    public float maxLifetime;
}
