# Makefile for SquadComplete

# Variables
API_DIR = squad-api
DRAFT_DIR = squad-draft
FUNC_DIR = squad-func

.PHONY: all build-api build-draft build-func trim-branches clean help

# Default target
all: build-api build-draft build-func

# Help
help:
	@echo "SquadComplete Makefile"
	@echo "----------------------"
	@echo "all            - Build everything"
	@echo "build-api      - Build the squad-api project"
	@echo "build-draft    - Build the squad-draft project"
	@echo "build-func     - Build the squad-func project"
	@echo "trim-branches  - Delete merged git branches"
	@echo "clean          - Clean all project artifacts"

# Build commands
build-api:
	@echo "Building squad-api..."
	dotnet build $(API_DIR)

build-draft:
	@echo "Building squad-draft..."
	@cd $(DRAFT_DIR) && npm run build

build-func:
	@echo "Building squad-func..."
	dotnet build $(FUNC_DIR)

# Git branch cleanup
# This will delete all local branches that have been merged into the current branch, 
# excluding common main branches and the current one.
trim-branches:
	@echo "Trimming merged git branches..."
	@merged_branches=$$(git branch --merged | grep -Ev "^\*|master|main|dev"); \
	if [ -n "$$merged_branches" ]; then \
		echo "$$merged_branches" | xargs git branch -d; \
	else \
		echo "No merged branches to trim."; \
	fi

trim-prune-branches:
	@echo "Trimming merged git branches and pruning remote branches..."
	@merged_branches=$$(git branch --merged | grep -Ev "^\*|master|main|dev"); \
	if [ -n "$$merged_branches" ]; then \
		echo "$$merged_branches" | xargs git branch -d; \
	else \
		echo "No merged branches to trim."; \
	fi
	@git fetch --prune


# Clean artifacts
clean:
	@echo "Cleaning artifacts..."
	@rm -rf $(DRAFT_DIR)/dist $(DRAFT_DIR)/node_modules
	@dotnet clean $(API_DIR)
	@dotnet clean $(FUNC_DIR)
