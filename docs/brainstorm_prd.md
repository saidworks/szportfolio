# Draft PRD — Personal Portfolio CMS (Brainstorm)

## 1. Project Snapshot
Name: Personal Portfolio CMS (Blazor/.NET Core)  
Purpose: A small CMS to host a personal resume, projects, blog (articles + comments), and contact functionality. Built with Blazor/Razor UI, .NET Core backend, Swagger-documented APIs, and MySQL persistence. Admin user (you) manages content; public users can view and comment where allowed.

## 2. Goals & Success Criteria
- Present a clean, professional online resume and portfolio that is easy to maintain.
- Publish technical articles with a comment system and moderate comments.
- Provide a simple admin interface for CRUD operations on content.
- Demonstrate a full-stack .NET skillset (Blazor/Razor, .NET Core Web API, EF Core + MySQL, Swagger).
Success metrics:
- All public APIs documented in Swagger.
- Admin can perform all CRUD operations for every content type.
- Blog supports comments with moderation; at least basic spam protection.
- Page load and API responses performant and responsive on mobile.

## 3. Audience & Personas
- Primary: Hiring managers / recruiters reviewing the portfolio.
- Secondary: Developers and readers of technical articles.
- Admin: The portfolio owner (role: Admin) who edits content and moderates comments.

## 4. Scope & Core Features
Public-facing:
- Resume page aggregating Projects, Education, Experience, Certifications, Languages, Hobbies.
- Blog section listing Articles with detail pages, tags, search, and comments.
- Contact page with a message form.

Admin:
- Authentication (Admin vs Viewer). Admin can manage content; Viewer is read-only or minimal.
- CRUD for: Users (admin only), Projects, Education, Experience, Certifications, Languages, Hobbies, Articles, Tags, Comments, Contact messages.
- Article editor with rich-text support (draft/publish workflow).
- Comment moderation (approve/reject/delete).
- Media management (image upload for projects/articles).
- Swagger UI for all API endpoints.

Data persistence:
- MySQL (recommended v8+) via Entity Framework Core with migrations and seed data.
