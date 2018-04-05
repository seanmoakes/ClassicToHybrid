# Script Details

A look at the data and behaviour of the scripts in the 'Classic Project'.

## Enemy.cs

There is no data stored in the Enemy class.

Behaviours:

1. Destroy the gameObject if there is no Player in the scene.
2. Set Health component to -1 if the gameObject's Position is not within the bounds defined in Settings.
3. If possible, fire a shot by instantiating a 'Shot' at the current position, headed towards the Player's current position.

## EnemyShootState.cs

Data:

1. Cooldown - a float to show how long an enemy needs to wait before they can shoot again.

There is no behaviour in this file.

## EnemySpawnSystem.cs

Data:

1. SpawnedEnemyCount - An int counting the number of Spawned Enemies.
2. Cooldown - a float to show how long before another Enemy can be spawned.
3. RandomState - A Random.State value to hold the current state of the Random Number Generator.

Behaviours:

Start

1. Set initial values for the Data elements
2. Copy the current RNG state. Call initState for the value '0xaf77' and store the resultant state in 'RandomState', copy the initial value back into Random.state.

Update

1. RNG Copy as in Start.
2. Decrease Cooldown by Time.deltaTime.
3. If Cooldown <= 0
    - Spawn a new enemy
    - Set enemy positition using ComputeSpawnLocation
    - Increase SpawnedEnemyCount.
    - Reset Cooldown using ComputeCooldown.
4. Reset RNG state.

ComputeCooldown

1. Return a value to be used in resetting 'Cooldown'

ComputeSpawnLocation

1. Set the transform position for an enemy to be a random point on the line y = settings.playfield.yMax, between x = settings.playfield.xMin and settings.playfield.xMax.

## Faction.cs

Data:

1. Type - An enum with values:
    - Enemy = 0
    - Player = 1
2. Value - A 'Type' variable.

There is no behaviour in this class/file.

## Health.cs

Data:

1. m_Value - a private float.
2. Value - a float property.
    - get - returns m_Value;
    - set - Assign a value to m_Value, if this is <= 0 then destroy the GameObject.

## MoveSpeed.cs

Data:

1. Speed - A float.

Behaviours:

1. Move Transform2D.Position in the direction of Transform2D.Heading by an amount based on the Speed value and Time.delaTime.

## Player.cs

Data:

1. Move - A float2 to hold the Move input values.
2. Shoot - A float2 to hold the Shoot input values.
3. FireCooldown - A float for how long before we can shoot again.
4. Fire - An auto-property bool determined by FireCooldown <=0 and math.length(Shoot).

Behaviours:

1. Assign values to Move, Shoot and FireCooldown from Inputs and time passed.
2. Update the player's Transform2D component.
3. If Fire is true - fire a shot by instantiating a 'Shot' at the current position, in the direction indicated by the Shoot axes.

## Shot.cs

Data:

1. TimeToLive - A float to determine the time a Shot has before it will be destroyed.
2. Energy - How much damage the Shot inflicts upon hitting a target.

Behaviours:

Update

1. Destroy the gameObject if there are no Health objects in the scene.
2. Get the faction of the gameObject.
3. For all Health Objects in the scene.
    - Get the Health Object's faction, collisionRadius, Transform2D
    - If the faction is not equal to the gameObject's faction

      - Use the square of the collisionRadius and the gameObjects Position to determine if a collision has occurred.
      - If so, decrease the value of the Health Object by Energy, and destroy the gameObject.

4. Decrease TimeToLive by Time.deltaTime, if the result is <=0 then destroy the gameObject.

GetCollisionRadius(TwoStickExampleSettings settings, Faction.Type faction)

1. If faction is Player return settings.playerCollisionRadius, else return settings.enemyCollisionRadius.

## ShotSystem.cs

Data:

1. ShotSpawnData - A Class to hold the following variables.
    - Position - A float2 to indicate the 2D Position where the shot will be spawned.
    - Heading - A float2 vector - the heading that a spawned shot will have.
    - Faction - The faction a spawned shot will have.

Behaviours:

static class ShotSpawnSystem - static void SpawnShot(ShotSpawnData data)

1. Use the faction from data to determine if we are spawning a PlayerShot prefab or a EnemyShot prefab.
2. Instantiate the prefab.
3. Assign the Position and Heading values from data to the prefab's Transform2D component.
4 Assign the Faction value from data to the prefab's Faction component.

## Transform2D.cs

Data:

1. Position - A float2 vector to represent a 2D position.
2. Heading - A float2 vector to represent a 2D heading.

Behaviours:

LateUpdate

1. Update the gameObject's transform.position with the values in Position.
2. Update the gameObject's rotation, using the values in Heading.


## TwoStickBootstrap.cs

Data:

1. Settings - A static reference to the TwoStickExampleSettings class.

Behaviours:

NewGame

1. Instantiate a Player Object and give it a position and a heading.

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]

    - [Call this method after the scene has loaded](https://docs.unity3d.com/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html)

public static void InitializeWithScene()

1. Find an object in the scene called "Settings" and assign this to the static variable Settings.

## TwoStickExampleSettings.cs

Data:

1. playerMoveSpeed - float - The movement speed of the Player.
2. playerFireCoolDown - float - The minimum time between successive shots by the Player.
3. enemySpeed - float - The movement speed of an Enemy.
4. enemyShootRate - float - The minimum time between successive shots by an Enemy.
5. playerCollisionRadius - float - The hit box for the Player.
6. enemyCollisionRadius - The hit box for an Enemy.
7. playfield - Rect - The bounds for the game.
8. PlayerShotPrefab - A prefab for the Player's shots.
9. EnemyShotPrefab - A prefab for the Enemies' shots.
10. PlayerPrefab - The Player prefab.
11. EnemyPrefab - The Enemy prefab.
12. EnemyFaction - The Enemy Faction prefab.

## UpdatePlayerHUD.cs

Data:

1. m_CachedHealth - private float - The last known value for the Player's Health.
2. NewGameButton - Button - A button to be shown at the start of the game.
3. HealthText - Text - Shows the Player's Health value.

Behaviours:

Start

1. Add a listener to NewGameButton to call TwoStickBootstrap.NewGame when clicked.

Update

1. If there is a Player then call UpdateAlive, if not call UpdateDead

UpdateDead

1. If it hasn't already been deactivated, deactivate HealthText.
2. If it isn't active, activate NewGameButton.

UpdateAlive

1. Activate HealthText and deactivate NewGameButton.
2. If there is a player, get the health value and store in the var displayedHealth, if not, set displayedHealth to 0.
3. If there is a difference between displayedHealth and m_CachedHealth.
    - if displayedHealth > 0
      - update HealthText to show the new value.
      - if not set HealthText to "GameOver"
    - update m_CachedHealth with displayHealth.
