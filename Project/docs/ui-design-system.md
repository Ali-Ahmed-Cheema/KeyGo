# KeyGo UI Design System

## Direction

KeyGo is a compact developer workspace: quiet, information-dense, local-first, and explicit about privacy. The primary visual signal is teal action color against a charcoal work surface. Panels frame tools and repeated information; the shell itself remains open and uncluttered.

## Layout

- 68px top bar for identity, project, model, and privacy state.
- 210px navigation rail for workspace and system sections.
- Flexible center work area for the active workflow.
- 260px inspector rail for context, status, and privacy details.
- 32px status bar for readiness and current operation.
- Minimum desktop window size: 1050x650.

## Typography

- Segoe UI is the Windows-native application typeface.
- Page titles use 28-34px semibold text.
- Section labels use 11px semibold uppercase text with increased contrast.
- Body text uses 14-16px with wrapping for diagnostics and provider messages.
- Do not use display-scale text inside compact panels.

## Color Tokens

- Window: `#101318`
- Panel: `#171B22`
- Raised panel: `#1E242D`
- Border: `#2A3340`
- Primary text: `#EDF2F7`
- Muted text: `#9BA8B8`
- Accent: `#5CD6C0`
- Accent surface: `#173D3A`
- Danger: `#F07878`
- Warning: `#E6B86B`

All colors are centralized in `src/KeyGo.App/Resources/Theme.xaml`.

## Components

- Navigation buttons are compact text controls with a large keyboard target.
- Primary buttons are reserved for actions that change state or begin work.
- Secondary buttons are used for inspection, refresh, and navigation.
- Provider cards expose connection state without exposing credentials.
- Chat messages distinguish user and assistant surfaces while keeping content selectable.
- Inspector content must show context scope, provider, model, and privacy state before cloud work.

## State Rules

Every asynchronous operation exposes a visible state: idle, loading, streaming, success, error, or cancelled. Errors use understandable language and retain the technical message for diagnostics. Buttons disable while their operation is running.

## Accessibility

- Controls use visible text labels and standard WPF controls for keyboard and screen-reader support.
- Password input is handled by `PasswordBox` and is never rendered back to the UI.
- Focusable controls use the standard Windows focus behavior.
- Status text is kept near the operation that owns it.
- Color is paired with text for provider, privacy, and error states.

## Integration Boundaries

Views render view models. View models call app services. App services compose `KeyGo.Core`. Filesystem scanning, provider calls, credentials, context selection, and agent investigation remain outside controls.

## Current Scope

The current shell connects real project indexing, provider verification/model discovery, streaming chat, conversation persistence, and read-only agent investigation. Diff review, file transaction approval, build/test execution, and a full code editor remain separate integration slices and should be added as view models over their existing Core services rather than implemented in XAML code-behind.
