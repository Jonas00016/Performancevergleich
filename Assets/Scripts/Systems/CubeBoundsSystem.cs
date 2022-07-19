using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class CubeBoundsSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<CubeTag>().ForEach((ref Translation translation) =>
        {
            if (translation.Value.y < 0f)
            {
                translation.Value.y = 0f;
            }
        }).ScheduleParallel();
    }
}
