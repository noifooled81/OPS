#!/bin/bash

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_ROOT="$REPO_ROOT/.test_logs/$(date +%Y%m%d_%H%M%S)"
SUMMARY_LOG="$LOG_ROOT/summary.log"

mkdir -p "$LOG_ROOT"
cd "$REPO_ROOT/tests" || exit 1

echo "Starting test run: $(date)" > "$SUMMARY_LOG"

for service_dir in */ ; do
    service_name=$(basename "$service_dir")

    for module_dir in "$service_dir"*/ ; do
        [ -d "$module_dir" ] || continue

        module_name=$(basename "$module_dir")

        module_out_dir="$LOG_ROOT/$service_name"
        mkdir -p "$module_out_dir"

        trx_file="$module_out_dir/$module_name.trx"

        # run tests with trx output
        dotnet test "$module_dir" \
            --logger "trx;LogFileName=$module_name.trx" \
            --results-directory "$module_out_dir" \
            > /dev/null 2>&1

        exit_code=$?

        if [ $exit_code -ne 0 ]; then
            echo "[FAILED] $service_name / $module_name -> $trx_file" >> "$SUMMARY_LOG"
        else
            echo "[PASSED] $service_name / $module_name -> $trx_file" >> "$SUMMARY_LOG"
        fi
    done
done

echo "Test run complete. Logs: $LOG_ROOT"
