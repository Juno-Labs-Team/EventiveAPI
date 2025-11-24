finished refactoring to C#, updating documentation soon

# EventiveAPI

Backend API server for the Eventive event management platform.

## Overview

EventiveAPI is a RESTful API server that handles:
- User authentication (OAuth with Google/Discord via Supabase)
- User profile management
- File uploads (avatars, media)
- Event CRUD operations
- Database management

## Tech Stack

- **Runtime**: .NET 8.0
- **Framework**: ASP.NET Core
- **Database**: PostgreSQL (via Supabase)
- **Authentication**: Supabase Auth with custom middleware
- **Storage**: Supabase Storage
- **Language**: C# with full type safety
- **ORM**: Supabase C# Client (Postgrest)

## Architecture

```
EventiveAPI/
├── Controllers/         # API endpoints and request handlers
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── SettingsController.cs
│   ├── UploadsController.cs
│   └── HealthController.cs
├── Services/            # Business logic
│   └── SupabaseService.cs
├── Middleware/          # Auth, validation, error handling
│   ├── AuthenticationMiddleware.cs
│   └── ErrorHandlingMiddleware.cs
├── Models/              # Database models and DTOs
│   ├── UserModels.cs
│   └── ApiResponse.cs
├── Configuration/       # Configuration classes
│   └── SupabaseConfig.cs
├── Validators/          # FluentValidation validators
├── Tests/               # Unit & integration tests (xUnit)
├── Properties/          # Launch settings
├── Program.cs           # Application entry point
├── Dockerfile           # Docker configuration
└── appsettings.json     # Configuration
```

## API Endpoints (Planned)

### Authentication
- `POST /api/auth/callback/google` - Google OAuth callback
- `POST /api/auth/callback/discord` - Discord OAuth callback
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Sign out user

### Users
- `GET /api/users/me` - Get current user profile
- `PUT /api/users/me` - Update current user profile
- `GET /api/users/:id` - Get user by ID (public profiles)
- `POST /api/users/avatar` - Upload user avatar
- `DELETE /api/users/avatar` - Delete user avatar

### Events (Future)
- `GET /api/events` - List events
- `POST /api/events` - Create event
- `GET /api/events/:id` - Get event details
- `PUT /api/events/:id` - Update event
- `DELETE /api/events/:id` - Delete event

### Settings
- `GET /api/settings` - Get user settings
- `PUT /api/settings` - Update user settings

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- PostgreSQL (or Supabase account)
- Docker (optional)

### Installation

```bash
# Clone the repository
git clone https://github.com/Juno-Labs-Team/EventiveAPI.git
cd EventiveAPI

# Restore dependencies
dotnet restore

# Set up environment variables
cp .env.example .env
# Edit .env with your Supabase credentials

# Run the application
dotnet run
```

### Environment Variables

```env
# Server
PORT=3000
ASPNETCORE_ENVIRONMENT=Development

# Supabase
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_SERVICE_ROLE_KEY=your-service-role-key
SUPABASE_ANON_KEY=your-anon-key

# CORS
CORS_ORIGINS=http://localhost:5173

# Storage
AVATAR_BUCKET=avatars
MAX_FILE_SIZE=5242880

# Rate Limiting
ENABLE_RATE_LIMITING=true
RATE_LIMIT_PERMITS=100
RATE_LIMIT_WINDOW_MINUTES=15
```

## Development

```bash
# Start dev server with hot reload
dotnet watch run

# Run tests
dotnet test

# Build for production
dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o ./publish

# Run published application
dotnet ./publish/EventiveAPI.CSharp.dll
```

## Docker Deployment

```bash
# Build image
docker build -t eventive-api .

# Run container
docker run -p 3001:3001 --env-file .env eventive-api

# Or use docker-compose
docker-compose up -d
```

## API Documentation

Once the server is running, API documentation is available at:
- Swagger UI: `http://localhost:3000/swagger`
- OpenAPI spec: `http://localhost:3000/swagger/v1/swagger.json`

## Database Schema

The API uses Supabase PostgreSQL with the following main tables:

### profiles
```sql
id              UUID PRIMARY KEY (references auth.users)
username        TEXT UNIQUE
display_name    TEXT
avatar_url      TEXT
bio             TEXT
role            TEXT DEFAULT 'user'
settings        JSONB DEFAULT '{}'
created_at      TIMESTAMPTZ DEFAULT NOW()
updated_at      TIMESTAMPTZ DEFAULT NOW()
```

### events (Future)
```sql
id              UUID PRIMARY KEY DEFAULT uuid_generate_v4()
title           TEXT NOT NULL
description     TEXT
start_date      TIMESTAMPTZ NOT NULL
end_date        TIMESTAMPTZ NOT NULL
location        TEXT
creator_id      UUID REFERENCES profiles(id)
created_at      TIMESTAMPTZ DEFAULT NOW()
updated_at      TIMESTAMPTZ DEFAULT NOW()
```

## Security

- All routes (except public endpoints) require JWT authentication via Supabase
- Row Level Security (RLS) enabled on all tables
- File uploads validated for type and size
- Rate limiting on all endpoints (configurable)
- CORS configured for frontend origin only
- Input validation with FluentValidation
- Security headers (X-Content-Type-Options, X-Frame-Options, etc.)
- Request size limits

## Testing

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test Tests/
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines.

## License

See [LICENSE](LICENSE) file for details.

---

**Status**: 🚧 In Development

This backend is being built to support the Eventive frontend application.
