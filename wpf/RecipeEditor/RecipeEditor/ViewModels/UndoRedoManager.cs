using CommunityToolkit.Mvvm.ComponentModel;

namespace RecipeEditor.ViewModels;

public partial class UndoRedoManager : ObservableObject
{
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();

    [ObservableProperty] private bool _canUndo;
    [ObservableProperty] private bool _canRedo;

    public void SaveState(string snapshot)
    {
        _undoStack.Push(snapshot);
        _redoStack.Clear();
        UpdateFlags();
    }

    public string? Undo(string current)
    {
        if (!_undoStack.TryPop(out var previous)) return null;
        _redoStack.Push(current);
        UpdateFlags();
        return previous;
    }

    public string? Redo(string current)
    {
        if (!_redoStack.TryPop(out var next)) return null;
        _undoStack.Push(current);
        UpdateFlags();
        return next;
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
        UpdateFlags();
    }

    private void UpdateFlags()
    {
        CanUndo = _undoStack.Count > 0;
        CanRedo = _redoStack.Count > 0;
    }
}
