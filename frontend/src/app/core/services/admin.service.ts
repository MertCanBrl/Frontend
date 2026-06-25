import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  UserDto,
  CourseDto,
  CreateUserRequest,
  UpdateUserRequest,
  CreateCourseRequest,
  UpdateCourseRequest,
  ApprovalListItemDto,
  AdminCourseContentDto,
} from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin`;

  getUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(`${this.base}/users`);
  }

  createUser(req: CreateUserRequest): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.base}/users`, req);
  }

  updateUser(id: number, req: UpdateUserRequest): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.base}/users/${id}`, req);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/users/${id}`);
  }

  getUserCourses(id: number): Observable<CourseDto[]> {
    return this.http.get<CourseDto[]>(`${this.base}/users/${id}/courses`);
  }

  getCourses(): Observable<CourseDto[]> {
    return this.http.get<CourseDto[]>(`${this.base}/courses`);
  }

  createCourse(req: CreateCourseRequest): Observable<CourseDto> {
    return this.http.post<CourseDto>(`${this.base}/courses`, req);
  }

  updateCourse(id: number, req: UpdateCourseRequest): Observable<CourseDto> {
    return this.http.put<CourseDto>(`${this.base}/courses/${id}`, req);
  }

  deleteCourse(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/courses/${id}`);
  }

  // ── Onay Yönetimi ─────────────────────────────────────────────────────────

  getApprovals(): Observable<ApprovalListItemDto[]> {
    return this.http.get<ApprovalListItemDto[]>(`${this.base}/approvals`);
  }

  approveCourseContent(courseId: number): Observable<void> {
    return this.http.post<void>(`${this.base}/approvals/${courseId}/approve`, {});
  }

  requestCourseRevision(courseId: number, note: string): Observable<void> {
    return this.http.post<void>(`${this.base}/approvals/${courseId}/request-revision`, { note });
  }

  getCourseContent(courseId: number): Observable<AdminCourseContentDto> {
    return this.http.get<AdminCourseContentDto>(`${this.base}/courses/${courseId}/content`);
  }

  downloadCoursePdf(courseId: number): Observable<Blob> {
    return this.http.get(`${this.base}/courses/${courseId}/pdf`, { responseType: 'blob' });
  }
}
