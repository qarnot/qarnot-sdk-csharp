#!/bin/bash

# 1. Check if we are on the master branch
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)

if [ "$CURRENT_BRANCH" != "master" ]; then
    echo "Error: You are currently on '$CURRENT_BRANCH'. Please switch to 'master' to run this."
    exit 1
fi

# Ensure there is actually something to rebase
if ! git rev-parse github-reference >/dev/null 2>&1; then
    echo "Error: 'github-reference' tag or branch not found."
    exit 1
fi

echo "Starting rebase of master commits onto 'github'..."

# 2. Rebase github-reference..HEAD onto the 'github' branch
# We use --onto to transplant the commits from master that are newer than 
# the github-reference directly onto the github branch.
# The '-i' flag will trigger the editor for squashing and redacting.

git checkout github
git rebase -i --onto github github-reference master

# Note: After the rebase finishes, git will leave you on a 'detached HEAD' 
# or on the master branch depending on the state. 
# To finalize 'github' as this new state:
git branch -f github HEAD
git checkout github

echo "Done. Your 'github' branch is now updated and squashed."
