About Lowpoly Treasure Chests
- The treasure chests come in three varieties: animated, closed and a version with a loose lid
	- The "Animated" prefab contains an animator controller set up with the provided animations.
	- The "Closed" prefab is a single static mesh. This prefab could be used in cases where the treasure chests do not need to animate or be opened
	- The "Loose_Lid" prefab consists of multiple separate meshes (base, lid, loot). These meshes can be shown/hidden or animated (through code).
- All three treasure chests share the same animations. These animations have been exported as a separate asset.
- All assets share a single material with atlassed textures. There is a albedo/gloss texture and a metallic texture.

/////////////////////////////

About Lowpoly Crates, Barrels & Jars
- Crates and barrels come in three versions:
	- Closed versions consisting of a single mesh
	- Loose lid versions with a lid that can be (re)moved
	- Broken versions that can be used when destroyed (by the player)
- Pottery comes in two versions:
	- Whole versions with an optional cork stopper
	- Cracked versions that can be used when destroyed (by the player)
- In order to destroy props, destroy whole prefabs and replace with broken/cracked version. Make sure to allign the broken/cracked prefabs with the whole ones. Additionally a (explosion) force can be applied to the newly instatiated broken/cracked version. 
- For performance reasons it may be desirable to deactivate/remove physics on broken pieces after X seconds.

/////////////////////////////

About Lowpoly Castle Dungeon Tileset:
- Tileset functions on a 3x3/1.5 unit scale.
- Floor tiles can be vertex snapped, walls should be positioned using precise positions.
- Only items not to be positioned on a grid are the doors, these need to be positioned within their frames.
- Edges (Pref_Wall_Edge, Pref_Ceiling_Edge_Wide, Pref_Ceiling_Edge_Narrow) can be used when the edges of ceilings are visible (as in under balconies).

/////////////////////////////
About Lowpoly medieval Fantasy Weapons
- To parent weapons to character models, add an empty gameobject to the rig (parented to the hand). The gameobject should be positioned in the center of the palm. Ensure that the forward vector is pointing out from the wrist. The up vector should be pointing up along the thumb.
- To parent shields to character models, add an empty gameobject to the rig (parented to the hand).  The game object should be positioned on the outside of the hand. The forward vector should point out from the back of the hand. The up vector should be pointing up along the thumb.