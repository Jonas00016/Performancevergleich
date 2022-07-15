using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class CubeSpawnSystem : SystemBase
{
    private EntityQuery cubeQuery;
    private BeginSimulationEntityCommandBufferSystem beginSumilationECB;
    private Entity prefab;
    private float spacingAmount = 1.5f;

    private float breakeTime = 1f;
    private float breakeUntill = 0f;

    protected override void OnCreate()
    {
        cubeQuery = GetEntityQuery(ComponentType.ReadWrite<CubeTag>());
        beginSumilationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (!Input.GetKey("space")) return;

        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<CubeAuthoringComponent>().prfab;
            return;
        }

        if (UnityEngine.Time.time < breakeUntill) return;
        breakeUntill = UnityEngine.Time.time + breakeTime;

        EntityCommandBuffer commandBuffer = beginSumilationECB.CreateCommandBuffer();
        Entity cubePrefab = prefab;
        float spacing = spacingAmount;
        int numCubes = cubeQuery.CalculateEntityCountWithoutFiltering();

        Job.WithCode(() =>
        {
            for (int x = 0; x < 10; x++)
            {
                for (int z = 0; z < 10; z++)
                {
                    Translation spawnPosition = new Translation { Value = new float3(x, numCubes / 100, z) * spacing };
                    Entity newCubeEntity = commandBuffer.Instantiate(cubePrefab);
                    commandBuffer.SetComponent(newCubeEntity, spawnPosition);
                }
                
            }
            
        }).Schedule();

        beginSumilationECB.AddJobHandleForProducer(Dependency);
    }
}
