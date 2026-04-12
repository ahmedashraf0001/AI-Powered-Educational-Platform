import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { coursesApi } from '@/api/courses.api';
import { lecturesApi } from '@/api/lectures.api';
import { materialsApi } from '@/api/materials.api';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { PageSpinner } from '@/components/ui/Spinner';
import { Modal } from '@/components/ui/Modal';
import { FileInput } from '@/components/ui/FileInput';
import { AnimatedPage } from '@/components/ui/AnimatedPage';
import { toast } from 'sonner';
import { useState, useRef, useEffect, useCallback } from 'react';
import { Plus, Trash2, Upload, Pencil, BarChart3, GripVertical } from 'lucide-react';

type LectureFormValues = {
  title: string;
  description: string;
  orderIndex: number;
};

function toTrimmedString(value: FormDataEntryValue | string | undefined | null) {
  return String(value ?? '').trim();
}

function firstFiniteNumber(values: Array<FormDataEntryValue | string | number | undefined | null>) {
  for (const value of values) {
    const parsed = Number(String(value ?? '').trim());
    if (Number.isFinite(parsed)) {
      return parsed;
    }
  }

  return null;
}

export default function CourseManagementPage() {
  const { courseId } = useParams<{ courseId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [showAddLecture, setShowAddLecture] = useState(false);
  const [editLectureId, setEditLectureId] = useState<string | null>(null);
  const [uploadLectureId, setUploadLectureId] = useState<string | null>(null);
  const [draggedIdx, setDraggedIdx] = useState<number | null>(null);
  const [dragOverIdx, setDragOverIdx] = useState<number | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);

  const { data: course, isLoading } = useQuery({
    queryKey: ['course', courseId],
    queryFn: () => coursesApi.getById(courseId!),
    enabled: !!courseId,
    select: (res) => res.data.data,
  });

  const { data: lectures } = useQuery({
    queryKey: ['course-lectures', courseId],
    queryFn: () => lecturesApi.getCourseLectures(courseId!),
    enabled: !!courseId,
    select: (res) => {
      const items = res.data.data || [];
      return [...items].sort((a: any, b: any) => (a.orderIndex ?? 0) - (b.orderIndex ?? 0));
    },
  });

  const courseForm = useForm({
    values: course
      ? { title: course.title, description: course.description, price: course.price }
      : undefined,
  });

  const lectureCount = lectures?.length ?? 0;
  const nextLectureOrder = lectureCount + 1;

  const lectureForm = useForm<LectureFormValues>();

  // Reset add form with next order index when modal opens
  useEffect(() => {
    if (showAddLecture) {
      lectureForm.reset({ title: '', description: '', orderIndex: nextLectureOrder });
    }
  }, [showAddLecture, nextLectureOrder]);

  const editLectureForm = useForm<LectureFormValues>();

  // Populate edit form when a lecture is selected for editing
  const editingLecture = lectures?.find((l: any) => l.id === editLectureId);
  useEffect(() => {
    if (editingLecture) {
      editLectureForm.reset({
        title: editingLecture.title,
        description: editingLecture.description || '',
        orderIndex: editingLecture.orderIndex ?? 1,
      });
    }
  }, [editLectureId, editingLecture]);

  const updateCourseMutation = useMutation({
    mutationFn: (data: { title: string; description: string; price: number }) => {
      const fd = new FormData();
      fd.append('title', data.title);
      fd.append('description', data.description);
      fd.append('price', String(data.price));
      return coursesApi.update(courseId!, fd);
    },
    onSuccess: () => {
      toast.success('Course updated');
      queryClient.invalidateQueries({ queryKey: ['course', courseId] });
    },
    onError: () => toast.error('Update failed'),
  });

  const addLectureMutation = useMutation({
    mutationFn: (data: LectureFormValues) =>
      lecturesApi.create(courseId!, {
        title: (data.title ?? '').trim(),
        description: (data.description ?? '').trim(),
        orderIndex: data.orderIndex,
      }),
    onSuccess: () => {
      toast.success('Lecture added');
      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
      queryClient.invalidateQueries({ queryKey: ['course', courseId] });
      setShowAddLecture(false);
      lectureForm.reset();
    },
    onError: (err: any) => {
      const message =
        err?.response?.data?.message ||
        err?.response?.data?.errors?.[0] ||
        err?.message ||
        'Failed to add lecture';
      toast.error(message);
    },
  });

  const updateLectureMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: LectureFormValues }) =>
      lecturesApi.update(id, {
        title: (data.title ?? '').trim(),
        description: (data.description ?? '').trim(),
        orderIndex: data.orderIndex,
      }),
    onSuccess: () => {
      toast.success('Lecture updated');
      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
      setEditLectureId(null);
    },
    onError: (err: any) => {
      const message =
        err?.response?.data?.message ||
        err?.response?.data?.errors?.[0] ||
        err?.message ||
        'Failed to update lecture';
      toast.error(message);
    },
  });

  const handleAddLectureSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const formData = new FormData(event.currentTarget);
    const fallbackValues = lectureForm.getValues();

    const title = toTrimmedString(
      formData.get('title') ?? formData.get('Title') ?? formData.get('lectureTitle') ?? fallbackValues.title
    );
    const description = toTrimmedString(
      formData.get('description') ??
      formData.get('Description') ??
      formData.get('lectureDescription') ??
      fallbackValues.description
    );
    const orderIndex = nextLectureOrder;

    lectureForm.clearErrors();

    if (!title) {
      lectureForm.setError('title', { type: 'manual', message: 'Lecture title is required' });
      return;
    }

    addLectureMutation.mutate({ title, description, orderIndex });
  };

  const handleEditLectureSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!editLectureId) return;

    const formData = new FormData(event.currentTarget);
    const fallbackValues = editLectureForm.getValues();
    const maxEditOrder = Math.max(lectureCount, 1);

    const title = toTrimmedString(
      formData.get('title') ?? formData.get('Title') ?? formData.get('lectureTitle') ?? fallbackValues.title
    );
    const description = toTrimmedString(
      formData.get('description') ??
      formData.get('Description') ??
      formData.get('lectureDescription') ??
      fallbackValues.description
    );
    const orderIndex = firstFiniteNumber([
      formData.get('orderIndex'),
      formData.get('OrderIndex'),
      formData.get('order'),
      formData.get('position'),
      fallbackValues.orderIndex,
      1,
    ]);

    editLectureForm.clearErrors();

    if (!title) {
      editLectureForm.setError('title', { type: 'manual', message: 'Lecture title is required' });
      return;
    }

    if (orderIndex === null || orderIndex < 1 || orderIndex > maxEditOrder) {
      editLectureForm.setError('orderIndex', {
        type: 'manual',
        message: `Order must be between 1 and ${maxEditOrder}`,
      });
      return;
    }

    updateLectureMutation.mutate({
      id: editLectureId,
      data: { title, description, orderIndex: Math.trunc(orderIndex) },
    });
  };

  const {
    name: addTitleName,
    ...addTitleField
  } = lectureForm.register('title', {
    required: 'Lecture title is required',
    validate: (v) => (v?.trim()?.length ? true : 'Lecture title is required'),
    maxLength: { value: 200, message: 'Maximum length is 200 characters' },
  });
  const { name: addDescriptionName, ...addDescriptionField } = lectureForm.register('description');

  const {
    name: editTitleName,
    ...editTitleField
  } = editLectureForm.register('title', {
    required: 'Lecture title is required',
    validate: (v) => (v?.trim()?.length ? true : 'Lecture title is required'),
    maxLength: { value: 200, message: 'Maximum length is 200 characters' },
  });
  const { name: editDescriptionName, ...editDescriptionField } = editLectureForm.register('description');
  const {
    name: editOrderName,
    ...editOrderField
  } = editLectureForm.register('orderIndex', {
    valueAsNumber: true,
    required: 'Order is required',
    min: { value: 1, message: 'Minimum is 1' },
    max: { value: lectureCount, message: `Maximum is ${lectureCount}` },
  });

  const deleteLectureMutation = useMutation({
    mutationFn: (id: string) => lecturesApi.delete(id),
    onSuccess: () => {
      toast.success('Lecture deleted');
      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
      queryClient.invalidateQueries({ queryKey: ['course', courseId] });
    },
  });

  const uploadMutation = useMutation({
    mutationFn: ({ lectureId, files }: { lectureId: string; files: FileList }) => {
      const fd = new FormData();
      const titles: string[] = [];
      Array.from(files).forEach((file) => {
        fd.append('Files', file);
        titles.push(file.name);
      });
      return materialsApi.upload(lectureId, fd, titles.join(','));
    },
    onSuccess: () => {
      toast.success('Material(s) uploaded! Indexing in progress...');
      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
      setUploadLectureId(null);
    },
    onError: () => toast.error('Upload failed'),
  });

  const deleteMaterialMutation = useMutation({
    mutationFn: (id: string) => materialsApi.delete(id),
    onSuccess: () => {
      toast.success('Material deleted');
      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
    },
  });

  // Drag-and-drop reorder
  const handleDrop = useCallback(async (fromIdx: number, toIdx: number) => {
    if (fromIdx === toIdx || !lectures) return;
    const sorted = [...lectures];
    const [moved] = sorted.splice(fromIdx, 1);
    sorted.splice(toIdx, 0, moved);

    // Update all affected lectures with new order indices
    const updates = sorted.map((lec: any, i: number) => ({
      id: lec.id,
      data: { title: lec.title, description: lec.description || '', orderIndex: i + 1 },
    }));

    try {
      await Promise.all(
        updates
          .filter((u, i) => (lectures[i] as any)?.id !== u.id || (lectures[i] as any)?.orderIndex !== u.data.orderIndex)
          .map((u) => lecturesApi.update(u.id, u.data))
      );
      toast.success('Lectures reordered');
      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
    } catch {
      toast.error('Failed to reorder');
    }
  }, [lectures, courseId, queryClient]);

  if (isLoading) return <PageSpinner />;
  if (!course) return <div className="p-8 text-center">Course not found</div>;

  return (
    <AnimatedPage>
    <div className="max-w-5xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold">Manage Course</h1>
        <Button
          variant="outline"
          onClick={() => navigate(`/teacher/courses/${courseId}/engagement`)}
        >
          <BarChart3 className="h-4 w-4 mr-2" /> Engagement
        </Button>
      </div>

      {/* Course Info */}
      <Card className="mb-8">
        <CardContent className="p-6">
          <form
            onSubmit={courseForm.handleSubmit((data) => updateCourseMutation.mutate(data))}
            className="space-y-5"
          >
            <h2 className="text-lg font-semibold border-b border-border pb-2">Course Information</h2>
            <Input
              label="Title"
              placeholder="Enter the course title"
              {...courseForm.register('title')}
            />
            <Textarea
              label="Description"
              placeholder="Describe what this course covers..."
              {...courseForm.register('description')}
            />
            <Input
              label="Price"
              type="number"
              step="0.01"
              placeholder="0.00"
              hint="Set to 0 for a free course."
              {...courseForm.register('price', { valueAsNumber: true })}
            />
            <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
              <Button type="submit" loading={updateCourseMutation.isPending}>Save Changes</Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {/* Lectures */}
      <div className="mb-8">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h2 className="text-lg font-bold">Lectures</h2>
            <p className="text-xs text-muted-foreground">Drag to reorder</p>
          </div>
          <Button size="sm" onClick={() => setShowAddLecture(true)}>
            <Plus className="h-4 w-4 mr-1" /> Add Lecture
          </Button>
        </div>

        <div className="space-y-3">
          {lectures?.map((lecture: any, idx: number) => (
            <div
              key={lecture.id}
              draggable
              onDragStart={(e) => {
                setDraggedIdx(idx);
                e.dataTransfer.effectAllowed = 'move';
              }}
              onDragOver={(e) => {
                e.preventDefault();
                e.dataTransfer.dropEffect = 'move';
                setDragOverIdx(idx);
              }}
              onDragLeave={() => setDragOverIdx(null)}
              onDrop={(e) => {
                e.preventDefault();
                setDragOverIdx(null);
                if (draggedIdx !== null) handleDrop(draggedIdx, idx);
                setDraggedIdx(null);
              }}
              onDragEnd={() => {
                setDraggedIdx(null);
                setDragOverIdx(null);
              }}
            >
            <Card
              className={`transition-all ${
                dragOverIdx === idx ? 'border-primary shadow-lg' : ''
              } ${draggedIdx === idx ? 'opacity-50' : ''}`}
            >
              <CardContent className="p-4">
                <div className="flex items-center justify-between mb-2">
                  <div className="flex items-center gap-2">
                    <GripVertical className="h-4 w-4 text-muted-foreground cursor-grab" />
                    <span className="text-sm text-muted-foreground">{idx + 1}.</span>
                    <h3 className="font-semibold">{lecture.title}</h3>
                  </div>
                  <div className="flex gap-1">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setUploadLectureId(lecture.id)}
                      title="Upload material"
                    >
                      <Upload className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setEditLectureId(lecture.id)}
                      title="Edit lecture"
                    >
                      <Pencil className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => deleteLectureMutation.mutate(lecture.id)}
                      title="Delete lecture"
                    >
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
                  </div>
                </div>
                {lecture.description && (
                  <p className="text-sm text-muted-foreground mb-2">{lecture.description}</p>
                )}
                {/* Materials */}
                {lecture.materials && lecture.materials.length > 0 && (
                  <div className="pl-4 space-y-1 mt-2">
                    {lecture.materials.map((mat: any) => (
                      <div
                        key={mat.id}
                        className="flex items-center justify-between text-sm py-1"
                      >
                        <div className="flex items-center gap-2">
                          <Badge variant="outline">{mat.type || 'File'}</Badge>
                          <span>{mat.title}</span>
                        </div>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => deleteMaterialMutation.mutate(mat.id)}
                        >
                          <Trash2 className="h-3 w-3 text-destructive" />
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
            </div>
          ))}
        </div>
      </div>

      {/* Add Lecture Modal */}
      <Modal open={showAddLecture} onClose={() => setShowAddLecture(false)} title="Add Lecture">
        <form onSubmit={handleAddLectureSubmit} className="space-y-5">
          <Input
            id="add-lecture-title"
            name={addTitleName}
            label="Title"
            placeholder="e.g. Getting Started with the Basics"
            error={lectureForm.formState.errors.title?.message}
            {...addTitleField}
          />
          <Textarea
            id="add-lecture-description"
            name={addDescriptionName}
            label="Description"
            placeholder="Briefly describe the content of this lecture..."
            {...addDescriptionField}
          />
          <Input
            id="add-lecture-order"
            label="Order (auto)"
            type="number"
            value={nextLectureOrder}
            readOnly
            hint={`This is auto-set to lecture #${nextLectureOrder}.`}
          />
          <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
            <Button type="submit" loading={addLectureMutation.isPending}>Add Lecture</Button>
            <Button variant="outline" type="button" onClick={() => setShowAddLecture(false)}>Cancel</Button>
          </div>
        </form>
      </Modal>

      {/* Edit Lecture Modal */}
      <Modal
        open={!!editLectureId}
        onClose={() => setEditLectureId(null)}
        title="Edit Lecture"
      >
        <form onSubmit={handleEditLectureSubmit} className="space-y-5">
          <Input
            id="edit-lecture-title"
            name={editTitleName}
            label="Title"
            placeholder="Enter the lecture title"
            error={editLectureForm.formState.errors.title?.message}
            {...editTitleField}
          />
          <Textarea
            id="edit-lecture-description"
            name={editDescriptionName}
            label="Description"
            placeholder="Briefly describe the content of this lecture..."
            {...editDescriptionField}
          />
          <Input
            id="edit-lecture-order"
            name={editOrderName}
            label="Order (position)"
            type="number"
            placeholder="1"
            hint={`Choose a position between 1 and ${lectureCount}.`}
            error={editLectureForm.formState.errors.orderIndex?.message}
            {...editOrderField}
          />
          <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
            <Button type="submit" loading={updateLectureMutation.isPending}>Save Changes</Button>
            <Button variant="outline" type="button" onClick={() => setEditLectureId(null)}>Cancel</Button>
          </div>
        </form>
      </Modal>

      {/* Upload Material Modal */}
      <Modal
        open={!!uploadLectureId}
        onClose={() => setUploadLectureId(null)}
        title="Upload Materials"
      >
        <form
          onSubmit={(e) => {
            e.preventDefault();
            const files = fileRef.current?.files;
            if (!files?.length || !uploadLectureId) return;
            uploadMutation.mutate({ lectureId: uploadLectureId, files });
          }}
          className="space-y-5"
        >
          <FileInput
            ref={fileRef}
            label="Files"
            multiple
            accept=".pdf,.mp4,.mp3,.wav,.ogg,.jpg,.jpeg,.png,.gif,.webp,.docx,.pptx,.txt,.md,.webm"
            hint="Max 100MB per file. Supports PDF, video, audio, images, and documents."
          />
          <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
            <Button type="submit" loading={uploadMutation.isPending}>Upload</Button>
            <Button variant="outline" type="button" onClick={() => setUploadLectureId(null)}>Cancel</Button>
          </div>
        </form>
      </Modal>
    </div>
    </AnimatedPage>
  );
}
