using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;

public partial class CubeSpawnSystem : SystemBase
{
    private const float UPSWING = 15f;

    private float3 spawnPosition = new float3(50f, 20f, 50f);

    private EntityQuery cubeQuery;
    private BeginSimulationEntityCommandBufferSystem beginSumilationECB;
    private Entity prefab;

    public int cubesSpawned = 0;

    protected override void OnCreate()
    {
        cubeQuery = GetEntityQuery(ComponentType.ReadWrite<CubeTag>());
        beginSumilationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<CubeAuthoringComponent>().prfab;
            return;
        }
    }

    [BurstCompile]
    public int SpawnNextWave()
    {
        if (prefab == Entity.Null) return -1;

        EntityCommandBuffer commandBuffer = beginSumilationECB.CreateCommandBuffer();
        Entity cubePrefab = prefab;
        int numCubes = cubeQuery.CalculateEntityCountWithoutFiltering();
        float3 newSpawnPosition = spawnPosition;
        float3 upwards = new float3(0f, 1f, 0f);
        float upswing = UPSWING;

        Job
            .WithCode(() =>
            {
                Translation spawnPosition = new Translation { Value = newSpawnPosition };

                Entity newCubeEntity = commandBuffer.Instantiate(cubePrefab);
                commandBuffer.SetComponent(newCubeEntity, spawnPosition);

                PhysicsVelocity upwardsVelocity = new PhysicsVelocity { Linear = upwards * upswing };
                commandBuffer.SetComponent(newCubeEntity, upwardsVelocity);
            }
        ).Schedule();

        beginSumilationECB.AddJobHandleForProducer(Dependency);

        return cubeQuery.CalculateEntityCountWithoutFiltering();
    }
}
