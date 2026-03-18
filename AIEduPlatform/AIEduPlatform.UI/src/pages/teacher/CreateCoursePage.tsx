import { useForm } from 'react-hook-form';
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

interface CreateCourseForm {
  title: string;
  description: string;
  price: number;
}

export default function CreateCoursePage() {
  const navigate = useNavigate();
  const fileRef = useRef<HTMLInputElement>(null);
  const [selectedCategories, setSelectedCategories] = useState<string[]>([]);
  const { register, handleSubmit, formState: { errors } } = useForm<CreateCourseForm>();

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
    onSuccess: (res) => {
      toast.success('Course created!');
      const courseId = res.data.data?.courseId;
      navigate(`/teacher/courses/${courseId}`);
    },
    onError: () => toast.error('Failed to create course'),
  });

  const toggleCategory = (catId: string) => {
    setSelectedCategories((prev) =>
      prev.includes(catId) ? prev.filter((id) => id !== catId) : [...prev, catId]
    );
  };

  return (
    <AnimatedPage>
    <div className="max-w-2xl mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold mb-8">Create Course</h1>
      <Card>
        <CardContent className="p-6">
          <form
            onSubmit={handleSubmit((data) => createMutation.mutate(data))}
            className="space-y-5"
          >
            {/* Basic Information */}
            <div className="space-y-4">
              <h2 className="text-lg font-semibold border-b border-border pb-2">Basic Information</h2>
              <Input
                label="Title"
                placeholder="e.g. Introduction to Machine Learning"
                {...register('title', { required: 'Title is required' })}
                error={errors.title?.message}
              />
              <Textarea
                label="Description"
                placeholder="Provide a detailed description of what students will learn..."
                hint="A good description helps students understand what to expect from the course."
                {...register('description', { required: 'Description is required' })}
                error={errors.description?.message}
              />
              <Input
                label="Price"
                type="number"
                step="0.01"
                placeholder="0.00"
                hint="Set to 0 for a free course."
                {...register('price', { valueAsNumber: true })}
              />
            </div>

            {/* Thumbnail */}
            <div className="space-y-4">
              <h2 className="text-lg font-semibold border-b border-border pb-2">Thumbnail</h2>
              <FileInput
                ref={fileRef}
                label="Course Thumbnail (optional)"
                accept="image/*"
                hint="Recommended size: 1280x720px. Supports JPG, PNG, and WebP."
              />
            </div>

            {/* Categories */}
            {categories && categories.length > 0 && (
              <div className="space-y-4">
                <h2 className="text-lg font-semibold border-b border-border pb-2">Categories</h2>
                <div>
                  <p className="text-sm text-muted-foreground mb-3">Select one or more categories that best describe your course.</p>
                  <div className="flex flex-wrap gap-2">
                    {categories.map((cat: any) => (
                      <button
                        key={cat.id}
                        type="button"
                        onClick={() => toggleCategory(cat.id)}
                        className={`px-3 py-1.5 rounded-full text-sm border transition-all duration-200 ${
                          selectedCategories.includes(cat.id)
                            ? 'bg-primary text-primary-foreground border-primary'
                            : 'bg-card border-border hover:border-primary/50'
                        }`}
                      >
                        {cat.name}
                      </button>
                    ))}
                  </div>
                </div>
              </div>
            )}

            {/* Actions */}
            <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
              <Button type="submit" loading={createMutation.isPending}>Create Course</Button>
              <Button variant="outline" type="button" onClick={() => navigate(-1)}>Cancel</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
    </AnimatedPage>
  );
}
