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

# Check for version bump argument
BUMP_TYPE="${1:-none}"
VERSION="$CURRENT_VERSION"

if [ "$BUMP_TYPE" = "major" ]; then
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
    echo -e "${RED}❌ Invalid bump type: ${BUMP_TYPE}${NC}"
    echo "Usage: ./release.sh [major|minor|patch]"
    echo "  major: 0.8.15 → 1.0.0"
    echo "  minor: 0.8.15 → 0.9.0"
    echo "  patch: 0.8.15 → 0.8.16"
    echo "  (no arg): Use current version"
    exit 1
else
    echo -e "${GREEN}📦 Using current version: ${VERSION}${NC}"
fi

# Update version in MatchZy.cs if bumped
if [ "$BUMP_TYPE" != "none" ]; then
    sed -i "" "s/ModuleVersion => \\\".*\\\"/ModuleVersion => \\\"${VERSION}\\\"/" src/MatchZy.cs
    echo -e "${GREEN}✓ Updated MatchZy.cs to version ${VERSION}${NC}"
fi

# Check if tag already exists
if git rev-parse "v${VERSION}" >/dev/null 2>&1; then
    echo -e "${RED}❌ Tag v${VERSION} already exists!${NC}"
    echo "Please bump to a new version or delete the existing tag:"
    echo "  git tag -d v${VERSION}"
    echo "  git push origin :refs/tags/v${VERSION}"
    exit 1
fi

# Clean previous builds
echo -e "\n${BLUE}🧹 Cleaning previous builds...${NC}"
rm -rf build/ bin/ obj/

# Restore dependencies
echo -e "\n${BLUE}📥 Restoring dependencies...${NC}"
dotnet restore

# Build and publish
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

# Commit changes
echo -e "\n${BLUE}💾 Committing changes...${NC}"
git add .
git commit -m "Release v${VERSION}"

# Create and push tag
echo -e "\n${BLUE}🏷️  Creating Git tag v${VERSION}...${NC}"
CURRENT_BRANCH=$(git branch --show-current)
git tag -a "v${VERSION}" -m "Release version ${VERSION}"
git push origin "$CURRENT_BRANCH"
git push origin "v${VERSION}"

# Set default repo for gh CLI (if not already set)
REPO_URL=$(git remote get-url origin | sed -E "s|.*github.com[:/](.*).git|\\1|")
gh repo set-default "$REPO_URL" 2>/dev/null || true

echo -e "\n${BLUE}📝 Generating changelog for GitHub release...${NC}"

# Simple changelog based on recent commits (excluding previous release tags)
CHANGELOG=$(git log -n 20 --pretty=format:"- %s" 2>/dev/null | grep -viE "^Release v[0-9]+\.[0-9]+\.[0-9]+$" || true)
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

# Create GitHub release
echo -e "\n${BLUE}🌟 Creating GitHub release...${NC}"
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
