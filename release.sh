#!/bin/bash
set -e

# Colors for output
GREEN="\033[0;32m"
BLUE="\033[0;34m"
YELLOW="\033[1;33m"
RED="\033[0;31m"
NC="\033[0m" # No Color

# Source .env if present so DISCORD_WEBHOOK_URL and others are available
if [ -f ".env" ]; then
    echo -e "${BLUE}Sourcing .env file...${NC}"
    set -a
    # shellcheck source=/dev/null
    source ".env"
    set +a
fi

echo -e "${BLUE}🚀 MatchZy Automated Release Script${NC}\n"

# Get current version from MatchZy.cs
CURRENT_VERSION=$(grep "ModuleVersion =>" src/MatchZy.cs | sed -E "s/.*\"(.*)\".*/\1/")
if [ -z "$CURRENT_VERSION" ]; then
    echo -e "${RED}❌ Could not detect version from MatchZy.cs${NC}"
    exit 1
fi

# Parse version components
IFS=. read -r MAJOR MINOR PATCH <<< "$CURRENT_VERSION"

# Check for version bump argument or explicit version
BUMP_TYPE="${1:-none}"
VERSION="$CURRENT_VERSION"

# If the first argument looks like X.Y.Z, treat it as an explicit version override
if [[ "$BUMP_TYPE" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    VERSION="$BUMP_TYPE"
    BUMP_TYPE="explicit"
    echo -e "${YELLOW}🔧 Using explicitly specified version: ${CURRENT_VERSION} → ${VERSION}${NC}"
elif [ "$BUMP_TYPE" = "major" ]; then
    MAJOR=$((MAJOR + 1))
    MINOR=0
    PATCH=0
    VERSION="${MAJOR}.${MINOR}.${PATCH}"
    echo -e "${YELLOW}🔼 Bumping MAJOR version: ${CURRENT_VERSION} → ${VERSION}${NC}"
elif [ "$BUMP_TYPE" = "minor" ]; then
    MINOR=$((MINOR + 1))
    PATCH=0
    VERSION="${MAJOR}.${MINOR}.${PATCH}"
    echo -e "${YELLOW}🔼 Bumping MINOR version: ${CURRENT_VERSION} → ${VERSION}${NC}"
elif [ "$BUMP_TYPE" = "patch" ]; then
    PATCH=$((PATCH + 1))
    VERSION="${MAJOR}.${MINOR}.${PATCH}"
    echo -e "${YELLOW}🔼 Bumping PATCH version: ${CURRENT_VERSION} → ${VERSION}${NC}"
elif [ "$BUMP_TYPE" != "none" ]; then
    echo -e "${RED}❌ Invalid bump type or version: ${BUMP_TYPE}${NC}"
    echo "Usage: ./release.sh [major|minor|patch|X.Y.Z]"
    echo "  major: 0.8.15 → 1.0.0"
    echo "  minor: 0.8.15 → 0.9.0"
    echo "  patch: 0.8.15 → 0.8.16"
    echo "  X.Y.Z: explicitly set version (e.g. 1.0.6)"
    echo "  (no arg): Use current version"
    exit 1
else
    echo -e "${GREEN}📦 Using current version: ${VERSION}${NC}"
fi

# Ensure working tree is completely clean before proceeding
if ! git diff --quiet || ! git diff --cached --quiet; then
    echo -e "${RED}❌ Working tree is not clean. Please commit or stash all changes before running the release script.${NC}"
    echo "Tip: run 'git status' to see pending changes."
    exit 1
fi

# Final confirmation before doing anything destructive
echo -e "\n${YELLOW}You are about to run a release for version v${VERSION}.${NC}"
echo -e "${YELLOW}This will clean builds, optionally update MatchZy.cs, commit, push, and create a GitHub release + tag.${NC}"
CURRENT_BRANCH=$(git branch --show-current 2>/dev/null || echo "unknown-branch")
echo -e "${YELLOW}Current Git branch: ${CURRENT_BRANCH}${NC}"
read -rp "$(echo -e \"${YELLOW}Continue with release v${VERSION}? [y/N]: ${NC}\")" CONFIRM_RELEASE
case "$CONFIRM_RELEASE" in
    y|Y|yes|YES)
        echo -e "${GREEN}Proceeding with release v${VERSION}...${NC}"
        ;;
    *)
        echo -e "${RED}Aborting release v${VERSION}. No changes were made.${NC}"
        exit 0
        ;;
esac

# Check if tag already exists (local)
if git rev-parse "v${VERSION}" >/dev/null 2>&1; then
    echo -e "${RED}❌ Tag v${VERSION} already exists!${NC}"
    echo "Please bump to a new version or delete the existing tag:"
    echo "  git tag -d v${VERSION}"
    echo "  git push origin :refs/tags/v${VERSION}"
    exit 1
fi

# Update version in MatchZy.cs BEFORE building (so the build includes the new version)
if [ "$BUMP_TYPE" != "none" ]; then
    echo -e "\n${BLUE}📝 Updating version in MatchZy.cs to ${VERSION}...${NC}"
    # Use different sed syntax for Linux vs macOS
    if [[ "$OSTYPE" == "darwin"* ]]; then
        sed -i "" "s/ModuleVersion => \\\".*\\\"/ModuleVersion => \\\"${VERSION}\\\"/" src/MatchZy.cs
    else
        sed -i "s/ModuleVersion => \\\".*\\\"/ModuleVersion => \\\"${VERSION}\\\"/" src/MatchZy.cs
    fi
    echo -e "${GREEN}✓ Updated MatchZy.cs to version ${VERSION}${NC}"
fi

# Clean previous builds
echo -e "\n${BLUE}🧹 Cleaning previous builds...${NC}"
rm -rf build/ bin/ obj/

# Restore dependencies
echo -e "\n${BLUE}📥 Restoring dependencies...${NC}"
dotnet restore

# Build and publish (now with the updated version)
echo -e "\n${BLUE}🔨 Building project (Release mode)...${NC}"
dotnet publish -c Release

# Create release directory structure under build/
RELEASE_DIR="MatchZy-${VERSION}"
BUILD_ROOT="build"
rm -rf "${BUILD_ROOT}/${RELEASE_DIR}" "${BUILD_ROOT}/${RELEASE_DIR}.zip"
mkdir -p "${BUILD_ROOT}/${RELEASE_DIR}/addons/counterstrikesharp/plugins/MatchZy"
mkdir -p "${BUILD_ROOT}/${RELEASE_DIR}/cfg/MatchZy"

# Copy plugin files to proper directory structure
echo -e "\n${BLUE}📂 Creating directory structure...${NC}"
cp -r build/Release/net8.0/publish/* "${BUILD_ROOT}/${RELEASE_DIR}/addons/counterstrikesharp/plugins/MatchZy/"

# Copy config files
echo -e "${BLUE}📂 Copying config files...${NC}"
cp -r cfg/MatchZy/* "${BUILD_ROOT}/${RELEASE_DIR}/cfg/MatchZy/"

# Create zip file
echo -e "\n${BLUE}🗜️  Creating release archive...${NC}"
mkdir -p "${BUILD_ROOT}"
(
  cd "${BUILD_ROOT}" && zip -r -q "${RELEASE_DIR}.zip" "${RELEASE_DIR}"
)

# Get file size for display
SIZE=$(du -h "${BUILD_ROOT}/${RELEASE_DIR}.zip" | cut -f1)
echo -e "${GREEN}✓ Created ${BUILD_ROOT}/${RELEASE_DIR}.zip (${SIZE})${NC}"

# Set default repo for gh CLI (if not already set)
REPO_URL=$(git remote get-url origin | sed -E "s|.*github.com[:/](.*).git|\\1|")
gh repo set-default "$REPO_URL" 2>/dev/null || true

echo -e "\n${BLUE}💾 Committing changes...${NC}"
git add .
git commit -m "Release v${VERSION}"

echo -e "\n${BLUE}📝 Generating changelog for GitHub release...${NC}"

# Changelog based on commits between the previous tag (if any) and HEAD
prev_tag=$(git tag --sort=-v:refname | head -n 1 || echo "")

log_range=""
if [ -n "$prev_tag" ]; then
    log_range="${prev_tag}..HEAD"
else
    # First tag in the repo – use all reachable commits
    log_range="HEAD"
fi

CHANGELOG=$(git log ${log_range} --pretty=format:"- %s" 2>/dev/null | grep -viE "^- Release v[0-9]+\.[0-9]+\.[0-9]+$" || true)
if [ -z "$CHANGELOG" ] || [ ${#CHANGELOG} -lt 10 ]; then
    CHANGELOG="- Release v${VERSION}"
fi

RELEASE_NOTES=$(cat <<EOF
## Changelog

${CHANGELOG}

## Installation

1. Download \`${RELEASE_DIR}.zip\`
2. Extract the contents to your CS2 server game/csgo/ directory
   - The zip contains the proper folder structure (\`addons/\` and \`cfg/\`)
3. Restart your server

## Requirements

- CounterStrikeSharp (latest version)
- CS2 Dedicated Server

## Configuration

Config files are located in \`csgo/cfg/MatchZy/\`:
- \`config.cfg\` - Main plugin configuration
- \`admins.json\` - Admin permissions
- \`database.json\` - Database settings
- \`live.cfg\`, \`warmup.cfg\`, \`knife.cfg\` - Match configs
EOF
)

# Push release commit (but no tag yet). This ensures the commit exists on GitHub
# before we ask GitHub to create a tag/release pointing at it.
echo -e "\n${BLUE}⬆️  Pushing release commit to origin...${NC}"
CURRENT_BRANCH=$(git branch --show-current)
git push origin "$CURRENT_BRANCH"

# Create GitHub release (this will also create tag vX.Y.Z on GitHub if it doesn't exist)
echo -e "\n${BLUE}🌟 Creating GitHub release (and tag v${VERSION})...${NC}"
gh release create "v${VERSION}" \
    "${BUILD_ROOT}/${RELEASE_DIR}.zip" \
    --title "MatchZy v${VERSION}" \
    --notes "$RELEASE_NOTES" \
    --draft=false \
    --latest

# Optional: send Discord webhook notification if configured
if [ -x "./discord-webhook.sh" ]; then
    echo -e "\n${BLUE}🔔 Sending Discord release notification (if configured)...${NC}"
    if [ -n "${DISCORD_WEBHOOK_URL:-}" ]; then
        ./discord-webhook.sh "${VERSION}" || echo -e "${YELLOW}⚠️ Discord webhook failed, continuing without stopping release.${NC}"
    else
        echo -e "${YELLOW}DISCORD_WEBHOOK_URL not set; skipping Discord notification.${NC}"
    fi
fi

# Cleanup
echo -e "\n${BLUE}🧹 Cleaning up temporary files...${NC}"
rm -rf "${BUILD_ROOT:?}/${RELEASE_DIR}"

echo -e "\n${GREEN}✅ Release v${VERSION} published successfully!${NC}"
echo -e "${GREEN}🔗 View release: https://github.com/${REPO_URL}/releases/tag/v${VERSION}${NC}"
