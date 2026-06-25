.PHONY: feature fix push build test up down help

# Default target when you just run 'make'
help:
	@echo "================================================="
	@echo "          Trading Forge System - Makefile        "
	@echo "================================================="
	@echo "Available commands:"
	@echo "  make feature name=<name>  - Pulls main and creates a new feature branch"
	@echo "                              (e.g., make feature name=TFS-42-new-api)"
	@echo "  make fix name=<name>      - Pulls main and creates a new bugfix branch"
	@echo "                              (e.g., make fix name=TFS-43-login-bug)"
	@echo "  make push                 - Pushes the current branch to GitHub"
	@echo ""
	@echo "  make build                - Builds the .NET solution"
	@echo "  make test                 - Runs the .NET tests"
	@echo ""
	@echo "  make up                   - Starts Docker (Database + API) in the background"
	@echo "  make down                 - Stops the Docker containers"
	@echo "================================================="

# --- Git Workflow ---
feature:
	@if [ -z "$(name)" ]; then echo "Error: Please provide a name (e.g., make feature name=my-feature)"; exit 1; fi
	git checkout main
	git pull origin main
	git checkout -b feature/$(name)
	@echo " Created and switched to branch: feature/$(name)"

fix:
	@if [ -z "$(name)" ]; then echo "Error: Please provide a name (e.g., make fix name=my-fix)"; exit 1; fi
	git checkout main
	git pull origin main
	git checkout -b fix/$(name)
	@echo " Created and switched to branch: fix/$(name)"

push:
	git push -u origin HEAD
	@echo " Branch pushed to origin!"

# --- .NET ---
build:
	dotnet build TradingForgeSystem.sln

test:
	dotnet test TradingForgeSystem.sln

# --- Docker ---
up:
	docker-compose up -d
	@echo " Docker containers are spinning up in the background!"

down:
	docker-compose down
	@echo " Docker containers stopped."
