using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Bloody_Mess;

public class DamageWorker_BloodSplatter : DamageWorker
{
    public override void ExplosionAffectCell(Explosion explosion, IntVec3 cell, List<Thing> damagedThings, List<Thing> ignoredThings, bool canThrowMotes)
    {
        Map map = explosion.Map;
        if (!cell.ShouldSpawnMotesAt(map)) return;

        float num = Mathf.Clamp01((explosion.Position - cell).LengthHorizontal / explosion.radius);

        Color value = Color.red; // Default blood color fallback
        if (explosion.preExplosionSpawnThingDef != null)
        {
            if (explosion.preExplosionSpawnThingDef == ThingDefOf.Filth_MachineBits)
            {
                value = Color.grey;
            }
            else if (explosion.preExplosionSpawnThingDef.graphicData != null)
            {
                value = explosion.preExplosionSpawnThingDef.graphicData.color;
            }
        }

        value.a = 1f - num;

        FleckCreationData dataStatic = FleckMaker.GetDataStatic(cell.ToVector3Shifted(), map, def.explosionCellFleck);
        dataStatic.rotation = Rand.Range(0, 360);
        dataStatic.instanceColor = value;
        map.flecks.CreateFleck(dataStatic);
    }

}
