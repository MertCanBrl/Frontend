export interface UserDto {
  id: number;
  fullName: string;
  email: string;
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
