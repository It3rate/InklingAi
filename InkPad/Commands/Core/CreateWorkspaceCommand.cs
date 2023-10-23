using Nums.Primitives;

namespace InkPad.Commands.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using InkPad.CommandEngine.BrainEngine;
    using InkPad.Tasks.Core;

    public class CreateWorkspaceCommand : BrainCommandBase
    {
        private CreateWorkspaceTask WorkspaceTask;
        public new Workspace Workspace => WorkspaceTask?.CreatedWorkspace;

        public CreateWorkspaceCommand()
        {
        }

        public override void Execute()
        {
            base.Execute();
            if (WorkspaceTask == null)
            {
                WorkspaceTask = new CreateWorkspaceTask();
            }
            AddTaskAndRun(WorkspaceTask);
        }

        public override void Unexecute()
        {
            base.Unexecute();
        }

        public override void Completed()
        {
        }
    }
}
