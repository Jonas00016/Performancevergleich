using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class BorderSpawnSystem : SystemBase
{
    private EntityQuery borderQuery;
    private BeginSimulationEntityCommandBufferSystem beginSumilationECB;
    private Entity prefab;

    protected override void OnCreate()
    {
        borderQuery = GetEntityQuery(ComponentType.ReadWrite<BorderTag>());
        beginSumilationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<BorderAuthoringComponent>().prfab;
            return;
        }

        EntityCommandBuffer commandBuffer = beginSumilationECB.CreateCommandBuffer();
        Entity borderPrefab = prefab;
        int borderCount = borderQuery.CalculateEntityCountWithoutFiltering();

        if (borderCount >= 1) return;

        Job.WithCode(() =>
        {
            Entity newBorderEntity = commandBuffer.Instantiate(borderPrefab);

            Translation spawnPosition = new Translation { Value = new float3(0, 0, 0) };
            commandBuffer.SetComponent(newBorderEntity, spawnPosition);
               
        }).Schedule();

        beginSumilationECB.AddJobHandleForProducer(Dependency);
    }
}
