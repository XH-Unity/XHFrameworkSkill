using System.Collections.Generic;
using UnityEngine;

public class UnitManager
{
    private static UnitManager _instance;
    public static UnitManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new UnitManager();
            return _instance;
        }
    }

    private readonly List<Unit> _units = new List<Unit>();

    public IReadOnlyList<Unit> Units => _units;

    public void Register(Unit unit)
    {
        if (!_units.Contains(unit))
            _units.Add(unit);
    }

    public void Unregister(Unit unit)
    {
        _units.Remove(unit);
    }

    public T GetUnit<T>() where T : Unit
    {
        foreach (var unit in _units)
        {
            if (unit is T t)
                return t;
        }
        return null;
    }

    public List<T> GetUnits<T>() where T : Unit
    {
        var result = new List<T>();
        foreach (var unit in _units)
        {
            if (unit is T t)
                result.Add(t);
        }
        return result;
    }
}
