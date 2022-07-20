using Unity.Entities;

[GenerateAuthoringComponent]
public struct EnvironmentAuthoringComponent : IComponentData
{
    public Entity prefab;
}
