using System;
using System.Collections.Generic;
using K1.Gameplay;


public class UnitModule : KModule
{
    private List<UnitBase> _units = new();
    private List<UnitBase> logicUnits = new();

    private List<UnitBase> _toRemoveUnits = new();
    private List<UnitBase> _toAddUnits = new();

    public Action<UnitBase> OnAddUnit;
    public Action<UnitBase> OnRemoveUnit;
    
    public List<UnitBase> UnitList => _units;
    public List<UnitBase> LogicUnitList => _units;
    
    public void RegisterGameUnit(UnitBase unit, bool enable)
    {
        if (enable)
        {
            _toAddUnits.Add(unit);
        }
        else
        {
            _toRemoveUnits.Add(unit);
        }
    }
    public void ManualPreLogic()
    {
        foreach (var unit in _toAddUnits)
        {
            unit.OnUnitActive();
            _units.Add(unit);
            if (unit.EnableOnLogic)
                logicUnits.Add(unit);
        }

        _toAddUnits.Clear();
        foreach (var unit in _toRemoveUnits)
        {
            unit.OnUnitInactive();
            _units.Remove(unit);
            if (unit.EnableOnLogic)
                logicUnits.Remove(unit);
        }
        _toRemoveUnits.Clear();
    }
    
    public void ManualLogic()
    {
        foreach (var unit in logicUnits)
        {
            unit.OnLogic();
        }
    }
    
}