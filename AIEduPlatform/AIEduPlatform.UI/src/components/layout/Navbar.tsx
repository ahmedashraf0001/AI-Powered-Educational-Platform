import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useNotificationStore } from '@/stores/notificationStore';
import { useUiStore } from '@/stores/uiStore';
import { useQuery } from '@tanstack/react-query';
import { cartApi } from '@/api/cart.api';
import { Button } from '@/components/ui/Button';
import { ThemeToggle } from '@/components/ui/ThemeToggle';
import {
  Bell,
  BookOpen,
  LogOut,
  Menu,
  ShoppingCart,
  User,
  Settings,
} from 'lucide-react';
import { useState, useEffect, useRef } from 'react';
import { AnimatePresence, motion } from 'framer-motion';

export function Navbar() {
  const { isAuthenticated, user, isTeacher, logout } = useAuth();
  const { unreadCount } = useNotificationStore();
  const { toggleSidebar } = useUiStore();
  const navigate = useNavigate();
  const [showUserMenu, setShowUserMenu] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const { data: cartCount = 0 } = useQuery({
    queryKey: ['cart'],
    queryFn: () => cartApi.get(),
    select: (res) => res.data.data?.items?.length ?? 0,
    enabled: isAuthenticated && !isTeacher,
  });

  // Close menu on outside click
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setShowUserMenu(false);
      }
    }
    if (showUserMenu) document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, [showUserMenu]);

  return (
    <header className="sticky top-0 z-40 w-full glass">
      <div className="flex h-14 items-center px-4 gap-4">
        {isAuthenticated && (
          <Button variant="ghost" size="icon" onClick={toggleSidebar}>
            <Menu className="h-5 w-5" />
          </Button>
        )}

        <Link to="/" className="flex items-center gap-2 font-bold text-lg group">
          <div className="h-8 w-8 rounded-lg bg-gradient-to-br from-primary to-accent flex items-center justify-center group-hover:shadow-lg group-hover:shadow-primary/25 transition-all duration-300">
            <BookOpen className="h-4 w-4 text-white" />
          </div>
          <span className="hidden sm:inline gradient-text">AIEduPlatform</span>
        </Link>

        <div className="ml-auto flex items-center gap-1">
          <ThemeToggle />

          {!isAuthenticated ? (
            <>
              <Button variant="ghost" onClick={() => navigate('/login')}>
                Login
              </Button>
              <Button variant="gradient" onClick={() => navigate('/register')}>Get Started</Button>
            </>
          ) : (
            <>
              <Button variant="ghost" size="icon" className="relative" onClick={() => navigate('/cart')}>
                <ShoppingCart className="h-5 w-5" />
                {cartCount > 0 && (
                  <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-primary text-primary-foreground text-xs flex items-center justify-center">
                    {cartCount > 9 ? '9+' : cartCount}
                  </span>
                )}
              </Button>

              <Button
                variant="ghost"
                size="icon"
                className="relative"
                onClick={() => navigate('/notifications')}
              >
                <Bell className="h-5 w-5" />
                {unreadCount > 0 && (
                  <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-destructive text-destructive-foreground text-xs flex items-center justify-center animate-pulse">
                    {unreadCount > 9 ? '9+' : unreadCount}
                  </span>
                )}
              </Button>

              <div className="relative" ref={menuRef}>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => setShowUserMenu(!showUserMenu)}
                >
                  <User className="h-5 w-5" />
                </Button>
                <AnimatePresence>
                  {showUserMenu && (
                    <motion.div
                      initial={{ opacity: 0, y: -5, scale: 0.95 }}
                      animate={{ opacity: 1, y: 0, scale: 1 }}
                      exit={{ opacity: 0, y: -5, scale: 0.95 }}
                      transition={{ duration: 0.15 }}
                      className="absolute right-0 top-full mt-2 w-56 rounded-xl border border-border bg-card shadow-xl py-1 z-50 overflow-hidden"
                    >
                      <div className="px-3 py-3 border-b border-border bg-gradient-to-r from-primary/5 to-accent/5">
                        <p className="text-sm font-medium">{user?.userName || 'User'}</p>
                        <p className="text-xs text-muted-foreground">{user?.email}</p>
                      </div>
                      <div className="py-1">
                        <Link
                          to="/profile"
                          className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-secondary transition-colors"
                          onClick={() => setShowUserMenu(false)}
                        >
                          <User className="h-4 w-4 text-muted-foreground" />
                          Profile
                        </Link>
                        <Link
                          to="/dashboard"
                          className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-secondary transition-colors"
                          onClick={() => setShowUserMenu(false)}
                        >
                          <BookOpen className="h-4 w-4 text-muted-foreground" />
                          Dashboard
                        </Link>
                        {isTeacher && (
                          <Link
                            to="/teacher/dashboard"
                            className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-secondary transition-colors"
                            onClick={() => setShowUserMenu(false)}
                          >
                            <Settings className="h-4 w-4 text-muted-foreground" />
                            Teacher Dashboard
                          </Link>
                        )}
                        <Link
                          to="/settings/ai-provider"
                          className="flex items-center gap-2 px-3 py-2 text-sm hover:bg-secondary transition-colors"
                          onClick={() => setShowUserMenu(false)}
                        >
                          <Settings className="h-4 w-4 text-muted-foreground" />
                          AI Settings
                        </Link>
                      </div>
                      <div className="border-t border-border pt-1">
                        <button
                          className="w-full text-left px-3 py-2 text-sm text-destructive hover:bg-destructive/10 flex items-center gap-2 transition-colors"
                          onClick={() => {
                            setShowUserMenu(false);
                            logout();
                          }}
                        >
                          <LogOut className="h-4 w-4" /> Logout
                        </button>
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </div>
            </>
          )}
        </div>
      </div>
      {/* Gradient border line */}
      <div className="absolute bottom-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-primary/30 to-transparent" />
    </header>
  );
}
