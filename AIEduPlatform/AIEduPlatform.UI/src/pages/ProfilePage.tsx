import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersApi } from '@/api/users.api';
import type { UserProfile } from '@/types';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { FileInput } from '@/components/ui/FileInput';
import { PageSpinner } from '@/components/ui/Spinner';
import { Badge } from '@/components/ui/Badge';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';
import { useState, useRef, useEffect } from 'react';
import { User, BookOpen, Award, Clock, GraduationCap, Users, MapPin, Link as LinkIcon, Mail, Calendar, Briefcase, Edit2, ShieldCheck, Linkedin } from 'lucide-react';
import { formatDate } from '@/utils/formatters';
import { useAuthStore } from '@/stores/authStore';
import { resolveUrl } from '@/utils/url';

export default function ProfilePage() {
  const queryClient = useQueryClient();
  const { roles } = useAuthStore();
  const isTeacher = roles.includes('Teacher');
  const [editing, setEditing] = useState(false);
  const avatarRef = useRef<HTMLInputElement>(null);

  const { data: profile, isLoading, isError, error } = useQuery({
    queryKey: ['profile'],
    queryFn: async () => {
      const res = await usersApi.getMe();
      return res.data.data as UserProfile;
    },
  });

  const { data: stats } = useQuery({
    queryKey: ['user-stats'],
    queryFn: async () => {
      const res = await usersApi.getStats();
      return res.data.data;
    },
    retry: 1,
  });

  const { data: studentDashboard } = useQuery({
    queryKey: ['student-dashboard'],
    queryFn: async () => {
      const res = await usersApi.getStudentDashboard();
      return res.data.data;
    },
    enabled: !isTeacher,
  });

  const { data: teacherDashboard } = useQuery({
    queryKey: ['teacher-dashboard'],
    queryFn: async () => {
      const res = await usersApi.getTeacherDashboard();
      return res.data.data;
    },
    enabled: isTeacher,
  });

  const form = useForm();
  const resolvedAvatarUrl = resolveUrl(profile?.avatarUrl) ?? '/placeholders/avatar.svg';
  const [avatarSrc, setAvatarSrc] = useState(resolvedAvatarUrl);

  useEffect(() => {
    setAvatarSrc(resolvedAvatarUrl);
  }, [resolvedAvatarUrl]);

  const updateMutation = useMutation({
    mutationFn: (data: FormData) => usersApi.updateMe(data),
    onSuccess: () => {
      toast.success('Profile updated');
      queryClient.invalidateQueries({ queryKey: ['profile'] });
      setEditing(false);
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const onSubmit = (values: Record<string, unknown>) => {
    const fd = new FormData();
    Object.entries(values).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== '') fd.append(k, v as string);
    });
    const files = avatarRef.current?.files;
    if (files?.[0]) fd.append('avatar', files[0]);
    updateMutation.mutate(fd);
  };

  if (isLoading) return <PageSpinner />;

  if (isError || !profile) {
    console.error("Profile load error:", error);
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
            {/* eslint-disable-next-line react-hooks/refs, @typescript-eslint/no-explicit-any */}
            <form onSubmit={form.handleSubmit(onSubmit as any)} className="space-y-5">
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

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Textarea
                  label="Qualifications"
                  placeholder="e.g. B.Sc. in Computer Science"
                  hint="Your degrees and certifications"
                  defaultValue={profile.qualifications ?? ''}
                  {...form.register('qualifications')}
                />
                <Textarea
                  label="Areas of Expertise"
                  placeholder="e.g. AI, Machine Learning, Web Development"
                  hint="What you specialize in"
                  defaultValue={profile.expertiseAreas ?? ''}
                  {...form.register('expertiseAreas')}
                />
              </div>

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
    <div className="max-w-5xl mx-auto px-4 py-8 space-y-6">
      {/* Hero Banner & Avatar Section */}
      <Card className="overflow-hidden border-none shadow-sm">
        <div className="h-32 md:h-48 bg-gradient-to-r from-primary/80 via-primary to-primary/60" />
        <CardContent className="p-0 sm:px-8 sm:pb-8 relative flex flex-col sm:flex-row gap-6">
          <div className="flex justify-center sm:justify-start -mt-16 sm:-mt-20">
            <div className="relative p-1.5 bg-background rounded-full shadow-sm">
              <img
                src={avatarSrc}
                alt="Avatar"
                className="w-32 h-32 sm:w-40 sm:h-40 rounded-full object-cover border-4 border-background bg-secondary"
                onError={() => setAvatarSrc('/placeholders/avatar.svg')}
              />
              <Button
                size="icon"
                variant="secondary"
                className="absolute bottom-2 right-2 rounded-full shadow-md w-10 h-10 border-2 border-background"
                onClick={() => {
                  form.reset();
                  setEditing(true);
                }}
                title="Edit Profile"
              >
                <Edit2 className="h-4 w-4" />
              </Button>
            </div>
          </div>
          
          <div className="flex-1 text-center sm:text-left pt-2 sm:pt-4 pb-6 sm:pb-0 px-4 sm:px-0">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
              <div>
                <h1 className="text-3xl font-bold text-foreground">
                  {profile.firstName && profile.lastName
                    ? `${profile.firstName} ${profile.lastName}`
                    : profile.userName}
                </h1>
                <div className="flex flex-wrap items-center justify-center sm:justify-start gap-2 mt-2">
                  <span className="text-muted-foreground flex items-center gap-1.5 text-sm">
                    <Mail className="h-4 w-4" /> {profile.email}
                  </span>
                  {profile.location && (
                    <span className="text-muted-foreground flex items-center gap-1.5 text-sm">
                      <span className="w-1 h-1 rounded-full bg-border" />
                      <MapPin className="h-4 w-4" /> {profile.location}
                    </span>
                  )}
                  <span className="text-muted-foreground flex items-center gap-1.5 text-sm">
                    <span className="w-1 h-1 rounded-full bg-border" />
                    <Calendar className="h-4 w-4" /> Joined {formatDate(profile.createdAt)}
                  </span>
                </div>
              </div>
              <div className="flex flex-wrap justify-center gap-2">
                {profile.roles.map((r) => (
                  <Badge key={r} variant={r === 'Teacher' ? 'warning' : 'outline'} className="text-sm px-3 py-1 shadow-sm">
                    {r === 'Teacher' && <ShieldCheck className="h-3.5 w-3.5 mr-1" />}
                    {r}
                  </Badge>
                ))}
              </div>
            </div>

            {profile.bio && (
              <p className="mt-4 text-foreground/90 max-w-3xl leading-relaxed whitespace-pre-wrap text-sm sm:text-base">
                {profile.bio}
              </p>
            )}
            
            {(profile.website || profile.linkedInUrl) && (
              <div className="flex flex-wrap items-center justify-center sm:justify-start gap-4 mt-5 pt-5 border-t border-border/50">
                {profile.website && (
                  <a href={profile.website} target="_blank" rel="noopener noreferrer" className="flex items-center gap-2 text-sm font-medium text-primary hover:text-primary/80 transition-colors">
                    <LinkIcon className="h-4 w-4" /> Portfolio / Website
                  </a>
                )}
                {profile.linkedInUrl && (
                  <a href={profile.linkedInUrl} target="_blank" rel="noopener noreferrer" className="flex items-center gap-2 text-sm font-medium text-blue-600 dark:text-blue-400 hover:opacity-80 transition-colors">
                    <Linkedin className="h-4 w-4" /> LinkedIn Profile
                  </a>
                )}
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left Column: Stats */}
        <div className="lg:col-span-2 space-y-6">
          {stats && (
            <Card>
              <div className="p-6">
                <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                  <Award className="h-5 w-5 text-primary" /> Activity Stats
                </h3>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  {isTeacher ? (
                    <>
                      <div className="bg-secondary/30 rounded-xl p-4 flex flex-col items-center justify-center text-center hover:bg-secondary/50 transition-colors border border-border/40">
                        <BookOpen className="h-7 w-7 mb-3 text-primary" />
                        <p className="text-3xl font-bold">{stats.coursesTaught}</p>
                        <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mt-1">Courses Taught</p>
                      </div>
                      <div className="bg-secondary/30 rounded-xl p-4 flex flex-col items-center justify-center text-center hover:bg-secondary/50 transition-colors border border-border/40">
                        <Users className="h-7 w-7 mb-3 text-success" />
                        <p className="text-3xl font-bold">{stats.coursesEnrolled}</p>
                        <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mt-1">Total Students</p>
                      </div>
                      <div className="bg-secondary/30 rounded-xl p-4 flex flex-col items-center justify-center text-center hover:bg-secondary/50 transition-colors border border-border/40">
                        <Award className="h-7 w-7 mb-3 text-warning" />
                        <p className="text-3xl font-bold">{stats.averageExamScore > 0 ? stats.averageExamScore.toFixed(1) : 'N/A'}</p>
                        <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mt-1">Avg Rating</p>
                      </div>
                      <div className="bg-secondary/30 rounded-xl p-4 flex flex-col items-center justify-center text-center hover:bg-secondary/50 transition-colors border border-border/40">
                        <GraduationCap className="h-7 w-7 mb-3 text-accent" />
                        <p className="text-3xl font-bold">{stats.examsTaken}</p>
                        <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mt-1">Exams Created</p>
                      </div>
                    </>
                  ) : (
                    <>
                      <div className="bg-secondary/30 rounded-xl p-4 flex flex-col items-center justify-center text-center hover:bg-secondary/50 transition-colors border border-border/40">
                        <BookOpen className="h-7 w-7 mb-3 text-primary" />
                        <p className="text-3xl font-bold">{stats.coursesEnrolled}</p>
                        <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mt-1">Courses Enrolled</p>
                      </div>
                      <div className="bg-secondary/30 rounded-xl p-4 flex flex-col items-center justify-center text-center hover:bg-secondary/50 transition-colors border border-border/40">
                        <Award className="h-7 w-7 mb-3 text-success" />
                        <p className="text-3xl font-bold">{stats.coursesCompleted}</p>
                        <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mt-1">Completed</p>
                      </div>
                      <div className="bg-secondary/30 rounded-xl p-4 flex flex-col items-center justify-center text-center hover:bg-secondary/50 transition-colors border border-border/40">
                        <Clock className="h-7 w-7 mb-3 text-accent" />
                        <p className="text-3xl font-bold">{stats.totalStudySessions}</p>
                        <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mt-1">Study Sessions</p>
                      </div>
                      <div className="bg-secondary/30 rounded-xl p-4 flex flex-col items-center justify-center text-center hover:bg-secondary/50 transition-colors border border-border/40">
                        <Award className="h-7 w-7 mb-3 text-warning" />
                        <p className="text-3xl font-bold">{stats.averageExamScore?.toFixed(0) ?? 0}%</p>
                        <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mt-1">Avg Exam Score</p>
                      </div>
                    </>
                  )}
                </div>
              </div>
            </Card>
          )}

          {/* Recent Activity */}
          <Card>
            <div className="p-6">
              <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                <Clock className="h-5 w-5 text-muted-foreground" /> Recent Activity
              </h3>
              
              {!isTeacher && studentDashboard?.recentActivity && studentDashboard.recentActivity.length > 0 ? (
                <div className="space-y-4">
                  {studentDashboard.recentActivity.slice(0, 5).map((activity, index) => (
                    <div key={index} className="flex items-start gap-3 pb-4 border-b border-border/40 last:border-0 last:pb-0">
                      <div className="bg-primary/10 rounded-full p-2 shrink-0">
                        <BookOpen className="h-4 w-4 text-primary" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-foreground">
                          {activity.lectureTitle} <span className="font-normal text-muted-foreground">in</span> {activity.courseTitle}
                        </p>
                        <p className="text-xs text-muted-foreground mt-1">
                          {activity.completedAt ? `Completed on ${formatDate(activity.completedAt)}` : 'In progress'}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              ) : isTeacher && teacherDashboard?.recentEnrollments && teacherDashboard.recentEnrollments.length > 0 ? (
                <div className="space-y-4">
                  {teacherDashboard.recentEnrollments.slice(0, 5).map((enrollment, index) => (
                    <div key={index} className="flex items-start gap-3 pb-4 border-b border-border/40 last:border-0 last:pb-0">
                      <div className="bg-success/10 rounded-full p-2 shrink-0">
                        <Users className="h-4 w-4 text-success" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-foreground">
                          {enrollment.studentName} <span className="font-normal text-muted-foreground">enrolled in</span> {enrollment.courseName}
                        </p>
                        <p className="text-xs text-muted-foreground mt-1">
                          {formatDate(enrollment.enrolledAt)}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-10 px-4">
                  <div className="bg-secondary/20 h-16 w-16 rounded-full flex items-center justify-center mx-auto mb-4">
                    <Calendar className="h-8 w-8 text-muted-foreground/50" />
                  </div>
                  <h4 className="text-foreground font-medium mb-1">No recent activity found</h4>
                  <p className="text-sm text-muted-foreground max-w-md mx-auto">
                    {isTeacher 
                      ? "When you create courses or students enroll, your recent interactions will appear here."
                      : "When you enroll in courses, complete materials, or finish study sessions, your activity will appear here."}
                  </p>
                </div>
              )}
            </div>
          </Card>
        </div>

        {/* Right Column: Details */}
        <div className="space-y-6">
          {/* Professional Details Card */}
          <Card className="h-full">
            <div className="p-6">
              <h3 className="text-lg font-semibold mb-5 flex items-center gap-2">
                <Briefcase className="h-5 w-5 text-muted-foreground" /> Additional Details
              </h3>
              
              <div className="space-y-6">
                {profile.qualifications ? (
                  <div>
                    <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-2">Qualifications</h4>
                    <p className="text-sm font-medium leading-relaxed">{profile.qualifications}</p>
                  </div>
                ) : (
                  <div>
                    <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-2">Qualifications</h4>
                    <p className="text-sm text-muted-foreground italic">Not specified</p>
                  </div>
                )}
                
                <div className="h-px w-full bg-border/50" />

                {profile.expertiseAreas ? (
                  <div>
                    <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-2">Areas of Expertise</h4>
                    <p className="text-sm font-medium leading-relaxed">{profile.expertiseAreas}</p>
                  </div>
                ) : (
                  <div>
                    <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider mb-2">Areas of Expertise</h4>
                    <p className="text-sm text-muted-foreground italic">Not specified</p>
                  </div>
                )}
              </div>
            </div>
          </Card>
        </div>
      </div>
    </div>
    </AnimatedPage>
  );
}

