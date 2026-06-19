import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { coursesApi } from '@/api/courses.api';
import { lecturesApi } from '@/api/lectures.api';
import { materialsApi } from '@/api/materials.api';
import { examsApi } from '@/api/exams.api';
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
import { useState, useEffect, useCallback, useRef } from 'react';
import { Plus, Trash2, Upload, Pencil, BarChart3, GripVertical, BookOpen, Settings, ListVideo, ArrowLeft, RefreshCw, FileText } from 'lucide-react';
import { z } from 'zod';

type LectureFormValues = {
  title: string;
  description: string;
  orderIndex: number;
};

type UploadMaterialFormValues = {
  files: FileList | null;
};

type IndexingStatusEventDetail = {
  courseId?: string;
  success?: boolean;
};

const uploadMaterialSchema = z.object({
  files: z.any().refine((files) => files instanceof FileList && files.length > 0, {
    message: 'Please select at least one file to upload',
  }),
});

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
  const [processingMaterialIds, setProcessingMaterialIds] = useState<Record<string, true>>({});
  const [failedMaterialIds, setFailedMaterialIds] = useState<Record<string, true>>({});
  const processingMaterialIdsRef = useRef<Record<string, true>>({});

  const [showCreateExam, setShowCreateExam] = useState(false);
  const examForm = useForm<{
    title: string;
    durationMinutes: number;
    startTime: string;
    endTime: string;
  }>();

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

  const { data: examsData } = useQuery({
    queryKey: ['course-exams', courseId],
    queryFn: () => examsApi.getByCourse(courseId!),
    enabled: !!courseId,
    select: (res) => (res.data.data?.items ?? []),
  });

  const markMaterialsAsProcessing = useCallback((materialIds: string[]) => {
    if (!materialIds.length) return;

    setProcessingMaterialIds((prev) => {
      const next = { ...prev };
      materialIds.forEach((id) => {
        if (id) {
          next[id] = true;
        }
      });
      return next;
    });

    setFailedMaterialIds((prev) => {
      const next = { ...prev };
      materialIds.forEach((id) => {
        if (id) {
          delete next[id];
        }
      });
      return next;
    });
  }, []);

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

  useEffect(() => {
    processingMaterialIdsRef.current = processingMaterialIds;
  }, [processingMaterialIds]);

  useEffect(() => {
    const handleIndexingStatus = (event: Event) => {
      const payload = (event as CustomEvent<IndexingStatusEventDetail>).detail;

      if (!payload?.courseId || payload.courseId !== courseId) {
        return;
      }

      if (payload.success) {
        setProcessingMaterialIds({});
        setFailedMaterialIds({});

        queryClient.setQueryData(['course-lectures', courseId], (previousData: any) => {
          const markLectureMaterialsIndexed = (lecture: any) => ({
            ...lecture,
            materials:
              lecture.materials?.map((mat: any) => ({
                ...mat,
                indexed: true,
                isAiIndexed: true,
                updatedAt: new Date().toISOString(),
              })) ?? [],
          });

          if (Array.isArray(previousData)) {
            return previousData.map(markLectureMaterialsIndexed);
          }

          if (Array.isArray(previousData?.data?.data)) {
            return {
              ...previousData,
              data: {
                ...previousData.data,
                data: previousData.data.data.map(markLectureMaterialsIndexed),
              },
            };
          }

          return previousData;
        });
      } else {
        const currentProcessingIds = Object.keys(processingMaterialIdsRef.current);
        setProcessingMaterialIds({});
        setFailedMaterialIds((prev) => {
          const next = { ...prev };
          currentProcessingIds.forEach((id) => {
            next[id] = true;
          });
          return next;
        });
      }

      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
    };

    window.addEventListener('aiedu:indexing-status', handleIndexingStatus);

    return () => {
      window.removeEventListener('aiedu:indexing-status', handleIndexingStatus);
    };
  }, [courseId, queryClient]);

  useEffect(() => {
    if (!lectures?.length) {
      return;
    }

    const indexedIds = new Set(
      lectures
        .flatMap((lecture: any) => lecture.materials ?? [])
        .filter((mat: any) => Boolean(mat.isAiIndexed ?? mat.indexed))
        .map((mat: any) => String(mat.id))
    );

    if (!indexedIds.size) {
      return;
    }

    setProcessingMaterialIds((prev) => {
      const next = { ...prev };
      let changed = false;

      Object.keys(next).forEach((id) => {
        if (indexedIds.has(id)) {
          delete next[id];
          changed = true;
        }
      });

      return changed ? next : prev;
    });

    setFailedMaterialIds((prev) => {
      const next = { ...prev };
      let changed = false;

      Object.keys(next).forEach((id) => {
        if (indexedIds.has(id)) {
          delete next[id];
          changed = true;
        }
      });

      return changed ? next : prev;
    });
  }, [lectures]);

  const editLectureForm = useForm<LectureFormValues>();
  const uploadMaterialForm = useForm<UploadMaterialFormValues>({
    resolver: zodResolver(uploadMaterialSchema),
    mode: 'onChange',
    defaultValues: {
      files: null,
    },
  });

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

  const createExamMutation = useMutation({
    mutationFn: (data: any) => {
      const { startTime, endTime, ...examData } = data;
      return examsApi.create(courseId!, {
        ...examData,
        startTime: new Date(startTime).toISOString(),
        endTime: new Date(endTime).toISOString(),
      });
    },
    onSuccess: (res) => {
      toast.success('Exam created!');
      setShowCreateExam(false);
      examForm.reset();
      queryClient.invalidateQueries({ queryKey: ['course-exams', courseId] });
      const examId = res.data.data?.examId;
      if (examId) navigate(`/teacher/exams/${examId}/questions`);
    },
    onError: () => toast.error('Failed to create exam'),
  });

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
    onError: (error: any) => toast.error(error?.userMessage ?? 'Update failed'),
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
  const {
    name: uploadFilesName,
    ref: uploadFilesRef,
    onChange: onUploadFilesChange,
    ...uploadFilesField
  } = uploadMaterialForm.register('files');

  const deleteLectureMutation = useMutation({
    mutationFn: (id: string) => lecturesApi.delete(id),
    onSuccess: () => {
      toast.success('Lecture deleted');
      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
      queryClient.invalidateQueries({ queryKey: ['course', courseId] });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? 'Failed to delete lecture'),
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
    onSuccess: (res) => {
      toast.success('Material(s) uploaded! Indexing in progress...');

      const payload = res.data.data as any;
      const materialIds = Array.isArray(payload?.materialIds)
        ? payload.materialIds.map((id: any) => String(id))
        : [];
      markMaterialsAsProcessing(materialIds);

      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
      setUploadLectureId(null);
      uploadMaterialForm.reset();
    },
    onError: (error: any) => toast.error(error?.userMessage ?? 'Upload failed'),
  });

  const deleteMaterialMutation = useMutation({
    mutationFn: (id: string) => materialsApi.delete(id),
    onSuccess: () => {
      toast.success('Material deleted');
      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? 'Failed to delete material'),
  });

  const reindexMaterialMutation = useMutation({
    mutationFn: (id: string) => materialsApi.reindex(id),
    onSuccess: (_res, materialId) => {
      toast.success('Indexing queued!');
      markMaterialsAsProcessing([materialId]);
      queryClient.invalidateQueries({ queryKey: ['course-lectures', courseId] });
    },
    onError: (error: any) => toast.error(error?.userMessage ?? 'Failed to queue indexing'),
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
      <div className="max-w-6xl mx-auto px-4 py-8 space-y-8">
        
        {/* Header */}
        <div className="flex flex-col md:flex-row items-start md:items-center justify-between gap-4 bg-primary/5 p-6 rounded-2xl border border-primary/10">
          <div className="flex gap-4 items-center">
            <div className="h-12 w-12 rounded-xl bg-primary/20 flex items-center justify-center text-primary shrink-0">
              <Settings className="h-6 w-6" />
            </div>
            <div>
              <h1 className="text-3xl font-bold tracking-tight">Manage Course</h1>
              <p className="text-muted-foreground mt-1">Configure settings and organize curriculum for <span className="font-semibold text-foreground">{course.title}</span>.</p>
            </div>
          </div>
          <div className="flex items-center gap-3 shrink-0">
            <Button variant="outline" onClick={() => navigate('/teacher/courses')}>
              <ArrowLeft className="h-4 w-4 mr-2" /> Back
            </Button>
            <Button
              variant="outline"
              className="bg-background/50 hover:bg-background"
              onClick={() => navigate(`/teacher/courses/${courseId}/engagement`)}
            >
              <BarChart3 className="h-4 w-4 mr-2 text-primary" /> Engagement
            </Button>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Settings Sidebar */}
          <div className="lg:col-span-1 space-y-6">
            <Card variant="glass" className="border-border/50 sticky top-24">
              <CardContent className="p-5 space-y-5">
                <div className="flex items-center gap-2 border-b border-border pb-3">
                  <BookOpen className="h-4 w-4 text-primary" />
                  <h2 className="text-lg font-semibold">Course Details</h2>
                </div>
                <form
                  onSubmit={courseForm.handleSubmit((data) => updateCourseMutation.mutate(data))}
                  className="space-y-4"
                >
                  <Input
                    label="Title"
                    placeholder="Enter the course title"
                    className="bg-background/50"
                    {...courseForm.register('title')}
                  />
                  <Textarea
                    label="Description"
                    placeholder="Describe what this course covers..."
                    className="bg-background/50 min-h-[120px]"
                    {...courseForm.register('description')}
                  />
                  <Input
                    label="Price ($)"
                    type="number"
                    step="0.01"
                    placeholder="0.00"
                    hint="Set to 0 for a free course."
                    className="bg-background/50"
                    {...courseForm.register('price', { valueAsNumber: true })}
                  />
                  <div className="pt-2">
                    <Button type="submit" className="w-full shadow-sm" loading={updateCourseMutation.isPending}>
                      Save Changes
                    </Button>
                  </div>
                </form>
              </CardContent>
            </Card>
          </div>

          {/* Curriculum Main Content */}
          <div className="lg:col-span-2 space-y-6">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 bg-card/50 p-4 rounded-xl border border-border/50">
              <div className="flex items-center gap-2">
                <ListVideo className="h-5 w-5 text-primary" />
                <div>
                  <h2 className="text-xl font-bold">Curriculum Setup</h2>
                  <p className="text-sm text-muted-foreground mt-0.5">Drag to reorder lectures</p>
                </div>
              </div>
              <Button size="sm" onClick={() => setShowAddLecture(true)} className="shadow-sm">
                <Plus className="h-4 w-4 mr-1.5" /> Add Lecture
              </Button>
            </div>

            <div className="space-y-3 pl-2">
              {lectures?.length === 0 ? (
                <div className="text-center py-12 border-2 border-dashed border-border/50 rounded-xl bg-background/30">
                  <ListVideo className="h-10 w-10 text-muted-foreground mx-auto mb-3 opacity-50" />
                  <p className="text-muted-foreground mb-4">No lectures have been added yet.</p>
                  <Button variant="outline" size="sm" onClick={() => setShowAddLecture(true)}>
                    <Plus className="h-4 w-4 mr-1.5" /> Create First Lecture
                  </Button>
                </div>
              ) : (
                lectures?.map((lecture: any, idx: number) => (
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
                    {lecture.materials.map((mat: any) => {
                      const materialId = String(mat.id);
                      const isAiIndexed = Boolean(mat.isAiIndexed ?? mat.indexed);
                      const isProcessing = Boolean(processingMaterialIds[materialId]);
                      const isFailed = Boolean(failedMaterialIds[materialId]) || (!isAiIndexed && !isProcessing);

                      return (
                        <div
                          key={mat.id}
                          className="flex items-center justify-between text-sm py-1"
                        >
                          <div className="flex items-center gap-2">
                            <Badge variant="outline">{mat.type || 'File'}</Badge>
                            <span>{mat.title}</span>
                            {isProcessing && (
                              <span title="AI is currently indexing this material in the background.">
                                <Badge variant="info" className="ml-1">
                                  <RefreshCw className="mr-1 h-3 w-3 inline animate-spin" />
                                  Indexing...
                                </Badge>
                              </span>
                            )}
                            {isFailed && (
                              <span title="AI indexing failed for this material. Click here to trigger indexing manually" className="cursor-pointer" onClick={() => reindexMaterialMutation.mutate(mat.id)}>
                                <Badge variant="warning" className="ml-1">
                                  <RefreshCw className="mr-1 h-3 w-3 inline" />
                                  Retry Indexing
                                </Badge>
                              </span>
                            )}
                          </div>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => deleteMaterialMutation.mutate(mat.id)}
                          >
                            <Trash2 className="h-3 w-3 text-destructive" />
                          </Button>
                        </div>
                      );
                    })}
                  </div>
                )}
              </CardContent>
            </Card>
            </div>
          )))}
            </div>
          </div>

          {/* Exams Main Content */}
          <div className="lg:col-span-2 space-y-6 mt-8">
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 bg-card/50 p-4 rounded-xl border border-border/50">
              <div className="flex items-center gap-2">
                <FileText className="h-5 w-5 text-primary" />
                <div>
                  <h2 className="text-xl font-bold">Assessments</h2>
                  <p className="text-sm text-muted-foreground mt-0.5">Manage course exams and quizzes</p>
                </div>
              </div>
              <Button size="sm" onClick={() => setShowCreateExam(true)} className="shadow-sm">
                <Plus className="h-4 w-4 mr-1.5" /> Create Exam
              </Button>
            </div>

            <div className="space-y-3 pl-2">
              {!examsData || examsData.length === 0 ? (
                <div className="text-center py-12 border-2 border-dashed border-border/50 rounded-xl bg-background/30">
                  <FileText className="h-10 w-10 text-muted-foreground mx-auto mb-3 opacity-50" />
                  <p className="text-muted-foreground mb-4">No exams have been created yet.</p>
                  <Button variant="outline" size="sm" onClick={() => setShowCreateExam(true)}>
                    <Plus className="h-4 w-4 mr-1.5" /> Create First Exam
                  </Button>
                </div>
              ) : (
                examsData.map((exam: any) => (
                  <Card key={exam.id} className="transition-all hover:border-primary/50">
                    <CardContent className="p-4 flex items-center justify-between">
                      <div>
                        <div className="flex items-center gap-2 mb-1">
                          <h3 className="font-semibold">{exam.title}</h3>
                          <Badge variant="outline">{exam.durationMinutes} min</Badge>
                          {exam.questionCount != null && (
                            <Badge variant="outline">{exam.questionCount} questions</Badge>
                          )}
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => navigate(`/teacher/exams/${exam.id}/questions`)}
                        >
                          <Pencil className="h-4 w-4 mr-1" /> Questions
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => navigate(`/teacher/exams/${exam.id}`)}
                        >
                          Details
                        </Button>
                      </div>
                    </CardContent>
                  </Card>
                ))
              )}
            </div>
          </div>
        </div>

      {/* Create Exam Modal */}
      <Modal open={showCreateExam} onClose={() => setShowCreateExam(false)} title="Create Exam">
        <form onSubmit={examForm.handleSubmit((data) => createExamMutation.mutate(data))} className="space-y-4">
          <Input label="Exam Title" required {...examForm.register('title')} />
          <Input
            type="number"
            label="Duration (minutes)"
            required
            {...examForm.register('durationMinutes', { valueAsNumber: true })}
          />
          <Input
            type="datetime-local"
            label="Start Time"
            required
            {...examForm.register('startTime')}
          />
          <Input
            type="datetime-local"
            label="End Time"
            required
            {...examForm.register('endTime')}
          />
          <Button type="submit" loading={createExamMutation.isPending} className="w-full">
            Create
          </Button>
        </form>
      </Modal>

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
        onClose={() => {
          setUploadLectureId(null);
          uploadMaterialForm.reset();
        }}
        title="Upload Materials"
      >
        <form
          onSubmit={uploadMaterialForm.handleSubmit((values) => {
            if (!uploadLectureId || !values.files || values.files.length === 0) return;
            uploadMutation.mutate({ lectureId: uploadLectureId, files: values.files });
          })}
          className="space-y-5"
        >
          <FileInput
            ref={uploadFilesRef}
            name={uploadFilesName}
            onChange={(event) => {
              onUploadFilesChange(event);
            }}
            {...uploadFilesField}
            label="Files"
            multiple
            accept=".pdf,.mp4,.mp3,.wav,.ogg,.jpg,.jpeg,.png,.gif,.webp,.docx,.pptx,.txt,.md,.webm"
            hint="Max 100MB per file. Supports PDF, video, audio, images, and documents."
            error={uploadMaterialForm.formState.errors.files?.message as string | undefined}
          />
          <div className="flex items-center gap-3 pt-4 border-t border-border mt-6">
            <Button
              type="submit"
              loading={uploadMutation.isPending}
              disabled={!uploadMaterialForm.formState.isValid || uploadMutation.isPending}
            >
              Upload
            </Button>
            <Button
              variant="outline"
              type="button"
              onClick={() => {
                setUploadLectureId(null);
                uploadMaterialForm.reset();
              }}
            >
              Cancel
            </Button>
          </div>
        </form>
      </Modal>
    </div>
    </AnimatedPage>
  );
}
