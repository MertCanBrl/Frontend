import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  UserDto,
  CourseDto,
  CreateUserRequest,
  CreateCourseRequest,
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

  getCourses(): Observable<CourseDto[]> {
    return this.http.get<CourseDto[]>(`${this.base}/courses`);
  }

  createCourse(req: CreateCourseRequest): Observable<CourseDto> {
    return this.http.post<CourseDto>(`${this.base}/courses`, req);
  }
}
