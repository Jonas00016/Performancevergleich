using Unity.Entities;

[GenerateAuthoringComponent]
public struct CubeAuthoringComponent : IComponentData
{
    public Entity prfab;
}