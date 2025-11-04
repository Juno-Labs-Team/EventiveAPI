# Eventive Architecture

This document describes the complete architecture of the Eventive platform after backend separation.

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌───────────────────────────────────────────────────────┐      │
│  │              React Frontend (tsa-project)             │      │
│  │                                                        │      │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐           │      │
│  │  │  Pages   │  │Components│  │ Contexts │           │      │
│  │  └──────────┘  └──────────┘  └──────────┘           │      │
│  │                                                        │      │
│  │  ┌──────────────────────────────────────────┐        │      │
│  │  │        API Client (apiClient.ts)         │        │      │
│  │  └──────────────────────────────────────────┘        │      │
│  │              │                         │              │      │
│  │              │                         │              │      │
│  │          REST API              Supabase Client       │      │
│  │              │                  (Auth & Realtime)     │      │
│  └──────────────┼─────────────────────────┼─────────────┘      │
│                 │                         │                     │
└─────────────────┼─────────────────────────┼─────────────────────┘
                  │                         │
                  ▼                         ▼
┌─────────────────────────────┐   ┌──────────────────────────┐
│   BACKEND LAYER (EventiveAPI)│   │   SUPABASE SERVICES      │
├─────────────────────────────┤   ├──────────────────────────┤
│                             │   │                          │
│  ┌────────────────────┐    │   │  ┌────────────────────┐  │
│  │   Express Server   │    │   │  │   Auth Service     │  │
│  │   (Node.js 20)     │    │   │  │   (OAuth/JWT)      │  │
│  └────────────────────┘    │   │  └────────────────────┘  │
│            │                │   │                          │
│            │                │   │  ┌────────────────────┐  │
│  ┌─────────┴─────────┐     │   │  │   Storage Service  │  │
│  │   Middleware      │     │   │  │   (File Uploads)   │  │
│  │  - Authentication │     │   │  └────────────────────┘  │
│  │  - Rate Limiting  │     │   │                          │
│  │  - Error Handling │     │   │  ┌────────────────────┐  │
│  └───────────────────┘     │   │  │   Realtime         │  │
│            │                │   │  │   (WebSockets)     │  │
│  ┌─────────┴─────────┐     │   │  └────────────────────┘  │
│  │   API Routes      │     │   │                          │
│  │  - /api/auth      │     │   └──────────┬───────────────┘
│  │  - /api/users     │     │              │
│  │  - /api/settings  │     │              │
│  │  - /api/uploads   │     │              │
│  │  - /api/events    │     │              │
│  └───────────────────┘     │              │
│            │                │              │
│            ▼                │              │
│  ┌─────────────────────┐   │              │
│  │  Supabase Client    │───┼──────────────┘
│  │  (Service Role)     │   │
│  └─────────────────────┘   │
│            │                │
└────────────┼────────────────┘
             │
             ▼
┌─────────────────────────────┐
│    DATABASE LAYER            │
├─────────────────────────────┤
│                             │
│   PostgreSQL (Supabase)     │
│                             │
│   ┌─────────────────────┐   │
│   │   profiles          │   │
│   │   - id              │   │
│   │   - username        │   │
│   │   - display_name    │   │
│   │   - avatar_url      │   │
│   │   - bio             │   │
│   │   - role            │   │
│   │   - settings (JSON) │   │
│   └─────────────────────┘   │
│                             │
│   ┌─────────────────────┐   │
│   │   events (future)   │   │
│   │   - id              │   │
│   │   - title           │   │
│   │   - description     │   │
│   │   - start_date      │   │
│   │   - end_date        │   │
│   │   - creator_id      │   │
│   └─────────────────────┘   │
│                             │
│   Row Level Security (RLS)  │
│                             │
└─────────────────────────────┘
```

## Request Flow

### 1. Authentication Flow

```
User Action → Frontend
              │
              ├─→ OAuth (Google/Discord)
              │   │
              │   ▼
              │   Supabase Auth
              │   │
              │   ├─→ Create Session
              │   ├─→ Generate JWT
              │   └─→ Return to Frontend
              │
              └─→ Store JWT in localStorage
                  │
                  └─→ Include in API requests
                      │
                      ▼
                  Backend API
                      │
                      ├─→ Validate JWT (middleware)
                      ├─→ Check user permissions
                      └─→ Execute request
```

### 2. Profile Update Flow

```
User Input → Frontend
              │
              ├─→ Validate data
              │
              ├─→ GET JWT from session
              │
              ▼
          API Request
       PUT /api/users/me
       Authorization: Bearer <JWT>
       Body: { display_name, username, bio }
              │
              ▼
          Backend API
              │
              ├─→ authenticateUser middleware
              │   ├─→ Verify JWT with Supabase
              │   └─→ Attach user to req.user
              │
              ├─→ Validate input data
              │
              ├─→ Update database
              │   └─→ Supabase Client (Service Role)
              │       └─→ PostgreSQL UPDATE
              │
              └─→ Return response
                  │
                  ▼
              Frontend
                  │
                  ├─→ Update UI
                  └─→ Show success message
```

### 3. File Upload Flow

```
User Selects File → Frontend
                     │
                     ├─→ Validate file (size, type)
                     │
                     ├─→ Create FormData
                     │
                     ▼
                 API Request
            POST /api/uploads/avatar
            Authorization: Bearer <JWT>
            Content-Type: multipart/form-data
            Body: FormData with file
                     │
                     ▼
                 Backend API
                     │
                     ├─→ authenticateUser middleware
                     │
                     ├─→ multer middleware (parse file)
                     │
                     ├─→ Validate file
                     │
                     ├─→ Delete old avatars
                     │   └─→ Supabase Storage
                     │
                     ├─→ Upload new avatar
                     │   └─→ Supabase Storage
                     │
                     ├─→ Get public URL
                     │
                     ├─→ Update profile.avatar_url
                     │   └─→ PostgreSQL UPDATE
                     │
                     └─→ Return URL
                         │
                         ▼
                     Frontend
                         │
                         ├─→ Update UI with new avatar
                         └─→ Show success message
```

## Data Flow

```
┌──────────────┐
│   Frontend   │
│  (React)     │
└──────┬───────┘
       │
       │ HTTP/REST (JSON)
       │
       ▼
┌──────────────┐      ┌──────────────┐
│   Backend    │─────→│   Supabase   │
│  (Express)   │←─────│   (Service)  │
└──────────────┘      └──────┬───────┘
                             │
                             ▼
                      ┌──────────────┐
                      │  PostgreSQL  │
                      │  (Database)  │
                      └──────────────┘
```

## Security Layers

```
┌─────────────────────────────────────────┐
│  1. Frontend Validation                 │
│     - Input validation                  │
│     - File type/size checks             │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│  2. API Gateway (Backend)               │
│     - CORS                              │
│     - Rate Limiting                     │
│     - Helmet (Security Headers)         │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│  3. Authentication Middleware           │
│     - JWT validation                    │
│     - User verification                 │
│     - Role checks                       │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│  4. Business Logic                      │
│     - Input sanitization                │
│     - Authorization checks              │
│     - Data validation (Zod)             │
└─────────────────┬───────────────────────┘
                  │
┌─────────────────▼───────────────────────┐
│  5. Database Layer                      │
│     - Row Level Security (RLS)          │
│     - Prepared statements               │
│     - Constraints & triggers            │
└─────────────────────────────────────────┘
```

## Deployment Architecture

```
┌─────────────────────────────────────────────────────┐
│                  PRODUCTION                          │
├─────────────────────────────────────────────────────┤
│                                                      │
│  ┌──────────────────┐         ┌──────────────────┐ │
│  │   CDN / Nginx    │         │   Load Balancer  │ │
│  │  (Frontend)      │         │   (Backend)      │ │
│  └────────┬─────────┘         └────────┬─────────┘ │
│           │                            │           │
│  ┌────────▼─────────┐         ┌────────▼─────────┐ │
│  │  Docker Container│         │  Docker Container│ │
│  │  (React Build)   │         │  (Express API)   │ │
│  │                  │         │                  │ │
│  │  Port: 80        │         │  Port: 3001      │ │
│  └──────────────────┘         └──────────────────┘ │
│                                        │           │
│                                ┌───────▼────────┐  │
│                                │   Supabase     │  │
│                                │   - Auth       │  │
│                                │   - Database   │  │
│                                │   - Storage    │  │
│                                └────────────────┘  │
│                                                      │
└─────────────────────────────────────────────────────┘

Environment URLs:
- Frontend: https://eventive.app
- Backend:  https://api.eventive.app
- Supabase: https://xyz.supabase.co
```

## Component Communication

### Frontend Components

```
App.tsx
  │
  ├── AuthProvider (Context)
  │   │
  │   └── Manages: user, profile, auth methods
  │
  ├── ToastProvider (Context)
  │   │
  │   └── Manages: toast notifications
  │
  ├── Navbar
  │   │
  │   └── Uses: AuthContext, apiClient
  │
  ├── OnboardingModal
  │   │
  │   └── Uses: AuthContext, apiClient, ToastContext
  │
  └── Routes
      │
      ├── Home (Public)
      ├── Login (Public)
      ├── Account (Protected)
      │   └── Uses: AuthContext, apiClient, ToastContext
      └── Settings (Protected)
          └── Uses: AuthContext, apiClient, ToastContext
```

### Backend Components

```
index.ts (Entry Point)
  │
  ├── Middleware Stack
  │   ├── helmet (security)
  │   ├── cors (CORS)
  │   ├── compression (gzip)
  │   ├── morgan (logging)
  │   ├── express.json (body parsing)
  │   └── express.urlencoded (form parsing)
  │
  ├── Routes
  │   ├── /api/auth → auth.routes.ts
  │   │   └── Uses: authLimiter
  │   │
  │   ├── /api/users → user.routes.ts
  │   │   └── Uses: authenticateUser, supabase
  │   │
  │   ├── /api/settings → settings.routes.ts
  │   │   └── Uses: authenticateUser, supabase
  │   │
  │   └── /api/uploads → upload.routes.ts
  │       └── Uses: authenticateUser, multer, supabase
  │
  └── Error Handlers
      ├── notFoundHandler (404)
      └── errorHandler (500)
```

## Technology Stack Comparison

### Before (Monolithic)
```
tsa-repository/
├── tsa-project/ (Frontend + Backend logic)
│   ├── Frontend: React
│   ├── "Backend": Direct Supabase calls
│   └── Auth: Supabase client SDK
```

### After (Separated)
```
tsa-repository/              EventiveAPI/
├── Frontend: React          ├── Backend: Express
├── API Client              ├── Auth Middleware
└── Auth: Supabase OAuth    ├── Business Logic
                            └── Supabase Service Client
```

## Benefits of Separation

### 1. Security
- ✅ Frontend can't bypass validation
- ✅ Centralized authentication
- ✅ Service role key never exposed
- ✅ Rate limiting at API level

### 2. Maintainability
- ✅ Clear separation of concerns
- ✅ Easier to test each layer
- ✅ Independent deployment
- ✅ Better code organization

### 3. Scalability
- ✅ Backend can scale independently
- ✅ Add caching layer easily
- ✅ Multiple frontends can use same API
- ✅ Easier to add features

### 4. Flexibility
- ✅ Can switch databases easily
- ✅ Can add multiple frontends (mobile, etc.)
- ✅ Can add GraphQL layer
- ✅ Can add message queues

---

This architecture provides a solid foundation for building a production-ready event management platform!
