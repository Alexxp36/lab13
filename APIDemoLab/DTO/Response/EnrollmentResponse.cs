using APIDemoLab.Models;

namespace APIDemoLab.DTO.Response;

public sealed record EnrollmentResponse(Student Student, Course Course, EnrollmentDetails Enrollment);

public sealed record EnrollmentDetails(int EnrollmentId, int StudentId, int CourseId, DateTime Date);
