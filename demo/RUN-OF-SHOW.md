# Run of show: "Watch a bad change ship, then make it impossible"

About 40 minutes. All commands are for PowerShell from the repo root. Nothing in this repo touches AWS.

## One-time setup (do this before the practice run)

1. Commit everything in this folder as the first commit and push to `main`:
   ```
   git add .
   git commit -m "Initial demo repo"
   git push -u origin main
   ```
2. On GitHub, open the **Actions** tab and confirm "Deploy (no tests)" ran green (this first push triggers it). If Actions asks you to enable workflows, enable them.
3. Mark the starting point so you can reset later:
   ```
   git tag demo-start
   git push origin demo-start
   ```
4. Run `dotnet test Wp1Fall26Aws.slnx` locally once. All 5 tests should pass. (These workflows and tests were checked for syntax but never run on GitHub before this, so a practice run matters.)

## Before class

- Repo is public (so branch protection is available on the free plan, if you use the bonus).
- Browser tabs ready: the repo's Actions tab, and the repo home.
- Terminal open in the repo root, on `main`, clean (`git status`).

## Act 1: The open gate (about 10 minutes)

**Say:** "Our deploy workflow ships on every push to main. Let's see what it checks."

1. Open `.github/workflows/deploy-no-tests.yml`. Point out there is no restore, build, or test step. Only publish, zip, and a deploy.
2. Break a test:
   ```
   .\demo\break-test.ps1
   dotnet test Wp1Fall26Aws.slnx
   ```
   Show the red failure locally. **Say:** "One test is failing. Would you want this in production?"
3. Push it straight to main:
   ```
   git add -A
   git commit -m "Change validator test"
   git push
   ```
4. In the Actions tab, open the run. It goes **green** and the job summary says "SHIPPED. No tests were run."

**Land the point:** "Green means every check that ran passed. The test check never ran, so the pipeline can't tell us anything about it."

## Act 2: Close the gate (about 15 minutes)

**Say:** "We need the tests to run first, and deploy to wait for them."

1. Add the new workflow and retire the old one:
   ```
   Copy-Item demo\pipeline.yml .github\workflows\pipeline.yml
   git rm .github/workflows/deploy-no-tests.yml
   ```
2. Open `.github/workflows/pipeline.yml` and walk through it. Hit these lines (they match the page 4 tutorial):
   - `on:` the three events
   - `permissions: contents: read` at the top
   - the `Test` step: "a failing test fails this job"
   - `upload-artifact`: "ship exactly what was tested"
   - `needs: build-test`
   - the `if:` line
   - the job-level `permissions` with `id-token: write`
3. Push it. The broken test is still in the repo, so this is the proof:
   ```
   git add -A
   git commit -m "Add pipeline with a test gate"
   git push
   ```
4. In Actions, open the run. **build-test goes red at the Test step. deploy shows as skipped.** Nothing shipped.

**Say:** "Same code that shipped an hour ago. Now it can't."

## Act 3: Fix it through a pull request (about 15 minutes)

1. Make a branch and fix the test:
   ```
   git switch -c fix-validator-test
   .\demo\fix-test.ps1
   git add -A
   git commit -m "Fix validator test"
   git push -u origin fix-validator-test
   ```
2. On GitHub, open the pull request. The run starts. **build-test goes green, deploy is skipped.** Open the run and click the skipped deploy job. **Say:** "Skipped because the `if:` is false for pull requests. This is why PR runs never need AWS credentials."
3. Merge the pull request. A new run starts on main. **Both jobs go green.** Open the deploy job:
   - Show the artifact download ("it's the exact build that passed").
   - Open "Show the OIDC claims". Point at `sub`: `repo:<owner>/horseapi-fa26-cicidemo:ref:refs/heads/main`. **Say:** "This is what AWS would see. Page 5 is about how the trust policy decides on this line. We will not touch the real role in class."
   - Show the job summary: simulated deploy, tested build.

## Bonus (if time): block a bad pull request

1. `git switch main; git pull; git switch -c break-it`
2. `.\demo\break-test.ps1`, commit, push, open a pull request. build-test is red.
3. (Optional, needs a public repo or paid plan) In **Settings > Rules > Rulesets**, require the **build-test** status check on main. Not `deploy`: it is skipped on every pull request, and a skipped check counts as passing. Show that the merge button is blocked.

## Reset to the start

```
git switch main
git reset --hard demo-start
git push --force origin main
git push origin --delete fix-validator-test break-it
```
(If you turned on branch protection or a ruleset, disable it first. Delete branches you did not create and the delete command will say so; that is fine.)

## If something goes wrong

- **Workflow doesn't start:** check the Actions tab is enabled for the repo and that the file is under `.github/workflows/`.
- **"Show the OIDC claims" prints nothing:** the job needs `id-token: write`; both workflows have it. Re-run the job once.
- **`dotnet restore` fails on `.slnx`:** the workflow installs .NET 10, which supports `.slnx`. If it fails, check the "Set up .NET 10" step ran.
- **A step name in the tutorials differs slightly from here:** the tutorials use AWS steps; this repo replaces them with the simulated deploy. The logic is identical.

`demo/deploy-no-tests.yml` is a spare copy of the Act 1 workflow, in case you want to restore it without resetting the repo (`Copy-Item demo\deploy-no-tests.yml .github\workflows\`).
