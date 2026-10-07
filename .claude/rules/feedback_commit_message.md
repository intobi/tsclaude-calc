---
name: ask-commit-message
description: Always ask the user for the git commit message before committing
metadata:
  type: feedback
---

Always ask the user what to name the commit before running `git commit`.

**Why:** the commit message was once chosen without asking, and the user wants to decide it themselves.

**How to apply:** Before every commit, ask for the message (you may suggest one, but wait for the answer). Keep the Co-Authored-By trailer from the attribution reminder.
