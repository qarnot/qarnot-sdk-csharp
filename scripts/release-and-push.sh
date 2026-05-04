#!/usr/bin/env bash

if [ $# -ne 1 ]; then
    echo "Usage: $0 tag"
    exit 1
fi

CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)

if [ "$CURRENT_BRANCH" != "github" ]; then
    echo "Releases must be done from the 'github' branch'"
    exit 3
fi

tag=$1

if [[ $tag =~ ^v[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    git tag -a "$tag"
else
    echo "Invalid tag, should be vX.Y.Z"
    exit 2
fi

echo "Tagged release $tag. Will now push the github branch to origin, push it to master on Github, and push the tag to both remotes. Press anything to proceed, Ctrl-C to abort"
read -r

git push origin github
git push github github:master
git push origin "$tag"
git push github "$tag"


echo ""
echo "======"
echo "Published $tag."
echo "Now don't forget to bump the 'github-reference' branch to where it should be, probably the master state"
echo "======"
