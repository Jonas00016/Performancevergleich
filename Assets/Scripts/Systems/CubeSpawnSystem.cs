using System.Diagnostics;
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
    public static bool forceCubeIncrementation = false;

    protected override void OnCreate()
    {
        cubeQuery = GetEntityQuery(ComponentType.ReadWrite<CubeTag>());
        beginSumilationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (!Input.GetKey("space") && !forceCubeIncrementation) return;
        
        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<CubeAuthoringComponent>().prfab;
            return;
        }

        if (UnityEngine.Time.time < breakeUntill && !forceCubeIncrementation) return;
        breakeUntill = UnityEngine.Time.time + breakeTime;
        forceCubeIncrementation = false;

        EntityCommandBuffer commandBuffer = beginSumilationECB.CreateCommandBuffer();
        Entity cubePrefab = prefab;
        float spacing = spacingAmount;
        int numCubes = cubeQuery.CalculateEntityCountWithoutFiltering();

        Unity.Mathematics.Random rand = new Unity.Mathematics.Random((uint)Stopwatch.GetTimestamp());

        Job.WithCode(() =>
        {
            for (int x = 0; x < 10; x++)
            {
                for (int z = 0; z < 10; z++)
                {
                    Entity newCubeEntity = commandBuffer.Instantiate(cubePrefab);

                    Translation spawnPosition = new Translation { Value = new float3(x, 50, z) * spacing };
                    commandBuffer.SetComponent(newCubeEntity, spawnPosition);

                    quaternion rotation = quaternion.RotateX(rand.NextFloat(-1f, 1f));
                    rotation = math.mul(rotation, quaternion.RotateY(rand.NextFloat(-1f, 1f)));
                    rotation = math.mul(rotation, quaternion.RotateZ(rand.NextFloat(-1f, 1f)));
                    Rotation spawnRotation = new Rotation { Value = rotation };
                    commandBuffer.SetComponent(newCubeEntity, spawnRotation);
                }
                
            }
            
        }).Schedule();

        beginSumilationECB.AddJobHandleForProducer(Dependency);
    }
}
