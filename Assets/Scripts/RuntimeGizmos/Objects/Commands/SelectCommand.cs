using System;
using CommandUndoRedo;
using UnityEngine;
using System.Collections.Generic;

namespace RuntimeGizmos
{
    public abstract class SelectCommand : ICommand
    {
        protected Transform target;
        protected TransformGizmo transformGizmo;

        public SelectCommand(TransformGizmo transformGizmo, Transform target)
        {
            this.transformGizmo = transformGizmo;
            this.target = target;
        }

        public abstract void Execute();
        public abstract void UnExecute();
    }

    public class AddTargetCommand : SelectCommand
    {
        List<Transform> targetRoots = new List<Transform>();

        public AddTargetCommand(TransformGizmo transformGizmo, Transform target, List<Transform> targetRoots) : base(transformGizmo, target)
        {
            //Since we might have had a child selected and then selected the parent, the child would have been removed from the selected,
            //so we store all the targetRoots before we add so that if we undo we can properly have the children selected again.
            this.targetRoots.AddRange(targetRoots);
        }

        public override void Execute()
        {
            transformGizmo.AddTargetServerRpc(transformGizmo.GetTransformID(target), false);
        }

        public override void UnExecute()
        {
            transformGizmo.RemoveTargetServerRpc(transformGizmo.GetTransformID(target), false);

            for (int i = 0; i < targetRoots.Count; i++)
            {
                transformGizmo.AddTargetServerRpc(transformGizmo.GetTransformID(targetRoots[i]), false);
            }
        }
    }

    public class RemoveTargetCommand : SelectCommand
    {
        public RemoveTargetCommand(TransformGizmo transformGizmo, Transform target) : base(transformGizmo, target) { }

        public override void Execute()
        {
            transformGizmo.RemoveTargetServerRpc(transformGizmo.GetTransformID(target), false);
        }

        public override void UnExecute()
        {
            transformGizmo.AddTargetServerRpc(transformGizmo.GetTransformID(target), false);
        }
    }

    public class ClearTargetsCommand : SelectCommand
    {
        List<Transform> targetRoots = new List<Transform>();

        public ClearTargetsCommand(TransformGizmo transformGizmo, List<Transform> targetRoots) : base(transformGizmo, null)
        {
            this.targetRoots.AddRange(targetRoots);
        }

        public override void Execute()
        {
            transformGizmo.ClearTargetsServerRpc(false);
        }

        public override void UnExecute()
        {
            for (int i = 0; i < targetRoots.Count; i++)
            {
                transformGizmo.AddTargetServerRpc(transformGizmo.GetTransformID(targetRoots[i]), false);
            }
        }
    }

    public class ClearAndAddTargetCommand : SelectCommand
    {
        List<Transform> targetRoots = new List<Transform>();

        public ClearAndAddTargetCommand(TransformGizmo transformGizmo, Transform target, List<Transform> targetRoots) : base(transformGizmo, target)
        {
            this.targetRoots.AddRange(targetRoots);
        }

        public override void Execute()
        {
            transformGizmo.ClearTargetsServerRpc(false);
            transformGizmo.AddTargetServerRpc(transformGizmo.GetTransformID(target), false);
        }

        public override void UnExecute()
        {
            transformGizmo.RemoveTargetServerRpc(transformGizmo.GetTransformID(target), false);

            for (int i = 0; i < targetRoots.Count; i++)
            {
                transformGizmo.AddTargetServerRpc(transformGizmo.GetTransformID(targetRoots[i]), false);
            }
        }
    }
}
