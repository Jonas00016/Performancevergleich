using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

public partial class EnemyMovementSystem : SystemBase
{
    private const float MOVEMENTSPEED = 100f;

    private EntityQuery enemyQuery;
    private EntityQuery playerQuery;

    protected override void OnCreate()
    {
        enemyQuery = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
        playerQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>(), ComponentType.ReadOnly<Translation>());
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        int enemyCount = enemyQuery.CalculateEntityCountWithoutFiltering();

        if (enemyCount <= 0) return;

        JobHandle playerRotationHandle = World.GetOrCreateSystem<PlayerRotateSystem>().rotationHandle;
        JobHandle playerMovementHandle = World.GetOrCreateSystem<PlayerMovementSystem>().movementHandle;

        NativeArray<Translation> playerTranslation = playerQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out JobHandle getPlayerTranslationHandler);
        NativeArray<JobHandle> jobHandlers = new NativeArray<JobHandle>(3, Allocator.TempJob);
        jobHandlers[0] = playerRotationHandle;
        jobHandlers[1] = playerMovementHandle;
        jobHandlers[2] = getPlayerTranslationHandler;
        Dependency = JobHandle.CombineDependencies(jobHandlers);
        jobHandlers.Dispose();

        float deltaTime = Time.DeltaTime;
        float movementSpeed = MOVEMENTSPEED;

        var moveEnemyHandler = Entities
            .WithAll<EnemyTag>()
            .WithReadOnly(playerTranslation)
            .WithDisposeOnCompletion(playerTranslation)
            .ForEach((ref PhysicsVelocity velocity, in Translation translation, in Rotation rotation) =>
            {
                if (playerTranslation.Length <= 0) return;

                velocity.Linear = math.mul(rotation.Value, math.normalizesafe(playerTranslation[0].Value - translation.Value, float3.zero) * movementSpeed * deltaTime);
            }
        ).ScheduleParallel(Dependency);

        Dependency = moveEnemyHandler;
    }
}
