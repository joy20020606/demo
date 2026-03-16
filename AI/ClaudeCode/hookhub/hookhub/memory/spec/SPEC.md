# HookHub — Product Specification (MVP)

## Overview
HookHub is a curated directory of open-source cloud hooks. Users can browse and discover hooks sourced from GitHub repositories, organized by category.

## What is a "Cloud Hook"?
A cloud hook is a reusable automation trigger or webhook integration designed for cloud platforms and DevOps pipelines. Examples include:
- GitHub / GitLab webhooks
- Serverless function triggers (AWS Lambda, Vercel, Netlify)
- CI/CD pipeline hooks (GitHub Actions, n8n, Zapier)
- Platform event hooks (Stripe, Slack, Discord integrations)

## MVP Scope
**In scope:** Display and browse hooks.
**Out of scope:** User accounts, submitting hooks, search, filtering, ratings.

## Data Model

### Hook
| Field       | Type   | Description                          |
|-------------|--------|--------------------------------------|
| id          | string | Unique identifier (slug)             |
| name        | string | Display name of the hook             |
| category    | string | Category (e.g., CI/CD, Serverless)   |
| description | string | Short description (max 160 chars)    |
| repoUrl     | string | Full URL to the GitHub repository    |

### Categories (initial set)
- CI/CD
- Serverless
- Webhooks
- Notifications
- DevOps
- Integrations

## Pages

### `/` — Home (Grid View)
- Page title: "HookHub — Discover Open Source Cloud Hooks"
- Displays all hooks in a responsive grid (3 columns desktop / 2 tablet / 1 mobile)
- Each card shows: name, category badge, description, "View Repo →" link

### Hook Card
- Name (bold)
- Category badge (colored by category)
- Description (truncated at 2 lines)
- "View Repo →" external link to GitHub repo (opens in new tab)

## Tech Stack
- Next.js 16 (App Router) + TypeScript
- Tailwind CSS v4
- Static data file (`data/hooks.ts`) for MVP — no database

## Data Source (MVP)
Hooks are stored as a static TypeScript array in `data/hooks.ts`.
Each entry is manually curated from real GitHub repositories.

## Non-Goals (MVP)
- No search or filtering
- No backend / database
- No user-submitted hooks
- No pagination (assume < 50 hooks for MVP)
