# Building gitvisual (SourceGit fork)

## Prerequisites

- **.NET SDK 10.0.x** (pinned by `global.json`, `rollForward: latestMajor`, no prereleases).
  Any OS-local install works; a user-local install without sudo:
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
  export DOTNET_ROOT="$HOME/.dotnet"
  export PATH="$HOME/.dotnet:$PATH"
  ```
- **git** with submodule support.
- macOS (arm64 verified), Linux, or Windows.

## Clone

The repo vendors AvaloniaEdit as a git submodule — a plain clone is NOT enough:

```bash
git clone git@github.com:joseignaciot/sourcegit.git
cd sourcegit
git submodule update --init --recursive
```

Remotes for fork hygiene:

```bash
git remote add upstream https://github.com/sourcegit-scm/sourcegit.git
git fetch upstream
```

- `origin` → fork (joseignaciot/sourcegit)
- `upstream` → sourcegit-scm/sourcegit (`main` tracks upstream; gitvisual work on `gitvisual/*` branches)

## Build

```bash
dotnet build src/SourceGit.csproj
```

Expected: `0 Warning(s) 0 Error(s)` — verified 2026-08-15 with SDK 10.0.400 on macOS arm64.

> If you see `CS0246: AbstractMargin / TextView / IBackgroundRenderer not found`, the AvaloniaEdit
> submodule is missing. Run `git submodule update --init --recursive` and rebuild.

## Run

```bash
cd src && dotnet run --project SourceGit.csproj
```

The app launches with the welcome screen; open any local repository to see the commit graph.

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `CS0246` AvaloniaEdit types missing | submodule not initialized | `git submodule update --init --recursive` |
| `dotnet: command not found` | SDK not on PATH | `export PATH="$HOME/.dotnet:$PATH"` |
| global.json SDK error | wrong SDK channel | install channel 10.0 (see above) |
