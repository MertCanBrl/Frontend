import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  InstructorCourseDto, CourseDetailDto, UpdateCourseRequest, UpdateCourseContentRequest,
  CourseTopicDto, SaveCourseTopicRequest,
  LearningOutcomeDto, SaveLearningOutcomeRequest,
  ProgramOutcomeDto, SaveProgramOutcomeRequest, UpdateProgramOutcomeRequest,
  MappingMatrixDto, MappingCellDto,
  SurveyQuestionDto, SaveSurveyQuestionRequest,
  ExamDto, ExamDetailDto, SaveExamRequest,
  ExamGradeEntryDto, SaveExamGradeEntryRequest,
  AssessmentComponentDto, SaveAssessmentComponentRequest,
  ComponentGradeEntryDto, SaveComponentGradeEntryRequest,
  StudentCourseResultDto, RiskAnalysisDto,
  CourseStatisticsDto, SaveGradesRequest,
  LearningOutcomeStatusDto, ComponentReportItemDto
} from '../models/course.models';

@Injectable({ providedIn: 'root' })
export class InstructorService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  // ── Mevcut endpointler ────────────────────────────────────────────────────

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

  updateProgramOutcome(id: number, req: UpdateProgramOutcomeRequest): Observable<ProgramOutcomeDto> {
    return this.http.put<ProgramOutcomeDto>(`${this.base}/program-outcomes/${id}`, req);
  }

  deleteProgramOutcome(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/program-outcomes/${id}`);
  }

  // ── Ders İçerikleri ──────────────────────────────────────────────────────

  getCourseContents(): Observable<InstructorCourseDto[]> {
    return this.http.get<InstructorCourseDto[]>(`${this.base}/instructor/course-contents`);
  }

  getCourseContentDetail(courseId: number): Observable<CourseDetailDto> {
    return this.http.get<CourseDetailDto>(`${this.base}/instructor/course-contents/${courseId}`);
  }

  updateCourseContentInfo(courseId: number, req: UpdateCourseContentRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/instructor/course-contents/${courseId}/general-info`, req);
  }

  submitCourseContentForReview(courseId: number): Observable<void> {
    return this.http.post<void>(`${this.base}/instructor/course-contents/${courseId}/submit-for-review`, {});
  }

  // Survey Questions
  getSurveyQuestions(courseId: number): Observable<SurveyQuestionDto[]> {
    return this.http.get<SurveyQuestionDto[]>(`${this.base}/instructor/course-contents/${courseId}/survey-questions`);
  }

  addSurveyQuestion(courseId: number, req: SaveSurveyQuestionRequest): Observable<SurveyQuestionDto> {
    return this.http.post<SurveyQuestionDto>(`${this.base}/instructor/course-contents/${courseId}/survey-questions`, req);
  }

  updateSurveyQuestion(courseId: number, id: number, req: SaveSurveyQuestionRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/instructor/course-contents/${courseId}/survey-questions/${id}`, req);
  }

  deleteSurveyQuestion(courseId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/instructor/course-contents/${courseId}/survey-questions/${id}`);
  }

  // ── Dönemdeki Dersler ────────────────────────────────────────────────────

  getSemesters(): Observable<string[]> {
    return this.http.get<string[]>(`${this.base}/instructor/semesters`);
  }

  getTermCourses(semester?: string): Observable<InstructorCourseDto[]> {
    const url = semester
      ? `${this.base}/instructor/term-courses?semester=${encodeURIComponent(semester)}`
      : `${this.base}/instructor/term-courses`;
    return this.http.get<InstructorCourseDto[]>(url);
  }

  getTermCourseDetail(courseId: number): Observable<CourseDetailDto> {
    return this.http.get<CourseDetailDto>(`${this.base}/instructor/term-courses/${courseId}`);
  }

  // Students
  getStudents(courseId: number): Observable<StudentCourseResultDto[]> {
    return this.http.get<StudentCourseResultDto[]>(`${this.base}/instructor/term-courses/${courseId}/students`);
  }

  // Grades
  saveStudentGrades(courseId: number, studentId: number, req: SaveGradesRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/instructor/term-courses/${courseId}/students/${studentId}/grades`, req);
  }

  // Exams
  getExams(courseId: number): Observable<ExamDto[]> {
    return this.http.get<ExamDto[]>(`${this.base}/instructor/term-courses/${courseId}/exams`);
  }

  getExamDetail(courseId: number, examId: number): Observable<ExamDetailDto> {
    return this.http.get<ExamDetailDto>(`${this.base}/instructor/term-courses/${courseId}/exams/${examId}`);
  }

  addExam(courseId: number, req: SaveExamRequest): Observable<ExamDetailDto> {
    return this.http.post<ExamDetailDto>(`${this.base}/instructor/term-courses/${courseId}/exams`, req);
  }

  updateExam(courseId: number, id: number, req: SaveExamRequest): Observable<ExamDetailDto> {
    return this.http.put<ExamDetailDto>(`${this.base}/instructor/term-courses/${courseId}/exams/${id}`, req);
  }

  deleteExam(courseId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/instructor/term-courses/${courseId}/exams/${id}`);
  }

  getExamGradeEntry(courseId: number, examId: number): Observable<ExamGradeEntryDto> {
    return this.http.get<ExamGradeEntryDto>(`${this.base}/instructor/term-courses/${courseId}/exams/${examId}/grade-entry`);
  }

  saveExamGradeEntry(courseId: number, examId: number, req: SaveExamGradeEntryRequest): Observable<{ message: string; gradedStudentCount: number }> {
    return this.http.put<{ message: string; gradedStudentCount: number }>(`${this.base}/instructor/term-courses/${courseId}/exams/${examId}/grade-entry`, req);
  }

  // Assessment Components
  getAssessmentComponents(courseId: number): Observable<AssessmentComponentDto[]> {
    return this.http.get<AssessmentComponentDto[]>(`${this.base}/instructor/term-courses/${courseId}/assessment-components`);
  }

  addAssessmentComponent(courseId: number, req: SaveAssessmentComponentRequest): Observable<AssessmentComponentDto> {
    return this.http.post<AssessmentComponentDto>(`${this.base}/instructor/term-courses/${courseId}/assessment-components`, req);
  }

  updateAssessmentComponent(courseId: number, id: number, req: SaveAssessmentComponentRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/instructor/term-courses/${courseId}/assessment-components/${id}`, req);
  }

  deleteAssessmentComponent(courseId: number, id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/instructor/term-courses/${courseId}/assessment-components/${id}`);
  }

  getComponentGradeEntry(courseId: number, componentId: number): Observable<ComponentGradeEntryDto> {
    return this.http.get<ComponentGradeEntryDto>(`${this.base}/instructor/term-courses/${courseId}/assessment-components/${componentId}/grade-entry`);
  }

  saveComponentGradeEntry(courseId: number, componentId: number, req: SaveComponentGradeEntryRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.base}/instructor/term-courses/${courseId}/assessment-components/${componentId}/grade-entry`, req);
  }

  // Risk Analysis
  getRiskAnalysis(courseId: number): Observable<RiskAnalysisDto[]> {
    return this.http.get<RiskAnalysisDto[]>(`${this.base}/instructor/term-courses/${courseId}/risk-analysis`);
  }

  // Statistics
  getStatistics(courseId: number): Observable<CourseStatisticsDto> {
    return this.http.get<CourseStatisticsDto>(`${this.base}/instructor/term-courses/${courseId}/statistics`);
  }

  // LO Status
  getLearningOutcomeStatus(courseId: number): Observable<LearningOutcomeStatusDto[]> {
    return this.http.get<LearningOutcomeStatusDto[]>(`${this.base}/instructor/term-courses/${courseId}/learning-outcome-status`);
  }

  // Component Report
  getComponentReport(courseId: number): Observable<ComponentReportItemDto[]> {
    return this.http.get<ComponentReportItemDto[]>(`${this.base}/instructor/term-courses/${courseId}/component-report`);
  }
}
