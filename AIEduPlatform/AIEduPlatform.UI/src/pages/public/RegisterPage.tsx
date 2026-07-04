import { RegisterForm } from '@/components/auth/RegisterForm';
import { BookOpen } from 'lucide-react';
import { motion } from 'framer-motion';

export default function RegisterPage() {
  return (
    <div className="min-h-screen flex items-center justify-center px-4 py-12 bg-background relative overflow-hidden">
      {/* Animated background orbs */}
      <motion.div
        className="absolute top-1/3 right-1/4 w-96 h-96 bg-primary/5 rounded-full blur-3xl"
        animate={{ scale: [1, 1.15, 1], opacity: [0.3, 0.5, 0.3] }}
        transition={{ duration: 9, repeat: Infinity, ease: 'easeInOut' }}
      />
      <motion.div
        className="absolute bottom-1/3 left-1/4 w-80 h-80 bg-accent/5 rounded-full blur-3xl"
        animate={{ scale: [1, 0.85, 1], opacity: [0.2, 0.4, 0.2] }}
        transition={{ duration: 11, repeat: Infinity, ease: 'easeInOut' }}
      />

      <motion.div
        className="w-full max-w-md relative z-10"
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, delay: 0.1 }}
      >
        <motion.div
          className="text-center mb-8"
          initial={{ scale: 0.8, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          transition={{ type: 'spring', stiffness: 200, delay: 0 }}
        >
          <div className="h-14 w-14 rounded-xl bg-gradient-to-br from-primary to-accent flex items-center justify-center mx-auto mb-4 shadow-lg shadow-primary/20">
            <BookOpen className="h-7 w-7 text-white" />
          </div>
          <h1 className="text-2xl font-bold">Create an account</h1>
          <p className="text-muted-foreground">Join Learnify today</p>
        </motion.div>
        <div className="border border-border rounded-xl p-6 bg-card/80 backdrop-blur-sm shadow-xl">
          <RegisterForm />
        </div>
      </motion.div>
    </div>
  );
}
