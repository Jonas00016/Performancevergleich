using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(CubeSpawnSystem))]
public partial class EnvironmentSpawnSystem : SystemBase
{
    private EntityQuery environmentQuery;
    private BeginSimulationEntityCommandBufferSystem beginSimulationECB;
    private Entity prefab;

    protected override void OnCreate()
    {
        environmentQuery = GetEntityQuery(ComponentType.ReadOnly<EnvironmentTag>());
        beginSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<EnvironmentAuthoringComponent>().prfab;
            return;
        }

        if (environmentQuery.CalculateEntityCountWithoutFiltering() >= 1) return;

        EntityCommandBuffer commandBuffer = beginSimulationECB.CreateCommandBuffer();
        Entity environmentPrefab = prefab;

        Job
            .WithCode(() =>
            {
                commandBuffer.Instantiate(environmentPrefab);
            }
        ).Schedule();

        beginSimulationECB.AddJobHandleForProducer(Dependency);
    }
}
