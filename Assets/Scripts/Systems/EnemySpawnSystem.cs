using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class EnemySpawnSystem : SystemBase
{
    private const int MAX_ENEMIES = 100000;
    private const int SPWAN_AMOUNT = 500;

    private EntityQuery enemyQuery;
    private BeginSimulationEntityCommandBufferSystem beginSimulationECB;
    private Entity prefab;
    private int enemyCount;

    protected override void OnCreate()
    {
        enemyQuery = GetEntityQuery(ComponentType.ReadWrite<EnemyTag>());
        beginSimulationECB = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<EnemyAuthoringComponent>().prefab;
            return;
        }
    }

    [BurstCompile]
    public int SpawnEnemies()
    {
        if (prefab == Entity.Null)
        {
            prefab = GetSingleton<EnemyAuthoringComponent>().prefab;
            return -1;
        }

        enemyCount = enemyQuery.CalculateEntityCountWithoutFiltering();

        if (enemyCount >= MAX_ENEMIES) return enemyCount;

        EntityCommandBuffer commandBuffer = beginSimulationECB.CreateCommandBuffer();
        Entity enemyPrefab = prefab;
        Random rnd = new Random((uint)Stopwatch.GetTimestamp());
        int spawnAmount = SPWAN_AMOUNT;

        Job.WithCode(() =>
        {
            for (int i = 0; i < spawnAmount; i++)
            {
                Entity newEnemyEntity = commandBuffer.Instantiate(enemyPrefab);

                float3 position = float3.zero;

                if (i < spawnAmount / 4 * 1)
                {
                    position = new float3(
                            rnd.NextFloat(-50f, -10f),
                            0f,
                            rnd.NextFloat(-50f, 50f)
                        );
                }
                else if (i < spawnAmount / 4 * 2)
                {
                    position = new float3(
                            rnd.NextFloat(-50f, 50f),
                            0f,
                            rnd.NextFloat(-50f, -10f)
                        );
                }
                else if (i < spawnAmount / 4 * 3)
                {
                    position = new float3(
                            rnd.NextFloat(50f, 10f),
                            0f,
                            rnd.NextFloat(-50f, 50f)
                        );
                }
                else
                {
                    position = new float3(
                            rnd.NextFloat(-50f, 50f),
                            0f,
                            rnd.NextFloat(50f, 10f)
                        );
                }

                Translation spawnPosition = new Translation { Value = position };

                commandBuffer.SetComponent(newEnemyEntity, spawnPosition);
            }

        }).Schedule();

        beginSimulationECB.AddJobHandleForProducer(Dependency);

        return enemyQuery.CalculateEntityCountWithoutFiltering();
    }
}
