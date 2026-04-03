# Previous Work Log

---

## Session: 2026-04-03 — Google Sign-In SDK Migration

### Overview
Migrated Google login from GPGS (Play Games) flow to Google Sign-In SDK flow so that the standard Google account picker UI appears on button press.

### Problem Diagnosed
1. `Unity.Tasks.dll` / `Unity.Compat.dll` in `Assets/Parse/Plugins/` conflicted with `mscorlib` and `netstandard` after google-signin-unity package import → CS0433 type ambiguity errors across UniTask, InputSystem, TMP, etc.
2. Previous `GoogleAuthService` used `PlayGamesAuthProvider` which requires Firebase **Play Games** provider — user had only enabled **Google** provider.
3. Previous early-return on `_auth.CurrentUser != null` would skip Firebase sign-in silently.

### Fix Applied
- **Deleted** `Assets/Parse/Plugins/Unity.Tasks.dll` and `Unity.Compat.dll` (+ .meta files) → resolved all CS0433 errors.
- **Rewrote** `Assets/Scripts/Core/Auth/GoogleAuthService.cs` using `Google.GoogleSignIn` SDK:
  - Constructor sets `GoogleSignIn.Configuration` with `WebClientId`, `RequestIdToken = true`, `UseGameSignIn = false`
  - `SignInWithGoogleAsync()`: `GoogleSignIn.DefaultInstance.SignIn()` → idToken → `GoogleAuthProvider.GetCredential(idToken, null)` → `SignInWithCredentialAsync`
  - `SignOut()` calls both `_auth.SignOut()` and `GoogleSignIn.DefaultInstance.SignOut()`
- Removed all GPGS (`PlayGamesPlatform`, `PlayGamesAuthProvider`) references from the auth service.

### Firebase Console Required
- Authentication → Sign-in method → **Google** provider must be enabled (already done by user).
- SHA-1 fingerprint of the Android keystore must be registered in Firebase project settings.

### Pending (Requires Unity Editor Action)
- Add `NewUserSetupPresenter` + `NewUserSetupView` components to a GameObject in the `GameTest` scene
- Assign `_starterSpirit` ScriptableObject in Inspector
- Wire up `_panel`, `_nicknameInput`, `_confirmButton`, `_errorText` in `NewUserSetupView` Inspector
- Unity Inspector: Assign `GoogleLoginButton` in `LoginView` component in `GoogleLogin` scene
