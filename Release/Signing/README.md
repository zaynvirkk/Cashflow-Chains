# Local upload key

A new RSA-4096 upload key was prepared for this new app. The JKS and randomly generated passwords remain in the ignored `.local-signing/` directory with restricted permissions. Do not commit them or share them in chat. Back up that directory privately before relying on the key for Play uploads.

`upload-certificate.pem` is the public certificate. It is safe to supply to Play Console. `python3 Tools/sign-release.py` signs the built APK and AAB without putting password values in command-line arguments or logs. It preserves the existing local key.

Google Play App Signing setup and actual upload are separate operator actions. No account or app was created in Play Console.
