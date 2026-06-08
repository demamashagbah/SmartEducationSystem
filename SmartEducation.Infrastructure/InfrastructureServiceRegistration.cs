using Microsoft.Extensions.DependencyInjection;
using SmartEducation.Application.Interfaces.Services;
using SmartEducation.Infrastructure.Services;

namespace SmartEducation.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<ISubjectService, SubjectService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IClassRoomService, ClassRoomService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IAcademicYearService, AcademicYearService>();
            services.AddScoped<ICurriculumService, CurriculumService>();
            services.AddScoped<IExamService, ExamService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<ITeacherGuideService, TeacherGuideService>();
            services.AddScoped<ICurriculumPlanService, CurriculumPlanService>();
            services.AddScoped<IAdminStudentService, AdminStudentService>();
            services.AddScoped<ITeacherAssignmentService, TeacherAssignmentService>();
            services.AddScoped<ITeacherManagementService, TeacherManagementService>();
            services.AddScoped<IParentManagementService, ParentManagementService>();

            return services;
        }
    }
}
