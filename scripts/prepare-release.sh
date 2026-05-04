#!/bin/bash

# 1. Strict branch check
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
if [ "$CURRENT_BRANCH" != "master" ]; then
    echo "Error: Must be on 'master' to start."
    exit 1
fi

# Capture the exact point master is at right now
MASTER_START_POINT=$(git rev-parse HEAD)

echo "Moving to a detached state to protect 'master' pointer..."

# 2. The Fix: Go to a Detached HEAD at master's position
# This ensures that when we rebase, no branch label (master) is attached to the movement.
git checkout "$MASTER_START_POINT" --quiet

echo "Opening interactive rebase: Squash your commits and redact the message."
# 3. Rebase the detached commits onto the github branch
# We are rebasing the range: github-reference..CURRENT_DETACHED_HEAD
# And placing it --onto the github branch.
if git rebase -i --onto github github-reference; then
    
    # 4. Update 'github' branch to this new result
    # Only now do we move a branch pointer, and we move 'github', not 'master'.
    git branch -f github HEAD
    echo "Success: 'github' updated and squashed."
else
    echo "Rebase failed or aborted."
fi

# 5. Guaranteed return to master
# master is still exactly where we left it at MASTER_START_POINT.
git checkout master --quiet

echo "Verified: 'master' is still at $(git rev-parse --short master)"
