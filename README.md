# ClassicToHybrid

## Aims

Convert the ECS sample TwoStickShooter project 'Classic' into 'Hybrid' using a workflow that could work for other projects.
The assumptions made are based on the existing hybrid project.
### Caveats

The ideal solution would minimize any game breaking changes - need to plan changes accordingly.

### Plan

- Assess current components for suitability in ECS.
- Identify systems to be put in place.
- Rename components and systems if their nature is changed, or if there is a more appropriate name.
- Determine the least impactive systems which can be implemented first.
- Implement these systems first, testing to ensure as little breakage to the game as is possible.

## Practise

### Component Assessment

Begin by looking at the scripts in the Component folder and assessing their Suitability as components in the ECS.

#### Enemy.cs

Contains an update method but no member data - leave a marker class so we can have the Enemy Prefab, move the logic into systems.

#### EnemyShootState.cs, Faction.cs

Contain only data, no changes necessary.

#### Health.cs

Single data value, logic which destroys GameObject when value is below 0 - Keep the value, move the logic into a system.

#### MoveSpeed.cs

Single data value, logic to update the position of a moving object - move logic into a system.

#### Player.cs

Multiple data values,  logic to update input values and act on them - group data accordingly and move logic into systems.

#### Shot.cs

Multiple data values, logic to update position of a shot, destroy upon impact or time out - move logic to systems.

#### Transform2D.cs

Position and Heading values, logic to update the transform - move elsewhere, consider splitting the data.

### Existing 'Systems'

#### EnemySpawnSystem.cs

Contains state data which should be moved into a component. - This lines up with the EnemySpawnSystemState script in the hybrid project.

#### ShotSystem

Spawn shots - should be renamed the ShotSpawnSystem as this lines up with the static class it contains.

### Additional Systems needed

Based on the existing logic in the components, we need systems to: 

1. Move game objects.
2. Remove dead gameobjects
3. Apply Damage
4. Take input.
5. Move the player
6. Update transforms.
7. Tell the enemies when to shoot
8. Destroy Shots

These match to the following scripts in the Hybrid Project.
1. MoveSystem.cs
2. RemoveDeadSystem.cs
3. DamageSystem.cs
4. PlayerInputSystem.cs
5. PlayerMoveSystem.cs
6. SyncTransformSystem.cs
7. EnemyShootSystem.cs
8. ShotDestroySystem.cs

### Easy changes to make.

#### Rename any files.

ShotSystem -> ShotSpawnSystem
Player -> PlayerInput - make sure to rename all references to Player.

#### Add files to be used later

Create the following files but leave them empty for now.

- MoveSystem.cs
- RemoveDeadSystem.cs
- DamageSystem.cs
- PlayerInputSystem.cs
- PlayerMoveSystem.cs
- SyncTransformSystem.cs
- EnemyShootSystem.cs
- ShotDestroySystem.cs

In the components folder
- Position2D.cs
- Heading2D.cs
- EnemySpawnSystemState.cs

Note that this is not how we would normally do this,

#### Allow the ECS to see game objects

Reading through the online documentation, it states that you need to add a game object entity script to all game objects needed to be seen be the ECS.

To do this, in the editor, select a prefab, click 'Add Component' and select Scripts/Unity.Entities/Game Object Entity.
Do this for the following.
- Enemy
- EnemyShot
- Player
- PlayerShot

Check that your game still works, and continue.

### First ECS system, RemoveDeadSystem.cs

In the component classes, several of the classes still contain logic we need to remove and put into systems. One of the smallest examples of this is in Health.cs where the code checks to see if the object has health less than or equal to 0. If so then destroy the game object.

There are other reasons a game object could be considered 'dead', but we will worry about them after solving this one.

#### RemoveDeadSystem.cs 0.1

```C#
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace TwoStickClassicExample
{
	// ComponentSystem is a class that can work with both 
	// components and GameObjects*
    public class RemoveDeadSystem : ComponentSystem
    {
		// The component group needed to work with this system
        public struct Entities
        {
			// The Length of the componentgroup**
            public int Length;

			// The Game object associated with the entity
			// Restricts the group to GameObject based Entities.**
            public GameObjectArray gameObjects;

			// Lets us access entities with a health component.**
            public ComponentArray<Health> healths;
        }

		// Component group injection gives us a component group that can be accessed by index
		// eg. entities.health[0]
        [Inject] private Entities entities;

        protected override void OnUpdate()
        {
			// List to hold any GameObject to be destroyed
            var toDestroy = new List<GameObject>();

			// Iterate over all entities matching the declared ComponentGroup required types
            for (int i = 0; i < entities.Length; i++)
            {
			// Logic from Health.cs
                if (entities.healths[i].Value <= 0)
                    toDestroy.Add(entities.gameObjects[i]);
            }

			// Destroy all objects that we need to
            foreach (var go in toDestroy)
            {
                Object.Destroy(go);
            }
        }
    }
}
```
* See https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/Documentation/content/getting_started.md
**See https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/Documentation/content/ecs_in_detail.md

Comment out the logic in Health.cs so you are left with a class that inherits from Monobehaviour and has a public float variable called Value. Check that your game still works. If it does, delete the commented out code and move on to the next task.

#### Finishing RemoveDeadSystem.cs

We need to find and include the other factors which kill gameObjects. Going out of bounds results in death, as does the death of the player. Out of bounds whilst addressed the Enemy.cs logic, doesn't appear to be a factor in the hybrid project, but we will attempt to implement it anyway.

Add a PlayerCheck struct.
```C#
struct PlayerCheck
{
    public int Length;
    [ReadOnly] public ComponentArray<PlayerInput> PlayerInput;
}

[Inject] private PlayerCheck m_PlayerCheck;
```
Modify the OnUpdate method body

```C#
protected override void OnUpdate()
{
// See if we have any players
var playerDead = m_PlayerCheck.Length == 0;

// List to hold any GameObject to be destroyed
var toDestroy = new List<GameObject>();

// Out of Bounds Checking
var settings = TwoStickBootstrap.Settings;
var minY = settings.playfield.yMin;
var maxY = settings.playfield.yMax;

// Iterate over all entities matching the declared ComponentGroup required types
for (int i = 0; i < entities.Length; i++)
{
    var position = entities.gameObjects[i].GetComponent<Transform2D>().Position;
    // Logic from Health.cs and Enemy.cs
    if (entities.healths[i].Value <= 0 || playerDead || position.y > maxY || position.y < minY)
	...
```

### Next system SyncTransformSystem.cs
We are going to create a system to synchronise the data in Tranform2D with the gameobject transform.

#### SyncTransformSystem.cs 0.1

```C#
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TwoStickClassicExample
{
    public class SyncTransformSystem : ComponentSystem
    {
        public struct Data
        {
            [ReadOnly] public Transform2D Transform;
            public Transform Output;
        }

        protected override void OnUpdate()
        {
            foreach (var entity in GetEntities<Data>())
            {
                float2 p = entity.Transform.Position;
                float2 h = entity.Transform.Heading;
                entity.Output.position = new float3(p.x, 0, p.y);
                if (!h.Equals(new float2(0f, 0f)))
                    entity.Output.rotation = Quaternion.LookRotation(new float3(h.x, 0f, h.y), new float3(0f, 1f, 0f));
            }
        }
    }
}
```
Comment out the update code in Tranform2D and run the program to make sure it works. We will revisit this later when we include Position2d and Heading2D.

### Player Systems

#### PlayerInputSystem.cs

```C#
using Unity.Entities;
using UnityEngine;

namespace TwoStickClassicExample
{
    public class PlayerInputSystem : ComponentSystem
    {
        struct PlayerData
        {
            public PlayerInput Input;
        }
        // No need for code injection when simply looping through entities

        protected override void OnUpdate()
        {   
            // Get Time.deltaTime outside of the loop to prevent multiple calls
            float dt = Time.deltaTime;

            // GetEntities<T>() gives us the ComponentGroup matching the components
            // in struct 'T'
            foreach (var entity in GetEntities<PlayerData>())
            {
                var pi = entity.Input;

                // From PlayerInput's update method
                pi.Move.x = Input.GetAxis("Horizontal");
                pi.Move.y = Input.GetAxis("Vertical");
                pi.Shoot.x = Input.GetAxis("ShootX");
                pi.Shoot.y = Input.GetAxis("ShootY");
                
                pi.FireCooldown = Mathf.Max(0.0f, pi.FireCooldown - dt);
            }
        }
    }
}
```