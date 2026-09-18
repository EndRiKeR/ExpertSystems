#!/usr/bin/env bash
# Рендерит .dot-файл в PNG рядом с ним через Graphviz.
# Использование: ./render_graph.sh Examples/graph.dot [Examples/graph.png]
set -euo pipefail

if [ $# -lt 1 ]; then
    echo "Использование: $0 <graph.dot> [graph.png]" >&2
    exit 1
fi

dot_file="$1"
png_file="${2:-${dot_file%.*}.png}"

if ! command -v dot >/dev/null 2>&1; then
    echo "Graphviz (dot) не найден в PATH — PNG не создан" >&2
    exit 1
fi

dot -Tpng "$dot_file" -o "$png_file"
echo "PNG сохранён в $png_file"
