import { Link, NavLink, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { useNotificationStore } from '@/stores/notificationStore';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { cartApi } from '@/api/cart.api';
import { notificationsApi } from '@/api/notifications.api';
import { Button } from '@/components/ui/Button';
import { ThemeToggle } from '@/components/ui/ThemeToggle';
import {
  Bell,
  BookOpen,
  LogOut,
  ShoppingCart,
  User,
  Settings,
  MessageSquare,
  AlertCircle,
  CheckCircle,
  Info,
  Calendar,
  Trash2
} from 'lucide-react';
import { useState, useEffect, useRef } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { formatRelative } from '@/utils/formatters';
import { getNotificationNavigationPath } from '@/utils/notificationNavigation';
import type { NotificationDto } from '@/types';
import { toast } from 'sonner';

const getNotificationIcon = (type: string, sizeClasses = "h-4 w-4") => {
  switch (type?.toLowerCase()) {
    case 'message':
    case 'reply':
    case 'discussion':
      return <MessageSquare className={`${sizeClasses} text-blue-500`} />;
    case 'alert':
    case 'warning':
    case 'important':
      return <AlertCircle className={`${sizeClasses} text-destructive`} />;
    case 'success':
    case 'grade':
    case 'completed':
    case 'submissiongraded':
    case 'gradeapproved':
    case 'gradeupdated':
    case 'paymentsuccess':
    case 'checkoutsuccess':
      return <CheckCircle className={`${sizeClasses} text-green-500`} />;
    case 'event':
    case 'exam':
    case 'deadline':
    case 'newexamposted':
    case 'newmaterialuploaded':
    case 'newlectureadded':
      return <Calendar className={`${sizeClasses} text-amber-500`} />;
    case 'info':
    case 'system':
      return <Info className={`${sizeClasses} text-blue-400`} />;
    default:
      return <Bell className={`${sizeClasses} text-primary`} />;
  }
};

export function Navbar() {
  const { isAuthenticated, user, isTeacher, logout } = useAuth();
  const { unreadCount } = useNotificationStore();
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [showNotificationPanel, setShowNotificationPanel] = useState(false);
  const [showCartPanel, setShowCartPanel] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const notificationsRef = useRef<HTMLDivElement>(null);
  const cartRef = useRef<HTMLDivElement>(null);

  const { data: cartData } = useQuery({
    queryKey: ['cart'],
    queryFn: () => cartApi.get(),
    select: (res) => res.data.data,
    enabled: isAuthenticated && !isTeacher,
  });
  
  const cartCount = cartData?.items?.length ?? 0;

  const { data: recentNotifications = [] } = useQuery({
    queryKey: ['notifications', 'navbar-recent'],
    queryFn: () => notificationsApi.getAll({ Page: 1, PageSize: 5 }),
    select: (res) => (res.data.data?.items ?? []) as NotificationDto[],
    enabled: isAuthenticated && showNotificationPanel,
  });

  const markAllReadMutation = useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: () => {
      useNotificationStore.getState().markAllAsRead();
      useNotificationStore.getState().setUnreadCount(0);
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ['unread-notification-count'] });
    },
    onError: (error: any) => {
      toast.error(error?.userMessage ?? 'Failed to mark notifications as read.');
    },
  });

  const deleteAllMutation = useMutation({
    mutationFn: () => notificationsApi.deleteAll(),
    onSuccess: () => {
      useNotificationStore.getState().setUnreadCount(0);
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ['unread-notification-count'] });
      setShowNotificationPanel(false);
      toast.success('All notifications cleared');
    },
  });

  const markReadMutation = useMutation({
    mutationFn: (id: string) => notificationsApi.markAsRead(id),
    onSuccess: (_data, id) => {
      useNotificationStore.getState().markAsRead(id);
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
      queryClient.invalidateQueries({ queryKey: ['unread-notification-count'] });
    }
  });

  const handleNotificationClick = (notification: NotificationDto) => {
    setShowNotificationPanel(false);
    if (!notification.isRead) {
      markReadMutation.mutate(notification.id);
    }

    navigate(getNotificationNavigationPath(notification));
  };
  // Close menu on outside click
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setShowUserMenu(false);
      }
      if (notificationsRef.current && !notificationsRef.current.contains(e.target as Node)) {
        setShowNotificationPanel(false);
      }
      if (cartRef.current && !cartRef.current.contains(e.target as Node)) {
        setShowCartPanel(false);
      }
    }
    if (showUserMenu || showNotificationPanel || showCartPanel) {
      document.addEventListener('mousedown', handleClick);
    }
    return () => document.removeEventListener('mousedown', handleClick);
  }, [showUserMenu, showNotificationPanel, showCartPanel]);

  const handleNotificationBellClick = () => {
    setShowNotificationPanel((prev) => {
      const next = !prev;
      if (next) {
        markAllReadMutation.mutate();
      }
      return next;
    });
  };

  const studentLinks = [
    { to: '/dashboard', label: 'Dashboard' },
    { to: '/courses', label: 'Browse Courses' },
    { to: '/my-enrollments', label: 'My Enrollments' },
    { to: '/my-submissions', label: 'My Submissions' },
    { to: '/my-grades', label: 'My Grades' },
  ];

  const teacherLinks = [
    { to: '/teacher/dashboard', label: 'Teacher Dashboard' },
    { to: '/teacher/courses', label: 'My Courses' },
    { to: '/teacher/courses/create', label: 'Create Course' },
    { to: '/teacher/exams', label: 'Exams' },
    { to: '/teacher/grading', label: 'Grading' },
  ];

  const isTeacherMode = isTeacher && location.pathname.startsWith('/teacher');

  const navLinks = isTeacher 
    ? (isTeacherMode ? teacherLinks : studentLinks)
    : studentLinks;

  return (
    <header className="sticky top-0 z-40 w-full glass">
      <div className="flex h-14 items-center px-4 gap-4">
        <Link to="/" className="flex items-center gap-2 font-bold text-lg group">
          <div className="h-8 w-8 rounded-lg bg-gradient-to-br from-primary to-accent flex items-center justify-center group-hover:shadow-lg group-hover:shadow-primary/25 transition-all duration-300">
            <BookOpen className="h-4 w-4 text-white" />
          </div>
          <span className="hidden sm:inline gradient-text">AIEduPlatform</span>
        </Link>

        {isAuthenticated && (
          <nav className="hidden lg:flex items-center gap-1 overflow-x-auto whitespace-nowrap max-w-[55vw]">
            {navLinks.map((link) => (
              <NavLink
                key={link.to}
                to={link.to}
                className={({ isActive }) =>
                  `px-2.5 py-1.5 rounded-md text-xs font-medium transition-colors ${
                    isActive
                      ? 'bg-primary/10 text-primary'
                      : 'text-muted-foreground hover:text-foreground hover:bg-secondary/70'
                  }`
                }
              >
                {link.label}
              </NavLink>
            ))}
          </nav>
        )}

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
              {isTeacher && (
                <Button
                  variant="outline"
                  size="sm"
                  className="hidden md:flex h-7 ml-1 mr-1 text-[11px] px-2.5 font-medium border-primary/20 text-primary hover:bg-primary/10 transition-colors"
                  onClick={() => navigate(isTeacherMode ? '/dashboard' : '/teacher/dashboard')}
                >
                  {isTeacherMode ? 'Switch to Student View' : 'Switch to Instructor View'}
                </Button>
              )}

              {!isTeacher && (
              <div className="relative" ref={cartRef}>
                <Button 
                  variant="ghost" 
                  size="icon" 
                  className="relative" 
                  onClick={() => setShowCartPanel(!showCartPanel)}
                >
                  <ShoppingCart className="h-5 w-5" />
                  {cartCount > 0 && (
                    <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-primary text-primary-foreground text-xs flex items-center justify-center">
                      {cartCount > 9 ? '9+' : cartCount}
                    </span>
                  )}
                </Button>

                <AnimatePresence>
                  {showCartPanel && (
                    <motion.div
                      initial={{ opacity: 0, y: -6, scale: 0.98 }}
                      animate={{ opacity: 1, y: 0, scale: 1 }}
                      exit={{ opacity: 0, y: -6, scale: 0.98 }}
                      transition={{ duration: 0.16 }}
                      className="absolute right-0 top-full mt-2 w-80 sm:w-96 rounded-xl border border-border bg-card shadow-2xl z-50 overflow-hidden ring-1 ring-black/5"
                    >
                      <div className="px-4 py-3 border-b border-border flex items-center justify-between bg-secondary/20">
                        <div className="flex items-center gap-2">
                          <p className="text-sm font-semibold tracking-tight">Shopping Cart</p>
                          {cartCount > 0 && (
                            <span className="px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-primary-foreground bg-primary rounded-full">
                              {cartCount} {cartCount === 1 ? 'Item' : 'Items'}
                            </span>
                          )}
                        </div>
                      </div>

                      <div className="max-h-[350px] overflow-y-auto">
                        {!cartData || cartData.items.length === 0 ? (
                          <div className="px-4 py-10 flex flex-col items-center justify-center text-center text-sm text-muted-foreground gap-3">
                            <div className="h-12 w-12 rounded-full bg-secondary/50 flex items-center justify-center">
                              <ShoppingCart className="h-5 w-5 opacity-20" />
                            </div>
                            <p>Your cart is empty.</p>
                          </div>
                        ) : (
                          cartData.items.map((item: any) => (
                            <div
                              key={item.cartItemId}
                              className="w-full text-left px-4 py-3.5 border-b border-border/40 hover:bg-secondary/20 transition-colors relative flex items-start gap-4"
                            >
                              <div className="h-12 w-16 bg-muted rounded overflow-hidden flex-shrink-0">
                                {item.courseThumbnailUrl ? (
                                  <img src={item.courseThumbnailUrl} alt={item.courseTitle} className="h-full w-full object-cover" />
                                ) : (
                                  <div className="h-full w-full flex items-center justify-center bg-secondary">
                                    <BookOpen className="h-4 w-4 text-muted-foreground" />
                                  </div>
                                )}
                              </div>
                              <div className="flex-1 min-w-0">
                                <p className="text-sm font-semibold tracking-tight line-clamp-2 text-foreground">
                                  {item.courseTitle}
                                </p>
                                <p className="text-xs text-muted-foreground mt-1">
                                  {item.teacherName}
                                </p>
                                <div className="mt-1.5 flex items-center gap-2">
                                  <span className="text-sm font-bold text-primary">${item.priceAtTimeOfAdding.toFixed(2)}</span>
                                  {item.originalPrice > item.priceAtTimeOfAdding && (
                                    <span className="text-xs text-muted-foreground line-through">${item.originalPrice.toFixed(2)}</span>
                                  )}
                                </div>
                              </div>
                            </div>
                          ))
                        )}
                      </div>

                      {cartData && cartData.items.length > 0 && (
                        <div className="p-4 border-t border-border bg-secondary/10 space-y-3">
                          <div className="flex items-center justify-between text-sm font-semibold">
                            <span>Subtotal</span>
                            <span className="text-lg">${cartData.subtotal.toFixed(2)}</span>
                          </div>
                          <div className="grid grid-cols-2 gap-2">
                            <Button
                              variant="outline"
                              size="sm"
                              className="w-full text-sm"
                              onClick={() => {
                                setShowCartPanel(false);
                                navigate('/checkout');
                              }}
                            >
                              View Cart
                            </Button>
                            <Button
                              variant="gradient"
                              size="sm"
                              className="w-full text-sm font-semibold shadow-sm"
                              onClick={() => {
                                setShowCartPanel(false);
                                navigate('/checkout');
                              }}
                            >
                              Checkout
                            </Button>
                          </div>
                        </div>
                      )}
                    </motion.div>
                  )}
                </AnimatePresence>
              </div>
              )}

              <div className="relative" ref={notificationsRef}>
                <Button
                  variant="ghost"
                  size="icon"
                  className="relative"
                  onClick={handleNotificationBellClick}
                >
                  <Bell className="h-5 w-5" />
                  {unreadCount > 0 && (
                    <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-destructive text-destructive-foreground text-xs flex items-center justify-center animate-pulse">
                      {unreadCount > 9 ? '9+' : unreadCount}
                    </span>
                  )}
                </Button>

                <AnimatePresence>
                  {showNotificationPanel && (
                    <motion.div
                      initial={{ opacity: 0, y: -6, scale: 0.98 }}
                      animate={{ opacity: 1, y: 0, scale: 1 }}
                      exit={{ opacity: 0, y: -6, scale: 0.98 }}
                      transition={{ duration: 0.16 }}
                      className="absolute right-0 top-full mt-2 w-80 sm:w-96 rounded-xl border border-border bg-card shadow-2xl z-50 overflow-hidden ring-1 ring-black/5"
                    >
                      <div className="px-4 py-3 border-b border-border flex items-center justify-between bg-secondary/20">
                        <div className="flex flex-col gap-1">
                          <div className="flex items-center gap-2">
                            <p className="text-sm font-semibold tracking-tight">Notifications</p>
                            {unreadCount > 0 && (
                              <span className="px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-primary-foreground bg-primary rounded-full">
                                {unreadCount} New
                              </span>
                            )}
                          </div>
                          <span className="text-xs text-muted-foreground font-medium">Recent</span>
                        </div>
                        {recentNotifications.length > 0 && (
                          <Button 
                            variant="ghost" 
                            size="sm" 
                            className="h-7 text-xs px-2 text-muted-foreground hover:text-destructive hover:bg-destructive/10"
                            onClick={(e) => { e.stopPropagation(); deleteAllMutation.mutate(); }}
                          >
                            <Trash2 className="h-3 w-3 mr-1" /> Clear All
                          </Button>
                        )}
                      </div>

                      <div className="max-h-[350px] overflow-y-auto">
                        {recentNotifications.length === 0 ? (
                          <div className="px-4 py-8 flex flex-col items-center justify-center text-center text-xs text-muted-foreground gap-2">
                            <div className="h-10 w-10 rounded-full bg-secondary/50 flex items-center justify-center">
                              <Bell className="h-4 w-4 opacity-20" />
                            </div>
                            <p>You're all caught up!</p>
                          </div>
                        ) : (
                          recentNotifications.map((notification) => (
                            <button
                              key={notification.id}
                              type="button"
                              className={`w-full text-left px-3 py-3 border-b border-border/40 hover:bg-secondary/60 transition-colors relative ${
                                !notification.isRead ? 'bg-primary/5' : ''
                              }`}
                              onClick={() => {
                                handleNotificationClick(notification);
                              }}
                            >
                              {!notification.isRead && (
                                <div className="absolute left-0 top-0 bottom-0 w-1 bg-primary rounded-r-full" />
                              )}
                              <div className="flex items-start gap-2.5">
                                <div className={`flex-shrink-0 h-7 w-7 rounded-full flex items-center justify-center ${!notification.isRead ? 'bg-primary/10' : 'bg-secondary'}`}>
                                  {getNotificationIcon(notification.type, "h-3.5 w-3.5")}
                                </div>
                                <div className="min-w-0 flex-1">
                                  <p className={`text-xs tracking-tight line-clamp-2 ${!notification.isRead ? 'font-semibold text-foreground' : 'font-medium text-foreground/80'}`}>
                                    {notification.title || notification.message}
                                  </p>
                                  {notification.title && notification.title !== notification.message && (
                                    <p className="text-[10px] text-muted-foreground mt-0.5 line-clamp-1">
                                      {notification.message}
                                    </p>
                                  )}
                                  <p className={`text-[10px] mt-1 ${!notification.isRead ? 'text-primary font-medium' : 'text-muted-foreground'}`}>
                                    {formatRelative(notification.createdAt)}
                                  </p>
                                </div>
                                {!notification.isRead && (
                                  <div className="flex-shrink-0 mt-1 h-1.5 w-1.5 rounded-full bg-primary shadow-[0_0_8px_rgba(59,130,246,0.8)] animate-pulse" />
                                )}
                              </div>
                            </button>
                          ))
                        )}
                      </div>

                      <div className="p-2 border-t border-border bg-secondary/10">
                        <Button
                          variant="ghost"
                          size="sm"
                          className="w-full text-xs font-medium hover:bg-secondary hover:text-foreground text-muted-foreground"
                          onClick={() => {
                            setShowNotificationPanel(false);
                            navigate('/notifications');
                          }}
                        >
                          View all notifications
                        </Button>
                      </div>
                    </motion.div>
                  )}
                </AnimatePresence>
              </div>

              <div className="relative" ref={menuRef}>
                <Button
                  variant="ghost"
                  className="relative p-0.5 rounded-full h-8 w-8 hover:bg-secondary/80 border border-transparent hover:border-border transition-all"
                  onClick={() => setShowUserMenu(!showUserMenu)}
                >
                  <div className="h-full w-full rounded-full bg-primary/10 flex items-center justify-center overflow-hidden">
                    <User className="h-4 w-4 text-primary" />
                  </div>
                </Button>
                <AnimatePresence>
                  {showUserMenu && (
                    <motion.div
                      initial={{ opacity: 0, y: -6, scale: 0.98 }}
                      animate={{ opacity: 1, y: 0, scale: 1 }}
                      exit={{ opacity: 0, y: -6, scale: 0.98 }}
                      transition={{ duration: 0.16 }}
                      className="absolute right-0 top-full mt-2 w-64 rounded-xl border border-border bg-card shadow-2xl z-50 overflow-hidden ring-1 ring-black/5"
                    >
                      <div className="p-4 border-b border-border/50 bg-secondary/10 flex items-center gap-3">
                        <div className="h-10 w-10 rounded-full flex items-center justify-center bg-gradient-to-br from-primary/80 to-accent shrink-0 shadow-sm border border-primary/20">
                          <span className="text-primary-foreground font-semibold text-sm">
                            {user?.userName?.charAt(0).toUpperCase() || 'U'}
                          </span>
                        </div>
                        <div className="min-w-0 flex flex-col">
                          <p className="text-xs font-semibold truncate text-foreground leading-tight">
                            {user?.userName || 'User'}
                          </p>
                          <p className="text-[10px] text-muted-foreground truncate leading-tight mt-0.5">
                            {user?.email}
                          </p>
                          {isTeacher && (
                            <span className="mt-1.5 inline-flex items-center rounded-full bg-primary/10 px-2 py-0.5 text-[9px] font-bold tracking-widest text-primary w-fit uppercase">
                              Instructor
                            </span>
                          )}
                        </div>
                      </div>
                      <div className="py-2 px-1">
                        <Link
                          to="/profile"
                          className="flex items-center gap-2.5 px-3 py-2 text-xs font-medium rounded-lg mx-1 hover:bg-secondary/60 transition-colors text-foreground/80 hover:text-foreground"
                          onClick={() => setShowUserMenu(false)}
                        >
                          <User className="h-4 w-4 text-muted-foreground" />
                          Profile
                        </Link>
                        
                        {!isTeacher ? (
                          <Link
                            to="/dashboard"
                            className="flex items-center gap-2.5 px-3 py-2 text-xs font-medium rounded-lg mx-1 hover:bg-secondary/60 transition-colors text-foreground/80 hover:text-foreground"
                            onClick={() => setShowUserMenu(false)}
                          >
                            <BookOpen className="h-4 w-4 text-muted-foreground" />
                            Dashboard
                          </Link>
                        ) : (
                          <Link
                            to={isTeacherMode ? "/dashboard" : "/teacher/dashboard"}
                            className="flex items-center gap-2.5 px-3 py-2 text-xs font-medium rounded-lg mx-1 hover:bg-secondary/60 transition-colors text-primary/80 hover:text-primary bg-primary/5 hover:bg-primary/10 mt-1"
                            onClick={() => setShowUserMenu(false)}
                          >
                            {isTeacherMode ? <BookOpen className="h-4 w-4" /> : <Settings className="h-4 w-4" />}
                            {isTeacherMode ? "Switch to Student View" : "Switch to Instructor View"}
                          </Link>
                        )}
                      </div>
                      <div className="border-t border-border/50 p-1 bg-secondary/5">
                        <button
                          className="w-full flex items-center gap-2.5 px-3 py-2 text-xs font-medium rounded-lg text-left text-destructive hover:bg-destructive/10 transition-colors"
                          onClick={() => {
                            setShowUserMenu(false);
                            logout();
                          }}
                        >
                          <LogOut className="h-4 w-4" /> Log out
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
