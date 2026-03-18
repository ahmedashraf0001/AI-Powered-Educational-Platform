import { NavLink, useLocation } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useUiStore } from '@/stores/uiStore';
import { cn } from '@/utils/cn';
import { motion } from 'framer-motion';
import {
  Home,
  BookOpen,
  GraduationCap,
  FileText,
  Trophy,
  LayoutDashboard,
  PlusCircle,
  ClipboardList,
  FolderOpen,
  Mic,
  Bot,
  CheckSquare,
} from 'lucide-react';

interface SidebarLink {
  label: string;
  to: string;
  icon: React.ReactNode;
}

export function Sidebar() {
  const { isTeacher } = useAuth();
  const { sidebarOpen } = useUiStore();
  const location = useLocation();

  const studentLinks: SidebarLink[] = [
    { label: 'Dashboard', to: '/dashboard', icon: <Home className="h-5 w-5" /> },
    { label: 'Browse Courses', to: '/courses', icon: <BookOpen className="h-5 w-5" /> },
    { label: 'My Enrollments', to: '/my-enrollments', icon: <GraduationCap className="h-5 w-5" /> },
    { label: 'My Submissions', to: '/my-submissions', icon: <FileText className="h-5 w-5" /> },
    { label: 'My Grades', to: '/my-grades', icon: <Trophy className="h-5 w-5" /> },
  ];

  const teacherLinks: SidebarLink[] = [
    { label: 'Dashboard', to: '/teacher/dashboard', icon: <LayoutDashboard className="h-5 w-5" /> },
    { label: 'My Courses', to: '/teacher/courses', icon: <BookOpen className="h-5 w-5" /> },
    { label: 'Create Course', to: '/teacher/courses/create', icon: <PlusCircle className="h-5 w-5" /> },
    { label: 'Categories', to: '/teacher/categories', icon: <FolderOpen className="h-5 w-5" /> },
    { label: 'Exams', to: '/teacher/exams', icon: <ClipboardList className="h-5 w-5" /> },
    { label: 'Grading', to: '/teacher/grading', icon: <CheckSquare className="h-5 w-5" /> },
  ];

  const settingsLinks: SidebarLink[] = [
    { label: 'AI Provider', to: '/settings/ai-provider', icon: <Bot className="h-5 w-5" /> },
    { label: 'Voice Settings', to: '/settings/voice', icon: <Mic className="h-5 w-5" /> },
  ];

  const links = isTeacher ? teacherLinks : studentLinks;

  const publicPaths = ['/', '/login', '/register', '/verify-email'];
  if (publicPaths.includes(location.pathname)) return null;

  const renderLink = (link: SidebarLink) => (
    <NavLink
      key={link.to}
      to={link.to}
      className={({ isActive }) =>
        cn(
          'group relative flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition-all duration-200',
          isActive
            ? 'bg-primary/10 text-primary font-medium'
            : 'text-muted-foreground hover:bg-secondary hover:text-foreground',
          !sidebarOpen && 'justify-center px-2'
        )
      }
    >
      {({ isActive }) => (
        <>
          {isActive && (
            <motion.div
              layoutId="sidebar-active"
              className="absolute left-0 top-1 bottom-1 w-[3px] bg-primary rounded-r-full"
              transition={{ type: 'spring', stiffness: 300, damping: 30 }}
            />
          )}
          {link.icon}
          {sidebarOpen && <span>{link.label}</span>}
          {/* Tooltip when collapsed */}
          {!sidebarOpen && (
            <div className="absolute left-full ml-3 px-2.5 py-1.5 bg-popover text-popover-foreground text-xs rounded-lg shadow-lg border border-border opacity-0 group-hover:opacity-100 pointer-events-none transition-opacity duration-200 whitespace-nowrap z-50">
              {link.label}
            </div>
          )}
        </>
      )}
    </NavLink>
  );

  return (
    <motion.aside
      animate={{ width: sidebarOpen ? 240 : 64 }}
      transition={{ duration: 0.3, ease: [0.25, 0.1, 0.25, 1] }}
      className="fixed left-0 top-14 h-[calc(100vh-3.5rem)] bg-card border-r border-border z-30 overflow-hidden"
    >
      <nav className="flex flex-col gap-1 p-2 h-full">
        <div className="flex-1 space-y-1">
          {links.map(renderLink)}
        </div>

        <div className="border-t border-border pt-2 space-y-1">
          {settingsLinks.map(renderLink)}
        </div>
      </nav>
    </motion.aside>
  );
}
