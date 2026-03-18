import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersApi } from '@/api/users.api';
import type { UserProfile } from '@/types';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { FileInput } from '@/components/ui/FileInput';
import { PageSpinner } from '@/components/ui/Spinner';
import { Badge } from '@/components/ui/Badge';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';
import { useState, useRef } from 'react';
import { User, BookOpen, Award, Clock } from 'lucide-react';
import { formatDate } from '@/utils/formatters';

export default function ProfilePage() {
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState(false);
  const avatarRef = useRef<HTMLInputElement>(null);

  const { data: profile, isLoading, isError } = useQuery({
    queryKey: ['profile'],
    queryFn: () => usersApi.getMe(),
    select: (res) => res.data.data as UserProfile,
  });

  const { data: stats } = useQuery({
    queryKey: ['user-stats'],
    queryFn: () => usersApi.getStats(),
    select: (res) => res.data.data,
    retry: 1,
  });

  const form = useForm();

  const updateMutation = useMutation({
    mutationFn: (data: FormData) => usersApi.updateMe(data),
    onSuccess: () => {
      toast.success('Profile updated');
      queryClient.invalidateQueries({ queryKey: ['profile'] });
      setEditing(false);
    },
    onError: () => toast.error('Failed to update profile'),
  });

  const handleSubmit = form.handleSubmit((values) => {
    const fd = new FormData();
    Object.entries(values).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== '') fd.append(k, v as string);
    });
    const files = avatarRef.current?.files;
    if (files?.[0]) fd.append('avatar', files[0]);
    updateMutation.mutate(fd);
  });

  if (isLoading) return <PageSpinner />;

  if (isError || !profile) {
    return (
      <AnimatedPage>
        <div className="max-w-2xl mx-auto px-4 py-16 text-center">
          <User className="h-12 w-12 mx-auto mb-4 text-muted-foreground/40" />
          <p className="font-medium text-muted-foreground">Failed to load profile</p>
          <p className="text-sm text-muted-foreground/70 mt-1">Please try refreshing the page.</p>
        </div>
      </AnimatedPage>
    );
  }

  if (editing) {
    return (
      <AnimatedPage>
      <div className="max-w-2xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold mb-6">Edit Profile</h1>
        <Card>
          <CardContent className="p-6">
            <form onSubmit={handleSubmit} className="space-y-5">
              <FileInput
                ref={avatarRef}
                label="Avatar"
                accept="image/*"
                hint="Upload a profile picture (JPG, PNG, max 5MB)"
              />

              <div className="grid grid-cols-2 gap-4">
                <Input
                  label="First Name"
                  placeholder="Enter your first name"
                  defaultValue={profile.firstName ?? ''}
                  {...form.register('firstName')}
                />
                <Input
                  label="Last Name"
                  placeholder="Enter your last name"
                  defaultValue={profile.lastName ?? ''}
                  {...form.register('lastName')}
                />
              </div>

              <Textarea
                label="Bio"
                placeholder="Write a short bio about yourself..."
                hint="Tell others what you are passionate about"
                defaultValue={profile.bio ?? ''}
                {...form.register('bio')}
              />

              <Input
                label="Location"
                placeholder="e.g. Cairo, Egypt"
                defaultValue={profile.location ?? ''}
                {...form.register('location')}
              />

              {/* URLs section */}
              <div className="space-y-5 rounded-lg border border-border bg-secondary/30 p-4">
                <p className="text-sm font-medium text-foreground -mt-1">Links</p>
                <Input
                  label="Website"
                  placeholder="https://your-website.com"
                  hint="Your personal or portfolio website"
                  defaultValue={profile.website ?? ''}
                  {...form.register('website')}
                />
                <Input
                  label="LinkedIn URL"
                  placeholder="https://linkedin.com/in/your-profile"
                  defaultValue={profile.linkedInUrl ?? ''}
                  {...form.register('linkedInUrl')}
                />
              </div>

              <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
                <Button type="submit" variant="gradient" loading={updateMutation.isPending}>Save Changes</Button>
                <Button variant="outline" type="button" onClick={() => setEditing(false)}>Cancel</Button>
              </div>
            </form>
          </CardContent>
        </Card>
      </div>
      </AnimatedPage>
    );
  }

  return (
    <AnimatedPage>
    <div className="max-w-4xl mx-auto px-4 py-8">
      <Card>
        <CardContent className="p-6">
          <div className="flex items-start gap-6">
            <div className="relative">
              {profile.avatarUrl ? (
                <img src={profile.avatarUrl} alt="Avatar" className="w-24 h-24 rounded-full object-cover" />
              ) : (
                <div className="w-24 h-24 rounded-full bg-muted flex items-center justify-center">
                  <User className="h-10 w-10 text-muted-foreground" />
                </div>
              )}
            </div>
            <div className="flex-1">
              <div className="flex items-center gap-3">
                <h1 className="text-2xl font-bold">
                  {profile.firstName && profile.lastName
                    ? `${profile.firstName} ${profile.lastName}`
                    : profile.userName}
                </h1>
                {profile.roles.map((r) => (
                  <Badge key={r} variant={r === 'Teacher' ? 'warning' : 'default'}>{r}</Badge>
                ))}
              </div>
              {profile.title && <p className="text-muted-foreground mt-1">{profile.title}</p>}
              {profile.bio && <p className="mt-2">{profile.bio}</p>}
              <p className="text-sm text-muted-foreground mt-2">{profile.email}</p>
              <p className="text-xs text-muted-foreground">Joined {formatDate(profile.createdAt)}</p>
              <Button className="mt-4" onClick={() => {
                form.reset();
                setEditing(true);
              }}>
                Edit Profile
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {stats && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-6">
          <Card>
            <CardContent className="p-4 text-center">
              <BookOpen className="h-6 w-6 mx-auto mb-2 text-primary" />
              <p className="text-2xl font-bold">{stats.coursesEnrolled}</p>
              <p className="text-xs text-muted-foreground">Courses Enrolled</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 text-center">
              <Award className="h-6 w-6 mx-auto mb-2 text-success" />
              <p className="text-2xl font-bold">{stats.coursesCompleted}</p>
              <p className="text-xs text-muted-foreground">Completed</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 text-center">
              <Clock className="h-6 w-6 mx-auto mb-2 text-accent" />
              <p className="text-2xl font-bold">{stats.totalStudySessions}</p>
              <p className="text-xs text-muted-foreground">Study Sessions</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 text-center">
              <Award className="h-6 w-6 mx-auto mb-2 text-warning" />
              <p className="text-2xl font-bold">{stats.averageExamScore?.toFixed(0) ?? 0}%</p>
              <p className="text-xs text-muted-foreground">Avg Exam Score</p>
            </CardContent>
          </Card>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-6">
        {profile.location && (
          <Card>
            <CardContent className="p-4">
              <p className="text-sm text-muted-foreground">Location</p>
              <p className="font-medium">{profile.location}</p>
            </CardContent>
          </Card>
        )}
        {profile.website && (
          <Card>
            <CardContent className="p-4">
              <p className="text-sm text-muted-foreground">Website</p>
              <a href={profile.website} target="_blank" rel="noopener noreferrer" className="font-medium text-primary hover:underline">{profile.website}</a>
            </CardContent>
          </Card>
        )}
        {profile.qualifications && (
          <Card>
            <CardContent className="p-4">
              <p className="text-sm text-muted-foreground">Qualifications</p>
              <p className="font-medium">{profile.qualifications}</p>
            </CardContent>
          </Card>
        )}
        {profile.expertiseAreas && (
          <Card>
            <CardContent className="p-4">
              <p className="text-sm text-muted-foreground">Expertise</p>
              <p className="font-medium">{profile.expertiseAreas}</p>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
    </AnimatedPage>
  );
}
