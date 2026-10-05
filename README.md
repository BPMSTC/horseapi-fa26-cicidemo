# horseapi-fa26-cicidemo

Instructor demo repo for **Web Programming 1, Week 7**: why a pipeline should run the tests before it deploys.

This is a copy of the lab app (`wp1-fall26-aws`) with the AWS deploy replaced by a **simulated deploy**. Nothing here touches AWS, so it is safe to break, push, and reset.

- `.github/workflows/deploy-no-tests.yml`: deploys on every push to `main` and never runs the tests (the "before").
- `demo/pipeline.yml`: the build, then test, then deploy workflow with a test gate (the "after"). It is copied into `.github/workflows/` live during the demo.
- `demo/break-test.ps1` and `demo/fix-test.ps1`: break and repair one test.
- `demo/RUN-OF-SHOW.md`: the step-by-step script.
