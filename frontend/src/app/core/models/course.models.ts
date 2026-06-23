export interface InstructorCourseDto {
  id: number;
  code: string;
  name: string;
  semester: string;
  credit: number;
  akts: number;
  isMandatory: boolean;
  department: string;
  classYear: number;
  courseType: string;
  isLocked: boolean;
  contentStatus: string;
  topicCount: number;
  learningOutcomeCount: number;
}

export interface CourseDetailDto extends InstructorCourseDto {
  weeklyHours: number;
  instructorName: string | null;
  description: string | null;
  objective: string | null;
  submittedAt: string | null;
  approvedAt: string | null;
  reviewNote: string | null;
}

export interface UpdateCourseRequest {
  name: string;
  department: string;
  classYear: number;
  courseType: string;
  credit: number;
  akts: number;
  weeklyHours: number;
  isMandatory: boolean;
}

export interface UpdateCourseContentRequest {
  description: string | null;
  objective: string | null;
}

export interface CourseTopicDto {
  id: number;
  orderNumber: number;
  title: string;
  description: string | null;
}

export interface SaveCourseTopicRequest {
  orderNumber: number;
  title: string;
  description: string | null;
}

export interface LearningOutcomeDto {
  id: number;
  code: string;
  description: string;
  bloomLevel: string | null;
  component: string | null;
}

export interface SaveLearningOutcomeRequest {
  description: string;
  bloomLevel: string | null;
  component: string | null;
}

export interface ProgramOutcomeDto {
  id: number;
  code: string;
  description: string;
  details: string | null;
  groupId: number | null;
  groupName: string | null;
}

export interface SaveProgramOutcomeRequest {
  code: string;
  description: string;
  details: string | null;
  groupId: number | null;
}

export interface MappingCellDto {
  learningOutcomeId: number;
  programOutcomeId: number;
  contributionLevel: number;
}

export interface MappingMatrixDto {
  learningOutcomes: LearningOutcomeDto[];
  programOutcomes: ProgramOutcomeDto[];
  mappings: MappingCellDto[];
}

// Survey Questions
export interface SurveyQuestionDto {
  id: number;
  courseId: number;
  learningOutcomeId: number | null;
  learningOutcomeCode: string | null;
  questionText: string;
  isActive: boolean;
}

export interface SaveSurveyQuestionRequest {
  learningOutcomeId: number | null;
  questionText: string;
  isActive: boolean;
}

// Exams
export interface ExamDto {
  id: number;
  examType: string;
  examMethod: string;
  date: string | null;
  questionCount: number | null;
  description: string | null;
}

export interface SaveExamRequest {
  examType: string;
  examMethod: string;
  date: string | null;
  questionCount: number | null;
  description: string | null;
}

// Assessment Components
export interface AssessmentComponentDto {
  id: number;
  name: string;
  type: string;
  weight: number;
  date: string | null;
  description: string | null;
}

export interface SaveAssessmentComponentRequest {
  name: string;
  type: string;
  weight: number;
  date: string | null;
  description: string | null;
}

// Students
export interface StudentCourseResultDto {
  studentId: number;
  studentNo: string;
  fullName: string;
  email: string | null;
  midterm: number | null;
  final: number | null;
  makeUp: number | null;
}

// Risk Analysis
export interface RiskAnalysisDto {
  studentId: number;
  studentNo: string;
  fullName: string;
  gradeAverage: number | null;
  riskLevel: string;
  suggestion: string;
}

// Statistics (Dönem Sonu Raporları)
export interface GradeBucketDto {
  label: string;
  count: number;
}
export interface CourseStatisticsDto {
  totalStudents: number;
  gradedStudents: number;
  classAverage: number | null;
  passCount: number;
  failCount: number;
  distribution: GradeBucketDto[];
}

// Grades
export interface SaveGradesRequest {
  midterm: number | null;
  final: number | null;
  makeUp: number | null;
}
