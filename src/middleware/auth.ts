import { Request, Response, NextFunction } from 'express';
import { supabase } from '../config/supabase.js';

export interface AuthRequest extends Request {
  user?: {
    id: string;
    email: string;
    role: string;
  };
}

/**
 * Middleware to verify JWT token from Authorization header
 */
export async function authenticateUser(
  req: AuthRequest,
  res: Response,
  next: NextFunction
) {
  try {
    const authHeader = req.headers.authorization;
    
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return res.status(401).json({
        success: false,
        error: { message: 'No authorization token provided' },
      });
    }
    
    const token = authHeader.substring(7); // Remove 'Bearer ' prefix
    
    // Verify token with Supabase
    const { data: { user }, error } = await supabase.auth.getUser(token);
    
    if (error || !user) {
      return res.status(401).json({
        success: false,
        error: { message: 'Invalid or expired token' },
      });
    }
    
    // Fetch user profile to get role
    const { data: profile } = await supabase
      .from('profiles')
      .select('role')
      .eq('id', user.id)
      .single();
    
    // Attach user info to request
    req.user = {
      id: user.id,
      email: user.email || '',
      role: profile?.role || 'user',
    };
    
    return next();
  } catch (error) {
    console.error('Authentication error:', error);
    return res.status(500).json({
      success: false,
      error: { message: 'Authentication failed' },
    });
  }
}

/**
 * Middleware to require admin role
 */
export function requireAdmin(
  req: AuthRequest,
  res: Response,
  next: NextFunction
) {
  if (!req.user) {
    return res.status(401).json({
      success: false,
      error: { message: 'Authentication required' },
    });
  }
  
  if (req.user.role !== 'admin') {
    return res.status(403).json({
      success: false,
      error: { message: 'Admin access required' },
    });
  }
  
  return next();
}
