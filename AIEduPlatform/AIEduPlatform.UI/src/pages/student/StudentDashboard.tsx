import { useQuery } from '@tanstack/react-query';
import { usersApi } from '@/api/users.api';
import { coursesApi } from '@/api/courses.api';
import { useAuthStore } from '@/stores/authStore';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { useNavigate } from 'react-router-dom';
import {
  BookOpen,
  Activity,
  Flame,
  PlayCircle,
  ArrowRight,
  TrendingUp,
  Star,
  Target,
  GraduationCap,
  Sparkles
} from 'lucide-react';
import { motion } from 'framer-motion';
import { useEffect, useState } from 'react';
import type {
  StudentDashboard as StudentDashboardType,
  CourseListDto,
  RecommendationSectionsDto,
} from '@/types';
import { resolveUrl } from '@/utils/url';

function DashboardCourseCard({ course, onClick }: { course: CourseListDto, onClick: () => void }) {
  const thumbnailUrl = resolveUrl(course.thumbnailUrl) ?? '/placeholders/course-thumbnail.svg';
  const [imageSrc, setImageSrc] = useState(thumbnailUrl);

  useEffect(() => {
    setImageSrc(thumbnailUrl);
  }, [thumbnailUrl]);

  return (
    <Card 
      onClick={onClick}
      className="cursor-pointer flex flex-col h-full overflow-hidden hover:shadow-lg hover:border-primary/50 transition-all group bg-card"
    >
      <div className="aspect-video bg-muted relative overflow-hidden">
         <img
           src={imageSrc}
           alt={course.title}
           className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
           onError={() => setImageSrc('/placeholders/course-thumbnail.svg')}
         />
         <div className="absolute top-2 right-2 flex gap-2">
           {course.isFree && <Badge variant="outline" className="backdrop-blur-md bg-background/80 text-xs">Free</Badge>}
         </div>
      </div>
      <CardContent className="p-5 flex flex-col flex-1 relative">
        <h3 className="font-bold line-clamp-2 mb-2 group-hover:text-primary transition-colors text-base leading-tight pr-8">{course.title}</h3>
        <p className="text-xs text-muted-foreground mb-3 flex items-center gap-1.5 font-medium">
          <div className="h-5 w-5 rounded-full bg-primary/10 flex items-center justify-center">
            <BookOpen className="h-3 w-3 text-primary" />
          </div>
          {course.teacherName || "Expert Instructor"}
        </p>
        
        <div className="mt-auto flex items-center justify-between pt-4 border-t border-border/40">
           <div className="flex items-center gap-1">
             <Star className="h-4 w-4 text-orange-500 fill-orange-500" />
             <span className="font-semibold text-sm">{course.averageRating > 0 ? course.averageRating.toFixed(1) : "New"}</span>
             <span className="text-xs text-muted-foreground ml-0.5">({course.reviewCount || course.enrollmentCount})</span>
           </div>
           <span className="font-bold text-base text-primary">{course.isFree || course.price === 0 ? '' : `$${course.price}`}</span>
        </div>
      </CardContent>
    </Card>
  );
}

function SectionHeading({ title, icon, onToggleExpand, isExpanded, showToggle }: { title: string, icon?: React.ReactNode, onToggleExpand?: () => void, isExpanded?: boolean, showToggle?: boolean }) {
  return (
    <div className="flex items-center justify-between mt-10 mb-6 pb-2 border-b border-border/30">
      <h2 className="text-xl font-bold flex items-center gap-2.5">
        <div className="p-1.5 rounded-lg bg-primary/10 text-primary">
          {icon}
        </div>
        {title}
      </h2>
      {showToggle && onToggleExpand && (
        <Button variant="ghost" size="sm" onClick={onToggleExpand} className="text-muted-foreground hover:text-primary hover:bg-primary/10">
          {isExpanded ? 'Show less' : 'View all'} <ArrowRight className={`ml-1.5 h-4 w-4 transition-transform ${isExpanded ? '-rotate-90' : 'rotate-90'}`} />
        </Button>
      )}
    </div>
  );
}

function CourseSection({
  title,
  icon,
  courses,
  navigate,
  isLoading,
  emptyMessage
}: {
  title: string,
  icon: React.ReactNode,
  courses: CourseListDto[],
  navigate: (url: string) => void,
  isLoading?: boolean,
  emptyMessage?: string
}) {
  const [isExpanded, setIsExpanded] = useState(false);
  const showToggle = courses && courses.length > 3 && !isLoading;
  const displayedCourses = isExpanded ? courses : courses?.slice(0, 3);

  return (
    <section>
      <SectionHeading 
        title={title} 
        icon={icon} 
        onToggleExpand={() => setIsExpanded(!isExpanded)} 
        isExpanded={isExpanded}
        showToggle={showToggle}
      />
      <CourseRow
        courses={displayedCourses || []}
        navigate={navigate}
        isLoading={isLoading}
        emptyMessage={emptyMessage}
      />
    </section>
  );
}

function CourseRow({
  courses,
  navigate,
  isLoading = false,
  emptyMessage = 'No courses found.'
}: {
  courses: CourseListDto[],
  navigate: (url: string) => void,
  isLoading?: boolean,
  emptyMessage?: string
}) {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="h-72 animate-pulse bg-muted rounded-xl border border-border shadow-sm" />
        ))}
      </div>
    );
  }

  if (!courses || courses.length === 0) {
    return (
      <div className="rounded-2xl border-2 border-dashed border-border/60 bg-card p-10 text-center flex flex-col items-center justify-center">
        <div className="h-12 w-12 rounded-full bg-muted flex items-center justify-center mb-4">
          <BookOpen className="h-6 w-6 text-muted-foreground opacity-50" />
        </div>
        <p className="text-base text-muted-foreground font-medium">{emptyMessage}</p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 2xl:grid-cols-3 gap-6">
      {courses.map((course) => (
         <DashboardCourseCard key={course.courseId} course={course} onClick={() => navigate(`/courses/${course.courseId}`)} />
      ))}
    </div>
  );
}

export default function StudentDashboard() {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const [isTopRatedExpanded, setIsTopRatedExpanded] = useState(false);

  const { data: dashboard } = useQuery({
    queryKey: ['student-dashboard'],
    queryFn: () => usersApi.getStudentDashboard(),
    select: (res) => res.data.data as StudentDashboardType,
  });

  const {
    data: recommendationSections,
    isLoading: isSectionRecommendationsLoading,
    isFetching: isSectionRecommendationsFetching,
  } = useQuery({
    queryKey: ['recommendation-sections', 10],
    queryFn: () => coursesApi.getRecommendationSections(10),
    select: (res) => (res.data.data || null) as RecommendationSectionsDto | null,
    staleTime: 0,
    gcTime: 0,
    refetchOnMount: 'always',
    refetchOnReconnect: 'always',
  });

  const continueLearningData = recommendationSections?.continueLearning || [];

  // Keep discovery/recommendation cards focused on new courses, never already-enrolled ones.
  const enrolledCourseIds = new Set<string>([
    ...(dashboard?.courseProgress?.map((course) => course.courseId) || []),
    ...continueLearningData.map((course) => course.courseId),
    ...((recommendationSections?.topPicksForYou || [])
      .filter((course) => course.isEnrolled)
      .map((course) => course.courseId)),
  ]);

  const recommendedForYou = (recommendationSections?.topPicksForYou || [])
    .filter((course) => !enrolledCourseIds.has(course.courseId));
  const becauseYouLearned = (recommendationSections?.becauseYouLearned || [])
    .filter((course) => !enrolledCourseIds.has(course.courseId));
  const topCourses = (recommendationSections?.topCourses || [])
    .filter((course) => !enrolledCourseIds.has(course.courseId));
  const trendingCourses = (recommendationSections?.trendingCourses || [])
    .filter((course) => !enrolledCourseIds.has(course.courseId));
  const hasLearningHistory =
    (dashboard?.courseProgress?.length || 0) > 0 ||
    continueLearningData.length > 0 ||
    (recommendationSections?.topPicksForYou || []).some((course) => course.isEnrolled);
  const becauseYouLearnedTitle = recommendationSections?.becauseYouLearnedCourseTitle
    ? `Because You Learned ${recommendationSections.becauseYouLearnedCourseTitle}`
    : 'Because You Learned';
  const becauseYouLearnedEmptyMessage = recommendationSections?.becauseYouLearnedCourseTitle || hasLearningHistory
    ? 'No similar courses found yet for this learning path. Try another completed course or check back soon.'
    : 'Complete or enroll in more courses to unlock this section.';
  
  // Exclude Top Pick from rest
  const topPick = recommendedForYou.length > 0 ? recommendedForYou[0] : null;
  const filteredRecommended = recommendedForYou.filter(c => c.courseId !== topPick?.courseId);
  const isRecommendedLoading = isSectionRecommendationsLoading || isSectionRecommendationsFetching;

  const handleContinueCourse = (courseId: string) => {
    const continueData = continueLearningData.find((c) => c.courseId === courseId);
    if (continueData?.lectureId && continueData.lastMaterialId) {
      navigate(`/courses/${courseId}/lectures/${continueData.lectureId}?materialId=${continueData.lastMaterialId}`);
    } else {
      navigate(`/courses/${courseId}/learn`);
    }
  };

  const currentStreak = dashboard?.streak?.currentStreak || 0;
  const activeDays = dashboard?.streak?.activeDays || [false, false, false, false, false, false, false];
  const weekDays = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];

  const resumableCourses = (dashboard?.courseProgress || [])
    .filter((course) => course.totalMaterials > 0)
    .sort((a, b) => new Date(b.enrolledAt).getTime() - new Date(a.enrolledAt).getTime());

  const heroCourse =
    resumableCourses.find((course) => course.progressPercentage > 0 && course.progressPercentage < 100) ||
    resumableCourses.find((course) => course.progressPercentage < 100) ||
    null;

  const totalEnrolled = (dashboard?.courseProgress || []).length;
  const totalCompleted = (dashboard?.courseProgress || []).filter((c) => c.progressPercentage === 100).length;

  return (
    <div className="max-w-screen-2xl mx-auto px-4 sm:px-6 lg:px-8 py-8 min-h-[calc(100vh-4rem)] bg-background">
      
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
        {/* Main Left Column (Content & Carousels) */}
        <div className="lg:col-span-8 space-y-10 min-w-0">
          
          {/* Header & Quick Stats */}
          <div className="space-y-6">
            {/* Dynamic Welcome Banner */}
            <div className="relative overflow-hidden bg-gradient-to-br from-primary/10 via-background to-background p-8 md:p-10 rounded-3xl border border-primary/20 shadow-sm grow-effect">
              {/* Decorative elements */}
              <div className="absolute top-0 right-0 -mr-8 -mt-8 opacity-5 pointer-events-none md:opacity-10 transition-opacity">
                <GraduationCap className="h-48 w-48 text-primary shrink-0" />
              </div>

              <div className="relative z-10 space-y-4">
                <motion.div
                  className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-primary/10 text-primary text-sm font-semibold shadow-sm"
                  initial={{ opacity: 0, scale: 0.9 }}
                  animate={{ opacity: 1, scale: 1 }}
                >
                  <Sparkles className="w-4 h-4 text-primary" />
                  <span>Welcome Back, Student!</span>
                </motion.div>
                
                <motion.h1
                  className="text-4xl md:text-5xl lg:text-6xl font-black tracking-tighter"
                  initial={{ opacity: 0, y: 10 }}
                  animate={{ opacity: 1, y: 0 }}
                >
                  Hey <span className="text-transparent bg-clip-text bg-gradient-to-r from-primary via-blue-500 to-indigo-600 drop-shadow-sm">{user?.firstName || 'Student'}</span>,<br className="hidden md:block" />
                  Ready to learn?
                </motion.h1>
                
                <motion.p 
                  className="text-muted-foreground text-lg md:text-xl max-w-2xl font-medium tracking-tight"
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  transition={{ delay: 0.1 }}
                >
                  Pick up right where you left off or discover new courses to boost your skills and advance your career today.
                </motion.p>
              </div>
            </div>

            <motion.div 
              className="grid grid-cols-3 gap-4"
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.2 }}
            >
              <Card className="bg-card border-none shadow-sm flex flex-col items-center justify-center p-4">
                <div className="h-10 w-10 rounded-full bg-blue-500/10 flex items-center justify-center mb-2">
                  <BookOpen className="h-5 w-5 text-blue-500" />
                </div>
                <p className="text-2xl font-bold text-foreground">{totalEnrolled}</p>
                <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Enrolled</p>
              </Card>
              <Card className="bg-card border-none shadow-sm flex flex-col items-center justify-center p-4">
                <div className="h-10 w-10 rounded-full bg-yellow-500/10 flex items-center justify-center mb-2">
                  <Star className="h-5 w-5 text-yellow-500" />
                </div>
                <p className="text-2xl font-bold text-foreground">{totalCompleted}</p>
                <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Completed</p>
              </Card>
              <Card className="bg-card border-none shadow-sm flex flex-col items-center justify-center p-4">
                <div className="h-10 w-10 rounded-full bg-orange-500/10 flex items-center justify-center mb-2">
                  <Flame className="h-5 w-5 text-orange-500" />
                </div>
                <p className="text-2xl font-bold text-foreground">{currentStreak}</p>
                <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">Day Streak</p>
              </Card>
            </motion.div>
          </div>

          {/* Jump Back In (Active Learning) */}
          {heroCourse && (
            <motion.section 
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3 }}
            >
              <h2 className="text-xl font-bold mb-4 flex items-center gap-2">
                <PlayCircle className="h-5 w-5 text-primary" />
                Resume Learning
              </h2>
              <Card className="overflow-hidden border-border shadow-md bg-card group relative">
                <div className="absolute inset-0 bg-gradient-to-br from-primary/5 via-transparent to-transparent opacity-50 pointer-events-none" />
                
                <div className="flex flex-col md:flex-row relative z-10">
                  <div className="md:w-2/5 bg-muted relative min-h-[200px] flex items-center justify-center overflow-hidden border-r border-border/50">
                    <div className="absolute inset-0 bg-gradient-to-br from-primary/20 to-accent/20 mix-blend-overlay z-10" />
                    <BookOpen className="h-16 w-16 text-primary/30 z-0" />
                    <div className="absolute top-4 left-4 z-20">
                      <Badge className="bg-background/90 backdrop-blur text-foreground border-border">Up Next</Badge>
                    </div>
                  </div>
                  <div className="p-6 flex-1 flex flex-col justify-center">
                    <h3 className="text-2xl font-bold line-clamp-2 leading-tight mb-2 group-hover:text-primary transition-colors">
                      {heroCourse.courseTitle}
                    </h3>
                    
                    <p className="text-muted-foreground mb-6 line-clamp-2 text-sm">
                      {heroCourse.completedMaterials > 0
                        ? `You've completed ${heroCourse.completedMaterials} out of ${heroCourse.totalMaterials} materials. Pick up exactly where you left off.`
                        : `This course has ${heroCourse.totalMaterials} materials ready. Start from the first one.`}
                    </p>
                    
                    <div className="flex items-center gap-4 mb-6">
                      <div className="w-full h-2.5 bg-secondary rounded-full overflow-hidden flex-1">
                        <motion.div
                          className="h-full bg-primary rounded-full relative"
                          initial={{ width: 0 }}
                          animate={{ width: `${heroCourse.progressPercentage}%` }}
                          transition={{ duration: 1, ease: 'easeOut', delay: 0.3 }}
                        >
                          <div className="absolute inset-0 bg-white/20 animate-pulse" />
                        </motion.div>
                      </div>
                      <span className="font-bold text-sm shrink-0">{heroCourse.progressPercentage}%</span>
                    </div>

                    <div className="mt-auto flex flex-wrap gap-3">
                      <Button onClick={() => handleContinueCourse(heroCourse.courseId)} className="w-full sm:w-auto shadow-primary/20 shadow-md">
                        <PlayCircle className="mr-2 h-5 w-5" /> 
                        {heroCourse.completedMaterials > 0 ? 'Continue' : 'Start Course'}
                      </Button>
                      <Button variant="outline" onClick={() => navigate(`/courses/${heroCourse.courseId}`)} className="w-full sm:w-auto">
                        Overview
                      </Button>
                    </div>
                  </div>
                </div>
              </Card>
            </motion.section>
          )}

          {/* Discover Sections */}
          <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: 0.4 }} className="space-y-12 pt-4">
            
            <CourseSection
              title="Recommended Matches"
              icon={<BookOpen className="text-primary h-6 w-6" />}
              courses={filteredRecommended}
              navigate={navigate}
              isLoading={isRecommendedLoading}
              emptyMessage="We are still learning your preferences. Browse more courses to improve recommendations."
            />

            <CourseSection
              title={becauseYouLearnedTitle}
              icon={<Target className="text-indigo-500 h-6 w-6" />}
              courses={becauseYouLearned}
              navigate={navigate}
              isLoading={isRecommendedLoading}
              emptyMessage={becauseYouLearnedEmptyMessage}
            />

            <CourseSection
              title="Trending Now"
              icon={<TrendingUp className="text-blue-500 h-6 w-6" />}
              courses={trendingCourses}
              navigate={navigate}
              isLoading={isRecommendedLoading}
            />
          </motion.div>
        </div>

        {/* Right Sidebar Column (Gamification & Top Picks) */}
        <div className="lg:col-span-4 space-y-8">
          
          {/* Gamification / Streak Widget Redesign */}
          <motion.div 
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.3 }}
          >
            <Card className="bg-card border border-border shadow-sm overflow-hidden">
              <div className="bg-gradient-to-r from-orange-500/10 to-transparent p-6 border-b border-border/50 flex items-center justify-between">
                <div>
                  <h3 className="font-bold text-lg text-foreground flex items-center gap-2">
                    <Flame className="h-5 w-5 text-orange-500" />
                    Learning Streak
                  </h3>
                  <p className="text-sm text-muted-foreground mt-1">Keep learning every day!</p>
                </div>
                <div className="text-3xl font-black text-foreground">{currentStreak}</div>
              </div>
              <CardContent className="p-6">
                <div className="flex flex-wrap justify-between gap-2">
                  {weekDays.map((day, idx) => (
                    <div key={idx} className="flex flex-col items-center gap-2">
                      <div className={`h-10 w-10 sm:h-12 sm:w-12 rounded-full flex items-center justify-center text-sm font-bold transition-all ${
                        activeDays[idx] ? 'bg-orange-500 text-white shadow-md ring-4 ring-orange-500/20' : 'bg-muted text-muted-foreground border border-border'
                      }`}>
                        {activeDays[idx] ? <Flame className="h-4 w-4 sm:h-5 sm:w-5" /> : <div className="h-2 w-2 rounded-full bg-border/50" />}
                      </div>
                      <span className="text-xs font-semibold text-muted-foreground">{day}</span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </motion.div>

          {/* Top Pick Sidebar Card */}
          {topPick && !isRecommendedLoading && (
            <motion.section
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.5 }}
            >
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-xl font-bold flex items-center gap-2">
                  <Star className="text-yellow-500 fill-yellow-500 h-5 w-5" />
                  Top Pick
                </h2>
              </div>
              <Card 
                className="overflow-hidden border border-border bg-card shadow-sm hover:shadow-md transition-all cursor-pointer group" 
                onClick={() => navigate(`/courses/${topPick.courseId}`)}
              >
                <div className="relative aspect-video overflow-hidden">
                  <img
                    src={resolveUrl(topPick.thumbnailUrl) ?? '/placeholders/course-thumbnail.svg'}
                    alt={topPick.title}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700"
                    onError={(event) => {
                      event.currentTarget.src = '/placeholders/course-thumbnail.svg';
                    }}
                  />
                  <div className="absolute top-3 left-3">
                    <Badge className="bg-yellow-500 hover:bg-yellow-600 text-white border-none shadow-sm">
                      Highest Match
                    </Badge>
                  </div>
                </div>
                
                <CardContent className="p-5 flex flex-col gap-3">
                  <h3 className="text-lg font-bold line-clamp-2 group-hover:text-primary transition-colors leading-snug">
                    {topPick.title}
                  </h3>
                  <p className="text-muted-foreground text-sm line-clamp-2">
                    {topPick.description || "Selected just for you to boost your skills."}
                  </p>
                  
                  <div className="mt-2 pt-4 border-t border-border/50 flex items-center justify-between text-sm">
                    <div className="flex items-center gap-1 font-semibold">
                      <span className="text-orange-500">{topPick.averageRating.toFixed(1)}</span>
                      <Star className="h-3.5 w-3.5 fill-orange-500 text-orange-500" />
                    </div>
                    <span className="font-bold">{topPick.isFree || topPick.price === 0 ? 'Free' : `$${topPick.price}`}</span>
                  </div>
                </CardContent>
              </Card>
            </motion.section>
          )}

          {/* Top Courses List */}
          <motion.section
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.6 }}
          >
             <div className="flex items-center justify-between mb-4">
                <h2 className="text-xl font-bold flex items-center gap-2">
                  <Activity className="text-green-500 h-5 w-5" />
                  Top Rated
                </h2>
                {topCourses.length > 4 && !isRecommendedLoading && (
                  <Button variant="ghost" size="sm" onClick={() => setIsTopRatedExpanded(!isTopRatedExpanded)} className="h-8">
                    {isTopRatedExpanded ? 'Show Less' : 'View All'}
                  </Button>
                )}
              </div>
              <Card className="p-2 border-border shadow-sm bg-card">
                <div className="flex flex-col gap-1 max-h-[400px] overflow-y-auto hide-scrollbar">
                  {!isRecommendedLoading && (isTopRatedExpanded ? topCourses : topCourses.slice(0, 4)).map(course => (
                    <div 
                      key={course.courseId} 
                      onClick={() => navigate(`/courses/${course.courseId}`)}
                      className="p-3 rounded-lg hover:bg-muted/50 cursor-pointer transition-colors flex items-center gap-3"
                    >
                      <div className="h-12 w-16 shrink-0 rounded bg-muted overflow-hidden">
                        <img 
                          src={resolveUrl(course.thumbnailUrl) ?? '/placeholders/course-thumbnail.svg'} 
                          className="w-full h-full object-cover"
                          alt={course.title}
                          onError={(e) => e.currentTarget.src = '/placeholders/course-thumbnail.svg'}
                        />
                      </div>
                      <div className="flex-1 min-w-0">
                        <h4 className="font-semibold text-sm truncate">{course.title}</h4>
                        <p className="text-xs text-muted-foreground truncate">{course.teacherName}</p>
                      </div>
                    </div>
                  ))}
                  {isRecommendedLoading && Array.from({ length: 3 }).map((_, i) => (
                    <div key={i} className="h-16 rounded-lg bg-muted animate-pulse m-2" />
                  ))}
                </div>
              </Card>
          </motion.section>

        </div>
      </div>
    </div>
  );
}

