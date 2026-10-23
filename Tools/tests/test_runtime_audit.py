"""A restart must not erase a failed native readability check."""
import json
import sys
from pathlib import Path

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from ui_map import archive_review, review_issues


def audit(path, issues):
    (path / 'text-overflows.json').write_text(json.dumps({'Issues': issues}))


def test_cold_restart_cannot_hide_earlier_clipped_text(tmp_path):
    clipped = {'Text': '3 VISITS / STEP', 'Screen': 'board'}
    audit(tmp_path, [clipped])
    (tmp_path / 'performance.json').write_text('{"Frames":100}')
    (tmp_path / 'actual-mix.wav').write_bytes(b'first-session-audio')
    archive_review(tmp_path)
    audit(tmp_path, [])
    (tmp_path / 'actual-mix.wav').write_bytes(b'second-session-audio')
    archive_review(tmp_path)
    assert clipped in review_issues(tmp_path)
    assert (tmp_path / 'session-0-actual-mix.wav').read_bytes() == b'first-session-audio'
    assert (tmp_path / 'session-1-actual-mix.wav').read_bytes() == b'second-session-audio'


def test_current_process_failure_is_checked_even_before_it_is_archived(tmp_path):
    audit(tmp_path, [])
    archive_review(tmp_path)
    clipped = {'Text': 'CURRENT FAILURE', 'Screen': 'empire'}
    audit(tmp_path, [clipped])
    assert clipped in review_issues(tmp_path)


def test_missing_or_malformed_audit_is_never_an_empty_pass(tmp_path):
    with pytest.raises(FileNotFoundError):
        review_issues(tmp_path)
    (tmp_path / 'text-overflows.json').write_text('interrupted-write')
    with pytest.raises(json.JSONDecodeError):
        review_issues(tmp_path)


def test_clean_sessions_still_pass(tmp_path):
    audit(tmp_path, [])
    archive_review(tmp_path)
    audit(tmp_path, [])
    assert review_issues(tmp_path) == []
