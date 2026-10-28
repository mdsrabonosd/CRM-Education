using CRM.Entities;
using CRM.Facade.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CRM.Web.UI.Controllers
{
    //[Authorize]
    public class CoursesController : Controller
    {
        private readonly ICourseFacade _courseFacade;
        private readonly IInstructorProfileFacade _instructorProfileFacade;

        public CoursesController(ICourseFacade courseFacade, IInstructorProfileFacade instructorProfileFacade)
        {
            _courseFacade = courseFacade;
            _instructorProfileFacade = instructorProfileFacade;
        }

        // GET: Courses
        public async Task<IActionResult> Index()
        {
            var courses = await _courseFacade.GetCoursesAsync();
            return View(courses);
        }

        // GET: Courses/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var course = await _courseFacade.GetCourseByIdAsync(id);

            if (course == null)
            {
                return NotFound();
            }

            var instructorList = await _instructorProfileFacade.GetInstructorProfilesAsync();
            var instructor = instructorList.FirstOrDefault(i => i.UserId == course.InstructorId);

            ViewBag.InstructorName = instructor != null ? instructor.Bio : $"Instructor #{course.InstructorId}";

            return View(course);
        }

        // GET: Courses/AddCourse/5 (Supports both Create and Edit)
        [HttpGet]
        public async Task<IActionResult> AddCourse(int id)
        {
            Course course = null;

            if (id > 0)
            {
                course = await _courseFacade.GetCourseByIdAsync(id);
            }

            if (course == null)
            {
                course = new Course();
            }

            var instructorList = await _instructorProfileFacade.GetInstructorProfilesAsync();

            ViewBag.Instructors = new SelectList(
                instructorList,
                "UserId",
                "Bio",
                course.InstructorId
            );

            return View(course);
        }

        // POST: Courses/AddCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(Course course)
        {
            try
            {
                if (course.CourseId > 0)
                {
                    await _courseFacade.UpdateCourseAsync(course);
                }
                else
                {
                    await _courseFacade.AddCourseAsync(course);
                }
            }
            catch (Exception ex)
            {
                var instructorList = await _instructorProfileFacade.GetInstructorProfilesAsync();
                ViewBag.Instructors = new SelectList(instructorList, "UserId", "Bio", course.InstructorId);
                ModelState.AddModelError("", "An error occurred while saving the course.");
                return View("AddCourse", course);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Courses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Note: If your ICourseFacade uses a different method name for deleting 
                // (e.g., DeleteCourse, RemoveCourseAsync), update it here to match your interface.
                await _courseFacade.DeleteCourseAsync(id);
            }
            catch (Exception ex)
            {
                // Handle deletion exceptions if necessary
            }

            return RedirectToAction(nameof(Index));
        }
    }
}