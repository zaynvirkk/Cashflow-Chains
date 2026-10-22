from pathlib import Path
import os
from .app import create_app
state=os.environ.get('CC2_STATE_DIRECTORY')
if not state:raise RuntimeError('Set CC2_STATE_DIRECTORY to a private writable directory')
app=create_app(Path(state)/'cc2.sqlite')
