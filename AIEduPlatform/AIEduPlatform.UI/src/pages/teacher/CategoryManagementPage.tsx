import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { categoriesApi } from '@/api/categories.api';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Modal } from '@/components/ui/Modal';
import { PageSpinner } from '@/components/ui/Spinner';
import { EmptyState } from '@/components/ui/Feedback';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { useForm } from 'react-hook-form';
import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { Plus, Trash2, Pencil, FolderOpen } from 'lucide-react';

interface CategoryFormData {
  name: string;
  description: string;
}

export default function CategoryManagementPage() {
  const queryClient = useQueryClient();
  const [showAdd, setShowAdd] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);

  const { data: categories, isLoading } = useQuery({
    queryKey: ['categories'],
    queryFn: () => categoriesApi.getAll(),
    select: (res) => res.data.data,
  });

  const createForm = useForm<CategoryFormData>();
  const editForm = useForm<CategoryFormData>();

  // Populate edit form when a category is selected
  const editingCategory = categories?.find((c: any) => c.id === editId);
  useEffect(() => {
    if (editingCategory) {
      editForm.reset({
        name: editingCategory.name,
        description: editingCategory.description || '',
      });
    }
  }, [editId, editingCategory]);

  const createMutation = useMutation({
    mutationFn: (data: CategoryFormData) => categoriesApi.create(data),
    onSuccess: () => {
      toast.success('Category created');
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      setShowAdd(false);
      createForm.reset();
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const updateMutation = useMutation({
    mutationFn: (data: CategoryFormData) => categoriesApi.update(editId!, data),
    onSuccess: () => {
      toast.success('Category updated');
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      setEditId(null);
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => categoriesApi.delete(id),
    onSuccess: () => {
      toast.success('Category deleted');
      queryClient.invalidateQueries({ queryKey: ['categories'] });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? ''),
  });

  if (isLoading) return <PageSpinner />;

  return (
    <AnimatedPage>
    <div className="max-w-3xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold">Categories</h1>
        <Button onClick={() => setShowAdd(true)}>
          <Plus className="h-4 w-4 mr-2" /> Add Category
        </Button>
      </div>

      {(!categories || categories.length === 0) ? (
        <EmptyState
          icon={<FolderOpen className="h-12 w-12" />}
          title="No categories yet"
          description="Create your first category to organize courses."
        />
      ) : (
        <div className="space-y-3">
          {categories.map((cat: any) => (
            <Card key={cat.id}>
              <CardContent className="p-4 flex items-center justify-between">
                <div>
                  <h3 className="font-semibold">{cat.name}</h3>
                  {cat.description && (
                    <p className="text-sm text-muted-foreground">{cat.description}</p>
                  )}
                  <p className="text-xs text-muted-foreground mt-1">{cat.courseCount ?? 0} courses</p>
                </div>
                <div className="flex gap-1">
                  <Button variant="ghost" size="sm" onClick={() => setEditId(cat.id)} title="Edit">
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => deleteMutation.mutate(cat.id)}
                    title="Delete"
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Add Category Modal */}
      <Modal open={showAdd} onClose={() => setShowAdd(false)} title="Add Category">
        <form onSubmit={createForm.handleSubmit((d) => createMutation.mutate(d))} className="space-y-5">
          <Input
            label="Name"
            placeholder="e.g. Data Science, Web Development"
            {...createForm.register('name', { required: true })}
          />
          <Textarea
            label="Description"
            placeholder="Briefly describe what this category covers..."
            hint="A short description helps instructors choose the right category for their courses."
            {...createForm.register('description')}
          />
          <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
            <Button type="submit" loading={createMutation.isPending}>Create Category</Button>
            <Button variant="outline" type="button" onClick={() => setShowAdd(false)}>Cancel</Button>
          </div>
        </form>
      </Modal>

      {/* Edit Category Modal */}
      <Modal open={!!editId} onClose={() => setEditId(null)} title="Edit Category">
        <form onSubmit={editForm.handleSubmit((d) => updateMutation.mutate(d))} className="space-y-5">
          <Input
            label="Name"
            placeholder="Enter the category name"
            {...editForm.register('name', { required: true })}
          />
          <Textarea
            label="Description"
            placeholder="Briefly describe what this category covers..."
            hint="A short description helps instructors choose the right category for their courses."
            {...editForm.register('description')}
          />
          <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
            <Button type="submit" loading={updateMutation.isPending}>Save Changes</Button>
            <Button variant="outline" type="button" onClick={() => setEditId(null)}>Cancel</Button>
          </div>
        </form>
      </Modal>
    </div>
    </AnimatedPage>
  );
}

