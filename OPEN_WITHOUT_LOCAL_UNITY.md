# If Unity cannot be installed on this computer

The source project is still usable. Choose one of these routes.

## Route A — ASU lab or another Unity computer (simplest)

1. Copy or clone this entire repository.
2. Sign in to Unity Hub on the available computer.
3. Add this folder as an existing project.
4. Allow Unity to open or upgrade it if the installed Unity 6 patch differs.
5. Choose **Crystal Escape > Prepare Project**.
6. Press Play and complete `Documentation/TESTING.md`.
7. Import the three real Tripo models and generated audio.
8. Choose **Crystal Escape > Build Windows** or **Build WebGL**.

## Route B — Unity Build Automation

1. Put this folder in a public or private Git repository.
2. Sign in to the Unity Dashboard.
3. Open **DevOps > Build Automation > Configurations**.
4. Connect the Git repository.
5. Set the project subfolder to the folder containing `Assets`, `Packages`, and `ProjectSettings`.
6. Let the service detect the version from `ProjectSettings/ProjectVersion.txt`, or select a compatible Unity 6 version.
7. Create a Windows or WebGL target and run a manual build.
8. Read the build log. Copy any errors into `Documentation/PROMPT_LOG.md` before fixing them.

Cloud build is not a substitute for playtesting. Run the downloaded build and verify Reward, Damage, both End states, audio, and restart.

