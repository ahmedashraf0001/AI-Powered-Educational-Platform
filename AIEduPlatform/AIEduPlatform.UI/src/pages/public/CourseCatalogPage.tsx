import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { coursesApi } from '@/api/courses.api';
import { categoriesApi } from '@/api/categories.api';
import { CourseCard } from '@/components/courses/CourseCard';
import { Input } from '@/components/ui/Input';
import { Button } from '@/components/ui/Button';
import { Pagination } from '@/components/ui/Pagination';
import { CourseCardSkeleton } from '@/components/ui/Skeleton';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { EmptyState } from '@/components/ui/Feedback';
import { useDebounce } from '@/hooks/useDebounce';
import { BookOpen, Search, SlidersHorizontal, X } from 'lucide-react';
import { motion } from 'framer-motion';
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

  const updateParams = (updates: Record<string, string>) => {
    const params = new URLSearchParams(searchParams);
    for (const [key, val] of Object.entries(updates)) {
      if (val) params.set(key, val);
      else params.delete(key);
    }
    params.set('Page', '1');
    setSearchParams(params);
  };

  const handleSearch = (value: string) => {
    setKeyword(value);
    updateParams({ Keyword: value });
  };

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
      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-2">Course Catalog</h1>
          <p className="text-muted-foreground">Explore courses and start learning</p>
        </div>

        {/* Search + Filter Toggle */}
        <div className="flex items-center gap-3 mb-4">
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search courses..."
              value={keyword}
              onChange={(e) => handleSearch(e.target.value)}
              className="pl-10"
            />
          </div>
          <Button
            variant={showFilters ? 'primary' : 'outline'}
            size="sm"
            onClick={() => setShowFilters(!showFilters)}
          >
            <SlidersHorizontal className="h-4 w-4 mr-2" /> Filters
          </Button>
          {hasActiveFilters && (
            <Button variant="ghost" size="sm" onClick={clearFilters}>
              <X className="h-4 w-4 mr-1" /> Clear
            </Button>
          )}
        </div>

        {/* Filter Bar */}
        {showFilters && (
          <motion.div
            className="flex flex-wrap items-center gap-3 mb-6 p-4 rounded-lg border bg-card"
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
          >
            <div className="space-y-1">
              <label className="text-xs font-medium text-muted-foreground">Category</label>
              <select
                value={categoryId}
                onChange={(e) => handleCategoryChange(e.target.value)}
                className="flex h-9 rounded-md border border-input bg-card px-3 py-1 text-sm shadow-sm transition-colors hover:border-primary/50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30"
              >
                <option value="">All Categories</option>
                {categories?.map((cat) => (
                  <option key={cat.id} value={cat.id}>
                    {cat.name} ({cat.courseCount})
                  </option>
                ))}
              </select>
            </div>
            <div className="space-y-1">
              <label className="text-xs font-medium text-muted-foreground">Price</label>
              <select
                value={priceFilter}
                onChange={(e) => handlePriceChange(e.target.value)}
                className="flex h-9 rounded-md border border-input bg-card px-3 py-1 text-sm shadow-sm transition-colors hover:border-primary/50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/30"
              >
                {priceFilters.map((pf) => (
                  <option key={pf.value} value={pf.value}>
                    {pf.label}
                  </option>
                ))}
              </select>
            </div>
          </motion.div>
        )}

        {isLoading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
            {Array.from({ length: 8 }).map((_, i) => (
              <CourseCardSkeleton key={i} />
            ))}
          </div>
        ) : !data || data.items.length === 0 ? (
          <EmptyState
            icon={<BookOpen className="h-12 w-12" />}
            title="No courses found"
            description={hasActiveFilters ? 'Try adjusting your filters' : 'No courses available yet'}
          />
        ) : (
          <>
            <motion.div
              className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6"
              variants={staggerContainer}
              initial="hidden"
              animate="visible"
            >
              {data.items.map((course: any) => (
                <motion.div key={course.courseId} variants={fadeInUp}>
                  <CourseCard course={course} />
                </motion.div>
              ))}
            </motion.div>
            <div className="mt-8">
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
    </AnimatedPage>
  );
}
