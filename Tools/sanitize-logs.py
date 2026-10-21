#!/usr/bin/env python3
"""Remove transient Unity Hub credentials from local build evidence."""
from pathlib import Path
import re,sys
for arg in sys.argv[1:]:
 p=Path(arg)
 if not p.is_file():continue
 s=p.read_text(errors='replace')
 s=re.sub(r'(?mi)(^-accessToken\s*\n)[^\r\n]+',r'\1[REDACTED]',s)
 s=re.sub(r'(?i)(-accessToken[= \t]+)[^\s"\']+',r'\1[REDACTED]',s)
 s=re.sub(r'(?i)(Bearer\s+)[A-Za-z0-9._-]+',r'\1[REDACTED]',s)
 p.write_text(s)
