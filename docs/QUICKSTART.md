# Quick Start Guide - EventiveAPI

Get the EventiveAPI backend running in 5 minutes!

## Prerequisites Checklist

- ✅ Node.js 20+ installed
- ✅ npm or pnpm installed
- ✅ Supabase account and project
- ✅ Supabase credentials ready

## 5-Minute Setup

### Step 1: Install Dependencies (1 min)

```bash
cd EventiveAPI
npm install
```

### Step 2: Configure Environment (2 min)

```bash
# Copy example environment file
cp .env.example .env
```

Edit `.env` file:

```env
# Required - Get from Supabase Dashboard
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_SERVICE_ROLE_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
SUPABASE_ANON_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

# Optional - Use defaults
PORT=3001
NODE_ENV=development
CORS_ORIGIN=http://localhost:5173
```

**Where to find Supabase credentials:**
1. Go to https://supabase.com/dashboard
2. Select your project
3. Go to Settings → API
4. Copy `URL` and `service_role` key (NOT the anon key for SERVICE_ROLE_KEY)

### Step 3: Start Server (30 sec)

```bash
npm run dev
```

You should see:
```
🚀 EventiveAPI server running on port 3001
📝 Environment: development
🔗 Health check: http://localhost:3001/health
📚 API docs: http://localhost:3001/docs
```

### Step 4: Test It Works (1 min)

```bash
# Test health endpoint
curl http://localhost:3001/health
```

Expected response:
```json
{
  "status": "ok",
  "timestamp": "2025-11-04T...",
  "uptime": 1.234,
  "environment": "development"
}
```

### Step 5: View API Docs (30 sec)

Open in browser: http://localhost:3001/docs

---

## 🎉 Done! Your backend is running!

## Next Steps

### Test with Your Frontend

1. **Add API URL to frontend .env:**
   ```bash
   # In tsa-repository/.env
   VITE_API_URL=http://localhost:3001
   ```

2. **Start both servers:**
   ```bash
   # Terminal 1 - Backend
   cd EventiveAPI
   npm run dev

   # Terminal 2 - Frontend
   cd tsa-repository/tsa-project
   npm run dev
   ```

3. **Test authentication:**
   - Login via frontend
   - Open browser console
   - Check Network tab for API calls to localhost:3001

### Test API Endpoints

#### Get User Profile (requires auth)

```bash
# 1. Get JWT token from frontend (login first)
# 2. Copy token from localStorage or browser dev tools
# 3. Use in API call:

curl -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  http://localhost:3001/api/users/me
```

Expected response:
```json
{
  "success": true,
  "data": {
    "id": "...",
    "username": "johndoe",
    "display_name": "John Doe",
    "avatar_url": "...",
    "bio": "...",
    "role": "user",
    "settings": {},
    "created_at": "...",
    "updated_at": "..."
  }
}
```

#### Upload Avatar

```bash
curl -X POST \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "avatar=@/path/to/image.jpg" \
  http://localhost:3001/api/uploads/avatar
```

Expected response:
```json
{
  "success": true,
  "data": {
    "url": "https://...supabase.co/storage/v1/object/public/avatars/..."
  }
}
```

---

## Troubleshooting

### "Module not found" errors
```bash
rm -rf node_modules package-lock.json
npm install
```

### "Port 3001 already in use"
```bash
# Option 1: Change PORT in .env
PORT=3002

# Option 2: Kill process using port 3001
# macOS/Linux:
lsof -ti:3001 | xargs kill -9

# Windows:
netstat -ano | findstr :3001
taskkill /PID <PID> /F
```

### "Cannot connect to Supabase"
- Verify `SUPABASE_URL` is correct
- Verify `SUPABASE_SERVICE_ROLE_KEY` is the **service_role** key (not anon key!)
- Check your internet connection
- Verify Supabase project is not paused

### "Authentication failed"
- Make sure you're using a valid JWT token
- Token must be from Supabase Auth (login via frontend first)
- Check token in Authorization header: `Bearer <token>`
- Token expires after 1 hour - get a new one

### TypeScript errors
```bash
npm run type-check
npm run build
```

---

## Development Tips

### Hot Reload
Changes to `.ts` files automatically restart the server (thanks to `tsx watch`).

### Logging
All requests are logged to console:
```
POST /api/users/me 200 45.123 ms - 234
```

### Environment
- `development` - Detailed logs, stack traces
- `production` - Minimal logs, no stack traces

### Testing
```bash
npm test              # Run all tests
npm run test:unit     # Unit tests only
npm run test:coverage # With coverage
```

---

## Docker Quick Start

Prefer Docker? Here's the 3-minute setup:

```bash
# 1. Copy environment file
cp .env.example .env
# Edit .env with your Supabase credentials

# 2. Start with Docker Compose
docker-compose up -d

# 3. Check logs
docker-compose logs -f api

# 4. Test
curl http://localhost:3001/health
```

Stop:
```bash
docker-compose down
```

---

## API Endpoints Reference

### Public Endpoints
- `GET /health` - Health check
- `GET /docs` - API documentation

### Protected Endpoints (Require JWT)
- `GET /api/users/me` - Get current user
- `PUT /api/users/me` - Update current user
- `GET /api/settings` - Get settings
- `PUT /api/settings` - Update settings
- `POST /api/uploads/avatar` - Upload avatar
- `DELETE /api/uploads/avatar` - Delete avatar

### Future Endpoints
- `GET /api/events` - List events
- `POST /api/events` - Create event
- `GET /api/events/:id` - Get event
- `PUT /api/events/:id` - Update event
- `DELETE /api/events/:id` - Delete event

---

## VS Code Setup (Optional)

Recommended extensions:
- ESLint
- Prettier
- REST Client

Create `.vscode/settings.json`:
```json
{
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll.eslint": true
  }
}
```

---

## Production Deployment

See full deployment guides:
- Docker: `ARCHITECTURE.md`
- Railway/Render: Check their docs
- AWS/GCP: See `docs/deployment/`

Basic steps:
1. Set `NODE_ENV=production` in .env
2. Build: `npm run build`
3. Start: `npm start`
4. Configure reverse proxy (Nginx/Caddy)
5. Set up SSL/TLS
6. Configure environment variables on hosting platform

---

## Need Help?

1. Check `SETUP_SUMMARY.md` for detailed info
2. Check `ARCHITECTURE.md` for system design
3. Check `MIGRATION.md` for frontend integration
4. Check `CONTRIBUTING.md` for development guidelines
5. Open an issue on GitHub

---

## Summary

✅ Backend is running on http://localhost:3001
✅ Health check works
✅ Ready to integrate with frontend
✅ Ready to add more features

**Next:** Read `MIGRATION.md` to integrate with your frontend!
