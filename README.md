# horseapi-fa26-cicidemo

A working CI/CD pipeline for the Web Programming 1 (Week 7) demo.

Every push to `main` builds the app, runs its unit and integration tests, and only then "deploys" it. Pull requests build and test but never deploy. The deploy is **simulated**: it prints what it would do and never touches AWS, so the repo is safe to push to at any time.


## What the pipeline does

```
Pull request into main:   build-test            (deploy is skipped)
Push or merge to main:    build-test  -->  deploy (simulated)
```

- **build-test** restores, builds, runs all 8 tests, publishes the API, and uploads the published folder as an artifact.
- **deploy** waits for build-test (`needs`), runs only on `main` (`if`), downloads the exact build that was tested, prints the OIDC claims GitHub would send to AWS, and prints a simulated deployment.

## The documents in this repo

Read them in this order:

| File                                                                | What it covers                                                                      |
| ------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| [docs/SETUP.md](docs/SETUP.md)                                       | One-time setup, running the tests locally, and a plain-language Git glossary        |
| [docs/PIPELINE-WALKTHROUGH.md](docs/PIPELINE-WALKTHROUGH.md)         | The workflow file explained block by block                                          |
| [docs/TESTING.md](docs/TESTING.md)                                   | The 8 tests, what each one proves, and how the S3 spy works                         |
| [docs/SECURITY-IN-THE-PIPELINE.md](docs/SECURITY-IN-THE-PIPELINE.md) | The security controls already in the workflow, and what a real AWS deploy would add |
| [docs/DEMO-SCRIPT.md](docs/DEMO-SCRIPT.md)                           | Step-by-step script for presenting and reproducing the demo                         |
| [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)                   | What to check when something doesn't work                                           |

## The app

A small C# / .NET 10 minimal API with three endpoints: `GET /`, `GET /health`, and `POST /documents` (a file upload that would be stored in Amazon S3). The tests never call AWS.

```
src/Wp1Fall26Aws.Api       the web API
src/Wp1Fall26Aws.Storage   validation and the S3 storage boundary
tests/Wp1Fall26Aws.Tests   xUnit unit and integration tests
.github/workflows/         pipeline.yml
```
