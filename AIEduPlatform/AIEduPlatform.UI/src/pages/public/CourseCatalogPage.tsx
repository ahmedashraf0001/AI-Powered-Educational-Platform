import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { coursesApi } from '@/api/courses.api';
import { categoriesApi } from '@/api/categories.api';
import { CourseCard } from '@/components/courses/CourseCard';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Pagination } from '@/components/ui/Pagination';
import { CourseCardSkeleton } from '@/components/ui/Skeleton';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { EmptyState } from '@/components/ui/Feedback';
import { useDebounce } from '@/hooks/useDebounce';
import { BookOpen, Search, SlidersHorizontal, X, Filter, Check, LayoutGrid } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { staggerContainer, fadeInUp } from '@/utils/motion';

const priceFilters = [
  { value: '', label: 'All Prices' },
  { value: 'free', label: 'Free' },
  { value: 'paid', label: 'Paid' },
];

export default function CourseCatalogPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const page = parseInt(searchParams.get('Page') || '1', 10);
  const [keyword, setKeyword] = useState(searchParams.get('Keyword') || '');
  const debouncedKeyword = useDebounce(keyword, 300);
  const [categoryId, setCategoryId] = useState(searchParams.get('CategoryId') || '');
  const [priceFilter, setPriceFilter] = useState(searchParams.get('Price') || '');
  const [showFilters, setShowFilters] = useState(!!(categoryId || priceFilter));

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: () => categoriesApi.getAll(),
    select: (res) => res.data.data ?? [],
    staleTime: 60_000,
  });

  const { data, isLoading } = useQuery({
    queryKey: ['courses', debouncedKeyword, page, categoryId, priceFilter],
    queryFn: () => {
      const params: any = { page, pageSize: 12 };
      if (categoryId) params.CategoryId = categoryId;
      return debouncedKeyword
        ? coursesApi.search(debouncedKeyword, params)
        : coursesApi.getAll(params);
    },
    select: (res) => {
      const result = res.data.data;
      if (!result) return result;
      if (!priceFilter) return result;
      const filtered = result.items.filter((c: any) =>
        priceFilter === 'free' ? c.isFree : !c.isFree
      );
      return { ...result, items: filtered };
    },
  });

  const updateParams = (updates: Record<string, string>, options?: { replace?: boolean }) => {
    const params = new URLSearchParams(searchParams);
    for (const [key, val] of Object.entries(updates)) {
      if (val) params.set(key, val);
      else params.delete(key);
    }
    params.set('Page', '1');
    setSearchParams(params, options);
  };

  const handleSearch = (value: string) => {
    setKeyword(value);
  };

  // Sync debounced keyword to the URL
  useEffect(() => {
    const currentParam = searchParams.get('Keyword') || '';
    if (debouncedKeyword !== currentParam) {
      updateParams({ Keyword: debouncedKeyword }, { replace: true });
    }
  }, [debouncedKeyword]);

  const handleCategoryChange = (value: string) => {
    setCategoryId(value);
    updateParams({ CategoryId: value });
  };

  const handlePriceChange = (value: string) => {
    setPriceFilter(value);
    updateParams({ Price: value });
  };

  const handlePageChange = (newPage: number) => {
    const params = new URLSearchParams(searchParams);
    params.set('Page', String(newPage));
    setSearchParams(params);
  };

  const clearFilters = () => {
    setCategoryId('');
    setPriceFilter('');
    setKeyword('');
    setSearchParams(new URLSearchParams());
  };

  const hasActiveFilters = !!(categoryId || priceFilter || keyword);

  return (
    <AnimatedPage>
      {/* Hero Banner Area */}
      <div className="bg-gradient-to-r from-primary/10 via-primary/5 to-background border-b border-border">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 md:py-16">
          <div className="max-w-3xl">
            <h1 className="text-4xl md:text-5xl font-extrabold tracking-tight text-foreground mb-4">
              Discover Your Next Skill
            </h1>
            <p className="text-lg text-muted-foreground mb-8 max-w-2xl leading-relaxed">
              Explore our extensive catalog of AI-enhanced courses. Filter by category, pricing, and find the perfect curriculum to accelerate your journey.
            </p>
            
            {/* Prominent Search */}
            <div className="relative max-w-xl group">
              <div className="absolute -inset-0.5 bg-gradient-to-r from-primary to-info rounded-xl blur opacity-25 group-focus-within:opacity-50 transition duration-500"></div>
              <div className="relative flex items-center bg-card rounded-xl border border-border shadow-sm w-full">
                <Search className="absolute left-4 h-5 w-5 text-muted-foreground pointer-events-none" />
                <input
                  type="text"
                  placeholder="Search over hundreds of courses..."
                  value={keyword}
                  onChange={(e) => handleSearch(e.target.value)}
                  className="pl-12 pr-12 h-14 border-0 bg-transparent shadow-none text-base rounded-xl focus:ring-0 focus:outline-none w-full text-foreground placeholder:text-muted-foreground"
                />
                {keyword && (
                  <button onClick={() => handleSearch('')} className="absolute right-4 text-muted-foreground hover:text-foreground">
                    <X className="h-4 w-4" />
                  </button>
                )}
              </div>
            </div>
            
            {/* Quick popular tags (Mocked via first 4 categories) */}
            {categories && categories.length > 0 && (
              <div className="flex flex-wrap items-center gap-2 mt-6">
                <span className="text-sm font-medium text-muted-foreground mr-2">Popular:</span>
                {categories.slice(0, 4).map((cat) => (
                  <button key={cat.id} onClick={() => handleCategoryChange(categoryId === cat.id ? '' : cat.id)}>
                    <Badge
                      variant={categoryId === cat.id ? "default" : "outline"}
                      className="cursor-pointer hover:bg-primary/20 transition-colors"
                    >
                      {cat.name}
                    </Badge>
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">
        
        {/* Mobile Filter Toggle */}
        <div className="flex md:hidden items-center justify-between mb-6">
          <h2 className="text-xl font-bold flex items-center gap-2">
            <LayoutGrid className="h-5 w-5 text-primary" /> All Courses
          </h2>
          <Button
            variant={showFilters ? 'primary' : 'outline'}
            size="sm"
            onClick={() => setShowFilters(!showFilters)}
            className="flex items-center gap-2"
          >
            <Filter className="h-4 w-4" /> {showFilters ? 'Hide Filters' : 'Show Filters'}
          </Button>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-12 gap-8 items-start">
          
          {/* Left Sidebar: Filters */}
          <AnimatePresence>
            {(showFilters || typeof window !== 'undefined' && window.innerWidth >= 768) && (
              <motion.aside
                initial={{ opacity: 0, x: -20 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -20 }}
                className="md:col-span-3 space-y-6 md:sticky md:top-24"
              >
                <div className="flex items-center justify-between md:mb-2">
                  <h3 className="font-semibold text-lg flex items-center gap-2">
                    <SlidersHorizontal className="h-4 w-4" /> Filters
                  </h3>
                  {hasActiveFilters && (
                    <button 
                      onClick={clearFilters}
                      className="text-xs text-primary hover:underline font-medium"
                    >
                      Clear all
                    </button>
                  )}
                </div>

                <Card className="border-border shadow-sm overflow-hidden">
                  <div className="p-4 bg-muted/30 border-b border-border/50">
                    <h4 className="font-medium text-sm">Categories</h4>
                  </div>
                  <CardContent className="p-4">
                    <div className="space-y-2.5">
                      <button
                        onClick={() => handleCategoryChange('')}
                        className={`w-full flex items-center justify-between text-sm transition-colors decoration-transparent ${categoryId === '' ? 'text-primary font-semibold' : 'text-muted-foreground hover:text-foreground'}`}
                      >
                        <span>All Categories</span>
                        {categoryId === '' && <Check className="h-4 w-4" />}
                      </button>
                      {categories?.map((cat) => (
                        <button
                          key={cat.id}
                          onClick={() => handleCategoryChange(cat.id)}
                          className={`w-full flex items-center justify-between text-sm transition-colors ${categoryId === cat.id ? 'text-primary font-medium' : 'text-muted-foreground hover:text-foreground'}`}
                        >
                          <span className="truncate pr-4 text-left">{cat.name}</span>
                          <span className="shrink-0 flex items-center gap-2">
                            <span className="text-xs bg-secondary px-1.5 py-0.5 rounded-md">{cat.courseCount}</span>
                            {categoryId === cat.id && <Check className="h-4 w-4 text-primary" />}
                          </span>
                        </button>
                      ))}
                    </div>
                  </CardContent>
                </Card>

                <Card className="border-border shadow-sm overflow-hidden">
                  <div className="p-4 bg-muted/30 border-b border-border/50">
                    <h4 className="font-medium text-sm">Pricing</h4>
                  </div>
                  <CardContent className="p-4">
                    <div className="space-y-2.5">
                      {priceFilters.map((pf) => (
                         <button
                           key={pf.value}
                           onClick={() => handlePriceChange(pf.value)}
                           className={`w-full flex items-center justify-between text-sm transition-colors ${priceFilter === pf.value ? 'text-primary font-medium' : 'text-muted-foreground hover:text-foreground'}`}
                         >
                           <span>{pf.label}</span>
                           {priceFilter === pf.value && <Check className="h-4 w-4 text-primary" />}
                         </button>
                      ))}
                    </div>
                  </CardContent>
                </Card>

              </motion.aside>
            )}
          </AnimatePresence>

          {/* Right Main Column: Results */}
          <div className="md:col-span-9">
            
            {/* Desktop header for results grid */}
            <div className="hidden md:flex justify-between items-end mb-6">
               <h2 className="text-2xl font-bold flex items-center gap-2">
                 <LayoutGrid className="h-6 w-6 text-primary" /> 
                 {keyword ? `Results for "${keyword}"` : 'All Courses'}
               </h2>
               <div className="text-sm text-muted-foreground">
                 {data?.totalCount ? `${data.totalCount} courses found` : ''}
               </div>
            </div>

            {isLoading ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6">
                {Array.from({ length: 6 }).map((_, i) => (
                  <CourseCardSkeleton key={i} />
                ))}
              </div>
            ) : !data || data.items.length === 0 ? (
              <Card className="border-dashed bg-transparent shadow-none mt-4">
                 <CardContent className="p-12">
                   <EmptyState
                     icon={<BookOpen className="h-12 w-12 text-muted-foreground/50" />}
                     title="No courses found"
                     description={hasActiveFilters ? 'Try removing some filters to see more results.' : 'No courses available right now.'}
                     action={hasActiveFilters ? <Button variant="outline" onClick={clearFilters} className="mt-4">Reset Filters</Button> : undefined}
                   />
                 </CardContent>
              </Card>
            ) : (
              <>
                <motion.div
                  className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-6"
                  variants={staggerContainer}
                  initial="hidden"
                  animate="visible"
                >
                  {data.items.map((course: any) => (
                    <motion.div key={course.courseId} variants={fadeInUp} className="h-full">
                      <CourseCard course={course} />
                    </motion.div>
                  ))}
                </motion.div>
                
                <div className="mt-12 flex justify-center">
                  <Pagination
                    page={data.page}
                    totalPages={data.totalPages}
                    onPageChange={handlePageChange}
                    hasPrevious={data.hasPrevious}
                    hasNext={data.hasNext}
                  />
                </div>
              </>
            )}
          </div>
          
        </div>
      </div>
    </AnimatedPage>
  );
}
