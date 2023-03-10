using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace PrimalEditor.Utilities;

public interface IUndoRedo
{
    string Name { get; }

    void Undo();
    void Redo();
}

public class UndoRedoAction : IUndoRedo
{
    private readonly Action _undoAction;
    private readonly Action _redoAction;

    public string Name { get; }


    public void Redo() => _redoAction();

    public void Undo() => _undoAction();

    private UndoRedoAction(string name)
    {
        this.Name = name;
    }

    public UndoRedoAction(string name, Action undoAction, Action redoAction) : this(name)
    {
        Debug.Assert(undoAction != null && redoAction != null);

        _undoAction = undoAction;
        _redoAction = redoAction;
    }
    public UndoRedoAction(string property, object instance, object undoValue, object redoValue, string name) :
        this(
            name,
            () => instance.GetType().GetProperty(property)?.SetValue(instance, undoValue),
            () => instance.GetType().GetProperty(property)?.SetValue(instance, redoValue))
    {

    }
}

public class UndoRedo
{
    private bool _enableAdd = true;

    private readonly ObservableCollection<IUndoRedo> _undoList = new();
    private readonly ObservableCollection<IUndoRedo> _redoList = new();

    public ReadOnlyObservableCollection<IUndoRedo> UndoList { get; }
    public ReadOnlyObservableCollection<IUndoRedo> RedoList { get; }

    public void Reset()
    {
        _undoList.Clear();
        _redoList.Clear();
    }

    public void Add(IUndoRedo cmd)
    {
        if (_enableAdd)
        {
            _undoList.Add(cmd);
            _redoList.Clear();
        }
    }

    public void Undo()
    {
        if (_undoList.Any())
        {
            var cmd = _undoList.Last();

            _undoList.RemoveAt(_undoList.Count - 1);
            _enableAdd = false;

            cmd.Undo();

            _enableAdd = true;
            _redoList.Insert(0, cmd);
        }
    }

    public void Redo()
    {
        if (_redoList.Any())
        {
            var cmd = _redoList.First();

            _redoList.RemoveAt(0);
            _enableAdd = false;

            cmd.Redo();

            _enableAdd = true;
            _undoList.Add(cmd);
        }
    }

    public UndoRedo()
    {
        UndoList = new ReadOnlyObservableCollection<IUndoRedo>(_undoList);
        RedoList = new ReadOnlyObservableCollection<IUndoRedo>(_redoList);
    }
}