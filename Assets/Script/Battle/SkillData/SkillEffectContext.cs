using System.Collections.Generic;

public class SkillEffectContext
{
    public Unit Caster;
    public Unit MainTarget;
    public List<GridCell> TargetGrids;
    public List<Unit> HitUnits;
}