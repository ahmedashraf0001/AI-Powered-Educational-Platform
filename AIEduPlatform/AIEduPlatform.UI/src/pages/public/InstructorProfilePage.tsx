import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { usersApi } from '@/api/users.api';
import { coursesApi } from '@/api/courses.api';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { PageSpinner } from '@/components/ui/Spinner';
import { Card, CardContent } from '@/components/ui/Card';
import { CourseCard } from '@/components/courses/CourseCard';
import { GraduationCap, MapPin, Globe, Linkedin } from 'lucide-react';
import { resolveUrl } from '@/utils/url';
import { formatDate } from '@/utils/formatters';
import { useEffect, useState } from 'react';

export default function InstructorProfilePage() {
  const { instructorId } = useParams<{ instructorId: string }>();

  const { data: profile, isLoading: isProfileLoading } = useQuery({
    queryKey: ['instructor-profile', instructorId],
    queryFn: () => usersApi.getById(instructorId!),
    select: (res) => res.data.data,
    enabled: !!instructorId,
  });

  const { data: coursesData, isLoading: isCoursesLoading } = useQuery({
    queryKey: ['instructor-courses', instructorId],
    queryFn: () => coursesApi.getByInstructor(instructorId!),
    select: (res) => res.data.data,
    enabled: !!instructorId,
  });

  const [avatarError, setAvatarError] = useState(false);

  useEffect(() => {
    setAvatarError(false);
  }, [profile?.avatarUrl]);

  if (isProfileLoading || isCoursesLoading) return <PageSpinner />;
  if (!profile) return <div className="p-8 text-center">User not found</div>;

  const isTeacher = profile.roles?.includes('Teacher');
  const fullName = profile.firstName || profile.lastName 
    ? `${profile.firstName || ''} ${profile.lastName || ''}`.trim() 
    : profile.userName;
  const resolvedAvatarUrl = resolveUrl(profile.avatarUrl) ?? '/placeholders/avatar.svg';
  const avatarSrc = avatarError ? '/placeholders/avatar.svg' : resolvedAvatarUrl;

  const hasContactInfo = profile.location || profile.website || profile.linkedInUrl;
  const hasAboutOrContact = profile.bio || hasContactInfo;
  


    


  return (
    <AnimatedPage>
      <div className="max-w-6xl mx-auto px-4 py-8">
        {/* Profile Header */}
        <Card className="mb-10 overflow-hidden border-none shadow-md">
          <div className="h-32 bg-gradient-to-r from-primary/30 to-primary/10"></div>
          <CardContent className="px-6 pb-6 relative pt-0 text-center sm:text-left">
            <div className="flex flex-col sm:flex-row items-center sm:items-end gap-6 -mt-16 sm:-mt-12 mb-6">
              <div className="relative">
                {profile.avatarUrl ? (
                  <img
                    src={avatarSrc}
                    alt={fullName}
                    className="w-32 h-32 rounded-full border-4 border-card object-cover bg-card shadow-sm"
                    onError={() => setAvatarError(true)}
                  />
                ) : (
                  <div className="w-32 h-32 rounded-full border-4 border-card bg-primary/10 flex items-center justify-center text-primary shadow-sm">
                    <GraduationCap className="w-12 h-12" />
                  </div>
                )}
                {isTeacher && (
                  <span className="absolute bottom-2 right-2 bg-primary text-primary-foreground text-[10px] font-bold px-2 py-0.5 rounded-full border-2 border-card shadow-sm">
                    INSTRUCTOR
                  </span>
                )}
              </div>
              <div className="flex-1 pb-2">
                <h1 className="text-3xl font-bold">{fullName}</h1>
                <p className="text-muted-foreground mt-1">
                  Member since {formatDate(profile.createdAt)}
                </p>
              </div>
            </div>

            {hasAboutOrContact ? (
              <div className="grid grid-cols-1 md:grid-cols-3 gap-8 text-sm pt-6 mt-6 border-t border-border/50">     
                <div className="md:col-span-2 space-y-4">
                  {profile.bio && (
                    <div>
                      <h3 className="font-semibold text-base mb-2">About Me</h3>  
                      <p className="text-muted-foreground whitespace-pre-wrap leading-relaxed">
                        {profile.bio}
                      </p>
                    </div>
                  )}
                </div>
                {hasContactInfo && (
                  <div className="space-y-4">
                    <h3 className="font-semibold text-base mb-2">Contact & Info</h3>
                    <ul className="space-y-3 text-muted-foreground">
                      {profile.location && (
                        <li className="flex items-center gap-3">
                          <MapPin className="h-4 w-4 text-primary/70" /> <span>{profile.location}</span>
                        </li>
                      )}
                      {profile.website && (
                        <li className="flex items-center gap-3 flex-wrap">
                          <Globe className="h-4 w-4 text-primary/70 shrink-0" />
                          <a href={profile.website} target="_blank" rel="noopener noreferrer" className="hover:text-primary transition-colors underline-offset-4 hover:underline break-all">
                            {profile.website.replace(/^https?:\/\//, '')}
                          </a>
                        </li>
                      )}
                      {profile.linkedInUrl && (
                        <li className="flex items-center gap-3 flex-wrap">
                          <Linkedin className="h-4 w-4 text-primary/70 shrink-0" />
                          <a href={profile.linkedInUrl} target="_blank" rel="noopener noreferrer" className="hover:text-primary transition-colors underline-offset-4 hover:underline break-all">
                            LinkedIn Profile
                          </a>
                        </li>
                      )}
                    </ul>
                  </div>
                )}
              </div>
            ) : (
              <div className="text-center py-10 mt-6 border-t border-border/50">
                <p className="text-muted-foreground">This user hasn't added any details to their profile yet.</p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Authored Courses */}
        {isTeacher && coursesData?.items && coursesData.items.length > 0 && (
          <div className="space-y-6">
            <div className="flex items-center justify-between">
              <h2 className="text-2xl font-bold flex items-center gap-2">
                <GraduationCap className="h-6 w-6 text-primary" />
                Courses by {fullName}
              </h2>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
              {coursesData.items.map((course) => (
                <CourseCard key={course.courseId} course={course} />
              ))}
            </div>
          </div>
        )}
      </div>
    </AnimatedPage>
  );
}
