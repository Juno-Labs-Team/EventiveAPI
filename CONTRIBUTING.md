# Contributing to EventiveAPI

Thank you for considering contributing to EventiveAPI! This document provides guidelines for contributing to the project.

## Development Setup

1. **Fork and clone the repository**
   ```bash
   git clone https://github.com/your-username/EventiveAPI.git
   cd EventiveAPI
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Set up environment variables**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

4. **Start development server**
   ```bash
   npm run dev
   ```

## Project Structure

```
EventiveAPI/
├── src/
│   ├── config/          # Configuration files
│   ├── routes/          # API route handlers
│   ├── middleware/      # Express middleware
│   ├── types/           # TypeScript type definitions
│   └── index.ts         # Entry point
├── tests/               # Test files
├── docs/                # Documentation
└── docker/              # Docker configuration
```

## Coding Standards

### TypeScript
- Use TypeScript for all new code
- Define interfaces for all data structures
- Avoid `any` types when possible
- Use ESM imports (`import/export`)

### Code Style
- Use 2 spaces for indentation
- Use single quotes for strings
- Add semicolons
- Run `npm run lint` before committing

### Naming Conventions
- **Files**: `camelCase.ts` for utilities, `kebab-case.routes.ts` for routes
- **Functions**: `camelCase`
- **Classes**: `PascalCase`
- **Constants**: `UPPER_SNAKE_CASE`
- **Interfaces**: `PascalCase` (no `I` prefix)

## API Endpoints

### Route Structure
```typescript
import { Router, Response } from 'express';
import { authenticateUser, AuthRequest } from '../middleware/auth.js';

const router = Router();

router.get('/example', authenticateUser, async (req: AuthRequest, res: Response) => {
  try {
    // Your logic here
    res.json({ success: true, data: {} });
  } catch (error: any) {
    res.status(500).json({
      success: false,
      error: { message: error.message },
    });
  }
});

export default router;
```

### Response Format
All API responses should follow this format:

**Success:**
```json
{
  "success": true,
  "data": { ... }
}
```

**Error:**
```json
{
  "success": false,
  "error": {
    "message": "Error description"
  }
}
```

## Testing

### Running Tests
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

### Writing Tests
- Place tests in `tests/` directory
- Use descriptive test names
- Test both success and error cases
- Mock external dependencies (Supabase, etc.)

Example:
```typescript
import { describe, it, expect } from 'vitest';
import { authenticateUser } from '../src/middleware/auth';

describe('authenticateUser middleware', () => {
  it('should reject requests without token', async () => {
    // Test implementation
  });
  
  it('should accept valid tokens', async () => {
    // Test implementation
  });
});
```

## Git Workflow

### Conventional Commits
We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(auth): add JWT token refresh endpoint
fix(upload): validate file size before upload
docs(readme): update installation instructions
```

### Branch Naming
- `feature/description` - New features
- `fix/description` - Bug fixes
- `docs/description` - Documentation updates
- `refactor/description` - Code refactoring

### Pull Request Process

1. **Create a feature branch**
   ```bash
   git checkout -b feature/my-new-feature
   ```

2. **Make your changes**
   - Write clean, tested code
   - Follow coding standards
   - Update documentation

3. **Commit your changes**
   ```bash
   git add .
   git commit -m "feat(scope): description"
   ```

4. **Push to your fork**
   ```bash
   git push origin feature/my-new-feature
   ```

5. **Create Pull Request**
   - Provide clear description
   - Reference related issues
   - Ensure CI passes

### Pull Request Checklist
- [ ] Code follows project style guidelines
- [ ] Tests added/updated and passing
- [ ] Documentation updated
- [ ] Commit messages follow Conventional Commits
- [ ] No console.logs in production code
- [ ] TypeScript types properly defined
- [ ] Error handling implemented

## Adding New Features

### New API Endpoint
1. Create route file in `src/routes/`
2. Implement route handlers
3. Add authentication middleware if needed
4. Update API documentation
5. Add tests
6. Register route in `src/index.ts`

### New Middleware
1. Create middleware file in `src/middleware/`
2. Export middleware function
3. Add tests
4. Document usage

## Security

### Reporting Security Issues
- **Do not** open public issues for security vulnerabilities
- Email security concerns to: [security@example.com]
- Include detailed description and steps to reproduce

### Security Best Practices
- Never commit sensitive data (API keys, passwords)
- Always validate user input
- Use parameterized queries
- Implement rate limiting
- Keep dependencies updated

## Documentation

### Code Documentation
- Add JSDoc comments for public functions
- Explain complex logic with inline comments
- Update API documentation for new endpoints

### API Documentation
- Update README.md with new endpoints
- Include request/response examples
- Document required parameters
- Note authentication requirements

## Questions?

- Open an issue for bugs or feature requests
- Start a discussion for questions
- Check existing issues before creating new ones

## License

By contributing, you agree that your contributions will be licensed under the same license as the project (MIT).
