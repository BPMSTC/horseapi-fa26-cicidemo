# Pipeline walkthrough: `.github/workflows/pipeline.yml`

This page explains the workflow in the order it appears in the file. Use it to prepare, and as the script for the "read the workflow" part of the demo.

## The big idea

A workflow is a chain: an **event** happens (a push, a pull request, a button press), which starts a **workflow**, which runs **jobs**, each made of **steps**, each job on its own **runner** (a fresh virtual machine GitHub provides).

GitHub's workflow syntax reference says jobs "run in parallel by default" and that each job runs in a runner environment chosen by `runs-on`. Two consequences matter here:

1. Without instructions, two jobs start at the same time. So deploy would not wait for the tests.
2. Each job gets its own clean machine. Files created in one job are not on the other job's machine. That is why we use an artifact to pass the build between jobs.

## `on:` the events that start the workflow

```yaml
on:
  pull_request:
    branches: [ main ]
  push:
    branches: [ main ]
  workflow_dispatch:
```

- `pull_request` into `main`: runs when someone opens or updates a PR. Runs build-test only.
- `push` to `main`: runs on every push or merge to `main`. Runs build-test and then deploy.
- `workflow_dispatch`: adds a manual **Run workflow** button on the Actions tab.

## `permissions:` the default access

```yaml
permissions:
  contents: read
```

Every job gets a token called `GITHUB_TOKEN`. This line sets the default for all jobs: read the repository, nothing else. GitHub's rule is that if you specify any permission, all the ones you don't specify are set to none.

## Job 1: `build-test`

```yaml
build-test:
  runs-on: ubuntu-latest
```

A fresh Linux virtual machine. Everything below happens on it, and it is thrown away afterward.

| Step | What it does | Why it is there |
|---|---|---|
| `actions/checkout@v4` | Copies the repository onto the runner | The runner starts empty |
| `actions/setup-dotnet@v4` with `10.0.x` | Installs the .NET 10 SDK | The app targets .NET 10 |
| `dotnet restore Wp1Fall26Aws.slnx` | Downloads the packages the projects need | Build needs them |
| `dotnet build ... --no-restore` | Compiles everything in Release mode | Catches compile errors early. `--no-restore` skips repeating the restore |
| `dotnet test ... --no-build` | Runs all 8 tests | **The gate.** If any test fails, this step fails and so does the job |
| `dotnet publish ... --output ./publish` | Produces the files that would be deployed | We publish only after the tests have passed |
| `actions/upload-artifact@v4` named `api-publish` | Saves the `./publish` folder | Hands the exact tested build to the deploy job |

The test command adds `--logger "console;verbosity=normal"` so the log lists every test by name, which is easier to show in a demo.

## Job 2: `deploy`

```yaml
deploy:
  needs: build-test
  if: github.event_name != 'pull_request' && github.ref == 'refs/heads/main'
  runs-on: ubuntu-latest
  permissions:
    contents: read
    id-token: write
```

- **`needs: build-test`**: deploy waits for build-test to finish successfully. GitHub's docs: if the needed job fails or is skipped, all jobs that need it are skipped. This is what makes the tests a gate.
- **`if:`**: a condition. `github.event_name` is the event that started the run. `github.ref` is the branch; for `main` it is `refs/heads/main`. So deploy runs only when the event is not a pull request and the branch is `main`.
- **`permissions:` at job level**: replaces the workflow default for this job only. We list `contents: read` again, and add `id-token: write`, which lets this one job request an OIDC token. No other job can.

| Step | What it does |
|---|---|
| `actions/download-artifact@v5` | Downloads `api-publish`: the exact files that passed the tests. Deploy never rebuilds anything |
| Create ZIP deployment bundle | Zips the folder into `deploy-package.zip`, the shape Elastic Beanstalk expects |
| Show the OIDC claims | Asks GitHub for an OIDC token and prints its *claims* (not the token). Look at `sub`: for a push to main it is `repo:<owner>/horseapi-fa26-cicidemo:ref:refs/heads/main` |
| SIMULATED deploy | Prints what it would deploy, lists the zip contents, and writes a summary on the run page. No AWS is touched |

## Build once, deploy what you tested

An easy mistake is to publish the app again inside the deploy job. Then the files that ship were never the files that were tested. Uploading the published folder from build-test and downloading it in deploy means **what ships is exactly what passed**.

## What happens in each situation

| Situation | build-test | deploy | Why |
|---|---|---|---|
| A pull request is opened | runs | skipped | the `if:` is false for pull requests |
| A change is pushed or merged to `main`, tests pass | passes | runs | event is a push, branch is main, build-test succeeded |
| A change is pushed to `main`, a test fails | fails | skipped | the job it `needs` failed |
| Someone presses **Run workflow** on `main` | runs | runs | `workflow_dispatch` is not a pull request |

## From this demo to the real deploy

Replace the **SIMULATED deploy** step with the AWS steps from the course repo (`aws-actions/configure-aws-credentials` to assume the role through OIDC, then the Elastic Beanstalk deploy action). Everything before it stays the same.
