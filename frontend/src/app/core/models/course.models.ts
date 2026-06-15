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
  topicCount: number;
  learningOutcomeCount: number;
}

export interface CourseDetailDto extends InstructorCourseDto {
  weeklyHours: number;
  instructorName: string | null;
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
