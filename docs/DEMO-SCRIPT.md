# Demo Script: Watch a Pipeline Test and Deploy Your Code

**Time:** about 20 minutes. **Goal:** show that every change is built and tested automatically, and that only tested code on `main` reaches the deploy step.

No breaking anything. No tags. No resets. Every step can be repeated exactly.

**Before you start:** finish `docs/SETUP.md` (clone the repo, run `dotnet test`, see 8 passing).

---

## Part 1: Look around (3 min)

1. Open the repo on GitHub. Point out three folders: `src/` (the app), `tests/` (the tests), `.github/workflows/` (the pipeline).
2. Open `.github/workflows/pipeline.yml`. Don't read it all. Show the two jobs: `build-test` and `deploy`.
3. Say: "Think of this as a conveyor belt. Station 1 builds and tests. Station 2 only starts if Station 1 passed."

## Part 2: Run the tests on your own computer (3 min)

```
dotnet test Wp1Fall26Aws.slnx
```

Expected: **8 passed, 0 failed**. These are the same tests the pipeline will run. If they pass here, they should pass there.

## Part 3: Make a small change on a branch (3 min)

A branch is your own copy of the code to try things without touching `main`.

```
git checkout -b demo-readme-change
```

Edit `README.md`: add a line such as `Demo run by <your name>.` Then:

```
git add README.md
git commit -m "Add a demo line to the README"
git push -u origin demo-readme-change
```

## Part 4: Open a pull request (4 min)

1. On GitHub click **Compare & pull request**, then **Create pull request**.
2. Click the **Checks** tab (or the "Actions" tab) and open the running workflow.
3. Watch `build-test` run: checkout, restore, build, **test**, publish, upload.
4. Point out the `deploy` job is **Skipped**. Say: "A pull request is untested, unreviewed code. The `if` condition keeps it away from deploy."
5. Open the **Run unit and integration tests** step log. Find the 8 test names with "Passed".

## Part 5: Merge and watch the full run (5 min)

1. Click **Merge pull request**.
2. Go to the **Actions** tab. A new run starts for `main`.
3. This time `deploy` runs after `build-test` succeeds.
4. Open the `deploy` job and show:
   - **Show the OIDC claims** step: the `sub` line says `...ref:refs/heads/main`. (See `docs/SECURITY-IN-THE-PIPELINE.md`.)
   - **SIMULATED deploy** step: nothing real is deployed.
5. Open the run **Summary** page and show:
   - the **job summary** written by the deploy step
   - the **Artifacts** section with `api-publish` (the built app)

## Part 6: Run it again without a change (1 min)

On the Actions tab choose **Build, test, and deploy (simulated)** then **Run workflow**. This is the `workflow_dispatch` trigger. It runs on `main` and is a handy way to re-run when nothing changed.

---

## Talking points

- **Why tests before deploy?** A failed test stops the belt. Bad code never reaches Station 2.
- **Why is deploy skipped on a PR?** Unreviewed code should not get deploy powers.
- **Why show the OIDC claims?** In a real deploy AWS reads them instead of a password.
- **What is missing?** Real AWS, action pinning, scanning. See the security doc.

## Cleaning up

Nothing needs to be undone. If you want to delete the demo branch:

```
git checkout main
git pull
git branch -d demo-readme-change
```
