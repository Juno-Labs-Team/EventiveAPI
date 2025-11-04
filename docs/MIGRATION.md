# Migration Guide: Moving Backend to EventiveAPI

This guide explains how to migrate backend functionality from `tsa-repository` to the new `EventiveAPI` backend.

## Overview

The goal is to separate concerns:
- **tsa-repository** → Frontend React app only
- **EventiveAPI** → Backend API server

## What Moves to EventiveAPI

### ✅ Backend Logic (Already Moved)
- Authentication routes (`/api/auth/*`)
- User profile management (`/api/users/*`)
- Settings management (`/api/settings/*`)
- File upload handling (`/api/uploads/*`)
- Supabase server-side operations
- Middleware (auth, rate limiting, error handling)

### 🔄 What Stays in Frontend
- React components
- UI/UX logic
- Client-side routing
- Styling (CSS)
- Supabase client SDK (for real-time features)

## Frontend Changes Required

### 1. Update Supabase Client Configuration

**Old (tsa-repository/tsa-project/src/lib/supabaseClient.ts):**
```typescript
import { createClient } from '@supabase/supabase-js';

export const supabase = createClient(supabaseUrl, supabaseAnonKey, {
  auth: {
    persistSession: true,
    autoRefreshToken: true,
    detectSessionInUrl: true,
  },
});
```

**New:** Keep the same, but auth operations will call the API.

### 2. Create API Client

Create `tsa-project/src/lib/apiClient.ts`:

```typescript
import { supabase } from './supabaseClient';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3001';

async function getAuthHeaders() {
  const { data: { session } } = await supabase.auth.getSession();
  return {
    'Authorization': `Bearer ${session?.access_token}`,
    'Content-Type': 'application/json',
  };
}

export const api = {
  // Users
  async getProfile() {
    const headers = await getAuthHeaders();
    const response = await fetch(`${API_URL}/api/users/me`, { headers });
    return response.json();
  },

  async updateProfile(data: any) {
    const headers = await getAuthHeaders();
    const response = await fetch(`${API_URL}/api/users/me`, {
      method: 'PUT',
      headers,
      body: JSON.stringify(data),
    });
    return response.json();
  },

  // Settings
  async getSettings() {
    const headers = await getAuthHeaders();
    const response = await fetch(`${API_URL}/api/settings`, { headers });
    return response.json();
  },

  async updateSettings(settings: any) {
    const headers = await getAuthHeaders();
    const response = await fetch(`${API_URL}/api/settings`, {
      method: 'PUT',
      headers,
      body: JSON.stringify(settings),
    });
    return response.json();
  },

  // Avatar Upload
  async uploadAvatar(file: File) {
    const headers = await getAuthHeaders();
    delete headers['Content-Type']; // Let browser set it for FormData
    
    const formData = new FormData();
    formData.append('avatar', file);
    
    const response = await fetch(`${API_URL}/api/uploads/avatar`, {
      method: 'POST',
      headers: { 'Authorization': headers.Authorization },
      body: formData,
    });
    return response.json();
  },

  async deleteAvatar() {
    const headers = await getAuthHeaders();
    const response = await fetch(`${API_URL}/api/uploads/avatar`, {
      method: 'DELETE',
      headers,
    });
    return response.json();
  },
};
```

### 3. Update AuthContext

**Changes needed in `tsa-project/src/contexts/AuthContext.tsx`:**

```typescript
// Keep OAuth methods as-is (handled by Supabase)
// Update profile fetching to optionally use API:

const fetchProfile = async (userId: string, force = false) => {
  // Option 1: Continue using Supabase directly (simpler, works with RLS)
  const { data } = await supabase
    .from('profiles')
    .select('*')
    .eq('id', userId)
    .single();
  
  // Option 2: Use API (better for complex logic, logging, etc.)
  // const { data } = await api.getProfile();
  
  setProfile(data);
};
```

### 4. Update Avatar Upload Component

**In `tsa-project/src/lib/avatarUpload.ts`:**

```typescript
import { api } from './apiClient';

export async function uploadAvatar({ file, userId, onProgress }: UploadAvatarOptions) {
  try {
    // Validate file first
    const validation = validateAvatarFile(file);
    if (!validation.valid) {
      return { success: false, error: validation.error };
    }

    onProgress?.(10);
    
    // Call API endpoint
    const result = await api.uploadAvatar(file);
    
    onProgress?.(100);
    
    if (!result.success) {
      return { success: false, error: result.error.message };
    }
    
    return { success: true, url: result.data.url };
  } catch (error: any) {
    return { success: false, error: error.message };
  }
}
```

### 5. Update Settings Page

**In `tsa-project/src/pages/settings.tsx`:**

```typescript
import { api } from '../lib/apiClient';

const handleSave = async () => {
  setSaving(true);
  try {
    const result = await api.updateSettings(settings);
    
    if (!result.success) {
      throw new Error(result.error.message);
    }
    
    await refreshProfile();
    toast.success('Settings saved successfully!');
  } catch (error: any) {
    toast.error(`Failed to save settings: ${error.message}`);
  } finally {
    setSaving(false);
  }
};
```

### 6. Add API_URL to Frontend .env

**tsa-repository/.env:**
```env
# Existing
VITE_SUPABASE_URL=https://your-project.supabase.co
VITE_SUPABASE_ANON_KEY=your-anon-key

# NEW: Backend API URL
VITE_API_URL=http://localhost:3001
```

## Deployment Strategy

### Phase 1: Parallel Operation (Current)
- Frontend continues using Supabase directly
- Backend API is ready but not yet consumed
- Test API endpoints independently

### Phase 2: Gradual Migration
- Start using API for non-critical features (settings, profile updates)
- Keep OAuth with Supabase (it works well)
- Monitor both systems

### Phase 3: Full Migration
- All database operations go through API
- Frontend only uses Supabase for:
  - OAuth authentication
  - Real-time subscriptions (if needed)
- Remove direct Supabase queries from frontend

## Benefits of Separation

1. **Security**: No direct database access from frontend
2. **Validation**: Centralized business logic and validation
3. **Logging**: Better monitoring and debugging
4. **Rate Limiting**: Protect against abuse
5. **Flexibility**: Easy to switch databases or add caching
6. **Testing**: Backend can be tested independently

## Docker Deployment

### Backend (EventiveAPI)
```bash
cd EventiveAPI
docker-compose up -d
# Runs on http://localhost:3001
```

### Frontend (tsa-repository)
```bash
cd tsa-repository
docker-compose up -d
# Runs on http://localhost:3000
```

Both containers can be on the same Docker network for communication.

## Testing the API

```bash
# Health check
curl http://localhost:3001/health

# Get profile (requires auth token)
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:3001/api/users/me

# Upload avatar
curl -X POST \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "avatar=@/path/to/image.jpg" \
  http://localhost:3001/api/uploads/avatar
```

## Next Steps

1. **Install dependencies**: `cd EventiveAPI && npm install`
2. **Configure .env**: Copy `.env.example` to `.env` and fill in values
3. **Start dev server**: `npm run dev`
4. **Test endpoints**: Use the API docs at `http://localhost:3001/docs`
5. **Update frontend**: Gradually migrate to using API client
6. **Deploy**: Use Docker or your preferred platform

## Questions?

- See `README.md` for full API documentation
- Check `src/routes/*.routes.ts` for endpoint implementations
- Review `src/middleware/auth.ts` for authentication flow
