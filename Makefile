SHELL := /bin/bash
.DEFAULT_GOAL := help

DOTNET ?= dotnet
TEST_PROJECT ?= ./tests/Truffleware.CodeAnalysis.Tests/Truffleware.CodeAnalysis.Tests.csproj
SNAPSHOT_DIR ?= ./tests/Truffleware.CodeAnalysis.Tests/Generators/Snapshots

.PHONY: help restore build test test-update-snapshots clean

help:
	@echo "Available targets:"
	@echo "  restore                Restore NuGet packages"
	@echo "  build                  Build all projects"
	@echo "  test                   Run tests"
	@echo "  test-update-snapshots  Run tests and update Verify snapshots"
	@echo "  clean                  Clean build outputs"

restore:
	$(DOTNET) restore

build: restore
	$(DOTNET) build --no-restore

docs: clean-docs
	docfx build docs/docfx.json

docs-full:
	# Generates new metadata, which is slow
	docfx docs/docfx.json

docs-serve: docs
	docfx serve docs/_site

test: build
	$(DOTNET) test --project $(TEST_PROJECT) --no-build

test-renew-snapshots: build
	@rm -f $(SNAPSHOT_DIR)/* && \
		$(DOTNET) test --project $(TEST_PROJECT) --no-build >/dev/null 2>&1; \
		rename.ul '.received.cs' '.verified.cs' $(SNAPSHOT_DIR)/* && \
		echo -e '\n\033[32mSnapshots updated.\033[0m'

clean:
	$(DOTNET) clean

clean-docs:
	git clean -df docs/_site/ docs/api/
