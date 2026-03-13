# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
npm run dev      # Start dev server at localhost:3000
npm run build    # Production build
npm run lint     # Run ESLint
npm start        # Start production server
```

No test framework is configured.

## Architecture

Next.js 16 app using the **App Router** (`app/` directory), React 19, TypeScript (strict), and Tailwind CSS v4.

- `app/layout.tsx` — root layout with Geist fonts and metadata
- `app/page.tsx` — home page
- `app/globals.css` — global styles (Tailwind v4)
- Path alias `@/` maps to the project root

No API routes or backend logic yet — currently a boilerplate scaffold.
