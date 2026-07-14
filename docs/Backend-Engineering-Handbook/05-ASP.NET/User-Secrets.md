# .NET User Secrets

## What Are User Secrets?

.NET user secrets store local Development settings outside the project directory.

They help prevent sensitive values from being accidentally committed to Git.

Examples include:

- Local API keys
- Development database passwords
- JWT signing keys
- OAuth client secrets used during local development

## Real-World Analogy

Think of the project as a shared maintenance manual and user secrets as a technician's locked drawer. The manual contains a label identifying which drawer belongs to the project, but the key material stays in the drawer rather than being photocopied into every copy of the manual.

## What User Secrets Are Not

User secrets are not an encrypted production vault.

They are stored in a file under the local user profile. Their main protection is that the file is outside the repository.

Use them for Development, not as a production secret-management system.

## `UserSecretsId`

The project file contains something resembling:

```xml
<UserSecretsId>unique-project-id</UserSecretsId>
```

This value is safe to commit. It is an identifier that tells .NET which local secrets file belongs to the project.

It is not a password and does not reveal the secret values.

Initialize user secrets with:

```bash
dotnet user-secrets init
```

This project already has a `UserSecretsId` because `dotnet user-jwts` configured local JWT support.

## Set a Secret

Use a colon to represent a hierarchical key:

```bash
dotnet user-secrets set \
  "Example:ApiKey" \
  "secret-value"
```

The equivalent logical configuration structure is:

```json
{
  "Example": {
    "ApiKey": "secret-value"
  }
}
```

Avoid placing secret values directly in shell history when a safer input method is available for a real credential.

## List Secrets Carefully

This command displays both keys and values:

```bash
dotnet user-secrets list
```

Do not paste its unmasked output into chat, issues, documentation, or logs.

On Linux or macOS, mask the values before sharing the key names:

```bash
dotnet user-secrets list |
  sed -E 's/( = ).*/\1[hidden]/'
```

The project currently shows JWT signing-key fields resembling:

```text
Authentication:Schemes:Bearer:SigningKeys:0:Value = [hidden]
Authentication:Schemes:Bearer:SigningKeys:0:Length = [hidden]
Authentication:Schemes:Bearer:SigningKeys:0:Issuer = [hidden]
Authentication:Schemes:Bearer:SigningKeys:0:Id = [hidden]
```

`SigningKeys:0` means the first item in a list of signing keys.

## Remove One Secret

Remove a key with:

```bash
dotnet user-secrets remove \
  "Example:ApiKey"
```

## Clear All Secrets

This deletes every user secret associated with the project:

```bash
dotnet user-secrets clear
```

Use this carefully. In this project, it would also delete the local JWT signing-key configuration, causing development JWTs to stop working until recreated.

## Configuration Priority

In Development, user secrets are loaded after JSON configuration and therefore override matching JSON keys.

```text
appsettings.json
    ↓ overridden by
appsettings.Development.json
    ↓ overridden by
User secrets
```

Environment variables and command-line arguments can still override user secrets.

## Temporary Pagination Demonstration

This command stores a Development-only override:

```bash
dotnet user-secrets set \
  "Pagination:DefaultPageSize" \
  "4"
```

While it exists:

```text
appsettings.json = 10
User secret      = 4
Effective value  = 4
```

Remove the demonstration value with:

```bash
dotnet user-secrets remove \
  "Pagination:DefaultPageSize"
```

The effective value then returns to 10.

Pagination is not actually secret. This is only a safe way to demonstrate how the provider overrides another configuration source.

## User Secrets and Environments

ASP.NET web projects automatically load user secrets when the environment is Development and a `UserSecretsId` exists.

They are not automatically used as a production secret store.

```text
Development
    → Local user secrets are available

Production
    → Use deployment configuration or a production vault
```

## Local JWT Signing Keys

The `dotnet user-jwts` tool stores its signing key through the user-secrets system.

The repository may safely contain local issuer and audience configuration:

```json
{
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "ValidAudiences": [
          "http://localhost:5090"
        ],
        "ValidIssuer": "dotnet-user-jwts"
      }
    }
  }
}
```

The signing-key value itself remains outside the repository.

## What Is Safe to Commit?

Usually safe:

- `UserSecretsId`
- Configuration key names
- Public issuer names
- Public audience names
- Code that reads secret configuration

Do not commit:

- Secret values
- JWT signing keys
- Database passwords
- API keys
- Client secrets
- Complete access tokens

## Production Secrets

Production secrets should be provided through a controlled deployment system.

For Azure, common choices include:

- Azure App Settings for deployment configuration
- Azure Key Vault for controlled secret storage
- Managed identities to reduce stored credentials

Development and Production should not share the same credentials.

## Common Mistakes

### Believing the secrets file is encrypted

The Secret Manager keeps values outside the project but does not promise encrypted storage.

### Sharing unmasked command output

`dotnet user-secrets list` prints values. Mask them before sharing diagnostic output.

### Committing a copied secret elsewhere

Moving a secret out of `appsettings.json` does not help if it is then pasted into a README, test file, shell script, or issue.

### Clearing JWT secrets accidentally

`dotnet user-secrets clear` removes every key, including keys created by `dotnet user-jwts`.

## Key Points

```text
UserSecretsId
    → Safe project identifier

User secrets file
    → Local values outside the repository

Development
    → Loads user secrets automatically

Production
    → Requires a real deployment secret system
```

## Reference

- [Safe storage of app secrets in development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
