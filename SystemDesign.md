# Designing the systems for the Hybrid Project

## SyncTransformSystem

Look at the behaviour in Transform2D

```C#
transform.position = new float3(Position.x, 0, Position.y);
transform.rotation = Quaternion.LookRotation(new Vector3(Heading.x, 0f, Heading.y), Vector3.up);
```

So we need access to the Transform Component, to get Transform.position and Transform.rotation, and we need access to the Transform2D, to get the Position, and Heading.

So in this case, Transform2D is the Identifying component, and Transform is the non-identifying component.

The system to perform this is called SyncTransformSystem in the hybrid project. So we will do the same.

### SyncTransformSystem.cs

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

Following this, I substituted all references to Transform Position2D and Heading2D. Tested the build, then removed the Transform2D Component from all prefabs. Tested the build once more and finally deleted Transform2.cs.

### Player Input and Move Systems

Looking at Player.cs, the behaviour in the Update function can be split into two sections, one to update the input values, one to act on these input values. Looking at the hybrid project, we can see that these systems are PlayerMoveSystem.cs and PlayerInputSystem.cs.

#### Player Input

This system only needs to access the data stored in the Player. So the component group is as follows.

```C#
struct PlayerData
{
    public Player Input;
}
```

The behaviour can almost be copied directly.

```C#
Move.x = Input.GetAxis("Horizontal");
Move.y = Input.GetAxis("Vertical");
Shoot.x = Input.GetAxis("ShootX");
Shoot.y = Input.GetAxis("ShootY");

FireCooldown = Mathf.Max(0.0f, FireCooldown - Time.deltaTime);
```

Put this inside the OnUpdate function, add a doreach loop with a call to GetEntities of type, PlayerData, and you get the following.

```C#
protected override void OnUpdate()
{
    float dt = Time.deltaTime;

    foreach (var entity in GetEntities<PlayerData>())
    {
        var pi = entity.Input;

        pi.Move.x = Input.GetAxis("Horizontal");
        pi.Move.y = Input.GetAxis("Vertical");
        pi.Shoot.x = Input.GetAxis("ShootX");
        pi.Shoot.y = Input.GetAxis("ShootY");

        pi.FireCooldown = Mathf.Max(0.0f, pi.FireCooldown - dt);
    }
}
```

To test this, follow the same tried and true formula, comment out the old code, test, then remove the old code.

It makes sense to rename Player.cs to PlayerInput.cs, as that is the only data that will be stored in the file when we are done removing the behaviour.

#### Player Move

The remaining behaviour from PlayerInput.cs (formerly Player.cs).

```C#
var settings = TwoStickBootstrap.Settings;

GetComponent<Position2D>().Value += Time.deltaTime * Move * settings.playerMoveSpeed;

if (Fire)
{
    GetComponent<Heading2D>().Value = math.normalize(Shoot);

    FireCooldown = settings.playerFireCoolDown;
    
    var newShotData = new ShotSpawnData()
    {
        Position = GetComponent<Position2D>().Value,
        Heading = GetComponent<Heading2D>().Value,
        Faction = GetComponent<Faction>()
    };
    
    ShotSpawnSystem.SpawnShot(newShotData);
}
```

So far we have made systems which define and access Enities like this:

```C#
struct EntityData
{
    Component Required1;
    Component Required2;
    ...
}

OnUpdate()
{
    foreach(var entity in GetEntities<EntityData>())
    {
        // Behaviour.
    }
}
```

If we use [Component Injection](https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/Documentation/content/ecs_in_detail.md#component-group-injection) we can access the entity and its components iteratively.

```C#
class MySystem : ComponentSystem
{
    public struct Group
    {
        // Length/Size of the array
        public int Length;

        // ComponentArray lets us access any of the existing class Component
        public ComponentArray<Rigidbody> Rigidbodies;

        // EntityArray give access to the Entity ID.
        public EntityArray Entities;

        // The GameObject Array both lets us retrieve the game object and constrains the group to only contain GameObject based entities.
        public GameObjectArray GameObjects;

        // Excludes entities that contain a MeshCollider from the group
        public SubtractiveComponent<MeshCollider> MeshColliders;
    }

    /*
    SM - Create a ComponentGroup, based on the required Component types of the struct 'Group'. Injects data to its variables before OnCreateManager, OnDestroyManager and OnUpdate are run.
    */
    [Inject] private Group m_Group;

    protected override void OnUpdate()
    {
        // Iterate over all entities matching the declared ComponentGroup required types
        for (int i = 0; i != m_Group.Length; i++)
        {
            m_Group.Rigidbodies[i].position = m_Group.Position[i].Value;

            Entity entity = m_Group.Entities[i];
            GameObject go = m_Group.GameObjects[i];
        }
    }
}
```

Iterating over the entities allows us to access the Components by index.

```C#
...
[Inject] private Group m_Group;

protected override void OnUpdate()
{
    /*
    SM - We can use this similarly to before
    */
    for (int i = 0; i != m_Group.Length; i++)
    {
        m_group.ComponentA[i].doSomething();
        m_group.ComponentB[i].Value = UpdatedValue;
    }

    /*
    SM - One advantage this gives us is an easy way to filter out entities and
    do something with the remaining entities
    */

    var filteredListOfGameObjects = new List<GameObject>();
    float threshold = x;

    for (int i = 0; i != m_Group.Length; i++)
    {
        bool CanProceed = m_group.FilterComponent[i] >= threashold;

        if(CanProceed)
        {
            fiteredListOfGameObjects.Add(m_group.GameObject[i]);
        }
    }

    foreach (var filtered in fiteredListOfGameObjects)
    {
        filtered.DoSomething();
    }
}
```

How is this relevant to the PlayerMoveSystem? In the current behaviour in PlayerInput, if 'Fire' is true then we go through the process of firing a shot.

Applying this to the system PlayerMoveSystem.

```C#
public struct Data
{
    public int Length;

    /*
    SM - Allows us to create and add to a list of FiringPlayers
    */
    public GameObjectArray GameObject;

    /*
    SM - Player Components that we need to access. The Player Input
    component means this system operates only on players.
    */
    public ComponentArray<Position2D> Position;
    public ComponentArray<Heading2D> Heading;
    public ComponentArray<PlayerInput> Input;
}

// Inject the above into the array m_Data.
[Inject] private Data m_Data;
```

Copy the remaining behaviour from PlayerInput.
```C#
var settings = TwoStickBootstrap.Settings;

GetComponent<Position2D>().Value += Time.deltaTime * Move * settings.playerMoveSpeed;

if (Fire)
{
    GetComponent<Heading2D>().Value = math.normalize(Shoot);

    FireCooldown = settings.playerFireCoolDown;
    
    var newShotData = new ShotSpawnData()
    {
        Position = GetComponent<Position2D>().Value,
        Heading = GetComponent<Heading2D>().Value,
        Faction = GetComponent<Faction>()
    };
    
    ShotSpawnSystem.SpawnShot(newShotData);
}
```

Modify this to work in the system.

```C#
/*
SM -Including Length gives a convenient way to check if
we need to continue.
*/
if (m_Data.Length == 0)
    return;

var settings = TwoStickBootstrap.Settings;

/*
SM - Moving Time.deltaTime outside of loops is more efficient,
and means all items in the loop receive the same value for deltaTime.
*/
float dt = Time.deltaTime;
var firingPlayers = new List<GameObject>();

for (int index = 0; index < m_Data.Length; ++index)
{
    var position = m_Data.Position[index];
    var heading = m_Data.Heading[index];

    var playerInput = m_Data.Input[index];

    //GetComponent<Position2D>().Value += Time.deltaTime * Move * settings.playerMoveSpeed;
    position.Value += dt * playerInput.Move * settings.playerMoveSpeed;

    //if(Fire)
    if (playerInput.Fire)
    {
        // GetComponent<Heading2D>().Value = math.normalize(Shoot);
        heading.Value = math.normalize(playerInput.Shoot);

        // FireCooldown = settings.playerFireCoolDown;
        playerInput.FireCooldown = settings.playerFireCoolDown;

        firingPlayers.Add(m_Data.GameObject[index]);
    }
}

foreach (var player in firingPlayers)
{
    var newShotData = new ShotSpawnData()
    {
        Position = player.GetComponent<Position2D>().Value,
        Heading = player.GetComponent<Heading2D>().Value,
        Faction = player.GetComponent<Faction>()
    };

    ShotSpawnSystem.SpawnShot(newShotData);
}
```

As you can see, very little had to be changed from the original code.

Test this by commenting out the remaining behaviour in the PlayerInput file. If working, delete and continue.

### UpdatePlayerHUD

Looking at the Hybrid project, we can see that there is no longer an object called HUD listed in the scene hierarchy, but there is still a HUD in the game. How does this work?

UpdatePlayerHUD has been turned into a Component System. So let's design our own to see how it compares.

What does the system need?

- A button called NewGameButton.
- A Text value HealthText.
- A float to cache the health.
- A way of assigning these to in game objects.
- Access to player data.

```C#
public struct PlayerData
{
    public int Length;
    public EntityArray Entity;
    public ComponentArray<PlayerInput> Input;
    public ComponentArray<Health> Health;
}

[Inject] PlayerData m_Players;
```

Now alter the behaviour to make this a system.

```C#
protected override void OnUpdate()
{
    //if (player != null)
    if(m_Players.Length > 0)
    {
        UpdateAlive();
    }
    else
    {
        UpdateDead();
    }
}

private void UpdateDead()
{
    if (HealthText != null)
    {
        HealthText.gameObject.SetActive(false);
    }
    if (NewGameButton != null)
    {
        NewGameButton.gameObject.SetActive(true);
    }
}

//private void UpdateAlive(PlayerInput playerInput)
private void UpdateAlive()
{
    HealthText.gameObject.SetActive(true);
    NewGameButton.gameObject.SetActive(false);
    
    //var displayedHealth = 0;
    //if (playerInput != null)
    //{
    //  displayedHealth = (int) playerInput.GetComponent<Health>().Value;
    //}
    
    /*
        SM - no need to check playerInput, would not be able to
        get here if it was null.
        */
    int displayedHealth = (int) m_Players.Health[0].Value;

    if (m_CachedHealth != displayedHealth)
    {
        if (displayedHealth > 0)
            HealthText.text = $"HEALTH: {displayedHealth}";
        // else
        //    HealthText.text = "GAME OVER";
        /*
            SM - The game immediately returns to the start screen,
            therefore it is unnecessary to display "GAME OVER".
            */
        m_CachedHealth = displayedHealth;
    }
}
```

Trying this results in a warning in the Console.

> The class named 'TwoStickClassicExample.UpdatePlayerHUD' is not derived from MonoBehaviour or ScriptableObject!

And when we run the project, clicking the new game button does nothing.

Hmm, well, this could be because there is still a HUD object in the Hierarchy.

Go to the hierarchy and click on 'HUD', there should be an error saying the following.

> The associated script can not be loaded. Please fix any compile errors and assign a valid script.

The Hybrid project does not have a 'HUD' in the hierarchy, maybe removing that will solve things.

OK the console is no longer showing me any warnings or errors, but still no functionality.

Looking at the Hybrid project, we need to rename 'start' to 'SetupComponentData' and call the function from the Bootstrap file.

```C#
World.Active.GetOrCreateManager<UpdatePlayerHUD>().SetupGameObjects();
```

By default, [a single 'World'](World.Active.GetOrCreateManager<UpdatePlayerHUD>().SetupGameObjects();) is created upon entering Play mode, and populated with all available systems in the project. This means that we can use this in the InitializeWithScene method to setup our HUD.

### EnemySpawnSystem

The last system we will look at in detail is EnemySpawnSystem, after this, the remaining systems are just more repetetions of what we have already done:

- Define the component struct.
- Copy portions of behaviour data into OnUpdate().
- Comment out said behaviour data.
- Test.
- Delete previous data if successful and move on.

EnemySpawnSystem is different in that there is currently an object in the scene called EnemySpawner, which needs to be there for us to have a location to spawn our enemies from.

We previously made a copy of the data in EnemySpawnSystem.cs called EnemySpawnSystemState.cs, but have yet to do anything with it.

Start by creating a new item in the heirarchy and dragging this script onto it. Name this EnemySpawnState, and create a prefab of it by dragging it into the Prefabs folder.

Delete the object from the scene, and reset the transform data in the prefab. Drag this prefab into the heirarchy, and add the game object entity component to it.

To allow the game to reference this, add the following to the settings class
>public EnemySpawnSystemState EnemySpawnState;

Back in the editor, click on 'Settings' and drag the EnemySpawnState object in the heirarchy into the corresponding value in the inspector.

Check that none of these preparations have broken anything. Save everything, and continue.

The nice thing about this file is that not much needs to be changed to make it into a System

```C#
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TwoStickClassicExample
{
    // Spawns new enemies.
    // public class EnemySpawnSystem : MonoBehaviour
    public class EnemySpawnSystem : ComponentSystem
    {
        //public int SpawnedEnemyCount;
        //public float Cooldown;
        //public Random.State RandomState;

        public struct State
        {
            public int Length;
            public ComponentArray<EnemySpawnSystemState> S;
        }

        [Inject] private State m_State;

        //void Start()
        public static void SetupComponentData()
        {
            var oldState = Random.state;
            Random.InitState(0xaf77);

            var state = TwoStickBootstrap.Settings.EnemySpawnState;

            //Cooldown = 0.0f;
            state.Cooldown = 0.0f;

            //SpawnedEnemyCount = 0;
            state.SpawnedEnemyCount = 0;

            //RandomState = Random.state;
            state.RandomState = Random.state;

            Random.state = oldState;
        }

        // protected void Update()
        protected override void OnUpdate()
        {
            var state = m_State.S[0];

            var oldState = Random.state;
            
            //Random.state = RandomState;
            Random.state = state.RandomState;

            //Cooldown -= Time.deltaTime;
            state.Cooldown -= Time.deltaTime;

            //if (Cooldown <= 0.0f)
            if (state.Cooldown <= 0.0f)
            {
                var settings = TwoStickBootstrap.Settings;
                var enemy = Object.Instantiate(settings.EnemyPrefab);

                ComputeSpawnLocation(enemy);

                //SpawnedEnemyCount++;
                state.SpawnedEnemyCount++;

                //Cooldown = ComputeCooldown(SpawnedEnemyCount);
                state.Cooldown = ComputeCooldown(state.SpawnedEnemyCount);

            }

            //RandomState = Random.state;
            state.RandomState = Random.state;
            Random.state = oldState;
        }

        private float ComputeCooldown(int stateSpawnedEnemyCount)
        {
            return 0.15f;
        }

        private void ComputeSpawnLocation(GameObject enemy)
        {
            var settings = TwoStickBootstrap.Settings;
            
            float r = Random.value;
            float x0 = settings.playfield.xMin;
            float x1 = settings.playfield.xMax;
            float x = x0 + (x1 - x0) * r;

            enemy.GetComponent<Position2D>().Value = new float2(x, settings.playfield.yMax);
            enemy.GetComponent<Heading2D>().Value = new float2(0, -TwoStickBootstrap.Settings.enemySpeed);
        }
    }

}
```

Similarly to UpdatePlayerHUD, we need to call SetupComponentData from outside the system. However, as this is a static function in this system, we can call this by adding EnemySpawnSystem.SetupComponentData() in the same place we do for the UpdatePlayerHUD system.

### And the rest?

Creating the other systems doesn't require anything we have not done already in the Systems so far. I don't think it necessarily matters which systems you implement first, but I did as follows:

MoveSystem

- Create MoveSystem.cs
- Copy Behaviour from MoveSpeed to MoveSystem
- In MoveSpeed.cs rename Speed to Value.
  - Re-enter movespeed values into prefabs: Enemy(1), EnemyShot(42), PlayerShot(60).
- Delete behaviour from MoveSpeed.cs.

ShotSpawnSystem

- Rename ShotSystem to ShotSpawnSystem.

ShotDestroySystem

- Create ShotDestroySystem.cs
- Create 2 component groups, one for shotData and one for playerInput. Inject used for playerInput.
- Copy behaviour from Shot.cs which destroys the shot if player is dead.
- Test.
- Remove the behaviour we copied from Shot.cs.

RemoveDeadSystem

- Create RemoveDeadSystem.cs
- Create and inject into component groups for entities with a health component, and entities with a PlayerInput Component.
- Copy behaviour from Health.cs, to RemoveDeadSystem.cs, which destroys object if health<=0.
- Re-enter Health values into Player(100) and Enemy(1) prefabs.
- Copy behaviour from Enemy.cs, to RemoveDeadSystem.cs, which destroys object if there is no player.
- Remove the copied behaviour from Enemy.cs and Health.cs.

EnemyShootSystem

- Create EnemyShootSystem.cs
- Create and inject into component groups for entities with an EnemyShootState component, and entities with a PlayerInput Component, both groups need access to the Position2D component also.
- Copy Shooting behaviour from Enemy.cs
- Test.
- Delete the behaviour.

DamageSystem

- Create the system PlayerDamageSystem in the file DamageSystem.cs
- Create and inject into component groups for ReceiverData and ShotData
- Copy GetCollisionRadius from Shot.cs
- Get Behaviour from Shot.cs.
- Test.
- Remove Update() and GetCollisionRadius from Shot.cs.
