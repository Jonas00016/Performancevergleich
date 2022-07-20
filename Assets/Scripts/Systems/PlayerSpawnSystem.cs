using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class PlayerSpawnSystem : SystemBase
{
    private EntityQuery playerQuery;
    private BeginSimulationEntityCommandBufferSystem beginSimulationECB;
    private Entity prefab;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(ComponentType.ReadWrite<PlayerTag>());
        beginSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<PlayerAuthoringComponent>().prefab;
            return;
        }

        int playerCount = playerQuery.CalculateEntityCountWithoutFiltering();

        if (playerCount >= 1) return;

        EntityCommandBuffer commandBuffer = beginSimulationECB.CreateCommandBuffer();
        Entity playerPrefab = prefab;

        Job.WithCode(() =>
        {
            commandBuffer.Instantiate(playerPrefab);
        }).Schedule();

        beginSimulationECB.AddJobHandleForProducer(Dependency);
    }
}
