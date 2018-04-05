# Using ECS On Existing Unity Projects

## Introduction

In this project I will show you a way of progressively integrating Unity's ECS in existing projects, using the ECS TwinStickShooter Sample projects for demonstration.

In the sample projects for the ECS, Unity include three versions of a TwinStickShooter project.

- Classic: How the project would be implemented without ECS.
- Hybrid: Make use of ECS systems while holding on to GameObjects etc.
- Pure: Makes full use of ECS, so no GameObjects in sight.

These projects are great as examples of what might be achieved, but they do very little to show how a developer might implement use of the ECS in an existing project.

## Requirements

- You need to be using at least Unity 2018.1.0b12 to work with the ECS.
- [Unity's Sample projects for the ECS](https://github.com/Unity-Technologies/EntityComponentSystemSamples)

## What We Will Do

- We are going to take the 'Classic' TwinStickShooter project use the 'Hybrid' project as a goal to work towards.
- We will do this in steps, keeping the game working after each step.

## How We Will Do This

- Examine the 'Classic' Project to identify the existing components and systems, their behaviours, and how they relate to the GameObjects.
- Design the systems we will use to replace the behaviours.
- Create and implement the systems one at a time. Ensuring we don't break the game at each step.

## The Classic Project

### Scripts

/Assets/GameCode

- EnemySpawnSystem.cs
- ShotSystem.cs
- TwoStickBootstrap.cs
- TwoStickExampleSettings.cs
- UpdatePlayerHUD.cs

/Assets/GameCode/Components

- Enemy.cs
- EnemyShootState.cs
- Faction.cs
- Health.cs
- MoveSpeed.cs
- Player.cs
- Shot.cs
- Transform2D.cs

[A more in depth look at the Scripts](./script_details.md)

### Prefabs

- Enemy
- EnemyFaction
- EnemyShot
- Player
- PlayerFaction
- PlayerShot

Looking at each prefab, we can make a table to show the common components amongst the prefabs.

| Component             | Enemy | EnemyFaction | Enemy Shot | Player | PlayerFaction | PlayerShot |
| --------------------- |:-----:|:------------:|:----------:|:------:|:-------------:|:----------:|
| Transform             |x      |x             |x           |x       |x              |x           |
| MeshRenderer          |x      |              |x           |x       |               |x           |
| Mesh_Filter           |x      |              |x           |x       |               |x           |
| Enemy                 |x      |              |            |        |               |            |
| EnemyShootState       |x      |              |            |        |               |            |
| Faction               |x      |x             |x           |x       |x              |x           |
| Health                |x      |              |            |x       |               |            |
| MoveSpeed             |x      |              |x           |        |               |x           |
| Transform2D           |x      |              |x           |x       |               |x           |
| Shot                  |       |              |x           |        |               |x           |
| Player                |       |              |            |x       |               |            |

### Scene Objects

- Main Camera
- Directional Light
- Settings - TwoStickExampleSettings.cs - default values for numerical data, prefab values need to be linked to the appropriate prefab.
- EnemySpawner - EnemySpawnSystem.cs
- StarField
  - part_starfield
  - part_starfield_distant
- Canvas
  - HealthText
  - NewGameButton
    - Text
- EventSystem
- HUD - UpdatePlayerHUD.cs data members linked to UI objects in Canvas.

## System Design

The Systems used in the hybrid project follow a rough template.

```C#
using Unity.Entities;  // Gives access to the ECS
using UnityEngine;

public class MySystem : ComponentSystem
{
    // One or more Structs of required components
    struct Data
    {
        // The required Components
    }

    // Update to be run on all matching Entities.
    protected override void OnUpdate()
    {
        // The Behavior
    }
}
```

### Required Component Structs

We can declare these simply as a [list of the components needed by the system and access them via the GetEntities method](https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/Documentation/content/getting_started.md#componentsystem---a-step-into-a-new-era), or we can [inject the data into a Component Group which can be iterated over to access the required Component types](https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/Documentation/content/ecs_in_detail.md#component-group-injection).

## Identifying the Systems

As we are working towards the Hybrid Project, it makes sense to get the list of Systems from there. Obviously this can't be done for other projects, in those cases I would suggest that you create Systems by identifying the behaviours as we did in [the depth look at the Scripts](./script_details.md) and determine which behaviours you want to keep together, and which ones you want to split into seperate Systems.

From the Hybrid Project we can see that the systems are:

- DamageSystem
- EnemyShootSystem
- EnemySpawnSystem
- MoveSystem
- PlayerInputSystem
- PlayerMoveSystem
- RemoveDeadSystem
- ShotDestroySystem
- ShotSpawnSystem
- SyncTransformSystem
- UpdatePlayerHUD

We can also see some other noteworthy changes to the project:

- There are additional component classes
  - EnemySpawnSystemState - The data previously in EnemySpawnSystem.cs.
  - Position2D - replaces the Position value from Transform2D.
  - Heading2D - replaces the Heading value from Transform2D.
- Transform2D has been removed.
- The following have been renamed:
  - Player - PlayerInput.
  - ShotSystem - ShotSpawnSystem.
- There is an additional EnemySpawnState prefab.

Now we have a target to get to, let's get started.

## Getting Started

Before making any systems, the easiest change to make is to rename the files as above. Then we can create the files EnemySpawnSystemState.cs, Position2D.cs and Heading2D.cs. Position2D and Heading2D should just contain a float2 called Value. Attach both Position2D and Heading2D to all gameObjects which have the Transform2D component.

Next we want get ready to remove Transform2D.

### SyncTransformSystem.cs

We know that Transform2D is responsible for updating the transform.position and transform.rotation of any gameObjects it is attached to, so before we can get rid of it, we need to replace the behaviour. We will do this in our first system, SyncTransformSystem.

Look at the behaviour in Transform2D

```C#
transform.position = new float3(Position.x, 0, Position.y);
transform.rotation = Quaternion.LookRotation(new Vector3(Heading.x, 0f, Heading.y), Vector3.up);
```

In this we need access to the transform component, and the Transform2D component, so the required Components struct will look like this.

```C#
// The required Component struct: Data
public struct Data
{
    // The Transform2D Component, declared as ReadOnly as it will not be mutated.
    [ReadOnly] public Transform2D FromTransform;
    // The transform Component.
    public Transform Output;
}
```

The behaviour will be in a method called OnUpdate.

```C#
protected override void OnUpdate()
{
    // Perform the behaviour for all entities that have the required Components.
    foreach (var entity in GetEntities<Data>())
    {
        // Access the components via entity."Component Name"
        float2 p = entity.FromTransform.Position;
        float2 h = entity.FromTransform.Heading;

        //transform.position = new float3(Position.x, 0, Position.y);
        entity.Output.position = new float3(p.x, 0, p.y);

        //transform.rotation = Quaternion.LookRotation(new Vector3(Heading.x, 0f, Heading.y), Vector3.up);

        // Only Apply if there is a heading input
        if (!h.Equals(new float2(0f, 0f)))
            entity.Output.rotation = Quaternion.LookRotation(new float3(h.x, 0f, h.y), new float3(0f, 1f, 0f));
    }
}
```

As you can see, I first copied in the behaviour from Transform2D, then comment it out and replicate for the current context. You can also note that references to Vector3 are replaced with float3.

The completed file.

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
            [ReadOnly] public Transform2D FromTransform;
            public Transform Output;
        }

        protected override void OnUpdate()
        {
            foreach (var entity in GetEntities<Data>())
            {

                float2 p = entity.FromTransform.Position;
                float2 h = entity.FromTransform.Heading;
                entity.Output.position = new float3(p.x, 0, p.y);
                if (!h.Equals(new float2(0f, 0f)))
                    entity.Output.rotation = Quaternion.LookRotation(new float3(h.x, 0f, h.y), new float3(0f, 1f, 0f));
            }
        }
    }

}
```

Test this by comment out the LateUpdate function in Transform2D. If the game still works, then you know that your first component system is now up and running.

Following this, I substituted all references to Transform2D  with Position2D and Heading2D. Tested the build, then removed the Transform2D Component from all prefabs. Tested the build once more and finally deleted Transform2.cs.

## The Workflow

In implementing this first System, we have a potential workflow to use for the remaining systems.

- Identify the behaviour(s).
- Create the required component groups.
- Copy behaviour into the System and adapt as necessary.
- Comment out source behaviour to test. Delete when successful.
- Push the successful build to VCS.

## Moving Forward

If you want to challenge yourself then I would stop reading now and go and see if this workflow works for you.

I [documented the process I followed in creating the systems](./SystemDesign.md), but I thought it might be useful to highlight some things I found interesting along the way.