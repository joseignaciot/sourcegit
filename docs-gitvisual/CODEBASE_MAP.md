# SourceGit Codebase Map (gitvisual fork)

Mapped 2026-08-15 against upstream `master` (v2026.17). Single project: `src/SourceGit.csproj`
(.NET 10, Avalonia 11.3, CommunityToolkit.Mvvm). No test project exists upstream.
Vendored dependency: `depends/AvaloniaEdit` (git submodule — must be initialized to build).

## Layout

| Area | Path | Notes |
|---|---|---|
| App shell | `src/App.axaml(.cs)`, `App.Commands.cs` | Single-window app; self-update check hits GitHub API (only outbound HTTP besides avatars) |
| Git CLI wrappers | `src/Commands/` | One class per git invocation; base `Command.cs` (process spawn, credential prompt handling) |
| Domain models | `src/Models/` | POCOs + parsers (Branch, Commit, Remote, CommitGraph, ...) |
| ViewModels | `src/ViewModels/` | MVVM, `ObservableObject`; heavy logic lives here |
| Views | `src/Views/` | AXAML + code-behind |
| AI providers | `src/AI/` | OpenAI/Azure etc. for commit-message generation |
| OS integration | `src/Native/` | `OS.cs` per-platform shell helpers |
| Locales | `src/Resources/Locales/*.axaml` | One resource dict per language |

## Commit graph (do NOT modify in gitvisual-mvp)

- `src/Models/CommitGraph.cs` (450 lines): parses `git log` output and computes the DAG layout
  (lanes, links).
- Rendering: `src/Views/Histories.axaml` (history view) + `src/Views/CommitGraph.cs` (custom
  drawn control).

## Workspace / launcher model

- `src/ViewModels/Workspace.cs` (66 lines): name, color, `List<string> Repositories` (absolute
  paths), `ActiveIdx`, `RestoreOnStartup`, `DefaultCloneDir`. Pure data — no behavior.
- `src/ViewModels/Launcher.cs` (478 lines): owns `AvaloniaList<LauncherPage> Pages`, tab
  management (`AddNewTab`, `CloseTab`).
- `src/ViewModels/LauncherPage.cs`: wrapper with `RepositoryNode Node`, `object Data`,
  `Models.DirtyState DirtyState`, `Popup`. **This is the dashboard's host pattern (design D1).**
- `src/Views/Welcome.axaml`: repo list / open-clone screen — structural model for the dashboard
  view.

## Data sources the dashboard reuses (all existing Commands)

| Need | Command | Git invocation |
|---|---|---|
| Current branch | `Commands/QueryCurrentBranch.cs` | `branch --show-current` |
| Ahead/behind | `Commands/QueryTrackStatus.cs` | `rev-list --left-right local...remote` |
| Dirty file count | `Commands/CountLocalChanges.cs` | status porcelain count |
| Full repo status | `Commands/QueryRepositoryStatus.cs` | combined query |

## Forge "integration" layer — URL-only (key finding)

- `src/Models/Remote.cs`: `TryGetVisitURL(out url)` and
  `TryGetCreatePullRequestURL(out url, mergeBranch)` — builds web URLs for
  GitHub/GitLab/Gitea/Gitee/Bitbucket; the app opens them in the browser
  (used from `src/Views/BranchTree.axaml.cs` and `RepositoryConfigure.axaml.cs`).
- **There is NO GitHub/GitLab API client** (no HttpClient-based provider layer; verified by grep:
  only `AvatarManager` for avatar images and the self-update check do outbound HTTP).
- git credential prompts are handled inside `Commands/Command.cs` (terminal credential helper
  flow). There is NO app-level secure token store — `ViewModels/Preferences.cs` persists to plain
  JSON and must NOT hold forge PATs (see design D2: OS keychain via `Native/OS.cs`-style
  shell-out).

## Navigation / where gitvisual features plug in

- Dashboard → new `LauncherPage` hosted by `Launcher`, data per `Workspace.Repositories` entry.
- Forge counts → new `src/Models/Forge/` provider layer (GitHub REST v3 / GitLab REST v4).
