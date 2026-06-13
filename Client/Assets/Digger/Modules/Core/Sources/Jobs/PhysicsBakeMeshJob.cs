using Unity.Burst;
using Unity.Jobs;

namespace Digger.Modules.Core.Sources.Jobs
{
    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance)]
    public struct PhysicsBakeMeshJob : IJob
    {
#if UNITY_6000_1_OR_NEWER
        public UnityEngine.EntityId MeshEntityId;
#else
        public int MeshInstanceId;
#endif

        public void Execute()
        {
#if UNITY_6000_1_OR_NEWER
            UnityEngine.Physics.BakeMesh(MeshEntityId, false);
#else
            UnityEngine.Physics.BakeMesh(MeshInstanceId, false);
#endif
        }
    }
}
