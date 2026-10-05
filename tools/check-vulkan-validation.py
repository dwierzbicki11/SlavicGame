#!/usr/bin/env python3
"""Fail a Vulkan runtime proof on validation errors or a missing proof log."""
import sys
from pathlib import Path

log = Path(sys.argv[1]).read_text()
errors = [line for line in log.splitlines() if 'Validation Error:' in line]
if errors:
    print('\n'.join(errors), file=sys.stderr)
    raise SystemExit(f'Vulkan validation reported {len(errors)} errors.')
if 'Scene FG runtime proof:' not in log or 'checks passed;' not in log:
    raise SystemExit('The renderer scene proof did not complete.')
print('Vulkan validation: zero errors; renderer scene proof completed.')
