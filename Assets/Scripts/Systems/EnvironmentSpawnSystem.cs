using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class EnvironmentSpawnSystem : SystemBase
{
    private EntityQuery environmentQuery;
    private BeginSimulationEntityCommandBufferSystem beginnSimulationECB;
    private Entity prefab;

    protected override void OnCreate()
    {
        environmentQuery = GetEntityQuery(ComponentType.ReadWrite<EnvironmentTag>());
        beginnSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<EnvironmentAuthoringComponent>().prefab;
            return;
        }

        int environmentCount = environmentQuery.CalculateEntityCountWithoutFiltering();

        if (environmentCount >= 1) return;

        EntityCommandBuffer commandBuffer = beginnSimulationECB.CreateCommandBuffer();
        Entity environmentPrefab = prefab;
        
        Job.WithCode(() =>
        {
            commandBuffer.Instantiate(environmentPrefab);
        }).Schedule();

        beginnSimulationECB.AddJobHandleForProducer(Dependency);
    }
}
