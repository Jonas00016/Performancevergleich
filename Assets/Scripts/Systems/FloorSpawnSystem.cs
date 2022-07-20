using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class FloorSpawnSystem : SystemBase
{
    private EntityQuery floorQuery;
    private BeginSimulationEntityCommandBufferSystem beginnSimulationECB;
    private Entity prefab;

    protected override void OnCreate()
    {
        floorQuery = GetEntityQuery(ComponentType.ReadWrite<FloorTag>());
        beginnSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<FloorAuthoringComponent>().prefab;
            return;
        }

        int floorCount = floorQuery.CalculateEntityCountWithoutFiltering();

        if (floorCount >= 1) return;

        EntityCommandBuffer commandBuffer = beginnSimulationECB.CreateCommandBuffer();
        Entity floorPrefab = prefab;
        
        Job.WithCode(() =>
        {
            commandBuffer.Instantiate(floorPrefab);
        }).Schedule();

        beginnSimulationECB.AddJobHandleForProducer(Dependency);
    }
}
