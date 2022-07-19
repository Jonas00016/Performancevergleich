/*using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class CubeRotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.WithAll<CubeTag>().ForEach((ref Rotation rotation) =>
        {
            rotation.Value = math.mul(rotation.Value, quaternion.RotateX(deltaTime));
            rotation.Value = math.mul(rotation.Value, quaternion.RotateY(deltaTime));
            rotation.Value = math.mul(rotation.Value, quaternion.RotateZ(deltaTime));
        }).ScheduleParallel();
    }
}
*/