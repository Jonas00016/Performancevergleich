using Unity.Entities;

[GenerateAuthoringComponent]
public struct ProjectileAuthoringComponent : IComponentData
{
    public Entity prefab;
}