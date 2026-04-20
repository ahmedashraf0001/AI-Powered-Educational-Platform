Build a fully-featured authenticated student dashboard (Home Page) for logged-in users, similar to Udemy or Coursera, while keeping the existing Home Page solely for unauthenticated (guest) users. Include gamified study streak tracking.

Context & Tech Stack:
- React 19, Vite, Tailwind CSS v4, TypeScript
- Lucide React for icons (e.g., Flame for streaks)
- Zustand for auth state, TanStack React Query for API fetching

Here are the specific requirements:

1. Routing & Access Control:
- Keep the current Home Page specifically for guests/logged-out users (displaying the marketing landing page).
- Create a new `StudentDashboard.tsx` page.
- Update the Router logic so that if an authenticated user navigates to `/`, they are rendered the new Dashboard instead.

2. Gamification & Study Streaks (New Header Section):
- Welcome Header: A modern greeting ("Welcome back, [Name]!") alongside a "Study Stats" summary.
- The Streak Tracker: Display a "Current Streak" counter prominently (e.g., "🔥 5 Day Streak").
- Weekly Heatmap/Activity: Include a small row of 7 circles or days (M T W T F S S) showing which days the user was active this week, using an accent color (like orange or primary blue) for active days and a muted gray for inactive days.

3. Dashboard Layout & UI Sections:
- "Jump Back In" (Hero Action): Highlight the single most recently accessed course or study session. Include the course thumbnail, title, last accessed lecture name, a progress bar (e.g., 65% complete), and a prominent primary "Resume Learning" button.
- "In Progress" Carousel/Grid: A horizontally scrollable row or a CSS grid of other active enrollments. Card components should feature:
  - Course thumbnail
  - Course title and instructor name
  - Thin UI progress bar indicating percentage completed
- "Recommended for You" / "Discover": A section fetching un-enrolled courses from the catalog. Display these as standard course cards with pricing (or "Free"), ratings, and a "View Course" button. Categories could include "Top Rated" or "Trending".

4. UI/UX Details:
- Make heavy use of Tailwind v4 styling: rounded corners (`rounded-xl` or `rounded-2xl`), subtle borders (`border-gray-200/dark:border-white/10`), soft shadows (`shadow-sm`, hover:`shadow-md`), and seamless Dark Mode support.
- Ensure the layout is fully responsive (1 column on mobile, 2 on tablet, 3-4 on desktop for grids).
- Show skeleton loaders while data is fetching using React Query `isLoading`.
- Handle empty states gracefully. If the user has no active courses, show an engaging illustration or a call-to-action to "Explore the Catalog". If the user has a 0-day streak, show an encouraging message like "Start your streak today!".

5. Implementation Steps:
- Create `src/pages/StudentDashboard.tsx` and accompanying components.
- Update the router to conditionally render `StudentDashboard` vs. the `LandingPage` depending on the `isAuthenticated` state from the Auth store.