import client from './client';
import type { ApiResponse, LectureDto, LectureDetailDto } from '@/types';

type LecturePayload = {
  title: string;
  description: string;
  orderIndex: number;
};

function normalizeLecturePayload(data: LecturePayload) {
  const title = (data.title ?? '').trim();
  const description = (data.description ?? '').trim();
  const orderIndex = Number.isFinite(data.orderIndex) ? data.orderIndex : 1;

  // Include camelCase, PascalCase, and legacy aliases for max compatibility.
  return {
    title,
    Title: title,
    description,
    Description: description,
    orderIndex,
    OrderIndex: orderIndex,
    lectureTitle: title,
    lectureDescription: description,
    order: orderIndex,
    position: orderIndex,
  };
}

function buildCreatePayload(courseId: string, data: LecturePayload) {
  return {
    ...normalizeLecturePayload(data),
    courseId,
    CourseId: courseId,
  };
}

function buildUpdatePayload(lectureId: string, data: LecturePayload) {
  return {
    ...normalizeLecturePayload(data),
    lectureId,
    LectureId: lectureId,
  };
}

function buildMultipartPayload(payload: Record<string, unknown>) {
  const formData = new FormData();
  for (const [key, value] of Object.entries(payload)) {
    if (value === null || value === undefined) continue;
    formData.append(key, String(value));
  }
  return formData;
}

function extractErrorMessage(error: unknown) {
  const responseData = (error as any)?.response?.data;
  if (typeof responseData === 'string') return responseData;

  const errors = responseData?.errors;
  if (Array.isArray(errors) && errors.length > 0) {
    return errors.join(' ');
  }

  if (typeof responseData?.message === 'string') {
    return responseData.message;
  }

  if (typeof responseData?.title === 'string') {
    return responseData.title;
  }

  return (error as any)?.message ?? '';
}

function shouldRetryMultipart(error: unknown, payloadTitle: string) {
  if (!payloadTitle) return false;

  const status = (error as any)?.response?.status;
  if (status !== 400 && status !== 415 && status !== 422) {
    return false;
  }

  const message = extractErrorMessage(error).toLowerCase();
  return message.includes('title') || message.includes('lecture title') || message.includes('validation');
}

async function postLectureWithFallback(courseId: string, data: LecturePayload) {
  const payload = buildCreatePayload(courseId, data);

  try {
    return await client.post<ApiResponse<{ lectureId: string }>>(`/courses/${courseId}/lectures`, payload);
  } catch (error) {
    if (!shouldRetryMultipart(error, payload.title)) {
      throw error;
    }

    return client.post<ApiResponse<{ lectureId: string }>>(
      `/courses/${courseId}/lectures`,
      buildMultipartPayload(payload),
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      }
    );
  }
}

async function putLectureWithFallback(lectureId: string, data: LecturePayload) {
  const payload = buildUpdatePayload(lectureId, data);

  try {
    return await client.put(`/courses/lectures/${lectureId}`, payload);
  } catch (error) {
    if (!shouldRetryMultipart(error, payload.title)) {
      throw error;
    }

    return client.put(`/courses/lectures/${lectureId}`, buildMultipartPayload(payload), {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
  }
}

export const lecturesApi = {
  getCourseLectures: (courseId: string, includeMaterials = true) =>
    client.get<ApiResponse<LectureDto[]>>(`/courses/${courseId}/lectures`, {
      params: { IncludeMaterials: includeMaterials },
    }),

  getById: (lectureId: string) =>
    client.get<ApiResponse<LectureDetailDto>>(`/lectures/${lectureId}`),

  create: (courseId: string, data: LecturePayload) => postLectureWithFallback(courseId, data),

  update: (lectureId: string, data: LecturePayload) => putLectureWithFallback(lectureId, data),

  delete: (lectureId: string) => client.delete(`/courses/lectures/${lectureId}`),
};
