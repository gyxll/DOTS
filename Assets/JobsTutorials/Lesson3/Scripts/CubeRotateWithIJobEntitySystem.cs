using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.DOD.Lesson3
{
    public partial struct CubeRotateIJobEntity:IJobEntity
    {
        public float deltaTime;
        void Execute(ref LocalTransform localTransform,in RotateSpeed rotateSpeed)
        {
            localTransform = localTransform.RotateY(rotateSpeed.speed * deltaTime);//注意这里要赋值
        }
    }
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(CubeRotateWithIJobEntitySystemGroup))]
    public partial struct CubeRotateWithIJobEntitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        [BurstCompile]
        public void OnUpdate(ref SystemState state) 
        {
            var rotateIJobEntity = new CubeRotateIJobEntity
            {
                deltaTime = SystemAPI.Time.DeltaTime
            };
            rotateIJobEntity.ScheduleParallel();
        }
    }
}

