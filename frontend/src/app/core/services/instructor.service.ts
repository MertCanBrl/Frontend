import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import {
  InstructorCourseDto, CourseDetailDto, UpdateCourseRequest,
  CourseTopicDto, SaveCourseTopicRequest,
  LearningOutcomeDto, SaveLearningOutcomeRequest,
  ProgramOutcomeDto, SaveProgramOutcomeRequest,
  MappingMatrixDto, MappingCellDto
} from '../models/course.models';

@Injectable({ providedIn: 'root' })
export class InstructorService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  // Courses
  getMyCourses(): Observable<InstructorCourseDto[]> {
    return this.http.get<InstructorCourseDto[]>(`${this.base}/instructor/my-courses`);
  }

  getCourseDetail(id: number): Observable<CourseDetailDto> {
    return this.http.get<CourseDetailDto>(`${this.base}/instructor/courses/${id}`);
  }

  updateCourse(id: number, req: UpdateCourseRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/instructor/courses/${id}`, req);
  }

  // Topics
  getTopics(courseId: number): Observable<CourseTopicDto[]> {
    return this.http.get<CourseTopicDto[]>(`${this.base}/courses/${courseId}/topics`);
  }

  addTopic(courseId: number, req: SaveCourseTopicRequest): Observable<CourseTopicDto> {
    return this.http.post<CourseTopicDto>(`${this.base}/courses/${courseId}/topics`, req);
  }

  updateTopic(courseId: number, id: number, req: SaveCourseTopicRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/courses/${courseId}/topics/${id}`, req);
  }

  deleteTopic(courseId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/courses/${courseId}/topics/${id}`);
  }

  // Learning Outcomes
  getLearningOutcomes(courseId: number): Observable<LearningOutcomeDto[]> {
    return this.http.get<LearningOutcomeDto[]>(`${this.base}/courses/${courseId}/learning-outcomes`);
  }

  addLearningOutcome(courseId: number, req: SaveLearningOutcomeRequest): Observable<LearningOutcomeDto> {
    return this.http.post<LearningOutcomeDto>(`${this.base}/courses/${courseId}/learning-outcomes`, req);
  }

  updateLearningOutcome(courseId: number, id: number, req: SaveLearningOutcomeRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/courses/${courseId}/learning-outcomes/${id}`, req);
  }

  deleteLearningOutcome(courseId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/courses/${courseId}/learning-outcomes/${id}`);
  }

  // LO-PO Mapping
  getMatrix(courseId: number): Observable<MappingMatrixDto> {
    return this.http.get<MappingMatrixDto>(`${this.base}/courses/${courseId}/mapping`);
  }

  updateMappingCell(courseId: number, cell: MappingCellDto): Observable<void> {
    return this.http.put<void>(`${this.base}/courses/${courseId}/mapping/cell`, cell);
  }

  // Program Outcomes
  getProgramOutcomes(): Observable<ProgramOutcomeDto[]> {
    return this.http.get<ProgramOutcomeDto[]>(`${this.base}/program-outcomes`);
  }

  addProgramOutcome(req: SaveProgramOutcomeRequest): Observable<ProgramOutcomeDto> {
    return this.http.post<ProgramOutcomeDto>(`${this.base}/program-outcomes`, req);
  }

  deleteProgramOutcome(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/program-outcomes/${id}`);
  }
}
