using Unity.Entities;

[GenerateAuthoringComponent]
public struct FloorAuthoringComponent : IComponentData
{
    public Entity prefab;
}
