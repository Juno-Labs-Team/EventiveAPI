refactoring to C#

# EventiveAPI

Backend API server for the Eventive event management platform.

## Overview

EventiveAPI is a RESTful API server that handles:
- User authentication (OAuth with Google/Discord)
- User profile management
- File uploads (avatars, media)
- Event CRUD operations
- Database management

## Tech Stack

- **Runtime**: Node.js 20+
- **Framework**: Express.js or Fastify (TBD)
- **Database**: PostgreSQL (via Supabase)
- **Authentication**: Supabase Auth
- **Storage**: Supabase Storage
- **TypeScript**: Full type safety
- **ORM**: Prisma or Drizzle (TBD)

## Architecture

```
EventiveAPI/
├── src/
│   ├── config/          # Configuration (database, auth, etc.)
│   ├── routes/          # API route definitions
│   │   ├── auth/        # Authentication routes
│   │   ├── users/       # User profile routes
│   │   ├── events/      # Event management routes
│   │   └── uploads/     # File upload routes
│   ├── controllers/     # Request handlers
│   ├── services/        # Business logic
│   ├── middleware/      # Auth, validation, error handling
│   ├── models/          # Database models/schemas
│   ├── utils/           # Helper functions
│   └── types/           # TypeScript types
├── prisma/              # Database schema & migrations
├── tests/               # Unit & integration tests
├── docker/              # Docker configuration
└── docs/                # API documentation
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
- Node.js 20+
- PostgreSQL (or Supabase account)
- npm or pnpm

### Installation

```bash
# Clone the repository
git clone https://github.com/Juno-Labs-Team/EventiveAPI.git
cd EventiveAPI

# Install dependencies
npm install

# Set up environment variables
cp .env.example .env
# Edit .env with your credentials

# Run database migrations
npm run db:migrate

# Start development server
npm run dev
```

### Environment Variables

```env
# Server
PORT=3001
NODE_ENV=development
CORS_ORIGIN=http://localhost:5173

# Supabase
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_SERVICE_ROLE_KEY=your-service-role-key
SUPABASE_ANON_KEY=your-anon-key

# JWT
JWT_SECRET=your-jwt-secret

# Storage
AVATAR_BUCKET=avatars
MAX_FILE_SIZE=5242880
```

## Development

```bash
# Start dev server with hot reload
npm run dev

# Run tests
npm test

# Lint code
npm run lint

# Type check
npm run type-check

# Build for production
npm run build

# Start production server
npm start
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
- Swagger UI: `http://localhost:3001/docs`
- OpenAPI spec: `http://localhost:3001/api-docs.json`

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

- All routes (except public endpoints) require JWT authentication
- Row Level Security (RLS) enabled on all tables
- File uploads validated for type and size
- Rate limiting on all endpoints
- CORS configured for frontend origin only
- Input validation with Zod or Joi

## Testing

```bash
# Run all tests
npm test

# Run unit tests
npm run test:unit

# Run integration tests
npm run test:integration

# Run with coverage
npm run test:coverage
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines.

## License

See [LICENSE](LICENSE) file for details.

---

**Status**: 🚧 In Development

This backend is being built to support the Eventive frontend application.
