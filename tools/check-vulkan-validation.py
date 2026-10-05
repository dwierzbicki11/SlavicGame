#!/usr/bin/env python3
"""Fail a Vulkan runtime proof on validation errors or a missing proof log."""
import sys
from pathlib import Path

log = Path(sys.argv[1]).read_text()
errors = [line for line in log.splitlines() if 'Validation Error:' in line]
if errors:
    print('\n'.join(errors), file=sys.stderr)
    raise SystemExit(f'Vulkan validation reported {len(errors)} errors.')
proof = ('Upscaler orientation runtime proof:' if '--upscaler-orientation' in sys.argv[2:]
         else 'Presentation FG runtime proof:' if '--presentation' in sys.argv[2:]
         else 'Scene FG runtime proof:')
if proof not in log or 'checks passed;' not in log:
    raise SystemExit('The required renderer proof did not complete: ' + proof)
print('Vulkan validation: zero errors; renderer proof completed: ' + proof)
