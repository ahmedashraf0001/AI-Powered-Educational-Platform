import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useAuth } from '@/hooks/useAuth';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Link } from 'react-router-dom';
import { useState } from 'react';
import { cn } from '@/utils/cn';
import { GraduationCap, BookOpenCheck } from 'lucide-react';

const registerSchema = z
  .object({
    fullName: z.string().min(2, 'Full name is required'),
    userName: z.string().min(3, 'Username must be at least 3 characters'),
    email: z.string().email('Invalid email address'),
    password: z.string().min(6, 'Password must be at least 6 characters'),
    confirmPassword: z.string(),
    bio: z.string().optional(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

type RegisterFormData = z.infer<typeof registerSchema>;

export function RegisterForm() {
  const { registerStudent, registerStudentLoading, registerTeacher, registerTeacherLoading } = useAuth();
  const [role, setRole] = useState<'student' | 'teacher'>('student');

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormData>({ resolver: zodResolver(registerSchema) });

  const isPending = registerStudentLoading || registerTeacherLoading;

  const onSubmit = (data: RegisterFormData) => {
    if (role === 'teacher') {
      registerTeacher({
        fullName: data.fullName,
        userName: data.userName,
        email: data.email,
        password: data.password,
        confirmPassword: data.confirmPassword,
        bio: data.bio || '',
      });
    } else {
      registerStudent({
        fullName: data.fullName,
        userName: data.userName,
        email: data.email,
        password: data.password,
        confirmPassword: data.confirmPassword,
      });
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      {/* Role selector */}
      <div>
        <label className="block text-sm font-medium text-foreground mb-2">I want to join as</label>
        <div className="grid grid-cols-2 gap-3">
          <button
            type="button"
            className={cn(
              'flex flex-col items-center gap-2 py-4 text-sm font-medium rounded-lg border-2 transition-all duration-200',
              role === 'student'
                ? 'border-primary bg-primary/10 text-primary shadow-sm shadow-primary/10 ring-1 ring-primary/20'
                : 'border-border bg-background text-muted-foreground hover:bg-secondary hover:border-border/80'
            )}
            onClick={() => setRole('student')}
          >
            <GraduationCap className="h-6 w-6" />
            Student
          </button>
          <button
            type="button"
            className={cn(
              'flex flex-col items-center gap-2 py-4 text-sm font-medium rounded-lg border-2 transition-all duration-200',
              role === 'teacher'
                ? 'border-primary bg-primary/10 text-primary shadow-sm shadow-primary/10 ring-1 ring-primary/20'
                : 'border-border bg-background text-muted-foreground hover:bg-secondary hover:border-border/80'
            )}
            onClick={() => setRole('teacher')}
          >
            <BookOpenCheck className="h-6 w-6" />
            Teacher
          </button>
        </div>
      </div>

      {/* Personal info */}
      <Input
        label="Full Name"
        placeholder="e.g. John Doe"
        error={errors.fullName?.message}
        {...register('fullName')}
      />
      <Input
        label="Username"
        placeholder="e.g. johndoe"
        hint="This will be your unique identifier"
        error={errors.userName?.message}
        {...register('userName')}
      />
      <Input
        label="Email"
        type="email"
        placeholder="you@example.com"
        error={errors.email?.message}
        {...register('email')}
      />

      {/* Password section */}
      <div className="space-y-5 rounded-lg border border-border bg-secondary/30 p-4">
        <p className="text-sm font-medium text-foreground -mt-1">Set your password</p>
        <Input
          label="Password"
          type="password"
          placeholder="At least 6 characters"
          error={errors.password?.message}
          {...register('password')}
        />
        <Input
          label="Confirm Password"
          type="password"
          placeholder="Re-enter your password"
          error={errors.confirmPassword?.message}
          {...register('confirmPassword')}
        />
      </div>

      {role === 'teacher' && (
        <Input
          label="Bio"
          placeholder="Tell students a bit about yourself and your expertise"
          hint="This will be visible on your profile"
          error={errors.bio?.message}
          {...register('bio')}
        />
      )}

      <Button type="submit" variant="gradient" className="w-full" loading={isPending}>
        Create Account
      </Button>
      <p className="text-center text-sm text-muted-foreground pt-2">
        Already have an account?{' '}
        <Link to="/login" className="font-medium text-primary hover:text-primary/80 hover:underline transition-colors">
          Sign in
        </Link>
      </p>
    </form>
  );
}
