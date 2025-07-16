using RimWorld;
using UnityEngine;
using Verse;

namespace Bloody_Mess;

public class ProjectileItem : Projectile
{
	private ThingDef itemDef;

	private int itemCount;

	private Material itemMat;

	private Mesh mesh;

	private float rotSpeed;

    public void SetItem(ThingDefCountClass defCount)
    {
        if (defCount == null)
        {
            defCount = new ThingDefCountClass(ThingDefOf.Gold, 100);
        }

        itemDef = defCount.thingDef;
        itemCount = Mathf.CeilToInt((float)defCount.count * Mod.settings.meatPercent);
        mesh = MeshPool.GridPlane(new Vector2(itemDef.size.x, itemDef.size.z));

        itemMat = (itemDef.graphic is Graphic_StackCount graphic_StackCount)
            ? graphic_StackCount.SubGraphicForStackCount(itemCount, itemDef).MatSingle
            : itemDef.DrawMatSingle;

        rotSpeed = Rand.Range(180, 720);
    }


    public override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		float num = base.ArcHeightFactor * GenMath.InverseParabola(base.DistanceCoveredFraction);
		Vector3 position = drawLoc + new Vector3(0f, 0f, 1f) * num;
		if (def.projectile.shadowSize > 0f)
		{
			DrawShadow(drawLoc, num);
		}
		Graphics.DrawMesh(mesh, position, Quaternion.AngleAxis(rotSpeed * base.DistanceCoveredFraction, Vector3.up), itemMat, 0);
		Comps_PostDraw();
	}

    public override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        if (itemCount > 0 && itemDef != null)
        {
            if (base.Map == null)
            {
                Verse.Log.Warning($"[Bloody_Mess] ProjectileItem.Impact() called with null Map. Pos: {base.Position}, Def: {itemDef.defName}");
                return;
            }

            if (itemDef.IsFilth)
            {
                if (!Mod.settings.clean)
                {
                    FilthMaker.TryMakeFilth(base.Position, base.Map, itemDef, itemCount);
                }
            }
            else
            {
                Thing thing = ThingMaker.MakeThing(itemDef);
                thing.stackCount = itemCount;
                if (!GenPlace.TryPlaceThing(thing, base.Position, base.Map, ThingPlaceMode.Near))
                {
                    thing.Destroy();
                }
                else
                {
                    thing.SetForbidden(true); // <-- Prevent colonist interaction
                }
            }
        }
        else if (itemDef == null)
        {
            Verse.Log.Warning("[Bloody_Mess] ProjectileItem.Impact() skipped: itemDef is null.");
        }

        base.Impact(hitThing, blockedByShield);
    }


}
