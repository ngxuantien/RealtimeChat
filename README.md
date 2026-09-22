# RealtimeChat API

Backend for a realtime chat application: REST API (ASP.NET Core) + SignalR for realtime features, MongoDB as the database.

## Tech stack

- **.NET 10 / ASP.NET Core Web API**
- **MongoDB** (MongoDB.Driver) — no EF Core, data access goes through a hand-written repository pattern
- **SignalR** — realtime features (messages, typing indicator, online status, reactions...)
- **JWT Bearer** — authentication (access token + refresh token)
- **BCrypt.Net** — password hashing
- **Cloudinary** — image/attachment storage
- **MailKit/MimeKit** — email sending (password reset)
- **Serilog** — logging (console sink)
- **Swashbuckle (Swagger)** — API docs

## Architecture

Follows a Clean Architecture style, split into 4 projects:

```
RealtimeChat.API            # Controllers, Hubs, Middlewares, DI extensions, entry point (Program.cs)
RealtimeChat.Application     # Service interfaces + implementations, DTOs, business logic
RealtimeChat.Domain          # Entities, enums — has no dependency on any other project
RealtimeChat.Infrastructure  # Infrastructure implementations: Mongo repositories, JWT, email, file storage (Cloudinary)
```

Dependency direction: `API → Infrastructure → Application → Domain` (Domain knows nothing about the layers above it).

### Folder structure

```
RealtimeChat.API/
├── Controllers/        # AuthController, UsersController, ConversationsController,
│                        # ConversationMembersController, MessagesController
├── Hubs/ChatHub.cs      # SignalR hub
├── Middlewares/         # ExceptionMiddleware (global error handling)
├── Extentions/          # DI registration grouped by concern (Mongo, Auth, CORS, Email, FileStorage, RateLimiting, Swagger)
└── Program.cs

RealtimeChat.Application/
├── Service/             # Business logic (UserService, MessageService, ConversationService, AuthService...)
├── Service/Interfaces/
├── DTOs/                # Request/response models grouped by module
├── Repositories/Interfaces/
└── Utils/               # Helpers (MessagePreviewHelper...)

RealtimeChat.Domain/
└── Entities/            # User, Conversation, ConversationMember, Message, MessageReaction,
                          # MessageAttachment, UserConnection, BaseEntity

RealtimeChat.Infrastructure/
├── Repositories/        # MongoRepository<T>, UnitOfWork
├── Services/            # JwtService, EmailSender (SMTP), CloudinaryFileStorageService
```

## Prerequisites

- .NET SDK 10.0
- MongoDB (local or Atlas)
- A Cloudinary account (avatar/attachment storage)
- An SMTP server (Gmail App Password or another SMTP provider) — used for password-reset emails

## Setup & run

1. Copy the sample config file:
   ```bash
   cp RealtimeChat.API/appsettings.Example.json RealtimeChat.API/appsettings.json
   ```
2. Fill in real values in `appsettings.json` (see [Configuration](#configuration) below). **This file is gitignored and must never be committed.**
3. Start MongoDB (defaults to `mongodb://localhost:27017` for a local instance).
4. Restore & run:
   ```bash
   dotnet restore
   dotnet run --project RealtimeChat.API
   ```
5. Swagger UI: `https://localhost:{port}/swagger`

## Configuration

Main keys in `appsettings.json`:

| Section | Key | Purpose |
|---|---|---|
| `MongoDbSettings` | `ConnectionString`, `DatabaseName` | MongoDB connection |
| `Jwt` | `Key`, `Issuer`, `Audience` | Signs/verifies access tokens. `Key` must be a sufficiently long random string — **never use the sample value** |
| `Cloudinary` | `CloudName`, `ApiKey`, `ApiSecret` | Uploads avatars and message attachments |
| `Smtp` | `Host`, `Port`, `Username`, `Password`, `FromEmail`, `FromName` | Sends password-reset emails. If using a personal Gmail account, use an App Password, not the account password |
| `App` | `FrontendBaseUrl` | FE base URL, used to build links in emails (e.g. the reset-password link) |

> Never commit real secrets. When rotating values, edit `appsettings.json`/`appsettings.Development.json` only (already gitignored) and keep `appsettings.Example.json` as a placeholder template.

## API overview

Base path: `/api`

### Auth (`/api/auth`)
| Method | Route | Notes |
|---|---|---|
| POST | `/login` | Rate limited: 5 requests/minute/IP |
| POST | `/logout/{userId}` | Requires token |
| POST | `/refresh-token` | |
| POST | `/forgot-password` | Rate limited: 3 requests/10 minutes/IP |
| POST | `/reset-password` | |

### Users (`/api/users`)
| Method | Route | Notes |
|---|---|---|
| POST | `/` | Register (multipart, includes avatar) |
| GET | `/{id}`, `/phone/{phoneNumber}`, `/`, `/search` | |
| PUT | `/{id}` | Update profile |
| POST | `/{id}/avatar` | |
| DELETE | `/{id}` | |

### Conversations (`/api/conversations`)
| Method | Route | Notes |
|---|---|---|
| POST | `/private`, `/group` | Create a 1-1 / group conversation |
| GET | `/user/{userId}`, `/{id}` | |
| PUT | `/{id}` | Rename group / change group avatar |
| POST | `/{id}/avatar` | |
| POST | `/{conversationId}/leave/{userId}` | Leave a group |
| DELETE | `/{conversationId}/user/{userId}` | Delete history for that user only |
| DELETE | `/{conversationId}/group` | Delete/disband a group |

### Conversation members (`/api/conversations/{conversationId}/members`)
| Method | Route | Notes |
|---|---|---|
| GET | `/` | List members |
| POST | `/` | Add a member |
| DELETE | `/{userId}` | Remove/kick a member |
| PATCH | `/{userId}/role` | Change role (admin/member) |
| PATCH | `/{userId}/read` | Mark as read |
| PATCH | `/mute` | Toggle mute notifications |
| GET | `/{userId}/unread-count` | |

### Messages (`/api/messages`)
| Method | Route | Notes |
|---|---|---|
| POST | `/` | Send a message |
| GET | `/conversation/{conversationId}?page=&pageSize=` | Fetch messages (paginated, newest first) |
| PUT | `/{messageId}/user/{userId}` | Edit a message — allowed only within **24h** of sending |
| DELETE | `/{messageId}/user/{userId}` | Unsend/recall a message — allowed only within **24h** of sending |
| POST | `/attachments` | Upload an attachment (~11MB limit) |
| GET | `/conversation/{conversationId}/attachments` | |
| POST | `/{messageId}/reactions` | Toggle a reaction (1 emoji per user per message) |

## Realtime — SignalR Hub

Endpoint: `/chatHub` (requires a JWT passed via query string on connect).

**Client → Hub methods:**
- `JoinConversation(conversationId)` / `LeaveConversation(conversationId)`
- `Typing(conversationId, userId)` / `StopTyping(conversationId, userId)`
- `MessageSeen(conversationId, userId, messageId)`

**Server → Client broadcasts:**
- `ReceiveMessage`, `MessageEdited`, `MessageDeleted`, `MessageRead`, `MessageReactionUpdated`
- `ConversationUpdated`, `ConversationInfoUpdated`
- `UserTyping`, `UserStoppedTyping`
- `UserOnlineStatusChanged`, `UserJoinedConversation`, `UserLeftConversation`

Every user automatically joins a personal group `user-{userId}` on connect (used for notifications that don't depend on which conversation is open), and joins a group per `conversationId` when opening a chat.

## Security

- JWT access token + refresh token, authenticated via `Authorization: Bearer`.
- IP-based rate limiting on `login` (5 req/min) and `forgot-password` (3 req/10 min) to mitigate brute-force attacks.
- Editing/unsending a message is limited to 24h after it was sent (enforced in `MessageService`).
- Passwords are hashed with BCrypt, never stored in plaintext.
- `appsettings.json`/`appsettings.*.json` holding real secrets are gitignored — only `appsettings.Example.json` is committed.

## Implementation notes

- No automated test project yet; manual testing is done via Swagger or `RealtimeChat.API/wwwroot/test-chat.html` (a simple SignalR test page).
- When running under the Visual Studio debugger, C# changes need Hot Reload or a debug session restart to take effect on the running server.
