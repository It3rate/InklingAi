using InkPad.Agents;
using InkPad.CommandEngine;
using InkPad.CommandEngine.BrainEngine;
using InkPad.Commands;
using InkPad.Commands.Core;
using Nums.CoreConcepts.Time;
using Nums.Primitives;

namespace InkPadTests.CommandTests
{

    public class CreateCommandTests : IDisposable
    {
        private CommandStack _stack;
        private Brain _brain;
        private Workspace _workspace;
        private Agent _agent;
        private Trait _trait;

        public CreateCommandTests()
        {
            _brain = Brain.ActiveBrain;
            _workspace = new Workspace(_brain);
            _agent = new Agent(null);// _workspace);
            _stack = new BrainCommandStack(_agent);
            _trait = Trait.CreateIn(_brain, "create command tests");
        }

        public void Dispose()
        {
            _brain.ClearAll();
        }

        [Fact]
        public void WorkspaceCommandTests()
        {
            var command = new CreateWorkspaceCommand();
            _stack.Do(command);
            Assert.Equal(1, _stack.UndoSize);
            Assert.Single(command.Tasks);
            //Assert.Equal(command.Workspace.Id, _brain.Workspaces[command.Workspace.Id].Id);
            _stack.Undo();
            Assert.Equal(0, _stack.UndoSize);
            Assert.Empty(command.Tasks);
            _stack.Redo();
            Assert.Equal(1, _stack.UndoSize);
            Assert.Single(command.Tasks);
        }

        [Fact]
        public void TraitCommandTests()
        {
            var command = new CreateTraitCommand("TraitTest");
            _stack.Do(command);
            Assert.Equal(1, _stack.UndoSize);
            Assert.Single(command.Tasks);
            Assert.Equal("TraitTest", _brain.TraitStore[command.Trait.Name].Name);
            _stack.Undo();
            Assert.Equal(0, _stack.UndoSize);
            Assert.Empty(command.Tasks);
            _stack.Redo();
            Assert.Equal(1, _stack.UndoSize);
            Assert.Single(command.Tasks);
        }
        [Fact]
        public void DomainCommandTests()
        {
            var command = new CreateDomainCommand(_workspace, _trait, "DomainCommandTests", 0, 10, -1000, 1000);
            _stack.Do(command);
            Assert.Equal(1, _stack.UndoSize);
            Assert.Equal(0, _stack.RedoSize);
            Assert.Equal(3, command.Tasks.Count);
            _stack.Undo();
            Assert.Equal(0, _stack.UndoSize);
            Assert.Equal(1, _stack.RedoSize);
            Assert.Empty(command.Tasks);
            _stack.Redo();
            Assert.Equal(1, _stack.UndoSize);
            Assert.Equal(3, command.Tasks.Count);
        }

        [Fact]
        public void NumberCommandTests()
        {
            var domainCommand = new CreateDomainCommand(_workspace, _trait, "NumberCommandTests", 0, 10, -1000, 1000);
            _stack.Do(domainCommand);

            var command = new CreateNumberCommand(domainCommand.Domain, 100, 200);
            _stack.Do(command);
            Assert.Equal(2, _stack.UndoSize);
            Assert.Equal(0, _stack.RedoSize);
            Assert.Equal(100, command.Number.Focal.StartTickPosition);
            Assert.Equal(200, command.Number.Focal.EndTickPosition);
            _stack.Undo();
            Assert.Equal(1, _stack.UndoSize);
            Assert.Equal(1, _stack.RedoSize);
            Assert.Empty(command.Tasks);
            _stack.Redo();
            Assert.Equal(2, _stack.UndoSize);
            Assert.Single(command.Tasks);
        }

        [Fact]
        public void DelayCommandTests()
        {
            var domainCommand = new CreateDomainCommand(_workspace, _trait, "DelayCommandTests", 0, 10, -1000, 1000);
            _stack.Do(domainCommand);
            var command = new CreateNumberCommand(domainCommand.Domain, 100, 200) { DefaultDelay = -500 };
            _stack.Do(command);
            Assert.Equal(1, _stack.UndoSize);
            Assert.Equal(0, _stack.RedoSize);
            var time = new MillisecondNumber(0, 600);
            _stack.Update(time, time);
            Assert.Equal(2, _stack.UndoSize);
            Assert.Equal(0, _stack.RedoSize);
            _stack.Undo();
            Assert.Equal(1, _stack.UndoSize);
            Assert.Equal(1, _stack.RedoSize);
            _stack.Redo();
            Assert.Equal(2, _stack.UndoSize);
            Assert.Equal(0, _stack.RedoSize);
        }
    }
}
