using InkPad.CommandEngine;
using Nums.Primitives;

namespace InkPad.Tasks.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CreateWorkspaceTask : TaskBase, ICreateTask
    {
        public Workspace CreatedWorkspace;

        public override bool IsValid => true;

        public CreateWorkspaceTask()
        {
        }
        public override void RunTask()
        {
            if (CreatedWorkspace == null)
            {
                CreatedWorkspace = new Workspace(TaskAgent.Brain);
            }
            else
            {
                TaskAgent.Brain.Workspaces.Add(CreatedWorkspace);
            }
        }

        public override void UnRunTask()
        {
            TaskAgent.Brain.Workspaces.Remove(CreatedWorkspace);
        }
    }
}
