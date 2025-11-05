import { Router, Response } from 'express';
import { authenticateUser, AuthRequest } from '../middleware/auth.js';
import { supabase } from '../config/supabase.js';

const router = Router();

/**
 * GET /api/settings
 * Get user settings
 */
router.get('/', authenticateUser, async (req: AuthRequest, res: Response) => {
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
      .select('settings')
      .eq('id', userId)
      .single();
    
    if (error) {
      return res.status(404).json({
        success: false,
        error: { message: 'Settings not found' },
      });
    }
    
    return res.json({
      success: true,
      data: profile.settings || {},
    });
  } catch (error: any) {
    return res.status(500).json({
      success: false,
      error: { message: error.message },
    });
  }
});

/**
 * PUT /api/settings
 * Update user settings
 */
router.put('/', authenticateUser, async (req: AuthRequest, res: Response) => {
  try {
    const userId = req.user?.id;
    
    if (!userId) {
      return res.status(401).json({
        success: false,
        error: { message: 'User not authenticated' },
      });
    }
    
    const newSettings = req.body;
    
    // Update settings
    const { data: profile, error } = await supabase
      .from('profiles')
      .update({
        settings: newSettings,
        updated_at: new Date().toISOString(),
      })
      .eq('id', userId)
      .select('settings')
      .single();
    
    if (error) {
      return res.status(400).json({
        success: false,
        error: { message: error.message },
      });
    }
    
    return res.json({
      success: true,
      data: profile.settings,
    });
  } catch (error: any) {
    return res.status(500).json({
      success: false,
      error: { message: error.message },
    });
  }
});

export default router;
