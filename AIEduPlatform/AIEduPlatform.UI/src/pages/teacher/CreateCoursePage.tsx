import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery } from '@tanstack/react-query';
import { coursesApi } from '@/api/courses.api';
import { categoriesApi } from '@/api/categories.api';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import { FileInput } from '@/components/ui/FileInput';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { useRef, useState } from 'react';
import { z } from 'zod';
import { showNotification } from '@/utils/notifications';
import { BookOpen, Tag, Image as ImageIcon, ArrowLeft, CheckCircle2 } from 'lucide-react';

const createCourseSchema = z.object({
  title: z.string().min(1, 'Title is required'),
  description: z.string().min(1, 'Description is required'),
  price: z.number().min(0, 'Price cannot be negative'),
});

type CreateCourseForm = z.infer<typeof createCourseSchema>;

export default function CreateCoursePage() {
  const navigate = useNavigate();
  const fileRef = useRef<HTMLInputElement>(null);
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  const { register, handleSubmit, formState: { errors, isValid } } = useForm<CreateCourseForm>({
    resolver: zodResolver(createCourseSchema),
    mode: 'onChange',
    defaultValues: {
      title: '',
      description: '',
      price: 0,
    },
  });

  const { data: categories } = useQuery({
    queryKey: ['categories'],
    queryFn: () => categoriesApi.getAll(),
    select: (res) => res.data.data ?? [],
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateCourseForm) => {
      const fd = new FormData();
      fd.append('Title', data.title);
      fd.append('Description', data.description);
      fd.append('Price', String(data.price));
      const file = fileRef.current?.files?.[0];
      if (file) fd.append('Thumbnail', file);
      selectedCategories.forEach((id) => fd.append('CategoryIds', id));
      return coursesApi.create(fd);
    },
    onSuccess: (res, variables) => {
      showNotification({ type: 'success', message: 'Course created!' });

      const payload = res.data.data as any;
      if (payload?.tagExtractionStatus === 'failed') {
        showNotification({
          type: 'warning',
          message: `Tag extraction failed for "${variables.title}". This may reduce course discoverability and engagement.`,
          persistent: true,
        });
      }

      if (payload?.indexingStatus === 'failed') {
        showNotification({
          type: 'warning',
          message: `Indexing failed for "${variables.title}". Students cannot use this content in AI study sessions.`,
          persistent: true,
        });
      }

      const courseId = res.data.data?.courseId;
      navigate(`/teacher/courses/${courseId}`);
    },
    onError: (error: any) => toast.error(error?.userMessage ?? 'Failed to create course'),
  });

  const toggleCategory = (catId: string) => {
    setSelectedCategories((prev) =>
      prev.includes(catId) ? prev.filter((id) => id !== catId) : [...prev, catId]
    );
  };

  return (
    <AnimatedPage>
      <div className="max-w-6xl mx-auto px-4 py-8 space-y-8">
        {/* Header */}
        <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4 bg-primary/5 p-6 rounded-2xl border border-primary/10">
          <div className="flex gap-4 items-center">
            <div className="h-12 w-12 rounded-xl bg-primary/20 flex items-center justify-center text-primary shrink-0">
              <BookOpen className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-3xl font-bold tracking-tight">Create Course</h1>
              <p className="text-muted-foreground mt-1">Design your course structure and details here to get started.</p>
            </div>
          </div>
          <Button variant="outline" className="shrink-0" onClick={() => navigate(-1)}>
            <ArrowLeft className="h-4 w-4 mr-2" /> Back
          </Button>
        </div>

        <form onSubmit={handleSubmit((data) => createMutation.mutate(data))} className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          
          <div className="lg:col-span-2 space-y-6">
            <Card variant="glass" className="border-border/50">
              <CardContent className="p-6 space-y-5">
                <div className="flex items-center gap-2 border-b border-border pb-3 mb-4">
                  <BookOpen className="h-5 w-5 text-primary" />
                  <h2 className="text-xl font-semibold">Basic Information</h2>
                </div>
                
                <div className="space-y-4">
                  <Input
                    label="Course Title"
                    placeholder="e.g. Introduction to Machine Learning"
                    {...register('title', { required: 'Title is required' })}
                    error={errors.title?.message}
                    className="bg-background/50"
                  />
                  <Textarea
                    label="Description"
                    placeholder="Provide a detailed description of what students will learn..."
                    hint="A good description helps students understand what to expect from the course."
                    rows={6}
                    {...register('description', { required: 'Description is required' })}
                    error={errors.description?.message}
                    className="bg-background/50"
                  />
                  <Input
                    label="Price ($)"
                    type="number"
                    step="0.01"
                    placeholder="0.00"
                    hint="Set to 0 for a free course."
                    {...register('price', { valueAsNumber: true })}
                    error={errors.price?.message}
                    className="bg-background/50"
                  />
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="space-y-6">
            {/* Thumbnail Card */}
            <Card variant="glass" className="border-border/50">
              <CardContent className="p-6 space-y-5">
                <div className="flex items-center gap-2 border-b border-border pb-3 mb-4">
                  <ImageIcon className="h-5 w-5 text-primary" />
                  <h2 className="text-xl font-semibold">Thumbnail</h2>
                </div>
                <div className="bg-background/50 p-8 rounded-xl border-2 border-dashed border-border flex flex-col items-center justify-center text-center space-y-4">
                  <div className="h-12 w-12 bg-primary/10 rounded-full flex items-center justify-center text-primary">
                    <ImageIcon className="h-6 w-6" />
                  </div>
                  <div className="w-full">
                    <FileInput
                      ref={fileRef}
                      label=""
                      accept="image/*"
                      hint="1280x720px. JPG, PNG, WebP."
                    />
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Categories Card */}
            {categories && categories.length > 0 && (
              <Card variant="glass" className="border-border/50">
                <CardContent className="p-6 space-y-5">
                  <div className="flex items-center gap-2 border-b border-border pb-3 mb-4">
                    <Tag className="h-5 w-5 text-primary" />
                    <h2 className="text-xl font-semibold">Categories</h2>
                  </div>
                  <p className="text-sm text-muted-foreground mb-4">Select the best tags for your course visibility.</p>
                  <div className="flex flex-wrap gap-2.5">
                    {categories.map((cat: any) => {
                      const isSelected = selectedCategories.includes(cat.id);
                      return (
                        <button
                          key={cat.id}
                          type="button"
                          onClick={() => toggleCategory(cat.id)}
                          className={`px-3.5 py-1.5 rounded-lg text-sm border font-medium flex items-center transition-all duration-200 ${
                            isSelected
                              ? 'bg-primary text-primary-foreground border-primary shadow-sm hover:bg-primary/90'
                              : 'bg-background hover:bg-secondary/10 border-border hover:border-secondary'
                          }`}
                        >
                          {cat.name}
                        </button>
                      );
                    })}
                  </div>
                </CardContent>
              </Card>
            )}
            
            {/* Actions */}
            <Card variant="glass" className="bg-primary/5 border-primary/20">
              <CardContent className="p-6 space-y-3">
                <Button 
                  size="lg" 
                  className="w-full shadow-sm" 
                  type="submit" 
                  loading={createMutation.isPending} 
                  disabled={!isValid || createMutation.isPending}
                >
                  <CheckCircle2 className="h-5 w-5 mr-2" /> Publish Course
                </Button>
                <div className="text-xs text-center text-muted-foreground mt-2">
                  You can edit course details at any time before making it public.
                </div>
              </CardContent>
            </Card>

          </div>
        </form>
      </div>
    </AnimatedPage>
  );
}
