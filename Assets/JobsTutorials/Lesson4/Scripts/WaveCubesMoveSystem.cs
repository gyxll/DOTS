using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;

namespace DOTS.DOD.Lesson4
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(Lesson4SystemGroups))]
    [UpdateAfter(typeof(WaveCubesGenerateSystem))]
    public partial struct WaveCubesMoveSystem : ISystem
    {
        static readonly ProfilerMarker profilerMarker = new ProfilerMarker("WaveCubeEntityJobs");
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using(profilerMarker.Auto())
            {
                var moveJob = new WaveCubeEntityMoveJob()
                {
                    elapsedTime = SystemAPI.Time.ElapsedTime
                };
                moveJob.ScheduleParallel();
            }            
        }
    }
}

