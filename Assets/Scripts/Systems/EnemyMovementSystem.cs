using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

public partial class EnemyMovementSystem : SystemBase
{
    private EntityQuery enemyQuery;

    protected override void OnCreate()
    {
        enemyQuery = GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
    }

    protected override void OnUpdate()
    {
        int enemyCount = enemyQuery.CalculateEntityCountWithoutFiltering();

        if (enemyCount <= 0) return;

        float deltaTime = Time.DeltaTime;

        var moveEnemyHandler = Entities.ForEach((ref PhysicsVelocity velocity, in Translation translation, in Rotation rotation) => {
            velocity.Linear = math.mul(rotation.Value, math.normalizesafe(float3.zero - translation.Value) * 300 * deltaTime);
        }).ScheduleParallel(Dependency);

        Dependency = moveEnemyHandler;
    }
}
