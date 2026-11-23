# AGENTS.md

Unity 6 (`6000.6.9f1`) 2D combat prototype. URP 2D, new Input System. Only scene in build settings: `Assets/Scenes/SampleScene.unity`.

## Verify
- No CLI build/test/lint and no test suite. Verify by opening the editor, entering Play Mode, and watching `Logs/Editor.log` for errors.
- Editor: `& "C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Unity.exe" -projectPath "D:\Game2D\Game2DCombat"`
- `.csproj`/`.sln` are Unity-generated and gitignored. Do not edit or commit them.
- Only the new Input System is enabled (`activeInputHandler: 1`); legacy `Input.*` APIs will not work.

## Code map
- `Assets/Scripts/**` compiles into `Assembly-CSharp` (only `Assets/Welcome` has an asmdef). Editor-only code must live in `Assets/Editor/**` (`Assembly-CSharp-Editor`).
- Player: `PlayerController` (singleton, input/movement) + `PlayerCombat` (attack/dash) + `PlayerHealth`, with `MeleeHitbox`.
- Enemies: `EnemyAI` (Roaming/Chasing/Attacking/Hurt/Dead) + `EnemyPathfinding` + `Slime : IEnemy` + `EnemyHealth`.
- Shared: `Knockback`, `Flash`. Contracts: `IDamageable.TakeDamage(int amount, Vector2 hitDirection)`, `IEnemy.Attack()`.
- Hit detection finds `IDamageable` via `GetComponentsInParent<MonoBehaviour>()`; enemies are excluded via `GetComponentInParent<EnemyAI>()`. Code resolves the player through `PlayerController.Instance`/`GetComponent`, not tags.
- Tags, layers and sorting layers are default (`Default` only), and `LayerMask` fields default to everything. Keep new sprites on `Default`; 2.5D depth is `sortingOrder = -y*10` plus camera custom-axis sort.

## Do not edit generated files
- `Assets/Scripts/Player/Player Controls.cs` is generated from `Assets/Scripts/Player/Player Controls.inputactions` (the header's path is stale). Change the `.inputactions` asset, not the `.cs`.
- `.meta` files carry GUIDs referenced by scenes/prefabs. Create/move/rename/delete assets through Unity so GUIDs stay consistent.

## Editor menu tools (code-driven, destructive)
- `Tools/Build Interactive RPG Map` (`MapSetup.cs`) rebuilds `ForestMap`: deletes all children except `ForestGround`, recreates props/colliders/sunbeams, and overwrites camera settings.
- `Tools/Setup Blue Slime` (`SlimeSetup.cs`) regenerates clips/controller/prefab, deletes existing `Enemies` children, respawns slimes, and adds `Knockback`/`Flash`/`PlayerHealth` to the Player.
- `Tools/Capture Scene View` (`CaptureHelper.cs`) writes to a hardcoded absolute path under `C:\Users\Laptop\.gemini\...`; update it before relying on it.

## Conventions
- Unity 6 API: `Rigidbody2D.linearVelocity`/`linearDamping` (not `velocity`/`drag`), `FindAnyObjectByType`.
- Expose inspector tuning via `[SerializeField] private` + `[Header]`; null-guard optional components as existing code does.
- Animator params: Player `moveX`/`moveY`; Slime `isWalking`, `Attack`, `Hurt`, `Death`, `isDead`.
- `.agents/agents/{csharp-coder,unity-artist}.md` hold Vietnamese subagent prompts; keep them in sync if roles change.

## Maintenance & Multi-Agent Rules
- Auto-update: Update this `AGENTS.md` whenever folder architecture, core interfaces (`IDamageable`, `IEnemy`), Animator parameters, or Editor tools change. Keep the total file under 200 lines.
- Multi-agent file ownership: When running multiple agents in parallel, assign distinct scripts/folders per agent; never let two agents edit the same `.unity` scene or `.prefab` file simultaneously.

## Current Progress
- [Update here before ending a session if a feature is mid-way]
