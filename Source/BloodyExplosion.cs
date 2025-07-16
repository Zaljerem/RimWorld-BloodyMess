using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Bloody_Mess;

[DefOf]
public static class BloodyExplosion
{
	public static DamageDef TD_BloodSplatterDamage;

	public static ThingDef TD_ProjectileBlood;

	public static ThingDef TD_ProjectileMeat;

	public static SoundDef Explosion_Rocket;

	public static SoundDef Hive_Spawn;

	private const float explosionRadius = 2.5f;

	private const float filthChanceBase = 0.5f;

	private const int filthCount = 2;

	private const float propagationSpeed = 0.25f;

	private const int numProjectiles = 4;

	private const int numProjectilesMeat = 1;

	private static List<ThingDefCountClass> potentialProjectileDefs = new List<ThingDefCountClass>();

    [DebugAction("General", null, false, false, false, false, false, 0, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void DoBloodyExplosion(Pawn pawn)
    {
        Map map = pawn.Map;
        IntVec3 position = pawn.Position;
        ThingDef bloodDef = pawn.RaceProps?.BloodDef;
        SoundDef explosionSound = (pawn.RaceProps?.IsFlesh == true ? Hive_Spawn : Explosion_Rocket);
        float num = (Mod.settings.clean ? 0f : 0.5f);
        DamageDef tD_BloodSplatterDamage = TD_BloodSplatterDamage;
        ThingDef preExplosionSpawnThingDef = bloodDef;
        float preExplosionSpawnChance = num;

        //GenExplosion.DoExplosion(position, map, 2.5f, tD_BloodSplatterDamage, null, -1, -1f, explosionSound, null, null, null, bloodDef, num / 2f, 2, null, applyDamageToExplosionCellsNeighbors: false, preExplosionSpawnThingDef, preExplosionSpawnChance, 2, 0f, damageFalloff: false, null, null, null, doVisualEffects: false, 0.25f);

        GenExplosion.DoExplosion(center: position, map: map, radius: 2.5f, damType: tD_BloodSplatterDamage, instigator: null, damAmount: -1, armorPenetration: -1f, explosionSound: explosionSound, weapon: null, projectile: null, intendedTarget: null, postExplosionSpawnThingDef: bloodDef, postExplosionSpawnChance: num / 2f, postExplosionSpawnThingCount: 2, postExplosionGasType: null, preExplosionSpawnThingDef: preExplosionSpawnThingDef, preExplosionSpawnChance: preExplosionSpawnChance, preExplosionSpawnThingCount: 2);

        IntVec3 position2 = pawn.Position;
        Vector3 drawPos = pawn.DrawPos;

        for (int i = 0; i < 4 * ((!Mod.settings.clean) ? 1 : 3); i++)
        {
            if (TD_ProjectileBlood != null && bloodDef != null)
            {
                ProjectileItem obj = (ProjectileItem)GenSpawn.Spawn(TD_ProjectileBlood, position2, map);
                obj?.SetItem(new ThingDefCountClass(bloodDef, 4));
                IntVec3 intVec = position + GenRadial.RadialPattern[Rand.Range(GenRadial.NumCellsInRadius(2.5f * (Mod.settings.clean ? 0.5f : 1.5f)), GenRadial.NumCellsInRadius(6.25f))];
                obj?.Launch(pawn, drawPos, intVec, intVec, ProjectileHitFlags.All);
            }

            if (i >= 1 || TD_ProjectileMeat == null)
                continue;

            ProjectileItem obj2 = (ProjectileItem)GenSpawn.Spawn(TD_ProjectileMeat, position2, map);

            if (pawn.def?.race?.meatDef != null)
            {
                float statValue = pawn.GetStatValue(StatDefOf.MeatAmount);
                if (statValue > 0f)
                {
                    potentialProjectileDefs.Add(new ThingDefCountClass(pawn.def.race.meatDef, (int)statValue));
                }
            }

            if (pawn.def?.race?.leatherDef != null)
            {
                float statValue2 = pawn.GetStatValue(StatDefOf.LeatherAmount);
                if (statValue2 > 0f)
                {
                    potentialProjectileDefs.Add(new ThingDefCountClass(pawn.def.race.leatherDef, (int)statValue2));
                }
            }

            if (pawn.def?.butcherProducts != null)
            {
                potentialProjectileDefs.AddRange(pawn.def.butcherProducts
                    .Where(dc => dc?.thingDef != null)
                    .Select(dc => new ThingDefCountClass(dc.thingDef, dc.count)));
            }

            if (!pawn.RaceProps?.Humanlike ?? false)
            {
                PawnKindLifeStage curKindLifeStage = pawn.ageTracker?.CurKindLifeStage;
                if (curKindLifeStage?.butcherBodyPart != null &&
                    pawn.health?.hediffSet?.GetNotMissingParts()?.Any(part => part.IsInGroup(curKindLifeStage.butcherBodyPart.bodyPartGroup)) == true &&
                    ((pawn.gender == Gender.Male && curKindLifeStage.butcherBodyPart.allowMale) ||
                     (pawn.gender == Gender.Female && curKindLifeStage.butcherBodyPart.allowFemale)))
                {
                    potentialProjectileDefs.Add(new ThingDefCountClass(curKindLifeStage.butcherBodyPart.thing, 1));
                }
            }

            ThingDefCountClass item = potentialProjectileDefs
                .Where(dc => dc != null && dc.thingDef != null)
                .RandomElementByWeightWithFallback(dc => dc.count);

            potentialProjectileDefs.Clear();

            if (item != null && item.thingDef != null)
            {
                obj2?.SetItem(item);
                IntVec3 intVec = position + GenRadial.RadialPattern[Rand.Range(GenRadial.NumCellsInRadius(2.5f * (Mod.settings.clean ? 0.5f : 1.5f)), GenRadial.NumCellsInRadius(6.25f))];
                obj2?.Launch(pawn, drawPos, intVec, intVec, ProjectileHitFlags.All);
            }
        }
    }

}
