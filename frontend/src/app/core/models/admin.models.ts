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

// ── Genel Anket Soruları ──────────────────────────────────────────────────

export interface GeneralSurveyQuestionDto {
  id: number;
  questionText: string;
  orderNumber: number;
}

export interface SaveGeneralSurveyQuestionRequest {
  questionText: string;
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

export interface SurveyQuestionLOWeightDto {
  learningOutcomeId: number;
  learningOutcomeCode: string;
  weightPercentage: number;
}

export interface SurveyQuestionDto {
  id: number;
  courseId: number;
  questionText: string;
  isActive: boolean;
  loWeights: SurveyQuestionLOWeightDto[];
  /** @deprecated backend artık loWeights kullanıyor */
  learningOutcomeId?: number | null;
  /** @deprecated backend artık loWeights kullanıyor */
  learningOutcomeCode?: string | null;
}

export interface AdminCourseContentDto {
  courseDetail: CourseDetailDto;
  topics: CourseTopicDto[];
  learningOutcomes: LearningOutcomeDto[];
  matrix: MappingMatrixDto;
  surveyQuestions: SurveyQuestionDto[];
}
