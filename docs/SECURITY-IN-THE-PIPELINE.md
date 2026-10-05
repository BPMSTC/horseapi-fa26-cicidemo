# Security in the Pipeline

This demo does not add any security tools. It only uses the security ideas taught in the Week 7 tutorials (especially page 5, "Who gets to deploy?"). This page lists what is applied here, what a real AWS deployment adds, and what this demo does NOT do.

## Part 1: What this demo applies

| Idea from the tutorials | Where to see it in `pipeline.yml` | Why it matters |
|---|---|---|
| **Least privilege by default.** The workflow starts with read-only access. | Top-level `permissions: contents: read` | If a step is compromised, it can read the code but cannot change the repo. |
| **Extra permission only where needed.** Only the deploy job may ask for an identity token. | `permissions: id-token: write` inside the `deploy` job only | The build/test job (which runs on every PR) never gets this power. |
| **Gate the deploy.** Deploy runs only after tests pass and only on `main`. | `needs: build-test` and `if: github.event_name != 'pull_request' && github.ref == 'refs/heads/main'` | Code from an unreviewed PR can never reach the deploy step. |
| **No stored secrets.** There are no passwords or keys in this repo or in GitHub Secrets. | Nothing to see: that is the point | There is nothing to leak. |
| **Short-lived identity (OIDC).** The deploy job shows the "claims" GitHub would send to AWS. | The step "Show the OIDC claims GitHub would send to AWS" | In a real deploy, AWS checks these claims instead of a password. |

### Reading the OIDC claims

After a merge to `main`, open the `deploy` job log. You will see something like:

```
aud: sts.amazonaws.com
iss: https://token.actions.githubusercontent.com
sub: repo:<owner>/horseapi-fa26-cicidemo:ref:refs/heads/main
repository: <owner>/horseapi-fa26-cicidemo
ref: refs/heads/main
```

The `sub` line is the important one. It says exactly which repo and which branch is asking. In a real AWS setup, the IAM role's **trust policy** says "only trust tokens whose `sub` is `repo:<owner>/<repo>:ref:refs/heads/main`". A pull request would have a different `sub` (`...:pull_request`), so AWS would refuse it. See tutorial page 5 for the trust-policy simulator.

The token itself is never printed. Only the claims are.

## Part 2: What a real AWS deployment adds

This demo's deploy step is simulated. Your real lab app (`wp1-fall26-aws`) needs more:

1. **An IAM role with a trust policy** scoped to your repo and the `main` branch (tutorial page 5).
2. **The `aws-actions/configure-aws-credentials` step**, which exchanges the OIDC token for temporary AWS credentials.
3. **Branch protection on `main`** that requires the `build-test` check to pass before merging. Require `build-test`, **not** `deploy`: a skipped job counts as passing, so requiring `deploy` would not protect anything. (Branch protection needs a public repo or a paid GitHub plan.)
4. **Pinning actions to a commit SHA** instead of a tag (see below).

## Part 3: Honest caveats

- **This demo uses tags, not SHAs.** The workflow says `actions/checkout@v4`, not `actions/checkout@<long-hash>`. A tag can be moved by whoever owns the action. That is how the tj-actions incident (CVE-2025-30066, tutorial page 5) worked. For a real deployment, pin to a full commit SHA. We use tags here only so the file stays readable for a first look.
- **No vulnerability scanning.** This demo does not run CodeQL, dependency scanning, or Dependabot. Those are good next steps, but they are not part of this demo.
- **Simulated deploy.** Nothing is sent to AWS, so there is nothing for an attacker to take. A real deploy raises the stakes, which is why the controls above matter.
- **Tests are not security tests.** The test that rejects a `.sh` upload (`PostDocuments_ShellScript_Returns400AndNeverCallsS3`) checks one input rule. It is not a full security review.
