import { Router, Response } from 'express';
import multer from 'multer';
import { authenticateUser, AuthRequest } from '../middleware/auth.js';
import { supabase } from '../config/supabase.js';
import { config } from '../config/index.js';

const router = Router();

// Configure multer for memory storage
const upload = multer({
  storage: multer.memoryStorage(),
  limits: {
    fileSize: config.storage.maxFileSize, // 5MB
  },
  fileFilter: (req, file, cb) => {
    // Only allow images
    if (file.mimetype.startsWith('image/')) {
      cb(null, true);
    } else {
      cb(new Error('Only image files are allowed'));
    }
  },
});

/**
 * POST /api/uploads/avatar
 * Upload user avatar
 */
router.post(
  '/avatar',
  authenticateUser,
  upload.single('avatar'),
  async (req: AuthRequest, res: Response) => {
    try {
      const userId = req.user?.id;
      
      if (!userId) {
        return res.status(401).json({
          success: false,
          error: { message: 'User not authenticated' },
        });
      }
      
      if (!req.file) {
        return res.status(400).json({
          success: false,
          error: { message: 'No file uploaded' },
        });
      }
      
      // Generate unique filename
      const fileExt = req.file.originalname.split('.').pop();
      const fileName = `${userId}/${Date.now()}.${fileExt}`;
      
      // Delete old avatars for this user
      const { data: existingFiles } = await supabase.storage
        .from(config.storage.avatarBucket)
        .list(userId);
      
      if (existingFiles && existingFiles.length > 0) {
        const filesToDelete = existingFiles.map((f) => `${userId}/${f.name}`);
        await supabase.storage
          .from(config.storage.avatarBucket)
          .remove(filesToDelete);
      }
      
      // Upload new avatar
      const { error: uploadError } = await supabase.storage
        .from(config.storage.avatarBucket)
        .upload(fileName, req.file.buffer, {
          contentType: req.file.mimetype,
          cacheControl: '3600',
          upsert: false,
        });
      
      if (uploadError) {
        return res.status(500).json({
          success: false,
          error: { message: `Upload failed: ${uploadError.message}` },
        });
      }
      
      // Get public URL
      const { data: urlData } = supabase.storage
        .from(config.storage.avatarBucket)
        .getPublicUrl(fileName);
      
      if (!urlData?.publicUrl) {
        return res.status(500).json({
          success: false,
          error: { message: 'Failed to get avatar URL' },
        });
      }
      
      // Update profile with new avatar URL
      const { error: updateError } = await supabase
        .from('profiles')
        .update({ 
          avatar_url: urlData.publicUrl,
          updated_at: new Date().toISOString(),
        })
        .eq('id', userId);
      
      if (updateError) {
        return res.status(500).json({
          success: false,
          error: { message: 'Failed to update profile' },
        });
      }
      
      res.json({
        success: true,
        data: {
          url: urlData.publicUrl,
        },
      });
    } catch (error: any) {
      res.status(500).json({
        success: false,
        error: { message: error.message },
      });
    }
  }
);

/**
 * DELETE /api/uploads/avatar
 * Delete user avatar
 */
router.delete('/avatar', authenticateUser, async (req: AuthRequest, res: Response) => {
  try {
    const userId = req.user?.id;
    
    if (!userId) {
      return res.status(401).json({
        success: false,
        error: { message: 'User not authenticated' },
      });
    }
    
    // Delete all avatars for this user
    const { data: files } = await supabase.storage
      .from(config.storage.avatarBucket)
      .list(userId);
    
    if (files && files.length > 0) {
      const filesToDelete = files.map((f) => `${userId}/${f.name}`);
      const { error: deleteError } = await supabase.storage
        .from(config.storage.avatarBucket)
        .remove(filesToDelete);
      
      if (deleteError) {
        return res.status(500).json({
          success: false,
          error: { message: deleteError.message },
        });
      }
    }
    
    // Update profile to remove avatar URL
    const { error: updateError } = await supabase
      .from('profiles')
      .update({ 
        avatar_url: null,
        updated_at: new Date().toISOString(),
      })
      .eq('id', userId);
    
    if (updateError) {
      return res.status(500).json({
        success: false,
        error: { message: 'Failed to update profile' },
      });
    }
    
    res.json({
      success: true,
      message: 'Avatar deleted successfully',
    });
  } catch (error: any) {
    res.status(500).json({
      success: false,
      error: { message: error.message },
    });
  }
});

export default router;
