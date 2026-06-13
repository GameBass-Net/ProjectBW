/// <summary>
/// Project : Easy Build System
/// Class : PlaceCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using Object = UnityEngine.Object;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Commands.Implementations
{
    public class PlaceCommand : BuildingCommand
    {
        private readonly BuildingPartData m_buildingPartData;
        private readonly BuildingSocket m_socket;
        private BuildingPart m_targetPart;

        public BuildingPart TargetPart => m_targetPart;

        public PlaceCommand(BuildingPart part, Vector3 position, Quaternion rotation, Vector3 scale, BuildingSocket socket = null)
        {
            m_buildingPartData = new BuildingPartData(part, position, rotation, scale);
            m_socket = socket;
        }

        public override void Execute()
        {
            BuildingPart prefab = BuildingManager.Instance.GetPartByPrefabId(m_buildingPartData.PrefabId);
            if (!prefab)
            {
                return;
            }

            BuildingPart placedPart = null;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameObject go = UnityEditor.PrefabUtility.InstantiatePrefab(prefab.gameObject) as GameObject;
                placedPart = go ? go.GetComponent<BuildingPart>() : null;
                if (!placedPart)
                {
                    return;
                }

                placedPart.transform.SetPositionAndRotation(m_buildingPartData.Position, m_buildingPartData.Rotation);
                placedPart.transform.localScale = m_buildingPartData.Scale;
                UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Place Building Part");
            }
            else
#endif
            {
                placedPart = Object.Instantiate(prefab, m_buildingPartData.Position, m_buildingPartData.Rotation);
                placedPart.transform.localScale = m_buildingPartData.Scale;
            }

            placedPart.IsRuntimeInstantiated = Application.isPlaying;
            placedPart.SetState(BuildingPart.BuildingState.Placed);

            if (m_socket != null)
            {
                placedPart.SetSocket(m_socket);
            }

            EventPublisher.Publish(new BuildingStateEvent.PlacedEventArgs(placedPart));
            m_targetPart = placedPart;
        }
    }
}