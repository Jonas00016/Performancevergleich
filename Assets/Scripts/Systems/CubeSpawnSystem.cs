using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class CubeSpawnSystem : SystemBase
{
    private const int AXIS_LENGTH = 50;

    private EntityQuery cubeQuery;
    private BeginSimulationEntityCommandBufferSystem beginSumilationECB;
    private Entity prefab;
    private float spacingAmount = 1.5f;

    public int spawnAmount;

    protected override void OnCreate()
    {
        spawnAmount = AXIS_LENGTH * AXIS_LENGTH;
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
    public void SpawnNextWave()
    {
        if (prefab == Entity.Null) return;

        EntityCommandBuffer commandBuffer = beginSumilationECB.CreateCommandBuffer();
        Entity cubePrefab = prefab;
        float spacing = spacingAmount;
        int numCubes = cubeQuery.CalculateEntityCountWithoutFiltering();
        int axisLength = AXIS_LENGTH;
        int nextSpawnAmount = spawnAmount;

        Job.WithCode(() =>
        {
            for (int x = 0; x < axisLength; x++)
            {
                for (int z = 0; z < axisLength; z++)
                {
                    Translation spawnPosition = new Translation { Value = new float3(x, numCubes / nextSpawnAmount, z) * spacing };
                    Entity newCubeEntity = commandBuffer.Instantiate(cubePrefab);
                    commandBuffer.SetComponent(newCubeEntity, spawnPosition);
                }

            }

        }).Schedule();

        beginSumilationECB.AddJobHandleForProducer(Dependency);
    }
}
