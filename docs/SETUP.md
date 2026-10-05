# Setup

## Before the first run

1. **Install the .NET 10 SDK** on the computer you will present from. Check with `dotnet --version`; it should start with `10.`.
2. **Run the tests locally first.** From the repository root:
   ```
   dotnet test Wp1Fall26Aws.slnx
   ```
   You should see **8 tests pass** (3 in `DocumentValidatorTests`, 2 in `S3DocumentStorageTests`, 3 in `DocumentApiTests`). If they don't, fix that before pushing anything. The pipeline runs the same command.
3. **Check that GitHub Actions is on.** Open the repo on GitHub, click the **Actions** tab, and enable workflows if it asks.
4. **Make the repository public** if you want to show branch protection later (it is free for public repositories; private repositories need a paid plan). It is optional for the demo itself.

## Pushing the pipeline for the first time

```
git add -A
git commit -m "Add the build, test, and deploy pipeline"
git push
```

That push goes to `main`, so it starts the workflow. Open the **Actions** tab and watch **build-test** and then **deploy** run. Both should go green.

## Plain-language Git glossary

- **Repository (repo):** the project folder plus its full history, stored on your computer and on GitHub.
- **Commit:** a saved snapshot of your changes, with a message. Commits are saved on your computer first.
- **Push:** send your commits to GitHub. This is what starts the workflow.
- **Branch:** a separate line of work. `main` is the main one. Changes on a branch don't affect `main` until they are merged.
- **Pull request (PR):** a request on GitHub to merge a branch into `main`. Our workflow runs on PRs so changes are tested before they merge.
- **Merge:** combine a branch into `main`. Merging a PR is a push to `main`, so it starts a full run, including the deploy.
- **Workflow, job, step, runner:** a workflow is the YAML file. It contains jobs. A job is a list of steps. A runner is the fresh virtual machine GitHub starts for a job.
- **Artifact:** a file or folder saved from one job so another job (or you) can download it later.
