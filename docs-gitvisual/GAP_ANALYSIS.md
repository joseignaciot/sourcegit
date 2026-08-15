# Gap Analysis: gitvisual vision vs SourceGit

Mapped 2026-08-15. Vision: GitKraken-style clarity for MANY repos on GitHub/GitLab, all
operations from the UI.

| Vision feature | SourceGit state | Gap | Extension point |
|---|---|---|---|
| Clear commit graph | ✅ Mature (`CommitGraph.cs` + `Histories.axaml`) | Polish only, later change | `src/Views/CommitGraph.cs` rendering |
| Multi-repo grouping | ⚠️ `Workspace` = flat path list + tabs | No workspace-scoped filter | Toggle in Welcome toolbar (gitvisual-mvp T3) |
| Multi-repo STATUS view | ✅ Welcome rows already show branch, ahead/behind, dirty badge (corrected 2026-08-15; earlier version of this doc wrongly claimed no such view) | None — reuse `RepositoryNode.Status` | — |
| Branch/commit ops from UI | ✅ Complete (merge, rebase, stash, worktrees, LFS, bisect...) | None | — |
| Create PR on GitHub/GitLab | ⚠️ Browser URL only (`Remote.TryGetCreatePullRequestURL`) | No in-app flow | Future: `src/Models/Forge/` providers |
| List/review/merge PRs-MRs | ❌ Nothing | No API client exists at all | `src/Models/Forge/` (gitvisual-mvp T4: counts first) |
| Secure forge credentials | ❌ Preferences = plain JSON | PATs need OS keychain | `ForgeCredentialStore` (T4, macOS `security` first) |
| GitLab self-managed hosts | ⚠️ URL detection only | Provider must accept custom host | `GitLabProvider` host param (T4) |

## Deliberately out of scope (gitvisual-mvp)

- Graph renderer rewrite — the graph is SourceGit's strength; we build ON it.
- In-app PR create/review/merge flows — after counts prove the forge layer.
- CI/CD, issues, releases management.

## Risk notes

- Fork drift: sync `upstream` weekly; upstreamable fixes isolated per AC4.3.
- AXAML learning curve: dashboard view modeled on `Welcome.axaml`.
