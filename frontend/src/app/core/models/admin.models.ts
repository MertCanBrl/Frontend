export interface UserDto {
  id: number;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  role: string;
}

export interface CourseDto {
  id: number;
  code: string;
  name: string;
  semester: string;
  credit: number;
  isMandatory: boolean;
  classYear: number;
  instructorId: number | null;
  instructorName: string | null;
}

export interface CreateUserRequest {
  title: string;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  isAdmin: boolean;
}

export interface UpdateUserRequest {
  fullName: string;
  email: string;
  phoneNumber: string | null;
  isAdmin: boolean;
}

export interface CreateCourseRequest {
  code: string;
  name: string;
  semester: string;
  credit: number;
  isMandatory: boolean;
  classYear: number;
  instructorId: number | null;
}

export interface UpdateCourseRequest {
  code: string;
  name: string;
  semester: string;
  credit: number;
  isMandatory: boolean;
  classYear: number;
  instructorId: number | null;
}

export interface ApprovalListItemDto {
  courseId: number;
  courseCode: string;
  courseName: string;
  instructorName: string | null;
  submittedAt: string | null;
  contentStatus: string;
}

export interface RequestRevisionRequest {
  note: string;
}

// ── Ders İçeriği Önizleme ─────────────────────────────────────────────────

export interface CourseDetailDto {
  id: number;
  code: string;
  name: string;
  semester: string;
  credit: number;
  akts: number;
  weeklyHours: number;
  isMandatory: boolean;
  department: string;
  classYear: number;
  courseType: string;
  isLocked: boolean;
  contentStatus: string;
  topicCount: number;
  learningOutcomeCount: number;
  instructorName: string | null;
  description: string | null;
  objective: string | null;
  submittedAt: string | null;
  approvedAt: string | null;
  reviewNote: string | null;
}

export interface CourseTopicDto {
  id: number;
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

export interface MappingCellDto {
  learningOutcomeId: number;
  programOutcomeId: number;
  contributionLevel: number;
}

export interface PreviewProgramOutcomeDto {
  id: number;
  code: string;
  description: string;
}

export interface MappingMatrixDto {
  learningOutcomes: LearningOutcomeDto[];
  programOutcomes: PreviewProgramOutcomeDto[];
  mappings: MappingCellDto[];
}

export interface SurveyQuestionDto {
  id: number;
  courseId: number;
  learningOutcomeId: number | null;
  learningOutcomeCode: string | null;
  questionText: string;
  isActive: boolean;
}

export interface AdminCourseContentDto {
  courseDetail: CourseDetailDto;
  topics: CourseTopicDto[];
  learningOutcomes: LearningOutcomeDto[];
  matrix: MappingMatrixDto;
  surveyQuestions: SurveyQuestionDto[];
}
