```md
# Copilot Instructions – UsersAndPosts

This repository is a **teaching-quality reference implementation** of a small but realistic fullstack system.

Copilot should prioritize **clarity, correctness, and architectural consistency** over brevity or cleverness.

---

## Overall Architecture

This is a **monorepo** with three main parts:

- **UsersAndPosts/**  
  ASP.NET Minimal API (C#), serving both:
  - a REST API under `/api/*`
  - a built React client from `wwwroot/`

- **UsersAndPostsClient/**  
  React + Vite frontend using React Router (Data Router)

- **UsersAndPosts.DtoContractGen/**  
  A .NET console tool that generates `dtos.json` from C# DTO records

Key principle:

> **C# DTO records are the source of truth.**  
> Everything else (JSON contract, TypeScript types) is derived from them.

---

## Backend (C# / ASP.NET)

### Style & Structure

- Use **Minimal API**, not MVC controllers
- Organize code **by domain/entity**, not by technical stereotype

Preferred structure:
```

User/
User.cs
UserDtos.cs
UserRepo.cs
UserEndpoints.cs
Post/
Post.cs
PostDtos.cs
PostRepo.cs
PostEndpoints.cs
Shared/
Db.cs
DbSeeder.cs

````

### DTOs

- DTOs are defined as **C# records**
- DTO records are explicitly marked and used by the DTO contract generator
- DTOs are **simple data carriers only**
  - no behavior
  - no validation logic
  - no framework attributes unless strictly necessary

### Data Access

- Use **raw SQL with SQLite**
- Do **not** use Entity Framework
- Keep repositories small, explicit, and readable
- Prefer clear SQL over abstraction

### API Design

- All API routes live under `/api`
- Use plural resource names (`/api/users`, `/api/posts`)
- Favor explicit, readable endpoints over generic patterns
- Avoid magic conventions

### Error Handling

- Prefer explicit checks and clear error messages
- Fail early when input or state is invalid
- Avoid silent fallbacks

---

## Frontend (React / Vite / TypeScript)

### General

- Use **React Router (Data Router)** with loaders and actions
- Avoid unnecessary state management libraries
- Prefer simple fetch-based data access

### API Access

- All API calls go through a shared helper
- API base path is `/api`
- API base can be overridden via `VITE_API_BASE`

```ts
const API_BASE = import.meta.env.VITE_API_BASE ?? "/api";
````

* Always validate that API responses are JSON before assuming structure

### Types

* **Never hand-write shared DTO types**
* Always import DTO types from:

  ```
  src/generated/dtos.ts
  ```
* That file is generated from `dtos.json`

### TypeScript Style

* Prefer explicit types at module boundaries
* Avoid `any`
* Fail loudly if expected shapes do not match

---

## DTO Contract Flow (Critical)

The expected contract chain is:

```
C# DTO records
   ↓
UsersAndPosts.DtoContractGen
   ↓
dtos.json
   ↓
UsersAndPostsClient/tools/generate-dtos.mjs
   ↓
src/generated/dtos.ts
```

Copilot should:

* Never suggest duplicating DTO definitions across layers
* Never suggest “quick fixes” that bypass this chain
* Treat contract drift as a **bug**, not a convenience issue

---

## Build & CI Expectations

* `dtos.json` is **generated at build time**, not committed
* Client build writes directly to `UsersAndPosts/wwwroot`
* CI validates:

  * DTO contract generation
  * client build
  * backend build and tests
  * that no tracked files are modified by build steps

Copilot suggestions should **not** break CI assumptions.

---

## CI Principles (Important)

This repository treats CI as a **first-class design concern**, not an afterthought.

Copilot should respect the following CI principles:

- CI acts as a **contract gate**, not just a compiler
- DTO contracts must be generated and validated during CI
- Client and server builds are expected to be **deterministic**
- Build steps must not modify tracked source files
- CI failures should be **explicit and actionable**

Copilot should avoid suggestions that:

- bypass contract generation or validation
- introduce environment-specific behavior that breaks CI
- rely on implicit defaults instead of explicit configuration
- hide errors behind fallbacks or silent behavior

When suggesting changes, assume that:
> “If this breaks CI, it is a design regression.”

CI is treated as a design-time feedback system, not a deployment tool.

---

## What to Avoid

Copilot should avoid suggesting:

* MVC Controllers instead of Minimal API
* Entity Framework or ORMs
* Sharing C# code directly with the frontend
* Implicit “magic” abstractions
* Auto-generated code without explaining intent
* Overengineering (generic repositories, base classes, etc.)

---

## Tone & Intent

Code in this repository should be:

* pedagogical
* explicit
* readable by students and junior developers
* suitable as reference material

When in doubt, prefer:

> **Clear intent over fewer lines of code.**

```
