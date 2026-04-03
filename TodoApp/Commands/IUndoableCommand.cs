namespace TodoApp.Commands
{
    public interface IUndoableCommand : ICommand
    {
        void Unexecute();
    }
}
