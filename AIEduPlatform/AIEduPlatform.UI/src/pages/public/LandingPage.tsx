import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/Button';
import { ThemeToggle } from '@/components/ui/ThemeToggle';
import { AnimatedCounter } from '@/components/ui/AnimatedCounter';
import { useAuthStore } from '@/stores/authStore';
import { BookOpen, Brain, BarChart3, Sparkles, GraduationCap, MessageSquare, ArrowRight, Mic, Layers } from 'lucide-react';
import { motion } from 'framer-motion';
import { staggerContainer, fadeInUp } from '@/utils/motion';

const features = [
  { icon: Brain, title: 'AI Study Tools', description: 'Chat with AI, generate flashcards, mind maps, quizzes, and summaries from your course materials.' },
  { icon: MessageSquare, title: 'Real-time AI Chat', description: 'RAG-powered chat with SSE streaming for instant, context-aware responses from your materials.' },
  { icon: Sparkles, title: 'Smart Grading', description: 'AI-assisted grading with detailed feedback, teacher approval workflow, and analytics.' },
  { icon: BarChart3, title: 'Engagement Tracking', description: 'Teachers monitor student engagement with color-coded risk indicators and automated alerts.' },
  { icon: GraduationCap, title: 'Exam System', description: 'Timed exams with auto-save, AI question generation, and comprehensive grade analytics.' },
  { icon: BookOpen, title: 'Material Viewer', description: 'Sectioned viewer for PDFs, videos, and audio with per-section AI actions and progress tracking.' },
  { icon: Mic, title: 'Dialogue Audio', description: 'AI-generated educational dialogues with text-to-speech, synchronized transcripts, and custom voices.' },
  { icon: Layers, title: 'Semantic Sections', description: 'Materials are automatically segmented and indexed for targeted study tools per section.' },
];

const stats = [
  { value: 8, suffix: '+', label: 'AI Features' },
  { value: 6, suffix: '+', label: 'Study Tools' },
  { value: 100, suffix: '%', label: 'Free to Start' },
  { value: 24, suffix: '/7', label: 'AI Available' },
];

export default function LandingPage() {
  const { isAuthenticated, isTeacher } = useAuthStore();

  const dashboardLink = isTeacher() ? '/teacher/dashboard' : '/dashboard';

  return (
    <div className="min-h-screen bg-background">
      {/* Nav */}
      <nav className="flex items-center justify-between px-6 py-4 max-w-7xl mx-auto">
        <Link to="/" className="flex items-center gap-2 font-bold text-xl group">
          <div className="h-9 w-9 rounded-lg bg-gradient-to-br from-primary to-accent flex items-center justify-center shadow-lg shadow-primary/20">
            <BookOpen className="h-5 w-5 text-white" />
          </div>
          <span className="gradient-text">AIEduPlatform</span>
        </Link>
        <div className="flex items-center gap-2">
          <ThemeToggle />
          <Link to="/courses">
            <Button variant="ghost">Courses</Button>
          </Link>
          {isAuthenticated ? (
            <Link to={dashboardLink}>
              <Button variant="gradient">Dashboard</Button>
            </Link>
          ) : (
            <>
              <Link to="/login">
                <Button variant="ghost">Login</Button>
              </Link>
              <Link to="/register">
                <Button variant="gradient">Get Started</Button>
              </Link>
            </>
          )}
        </div>
      </nav>

      {/* Hero */}
      <section className="relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-br from-primary/5 via-accent/5 to-transparent" />
        {/* Animated gradient orbs */}
        <motion.div
          className="absolute top-20 right-10 w-72 h-72 bg-primary/15 rounded-full blur-3xl"
          animate={{
            x: [0, 30, -20, 0],
            y: [0, -40, 20, 0],
            scale: [1, 1.1, 0.95, 1],
          }}
          transition={{ duration: 8, repeat: Infinity, ease: 'easeInOut' }}
        />
        <motion.div
          className="absolute bottom-10 left-10 w-96 h-96 bg-accent/15 rounded-full blur-3xl"
          animate={{
            x: [0, -30, 20, 0],
            y: [0, 30, -20, 0],
            scale: [1, 0.95, 1.1, 1],
          }}
          transition={{ duration: 10, repeat: Infinity, ease: 'easeInOut' }}
        />
        <motion.div
          className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-64 h-64 bg-primary/10 rounded-full blur-3xl"
          animate={{
            scale: [1, 1.2, 1],
            opacity: [0.3, 0.6, 0.3],
          }}
          transition={{ duration: 6, repeat: Infinity, ease: 'easeInOut' }}
        />

        <div className="relative max-w-5xl mx-auto text-center py-24 md:py-36 px-4">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0 }}
          >
            <span className="inline-block px-4 py-1.5 rounded-full bg-primary/10 text-primary text-sm font-medium mb-6 border border-primary/20">
              AI-Powered Learning Platform
            </span>
          </motion.div>

          <motion.h1
            className="text-5xl md:text-7xl font-extrabold tracking-tight mb-6 leading-tight"
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.15 }}
          >
            Learn Smarter with{' '}
            <span className="gradient-text">
              AI-Powered
            </span>{' '}
            Education
          </motion.h1>

          <motion.p
            className="text-lg md:text-xl text-muted-foreground max-w-2xl mx-auto mb-10"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.3 }}
          >
            An intelligent learning platform featuring AI study sessions, smart grading,
            real-time engagement tracking, and a NotebookLM-style study experience.
          </motion.p>

          <motion.div
            className="flex gap-4 justify-center"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.45 }}
          >
            {isAuthenticated ? (
              <Link to={dashboardLink}>
                <Button size="lg" variant="gradient" className="gap-2 text-base">
                  Go to Dashboard <ArrowRight className="h-4 w-4" />
                </Button>
              </Link>
            ) : (
              <Link to="/register">
                <Button size="lg" variant="gradient" className="gap-2 text-base">
                  Get Started <ArrowRight className="h-4 w-4" />
                </Button>
              </Link>
            )}
            <Link to="/courses">
              <Button variant="outline" size="lg" className="text-base">Browse Courses</Button>
            </Link>
          </motion.div>
        </div>
      </section>

      {/* Stats */}
      <section className="py-12 border-y border-border bg-card/50">
        <motion.div
          className="max-w-5xl mx-auto grid grid-cols-2 md:grid-cols-4 gap-8 px-4 text-center"
          variants={staggerContainer}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true }}
        >
          {stats.map((stat) => (
            <motion.div key={stat.label} variants={fadeInUp}>
              <AnimatedCounter
                target={stat.value}
                suffix={stat.suffix}
                className="text-3xl font-bold gradient-text"
              />
              <div className="text-sm text-muted-foreground mt-1">{stat.label}</div>
            </motion.div>
          ))}
        </motion.div>
      </section>

      {/* Features */}
      <section className="py-20 px-4">
        <div className="max-w-6xl mx-auto">
          <motion.div
            className="text-center mb-14"
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.5 }}
          >
            <h2 className="text-3xl md:text-4xl font-bold mb-4">
              Everything you need to learn effectively
            </h2>
            <p className="text-muted-foreground max-w-xl mx-auto">
              From AI-powered study sessions to smart grading, our platform provides all the tools
              students and teachers need.
            </p>
          </motion.div>

          <motion.div
            className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6"
            variants={staggerContainer}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, margin: '-100px' }}
          >
            {features.map((feature) => (
              <motion.div
                key={feature.title}
                variants={fadeInUp}
                whileHover={{ y: -4 }}
                className="group p-6 bg-card rounded-xl border border-border hover:border-primary/30 hover:shadow-xl hover:shadow-primary/5 transition-all duration-300"
              >
                <div className="h-12 w-12 rounded-xl bg-gradient-to-br from-primary/10 to-accent/10 flex items-center justify-center mb-4 group-hover:from-primary/20 group-hover:to-accent/20 transition-all duration-300">
                  <feature.icon className="h-6 w-6 text-primary" />
                </div>
                <h3 className="text-lg font-semibold mb-2">{feature.title}</h3>
                <p className="text-sm text-muted-foreground leading-relaxed">{feature.description}</p>
              </motion.div>
            ))}
          </motion.div>
        </div>
      </section>

      {/* CTA */}
      <motion.section
        className="py-20 px-4"
        initial={{ opacity: 0, scale: 0.95 }}
        whileInView={{ opacity: 1, scale: 1 }}
        viewport={{ once: true }}
        transition={{ duration: 0.5 }}
      >
        <div className="max-w-3xl mx-auto text-center bg-gradient-to-r from-primary to-accent rounded-2xl p-12 text-white relative overflow-hidden">
          {/* Decorative elements */}
          <div className="absolute top-4 right-8 w-20 h-20 bg-white/10 rounded-full blur-xl" />
          <div className="absolute bottom-4 left-8 w-32 h-32 bg-white/5 rounded-full blur-2xl" />
          <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-48 h-48 bg-white/5 rounded-full blur-2xl" />

          <h2 className="text-3xl font-bold mb-4 relative z-10">Ready to start learning?</h2>
          <p className="text-lg opacity-90 mb-8 relative z-10">
            Join our AI-powered platform and transform the way you learn.
          </p>
          <Link to={isAuthenticated ? dashboardLink : '/register'} className="relative z-10">
            <Button size="lg" variant="secondary" className="gap-2 text-base font-semibold">
              {isAuthenticated ? 'Go to Dashboard' : 'Create Free Account'} <ArrowRight className="h-4 w-4" />
            </Button>
          </Link>
        </div>
      </motion.section>

      {/* Footer */}
      <footer className="py-8 px-4 border-t border-border text-center text-sm text-muted-foreground">
        <p>&copy; {new Date().getFullYear()} AIEduPlatform. All rights reserved.</p>
      </footer>
    </div>
  );
}
