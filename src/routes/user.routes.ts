import { Router, Response } from 'express';
import { authenticateUser, AuthRequest } from '../middleware/auth.js';
import { supabase } from '../config/supabase.js';

const router = Router();

/**
 * GET /api/users/me
 * Get current user profile
 */
router.get('/me', authenticateUser, async (req: AuthRequest, res: Response) => {
  try {
    const userId = req.user?.id;
    
    if (!userId) {
      return res.status(401).json({
        success: false,
        error: { message: 'User not authenticated' },
      });
    }
    
    const { data: profile, error } = await supabase
      .from('profiles')
      .select('*')
      .eq('id', userId)
      .single();
    
    if (error) {
      return res.status(404).json({
        success: false,
        error: { message: 'Profile not found' },
      });
    }
    
    return res.json({
      success: true,
      data: profile,
    });
  } catch (error: any) {
    return res.status(500).json({
      success: false,
      error: { message: error.message },
    });
  }
});

/**
 * PUT /api/users/me
 * Update current user profile
 */
router.put('/me', authenticateUser, async (req: AuthRequest, res: Response) => {
  try {
    const userId = req.user?.id;
    
    if (!userId) {
      return res.status(401).json({
        success: false,
        error: { message: 'User not authenticated' },
      });
    }
    
    const { display_name, username, bio } = req.body;
    
    // Update profile
    const { data: profile, error } = await supabase
      .from('profiles')
      .update({
        display_name,
        username,
        bio,
        updated_at: new Date().toISOString(),
      })
      .eq('id', userId)
      .select()
      .single();
    
    if (error) {
      return res.status(400).json({
        success: false,
        error: { message: error.message },
      });
    }
    
    return res.json({
      success: true,
      data: profile,
    });
  } catch (error: any) {
    return res.status(500).json({
      success: false,
      error: { message: error.message },
    });
  }
});

/**
 * GET /api/users/:id
 * Get user profile by ID (public profiles only)
 */
router.get('/:id', async (req, res) => {
  try {
    const { id } = req.params;
    
    const { data: profile, error } = await supabase
      .from('profiles')
      .select('id, username, display_name, avatar_url, bio, created_at')
      .eq('id', id)
      .single();
    
    if (error || !profile) {
      return res.status(404).json({
        success: false,
        error: { message: 'Profile not found' },
      });
    }
    
    // Check if profile is public
    const { data: settings } = await supabase
      .from('profiles')
      .select('settings')
      .eq('id', id)
      .single();
    
    if (!settings?.settings?.publicProfile) {
      return res.status(403).json({
        success: false,
        error: { message: 'This profile is private' },
      });
    }
    
    return res.json({
      success: true,
      data: profile,
    });
  } catch (error: any) {
    return res.status(500).json({
      success: false,
      error: { message: error.message },
    });
  }
});

export default router;
