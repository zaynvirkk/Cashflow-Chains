# CC2 authoritative service slice

This is a locally tested, authenticated construction/production/verified-round service. It uses the **same pure C# domain** as Unity. SQLite commits account inventory, ledger, projections and request receipt together under `BEGIN IMMEDIATE`. It is not deployed, connected to the public game, or accepted for live payments.

Implemented endpoints: guest session, rotating access/refresh tokens, account snapshot, versioned manifest, account-bound expiring puzzle challenges, server replay/first-clear rewards, construction start/finish, capped production collection. Unknown grants, client clocks/balances/account IDs, locked challenges and unverified expansions are rejected. Exact request retries return the original result; changed payloads conflict. Missing or invalid Google Play verification returns503 with no currency grant or IAP confirmation.

Run tests from project root:

```bash
python3 -m pytest -q Backend/tests --junitxml=Artifacts/CC2/Backend/tests.xml
```

The development bridge can compile using Unity's pinned compiler/runtime for local tests. For supported independent hosting, build the same source using .NET10 LTS:

```bash
DOTNET_CLI_TELEMETRY_OPTOUT=1 dotnet publish Backend/DomainService/CC2Tools.csproj -c Release -o Builds/BackendDomain
```

Set `CC2_DOMAIN_DLL` to the absolute published DLL and `CC2_DOTNET` to the supported dotnet host. Then:

```bash
python3 -m venv Backend/.venv
Backend/.venv/bin/pip install -r Backend/requirements.txt
CC2_STATE_DIRECTORY="$PWD/.local-backend" CC2_DOMAIN_DLL="$PWD/Builds/BackendDomain/CC2Tools.dll" Backend/.venv/bin/uvicorn cc2.start:app --app-dir Backend --host 127.0.0.1 --port 8765 --no-access-log
```

Keep the state directory private and back up the SQLite database with SQLite's backup API. Server token/signing material is generated into that private directory; never ship it in Unity. The token database stores digests. No payment receipts or personal contact data are accepted by this slice. Dependency pins are in requirements.txt; runtime versions are recorded in test reports.

Before any public deployment: configure HTTPS/edge request limits, persistent storage/backups and restore drills, managed credentials, external account recovery, durable purchase verification/RTDN/refunds, operational alerts, catalog migrations and privacy/age/region controls. Domain/catalog hash drift refuses database startup until an explicit migration. Current guest recovery is session-token based, not linked cross-device recovery. New paid commerce remains disabled. No hosting account, deployment or spend has occurred.
