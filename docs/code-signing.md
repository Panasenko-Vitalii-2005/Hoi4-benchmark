# Code Signing Plan

## Purpose

This document defines the code-signing strategy for HOI4 Benchmark Windows releases.

Code signing is used to:

- identify the software publisher;
- verify that released binaries have not been modified;
- improve user confidence when downloading and running the application;
- reduce Windows SmartScreen warnings as publisher reputation is established.

## Current release status

### Version 0.1.0

The first public release will be distributed as an **unsigned self-contained Windows x64 ZIP archive**.

The release pipeline produces:

```text
HOI4Benchmark-v0.1.0-win-x64.zip
HOI4Benchmark-v0.1.0-win-x64.zip.sha256
```

The ZIP archive contains the published application and all required .NET and native runtime dependencies.

No executable or DLL inside the archive is digitally signed for version 0.1.0.

## Expected Windows behavior

Because version 0.1.0 is unsigned, Windows Defender SmartScreen may display an **“Windows protected your PC”** or **“Unknown publisher”** warning.

Users may need to:

1. click **More info**;
2. verify that the publisher is shown as unknown;
3. verify the downloaded SHA-256 checksum;
4. click **Run anyway** only if the file was downloaded from the official GitHub Releases page.

The project must not claim that an unsigned release is trusted or verified by Windows.

## Release integrity

Every public release must include a SHA-256 checksum file.

Example:

```text
HOI4Benchmark-v0.1.0-win-x64.zip.sha256
```

Users can verify the archive in PowerShell:

```powershell
Get-FileHash `
  .\HOI4Benchmark-v0.1.0-win-x64.zip `
  -Algorithm SHA256
```

The returned hash must match the value published with the GitHub Release.

Release archives must only be distributed through the official GitHub Releases page.

## Signing target

When code signing is introduced, the release pipeline should sign at least:

```text
HOI4Benchmark.App.exe
```

The following project-owned DLLs may also be signed:

```text
HOI4Benchmark.App.dll
HOI4Benchmark.Application.dll
HOI4Benchmark.Domain.dll
HOI4Benchmark.Infrastructure.dll
```

Third-party framework and NuGet package binaries must not be re-signed.

The ZIP archive itself is not signed with Authenticode. Its integrity remains protected by the published SHA-256 checksum.

## Signing technology

Windows releases should use **Microsoft Authenticode**.

The recommended signing tool is:

```text
signtool.exe
```

`SignTool` is provided by the Windows SDK and supports signing, signature verification, and timestamping.

The signing algorithm should be:

```text
SHA-256
```

Example signing command:

```powershell
signtool sign `
  /fd SHA256 `
  /f "$certificatePath" `
  /p "$certificatePassword" `
  /tr "$timestampUrl" `
  /td SHA256 `
  "artifacts\publish\win-x64\HOI4Benchmark.App.exe"
```

The exact timestamp URL must be provided by the selected certificate authority.

## Timestamping

Every production signature must include an RFC 3161 timestamp.

Timestamping allows a signature to remain valid after the code-signing certificate expires, provided the certificate was valid when the file was signed.

A release must fail if timestamping fails.

Unsigned or non-timestamped binaries must not be published as signed production releases.

## Certificate requirements

The project should obtain a publicly trusted Windows code-signing certificate before enabling production signing.

Possible approaches include:

- standard organization validation code-signing certificate;
- extended validation code-signing certificate;
- managed cloud signing service;
- Microsoft Trusted Signing, if available and appropriate for the project.

The selected certificate must:

- support Windows Authenticode;
- use SHA-256;
- be issued to the project owner or publishing organization;
- be valid for public software distribution;
- support secure automated CI usage.

A self-signed certificate may be used only for local development and workflow testing. It must never be used for a public release.

## Private-key security

The certificate private key must never be:

- committed to the repository;
- included in a release archive;
- uploaded as a workflow artifact;
- written to application logs;
- stored in plain text;
- shared through issues or pull requests.

If a password-protected PFX certificate is used, GitHub Actions should store:

```text
WINDOWS_SIGNING_CERTIFICATE_BASE64
WINDOWS_SIGNING_CERTIFICATE_PASSWORD
WINDOWS_TIMESTAMP_URL
```

as encrypted repository or environment secrets.

The release workflow should:

1. decode the certificate into a temporary file;
2. sign the published executable;
3. verify the signature;
4. delete the temporary certificate file;
5. create the ZIP only after signature verification succeeds.

The certificate should preferably be exposed only to a protected GitHub environment named:

```text
release
```

The environment may require manual approval before secrets become available.

## Planned GitHub Actions flow

Future signed release workflow:

```text
checkout
→ setup .NET and Windows SDK
→ restore
→ build
→ test
→ publish win-x64
→ load signing certificate
→ sign application executable
→ verify Authenticode signature
→ create ZIP
→ generate SHA-256 checksum
→ upload release assets
→ remove temporary certificate material
```

Example verification command:

```powershell
signtool verify `
  /pa `
  /v `
  "artifacts\publish\win-x64\HOI4Benchmark.App.exe"
```

The release job must stop immediately if signature verification fails.

## Signing order

The required order is:

```text
build
→ publish
→ sign
→ verify signature
→ archive
→ calculate checksum
→ upload
```

Files must never be modified after signing.

Changing a signed executable invalidates its signature.

## Logging rules

Signing logs may contain:

- executable path;
- certificate subject;
- certificate thumbprint;
- timestamp server;
- verification result.

Signing logs must never contain:

- certificate password;
- PFX contents;
- Base64 certificate value;
- private key material;
- full GitHub secret values.

## Certificate rotation

Before the certificate expires:

1. obtain a replacement certificate;
2. update the protected GitHub secrets;
3. test signing on a non-public workflow run;
4. verify the new publisher identity;
5. publish subsequent releases using the replacement certificate.

Where possible, the same publisher identity should be maintained across certificate renewals to preserve user trust and SmartScreen reputation.

## Revocation response

If the signing certificate or private key is compromised:

1. stop all release workflows;
2. revoke the certificate through the issuing certificate authority;
3. remove or rotate affected GitHub secrets;
4. inspect previous workflow runs and release artifacts;
5. notify users through a GitHub security advisory or release notice;
6. obtain a new certificate;
7. rebuild and re-sign affected releases when appropriate.

Previously published binaries must not be silently replaced.

## Decision for version 0.1.0

Version 0.1.0 will be:

```text
unsigned
self-contained
Windows x64 only
distributed as ZIP
published with SHA-256 checksum
downloadable only from GitHub Releases
```

The README and release notes must clearly state that the executable is unsigned and may trigger a Windows SmartScreen warning.

## Future milestone

Code signing should be introduced before one of the following occurs:

- the project begins regular public distribution;
- a Windows installer is introduced;
- download volume becomes significant;
- users report repeated SmartScreen trust concerns;
- the project is distributed outside GitHub Releases;
- an organization or sponsor funds a trusted certificate.

Until that milestone, checksums and GitHub-hosted release artifacts are the official integrity mechanism.
