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
  totalScore: number;
  weightPercentage: number | null;
  hasGrades: boolean;
  gradedStudentCount: number;
  totalStudentCount: number;
}

export interface ExamQuestionDto {
  id: number;
  questionNumber: number;
  description: string;
  score: number;
  difficulty: string;
  bookletAQuestionNumber: number | null;
  bookletBQuestionNumber: number | null;
  bookletCQuestionNumber: number | null;
  bookletDQuestionNumber: number | null;
  learningOutcomeIds: number[];
}

export interface ExamDetailDto extends ExamDto {
  questions: ExamQuestionDto[];
}

export interface SaveExamQuestionRequest {
  questionNumber: number;
  description: string;
  score: number;
  difficulty: string;
  bookletAQuestionNumber: number | null;
  bookletBQuestionNumber: number | null;
  bookletCQuestionNumber: number | null;
  bookletDQuestionNumber: number | null;
  learningOutcomeIds: number[];
}

export interface SaveExamRequest {
  examType: string;
  examMethod: string;
  date: string | null;
  questionCount: number | null;
  description: string | null;
  weightPercentage: number | null;
  questions: SaveExamQuestionRequest[];
}

// Assessment Components
export interface AssessmentComponentDto {
  id: number;
  name: string;
  type: string;
  weight: number;
  date: string | null;
  description: string | null;
  maxScore: number;
  isIncludedInAverage: boolean;
  gradeGroup: string | null;
  groupWeightPercentage: number;
  learningOutcomeIds: number[];
}

export interface SaveAssessmentComponentRequest {
  name: string;
  type: string;
  weight: number;
  date: string | null;
  description: string | null;
  maxScore: number;
  isIncludedInAverage: boolean;
  gradeGroup: string | null;
  groupWeightPercentage: number;
  learningOutcomeIds: number[];
}

export interface ComponentGradeEntryDto {
  componentId: number;
  name: string;
  type: string;
  maxScore: number;
  students: ComponentStudentGradeDto[];
}

export interface ComponentStudentGradeDto {
  studentId: number;
  studentNumber: string;
  fullName: string;
  score: number | null;
}

export interface SaveComponentGradeEntryRequest {
  students: Array<{ studentId: number; score: number | null; }>;
}

// Students
export interface StudentComponentScoreDto {
  componentId: number;
  componentName: string;
  score: number | null;
  maxScore: number;
}

export interface StudentCourseResultDto {
  studentId: number;
  studentNo: string;
  fullName: string;
  email: string | null;
  midterm: number | null;
  final: number | null;
  makeUp: number | null;
  weightedAverage: number | null;
  componentScores: StudentComponentScoreDto[];
  hasMissingGrades: boolean;
  missingComponentNames: string[];
}

// Risk Analysis
export interface RiskAnalysisDto {
  studentId: number;
  studentNo: string;
  fullName: string;
  gradeAverage: number | null;
  riskLevel: string;
  suggestion: string;
  hasMissingGrades: boolean;
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

// LO Status
export interface LoSourceDto {
  sourceType: string;
  sourceName: string;
  examType: string;
  averageNormalized: number | null;
  studentCount: number;
}

export interface LearningOutcomeStatusDto {
  learningOutcomeId: number;
  code: string;
  description: string;
  averageSuccess: number | null;
  sourceCount: number;
  sources: LoSourceDto[];
}

// Component Report
export interface ComponentReportItemDto {
  componentId: number;
  name: string;
  type: string;
  gradeGroup: string | null;
  groupWeightPercentage: number;
  maxScore: number;
  isIncludedInAverage: boolean;
  learningOutcomeIds: number[];
  totalStudents: number;
  gradedCount: number;
  averageScore: number | null;
  averageNormalized: number | null;
}

// Grades
export interface SaveGradesRequest {
  midterm: number | null;
  final: number | null;
  makeUp: number | null;
}

// Exam Grade Entry
export interface GradeEntryQuestionDto {
  id: number;
  questionNumber: number;
  description: string;
  maxScore: number;
}

export interface GradeEntryQuestionScoreDto {
  questionId: number;
  score: number | null;
}

export interface GradeEntryStudentDto {
  studentId: number;
  studentNumber: string;
  fullName: string;
  totalScore: number;
  isCompleted: boolean;
  questionScores: GradeEntryQuestionScoreDto[];
}

export interface ExamGradeEntryDto {
  examId: number;
  courseId: number;
  examType: string;
  examMethod: string;
  date: string | null;
  description: string | null;
  questions: GradeEntryQuestionDto[];
  students: GradeEntryStudentDto[];
}

export interface SaveExamGradeEntryRequest {
  students: Array<{
    studentId: number;
    questionScores: Array<{
      questionId: number;
      score: number;
    }>;
  }>;
}
